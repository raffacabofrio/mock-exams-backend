﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Domain.Exceptions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using MockExams.Infra.Database.UoW;
using MockExams.Infra.Database;
using System.Threading;

namespace MockExams.Service.Generic
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : BaseEntity
    {
        protected readonly ApplicationDbContext _ctx;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IValidator<TEntity> _validator;
        protected readonly DbSet<TEntity> _entity;

        public BaseService(ApplicationDbContext context, IUnitOfWork unitOfWork, IValidator<TEntity> validator)
        {
            _ctx = context;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _entity = _ctx.Set<TEntity>();
        }

        public virtual PagedList<TEntity> PagedList(int itemsPerPage = 10, int page = 1, string order = "CreatedAt Desc", string filter = "")
        {
            try
            {
                var query = _entity.AsQueryable();

                if (!string.IsNullOrEmpty(filter))
                    query = query.Where(filter);

                var total = query.Count();

                var skip = itemsPerPage * (page - 1);

                var items = query
                    .OrderBy(order)
                    .Skip(skip)
                    .Take(itemsPerPage)
                    .ToList();

                return new PagedList<TEntity>
                {
                    Items = items,
                    ItemsPerPage = itemsPerPage,
                    Page = page,
                    TotalItems = total,
                };
            }
            catch(Exception ex)
            {
                if (ex.Source == "System.Linq.Dynamic.Core")
                    throw new BizException(BizException.Error.BadRequest, ex.Message);
                else
                    throw;
            }

        }

        public virtual TEntity FindById(Guid Id)
        {
            var entity = _entity.Find(Id);

            if (entity == null) throw new BizException(BizException.Error.NotFound);

            return entity;
        }

        protected Result<TEntity> Validate(TEntity entity) => new Result<TEntity>(_validator.Validate(entity));

        protected Result<TEntity> Validate(TEntity entity, params Expression<Func<TEntity, object>>[] filter) => new Result<TEntity>(_validator.Validate(entity, options => options.IncludeProperties(filter)));

        public virtual Result<TEntity> Insert(TEntity entity)
        {
            var result = Validate(entity);

            if (result.Success)
            {
                _entity.Add(entity);
                _ctx.SaveChanges();
                result.Value = entity;
                result.SuccessMessage = "Inclusão efetuada com sucesso.";
            }
            else
            {
                result.Value = null;
            }

            return result;
        }

        public Result<TEntity> Delete(Guid Id)
        {
            var entity = _entity.Find(Id);

            if(entity == null) throw new BizException(BizException.Error.NotFound);

            _entity.Remove(entity);
            _ctx.SaveChanges();

            var result = new Result<TEntity>(entity);
            result.SuccessMessage = "Exclusão efetuada com sucesso.";
            return result;
        }

        public virtual Result<TEntity> Update(TEntity entity)
        {
            if (!_entity.Any(e => e.Id == entity.Id)) throw new BizException(BizException.Error.NotFound);

            _entity.Update(entity);
            _ctx.SaveChanges();

            var result = new Result<TEntity>(entity);
            result.SuccessMessage = "Alteração efetuada com sucesso.";
            return result;
        }

        protected Guid GetCurrentUserId()
        {
            return new Guid(Thread.CurrentPrincipal?.Identity?.Name);
        }

    }
}