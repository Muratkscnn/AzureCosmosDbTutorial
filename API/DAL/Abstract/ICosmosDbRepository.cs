using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WebApplication4.DAL.Abstract
{
    public interface ICosmosDbRepository<T>
    {
        Task<T> GetByIdAsync(string id,string partitionKey);
        Task<List<T>> GetAllAsync(string partitionKey);
        Task<List<T>> GetByFilterAsync(Expression<Func<T, bool>> filterExpression);
        Task<int> CountAsync(Expression<Func<T, bool>> filterExpression);
        Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<T> CreateAsync(T item);
        Task<T> UpdateAsync(string id,string partitionKey, T item);
        Task DeleteAsync(string id,string partitionKey);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filterExpression);
    }
}