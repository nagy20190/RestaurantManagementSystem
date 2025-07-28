using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryManagementSystem.Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll(); 
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
         IQueryable<T> FindByCondition(Expression<Func<T, bool>> predicate);
         Task<int> CountAsync();
         IQueryable<T> GetPaged(int pageNumber, int pageSize);
    }

}
