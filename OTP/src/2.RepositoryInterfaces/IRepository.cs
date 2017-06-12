using System;
using System.Linq;
using System.Linq.Expressions;

namespace OTP.RepositoryInterfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate = null);
        void Add(TEntity entity);
        void Delete(Guid id);
        void SaveChanges();
    }
}
