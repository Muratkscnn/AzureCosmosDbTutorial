using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using WebApplication4.DAL.Abstract;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Azure.Documents.Linq;

namespace WebApplication4.DAL.Concrete
{
    public class CosmosDbRepository<T> : ICosmosDbRepository<T> where T : class
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private readonly string _collectionId;

        public CosmosDbRepository(string databaseId, string collectionId)
        {
            _client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["EndPointUri"]),
                ConfigurationManager.AppSettings["PrimaryKey"]);
            _databaseId = databaseId;
            _collectionId = collectionId;
        }

        public async Task<T> GetByIdAsync(string id,string partitionKey)
        {
            var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);

            // Belge oluşturulurken belirttiğiniz partitionKey değerini kullanın
            var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };

            var response = await _client.ReadDocumentAsync(documentUri, requestOptions);
            return JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }

        public async Task<List<T>> GetAllAsync()
        {
            var query = _client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    }) // Tüm partition'ları tarayarak sorgu yapılmasına izin verir
                .AsDocumentQuery();

            var items = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>();
                items.AddRange(response);
            }

            return items;
        }

        public async Task<List<T>> GetByFilterAsync(Expression<Func<T, bool>> filterExpression)
        {
            var query = _client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    }) // Tüm partition'ları tarayarak sorgu yapılmasına izin verir
                .Where(filterExpression)
                .AsDocumentQuery();

            var items = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>();
                items.AddRange(response);
            }

            return items;
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filterExpression)
        {
            var query = _client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    }) // Tüm partition'ları tarayarak sorgu yapılmasına izin verir
                .Where(filterExpression)
                .AsDocumentQuery();

            var count = 0;

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>();
                count += response.Count;
            }

            return count;
        }

        public async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    }) // Tüm partition'ları tarayarak sorgu yapılmasına izin verir
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsDocumentQuery();

            var items = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>();
                items.AddRange(response);
            }

            return items;
        }

        public async Task<T> CreateAsync(T item)
        {
            var response =
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                    item);
            return JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }

        public async Task<T> UpdateAsync(string id, string partitionKey, T item)
        {
            var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
            var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };
            var response = await _client.ReplaceDocumentAsync(documentUri, item, requestOptions);
            return JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }


        public async Task DeleteAsync(string id, string partitionKey)
        {
            var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
            var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };
            await _client.DeleteDocumentAsync(documentUri, requestOptions);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filterExpression)
        {
            return (await CountAsync(filterExpression)) > 0;
        }


    }
}