using WebApplication4.Models;

namespace WebApplication4.DAL.Abstract
{
    public interface IStockDataRepository : ICosmosDbRepository<StockData>
    {
        
    }
}