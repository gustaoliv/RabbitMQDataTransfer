using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Types;

namespace Consumer
{
    public static class Startup
    {
        public static DatabaseSettings SetupMongoConfig(IConfiguration configuration)
        {
            DatabaseSettings settings = new DatabaseSettings()
            {
                ConnectionString    = configuration["MONGO_CONNECTIONSTRING"],
                DatabaseName        = configuration["MONGO_DATABASENAME"],
                CollectionName      = configuration["MONGO_COLLECTIONNAME"]
            };

            return settings;
        }
    }
}
