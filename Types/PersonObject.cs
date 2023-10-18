using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Types
{
    public class PersonObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id           { get; set; }

        public string Name          { get; set; }

        public string CPF           { get; set; }

        public string BirthDate     { get; set; }
        public string Gender        { get; set; }
        public string Phone         { get; set; }
        public bool Exported        { get; set; }
    }
}
