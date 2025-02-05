
using MongoDB.Driver;
using JWTAUTHAPP.Models;

namespace JWTAUTHAPP.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        
        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString); 
            _database = client.GetDatabase(settings.DatabaseName);  
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}
