﻿using Domain.Common;
using System;

namespace Domain;

public class Question : BaseEntity
{
    public Guid ExamId { get; set; }
    public string Title { get; set; }
    public string Option1 { get; set; }
    public string Option2 { get; set; }
    public string Option3 { get; set; }
    public string Option4 { get; set; }
    public string Option5 { get; set; }
    public string CorrectOptions { get; set; } // Exemplos válidos "1" ou "1,2"
    public int DifficultyLevel { get; set; } // 1 = Fácil, 2 = Médio, 3 = Difícil

    // Relacionamento
    public virtual Exam Exam { get; set; }
}
