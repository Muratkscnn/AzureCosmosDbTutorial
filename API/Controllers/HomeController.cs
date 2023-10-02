using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using WebApplication4.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using WebApplication4.DAL.Abstract;
using WebApplication4.DAL.Concrete;

namespace WebApplication4.Controllers
{
    public class HomeController : ApiController
    {
        private readonly IStockDataRepository _stockDataRepository;

        public HomeController(IStockDataRepository stockDataRepository)
        {
            _stockDataRepository = stockDataRepository;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<HttpResponseMessage> GetById(string id, string partitionKey)
        {
            var item = await _stockDataRepository.GetByIdAsync(id, partitionKey);
            return Request.CreateResponse(HttpStatusCode.OK, item);
        }
        [HttpGet]
        [Route("Home/GetById")]
        public async Task<HttpResponseMessage> Create(StockData item)
        {
            await _stockDataRepository.CreateAsync(item);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        [HttpGet]
        [Route("saveData")]
        public async Task<HttpResponseMessage> SaveData()
        {
            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync("https://localhost:44319/AssetSelect/GetLocalIndexAssets/XU030"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<AssetSelectResult>>(apiResponse);
                    foreach (var item in data)
                    {
                        using (var stockDataResponse = await client.GetAsync("https://localhost:44319/AssetSelect/GetStockData/"+ item.Code))
                        {
                            string stockDataApiResponse = await stockDataResponse.Content.ReadAsStringAsync();
                            var stockDataList = JsonConvert.DeserializeObject<List<ClosePriceModel>>(stockDataApiResponse);
                            foreach (var stockDataItem in stockDataList)
                            {
                                StockData stockData = new StockData()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    AdjClose = Convert.ToDouble(stockDataItem.AdjClose),
                                    Date = stockDataItem.Date,
                                    Code = stockDataItem.Symbol
                                };
                                await _stockDataRepository.CreateAsync(stockData);
                            }
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
    public class AssetSelectResult
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class ClosePriceModel
    {
        public string Symbol { get; set; }
        public string Date { get; set; }
        public float? AdjClose { get; set; }
    }
}