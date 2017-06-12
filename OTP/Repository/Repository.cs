using System;
using System.Linq;
using System.Linq.Expressions;
using OTP.Domain.Models;
using OTP.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Repo
{
    public sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseClass
    {
        private readonly DbContext _dbContext;

        public Repository(DbContext context)
        {
            _dbContext = context;
        }

        public void Add(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
            _dbContext.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var entity = _dbContext.Set<TEntity>().Find(id);
            _dbContext.Set<TEntity>().Remove(entity);
            _dbContext.SaveChanges();
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate = null)
        {
            return predicate == null ? _dbContext.Set<TEntity>() :
                _dbContext.Set<TEntity>().Where(predicate);
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }
    }
}
