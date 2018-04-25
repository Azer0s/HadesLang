using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;

// ReSharper disable InconsistentNaming

namespace redis
{
    public class Library
    {
        public string get(string key,string connection)
        {
            key = key.Trim('\'');
            connection = connection.Trim('\'');
            try
            {
                var client = new RedisManagerPool(connection).GetClient();
                return client.Get<string>(key);
            }
            catch (Exception)
            {
                // ignored
            }
            return string.Empty;
        }

        public string set(string key, string value, string connection)
        {
            key = key.Trim('\'');
            value = value.Trim('\'');
            connection = connection.Trim('\'');
            
            Console.WriteLine(key);
            Console.WriteLine(value);
            Console.WriteLine(connection);
            
            try
            {
                var client = new RedisManagerPool(connection).GetClient();
                client.Set(key, value);
                return "true";
            }
            catch (Exception)
            {
                // ignored
            }
            return "false";
        }
    }
}
