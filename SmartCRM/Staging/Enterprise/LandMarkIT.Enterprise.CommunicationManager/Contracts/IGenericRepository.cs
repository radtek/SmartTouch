using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LandmarkIT.Enterprise.CommunicationManager.Contracts
{
    public interface IGenericRepository<T>
        where T : class
    {
        IQueryable<T> AsQueryable();
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        T Single(Expression<Func<T, bool>> predicate);
        T SingleOrDefault(Expression<Func<T, bool>> predicate);
        T First(Expression<Func<T, bool>> predicate);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Edit(T entity);
        void Delete(T entity);
        void Attach(T entity);
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
        IEnumerable<T> OrderBy<Tkey>(Expression<Func<T, Tkey>> keySelector);
        IEnumerable<T> OrderByDescending<Tkey>(Expression<Func<T, Tkey>> keySelector);
        void RemoveRange(IEnumerable<T> entities);
    }
}
