using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Types;

namespace Supplier
{
    public static class Startup
    {
        public static DatabaseSettings SetupMongoConfig(IConfiguration configuration)
        {
            DatabaseSettings settings = new DatabaseSettings()
            {
                ConnectionString    = configuration["CONNECTIONSTRING"],
                DatabaseName        = configuration["DATABASENAME"],
                CollectionName      = configuration["COLLECTIONNAME"]
            };

            return settings;
        }

    }
}
