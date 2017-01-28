using MongoDB.Driver;
    
namespace Production_Electricite.Controllers.utils
{
    public class MongoDB
    {
        public static IMongoClient _client;
        public static IMongoDatabase _database;

        public void connectToMongo()
        {
            _client = new MongoClient("mongodb://localhost:27017");
            _database = _client.GetDatabase("local");
        }

        /// <summary>
        /// récupère une collection MongoDB
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public IMongoCollection<T> getCollection<T>(string collection)
        {
            connectToMongo();
            return _database.GetCollection<T>(collection);
        }

    }
}