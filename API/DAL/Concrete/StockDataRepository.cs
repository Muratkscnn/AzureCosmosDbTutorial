using WebApplication4.DAL.Abstract;
using WebApplication4.Models;

namespace WebApplication4.DAL.Concrete
{
    public class StockDataRepository : CosmosDbRepository<StockData>, IStockDataRepository
    {
        public StockDataRepository() : base("ToDoList", "StockData")
        {

        }
    }
}