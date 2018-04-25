using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;
using StringExtension;

// ReSharper disable InconsistentNaming

namespace redis
{
    public class Library
    {
        public string get(string key,string connection)
        {
            key = key.Trim('\'');
            var con = connection.Trim('{', '}').StringSplit(',').ToList();

            if (con.Count != 3)
            {
                return string.Empty;
            }

            Console.WriteLine(key);
            Console.WriteLine(connection);

            try
            {
                var client = new RedisClient(new RedisEndpoint(con[0].Trim('\''), int.Parse(con[1].Trim('\'')), con[2].Trim('\'')));
                return client.Get<string>(key);
            }
            catch (Exception e)
            {
                // ignored
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="connection">
        /// Connection[0] - connection string
        /// Connection[1] - port
        /// Connection[2] - password
        /// </param>
        /// <returns></returns>
        public string set(string key, string value, string connection)
        {
            key = key.Trim('\'');
            value = value.Trim('\'');
            var con = connection.Trim('{', '}').StringSplit(',').ToList();

            if (con.Count != 3)
            {
                return "false";
            }
            
            Console.WriteLine(key);
            Console.WriteLine(value);
            Console.WriteLine(connection);
            
            try
            {
                var client = new RedisClient(new RedisEndpoint(con[0].Trim('\''), int.Parse(con[1].Trim('\'')), con[2].Trim('\'')));
                client.Set(key, value);
                return "true";
            }
            catch (Exception e)
            {
                // ignored
            }
            return "false";
        }
    }
}
