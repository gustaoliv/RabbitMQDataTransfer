using MongoDB.Driver;
using Types;


namespace Consumer.Services
{
    public class Repository
    {
        private readonly IMongoCollection<CodeObject> _collection;

        public Repository(DatabaseSettings databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.DatabaseName);

            _collection = mongoDatabase.GetCollection<CodeObject>(
                databaseSettings.CollectionName);
        }

        public async Task<List<CodeObject>> GetAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<CodeObject?> GetAsync(int id) =>
            await _collection.Find(x => x.MT_CODIGO == id).FirstOrDefaultAsync();

        public async Task CreateAsync(CodeObject newNameObj) =>
            await _collection.InsertOneAsync(newNameObj);

        public async Task UpdateAsync(int id, CodeObject updatedNameObj) =>
            await _collection.ReplaceOneAsync(x => x.MT_CODIGO == id, updatedNameObj);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(x => x.MT_METODO == id);
    }
}
