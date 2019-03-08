using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using LandmarkIT.Enterprise.CommunicationManager.Database;

namespace LandmarkIT.Enterprise.CommunicationManager.Repositories
{
    public class EfGenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        private readonly DbSet<T> _dbSet;
        private readonly DbContext _entitiesContext;

        public EfGenericRepository(DbSet<T> dbSet, DbContext entitiesContext)
        {
            _dbSet = dbSet;
            _entitiesContext = entitiesContext;
        }

        #region IGenericRepository<T> implementation

        public virtual IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public IEnumerable<T> GetAll()
        {
            if (typeof(T).Equals(typeof(JobDb)))
                return _dbSet.Include("ScheduledJobs");            
            else
                return _dbSet;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
        public T Single(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).Single();
        }
        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).SingleOrDefault();
        }
        public T First(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).First();
        }
        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).FirstOrDefault();
        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _entitiesContext.Entry(entity).State = EntityState.Added;
        }
        public void Edit(T entity)
        {
            _dbSet.Add(entity);
            _entitiesContext.Entry(entity).State = EntityState.Modified;
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _entitiesContext.Entry(entity).State = EntityState.Deleted;
        }        
        public void Attach(T entity)
        {
            _dbSet.Attach(entity);            
        }
        public IEnumerable<T> OrderBy<Tkey>(Expression<Func<T, Tkey>> keySelector)
        {
            return _dbSet.OrderBy(keySelector);            
        }
        public IEnumerable<T> OrderByDescending<Tkey>(Expression<Func<T, Tkey>> keySelector)
        {
            return _dbSet.OrderByDescending(keySelector);
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
        // And so on ...

        #endregion
    }
}
