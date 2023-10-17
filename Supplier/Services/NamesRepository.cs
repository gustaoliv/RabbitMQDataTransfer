using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Types;

namespace Supplier.Services
{
    public class NamesRepository
    {
        private readonly IMongoCollection<NameObject> _collection;

        public NamesRepository(DatabaseSettings databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.DatabaseName);

            _collection = mongoDatabase.GetCollection<NameObject>(
                databaseSettings.CollectionName);
        }

        public async Task<List<NameObject>> GetAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<NameObject?> GetAsync(string id) =>
            await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(NameObject newNameObj) =>
            await _collection.InsertOneAsync(newNameObj);

        public async Task UpdateAsync(string id, NameObject updatedNameObj) =>
            await _collection.ReplaceOneAsync(x => x.Id == id, updatedNameObj);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(x => x.Id == id);
    }
}
