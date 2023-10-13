using Anurgenlib;
using CansolveAnkurLib;
using CansolveApplication.Model;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace CansolveApplication.CansolveApplicationServises
{

    public class CansolveServises : IcansolveServises
    {
        public  IMongoCollection<CansolveEntity> sampleData { get; set; }
     
        public CansolveServises(IOptions<MongoSocket> connect)
        {
            MongoClient client = new MongoClient(connect.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(connect.Value.DatabaseName);
            sampleData = database.GetCollection<CansolveEntity>(connect.Value.CollectionName);

        }

        /// <@Ankur_MALL>
        /// 
        /// </summary>
        /// <param name="EventTime"></param>
        /// <returns></returns>
        public async Task<List<CansolveEntity>> GetByEvenTimeAsync(DateTime EventTime)
        {
            var RES = await sampleData.Find(X => X.EventTime == EventTime).ToListAsync();
            return RES;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<CansolveEntity> GetByIdAsync(string tname)
        {
            return await sampleData.Find(x => x.TagName == tname).FirstAsync();
        }


        /// <summary>
        /// @aVGAnkurMall
        /// </summary>
        /// <param name="EVENTtIME"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public async Task<AggregationModelResult> GetAvgValue(DateTime eventTime)
        {
            var connectionstring = "mongodb://10.2.10.19:27017";
            string databaseName = "RealTime";
            string CollectionsName = "CansolvData";
            if (eventTime == null)
            {
                return null;
            }

            var res = new CansolvJfrog(connectionstring, databaseName, CollectionsName);
            var result = await Task.Run(() => res.CalculateAverageForEventTime(eventTime));
            return new AggregationModelResult
            {
                Id = result.Id,
                Average = result.Average,
                EventTime = result.EventTime,
            };
                


            
        }



    }



    public class MongoSocket
        {
            public string CollectionName { get; set; }
            public string ConnectionString { get; set; }
            public string DatabaseName { get; set; }
        }

        public static class MongoCollectionExtensions
        {
        /// <summary>
        /// writternby@AnkurMall_This template method for the pasing to beson for pass the calculations the mongo ! 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
            public static IMongoCollection<BsonDocument> AsBsonDocumentCollection<T>(this IMongoCollection<T> collection)
            {
                return collection as IMongoCollection<BsonDocument>;
            }
        }

    }

