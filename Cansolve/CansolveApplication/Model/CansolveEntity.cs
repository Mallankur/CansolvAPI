using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CansolveApplication.Model
{
    [BsonIgnoreExtraElements]
    public class CansolveEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string TagName { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime EventTime { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public Decimal128 Value { get; set; }


    }
}
