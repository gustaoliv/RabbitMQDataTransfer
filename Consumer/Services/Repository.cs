using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Types;

namespace Consumer.Services
{
    public class Repository
    {
        private readonly IMongoCollection<PersonObject> _collection;

        public Repository(DatabaseSettings databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.DatabaseName);

            _collection = mongoDatabase.GetCollection<PersonObject>(
                databaseSettings.CollectionName);
        }

        public async Task<List<PersonObject>> GetAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<PersonObject?> GetAsync(string id) =>
            await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(PersonObject newNameObj) =>
            await _collection.InsertOneAsync(newNameObj);

        public async Task UpdateAsync(string id, PersonObject updatedNameObj) =>
            await _collection.ReplaceOneAsync(x => x.Id == id, updatedNameObj);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(x => x.Id == id);
    }
}
