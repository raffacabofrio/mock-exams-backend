﻿using AutoMapper;
using Domain;
using FluentValidation;
using MockExams.Infra.Database.UoW;
using MockExams.Infra.Database;
using MockExams.Infra.Sms;
using MockExams.Service.Generic;
using Domain.DTOs;
using System;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Domain.Enums;
using Domain.DTOs.Exam;
using System.Threading.Tasks;
using Domain.Common;
using Service.Exam.Generator;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace MockExams.Service;

public class ExamService : BaseService<Exam>, IExamService
{
    private readonly IMapper _mapper;
    private readonly IExamGeneratorService _generator;
    protected ILogger<ExamService> _logger;


    public ExamService(ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IValidator<Exam> validator, IMapper mapper,
        IUserEmailService userEmailService, ISmsService smsService, IExamGeneratorService generator, ILogger<ExamService> logger) : base(context, unitOfWork, validator)
    {
        _mapper = mapper;
        _generator = generator;
        _logger = logger;
    }

    public StartExamAttemptDto StartExamAttempt(Guid? userId, Guid examId)
    {
        // 1 - validar
        if(!userId.HasValue)
            throw new BizException(BizException.Error.NotAuthorized);

        var exam = _ctx.Exams.FirstOrDefault(e => e.Id == examId);
        if (exam == null)
            throw new BizException(BizException.Error.NotFound, $"Não encontramos o exame '{examId.ToString()}'.");

        // 2 - criar tentativa de exame
        var attempt = new ExamAttempt()
        {
            UserId = userId.Value,
            ExamId = examId,

        };

        // 3 - sortear n questões e popular a lista de respostas vazias
        var randomQuestions = _ctx.Questions
            .Where(q => q.ExamId == examId)
            .OrderBy(q => Guid.NewGuid()) // Ordena aleatoriamente
            .Take( exam.TotalQuestionsPerAttempt ) // Seleciona N perguntas
            .ToList();

        attempt.Answers = randomQuestions.Select(q => new Answer() 
        {
            QuestionId = q.Id,
            ExamAttemptId = attempt.Id
        }).ToList();

        _ctx.ExamAttempts.Add(attempt);
        _ctx.SaveChanges();

        var startDto = new StartExamAttemptDto()
        {
            ExamId = examId,
            UserId = userId.Value,
            Questions = _mapper.Map<IList<QuestionDto>>(randomQuestions)
        };

        return startDto;
    }

    public ExamAttemptDto FinishExamAttempt(Guid? userId, FinishExamAttemptDto finishDto)
    {
        // 1 - validar
        if (!userId.HasValue)
            throw new BizException(BizException.Error.NotAuthorized);

        var attempt = _ctx.ExamAttempts.FirstOrDefault(a => a.Id == finishDto.ExamAttemptId);
        if (attempt == null)
            throw new BizException(BizException.Error.NotFound, $"Não encontramos a tentativa de exame '{finishDto.ExamAttemptId.ToString()}'.");

        if(attempt.Status == ExamAttemptStatus.Completed)
            throw new BizException(BizException.Error.BadRequest, "Esta tentativa de exame já foi finalizada.");
        
        if(userId != attempt.UserId)
                throw new BizException(BizException.Error.Forbidden, "Você não tem permissão para finalizar esta tentativa de exame porque não é sua.");

        // 2 - popular respostas
        foreach (var answer in attempt.Answers)
        {
            var answerDto = finishDto.Answers.FirstOrDefault(a => a.QuestionId == answer.QuestionId);
            if (answerDto == null)
                throw new BizException(BizException.Error.BadRequest, $"Não encontramos a resposta para a questão '{answerDto.QuestionId.ToString()}'.");
            
            answer.SelectedOptions = answerDto.SelectedOptions;
            answer.IsCorrect = answer.SelectedOptions == answer.Question.CorrectOptions;
        }

        // 3 - calcular resultado
        attempt.Status = ExamAttemptStatus.Completed;
        attempt.FinishedAt = DateTime.UtcNow;
        attempt.TimeSpentSeconds = (int)(attempt.FinishedAt - attempt.CreatedAt).Value.TotalSeconds;

        var totalCorrect = attempt.Answers.Count(a => (bool)a.IsCorrect);
        var totalQuestions = attempt.Answers.Count();
        attempt.Score = (int)((totalCorrect / (double)totalQuestions) * 100);

        _ctx.SaveChanges();

        var attemptDto = _mapper.Map<ExamAttemptDto>(attempt);
        return attemptDto;
    }

    public IList<MyExamAttemptDto> MyExamAttempts(Guid? userId)
    {
        if (!userId.HasValue)
            throw new BizException(BizException.Error.NotAuthorized);


        var attempts = _ctx.ExamAttempts
            .Include(a => a.Exam)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)  
            .ToList();

        var attemptsDto = _mapper.Map<IList<MyExamAttemptDto>>(attempts);
        return attemptsDto;
    }

    public MyExamAttemptDetailsDto MyExamAttemptDetails(Guid? userId, Guid attemptId)
    {
        if (!userId.HasValue)
            throw new BizException(BizException.Error.NotAuthorized);

        var attempt = _ctx.ExamAttempts
            .Include(a => a.Exam)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
            .FirstOrDefault(a => a.Id == attemptId && a.UserId == userId);

        if (attempt == null)
            throw new BizException(BizException.Error.NotFound);

        var questionsWithAnswers = _mapper.Map<List<QuestionWithAnswerDto>>(attempt.Answers);
        var result = _mapper.Map<MyExamAttemptDetailsDto>(attempt.Exam);

        result.Id = attempt.Id;
        result.TimeSpentSeconds = attempt.TimeSpentSeconds;
        result.Score = attempt.Score;
        result.FinishedAt = attempt.FinishedAt;
        result.QuestionWithAnswer = questionsWithAnswers;
        result.Status = attempt.Status;

        return result;
    }

    public async Task<List<ExamDto>> Search(string term = "")
    {
        if (string.IsNullOrEmpty(term) || term.Length < 2)
            throw new BizException(BizException.Error.BadRequest, "Favor refinar mais o termo de pesquisa.");

        var exams = await FullTextSearch(term);
            
        if (!exams.Any())
        {
            try
            {
                _logger.LogInformation($"Nenhum exame encontrado com o termo '{term}'. Tentando criar um novo usando IA.");

                var newExam = await _generator.GenerateAsync(term);
                newExam.TimeSpentMaxSeconds = 600; // 10 minutos
                newExam.TotalQuestionsPerAttempt = 5;
                _ctx.Exams.Add(newExam);
                _ctx.SaveChanges();

                exams.Add(newExam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar exame via IA.");
            }
            
        }

        var examsDto = _mapper.Map<List<ExamDto>>(exams);
        return examsDto;
    }

    private async Task<List<Exam>> FullTextSearch(string term)
    {
        term = term.ToLower().Trim();

        var tokens = term
            .Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var isaboutClause = string.Join(", ", tokens.Select(t =>
            $"\"{t.Replace("\"", "").ToUpper()}*\" WEIGHT({(1.0 / tokens.Length).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)})"));

        var fullTextFilter = $"ISABOUT ({isaboutClause})";

        var sql = $@"
        SELECT TOP 10 e.*
        FROM CONTAINSTABLE(Exams, (Title, Description), {{0}}) AS ft
        JOIN Exams e ON e.Id = ft.[KEY]
        ORDER BY ft.[RANK] DESC";

        return await _ctx.Exams
            .FromSqlRaw(sql, fullTextFilter)
            .ToListAsync();
    }
}