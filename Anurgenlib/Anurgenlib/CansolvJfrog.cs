using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace Anurgenlib
{
    public class CansolvJfrog
    {
        private IMongoCollection<BsonDocument> collection;
        /// <summary>
        /// theclientconnwction@ankur
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"></param>
        /// <param name="collectionName"></param>
        public CansolvJfrog(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            collection = database.GetCollection<BsonDocument>(collectionName);
        }
        /// <summary>
        /// AnkurMall@ mongoPipelinefILTEFOR
        /// </summary>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public AggregationModel CalculateAverageForEventTime(DateTime eventTime)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("EventTime", eventTime);

            var random = new Random();
            int randomId = random.Next(1,50); // Generate a random integer

            var aggregation = collection.Aggregate()
                .Match(filter)
                .Group(new BsonDocument
                {
            { "_id", randomId },
            { "average", new BsonDocument("$avg", "$Value") }
                });

            var result = aggregation.FirstOrDefault();

            if (result != null)
            {
                var averageValue = result.GetValue("average", 0.0).ToDouble();

                return new AggregationModel
                {
                    Id = randomId,
                    EventTime = eventTime,
                    Average = averageValue
                };
            }

            return null;
        }
    }
}
