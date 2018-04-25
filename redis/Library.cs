﻿using System;
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
