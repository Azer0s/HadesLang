using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ServiceStack.Redis;
using Console = Colorful.Console;

namespace HadesWeb
{
    class Log
    {
        public static RedisManagerPool Manager;
        public static bool _log = false;

        #region Log

        public static readonly Color Blue = Color.FromArgb(240, 6, 153);
        private static readonly Color Yellow = Color.FromArgb(247, 208, 2);
        private static readonly Color Purple = Color.FromArgb(69, 78, 158);
        private static readonly Color Red = Color.FromArgb(191, 26, 47);
        private static readonly Color Green = Color.FromArgb(1, 142, 66);

        private static string Time()
        {
            var now = $"[{DateTime.UtcNow:o}]";
            Colorful.Console.Write(now, Yellow);
            return now;
        }

        public static void Error(string message)
        {
            var now = Time();

            if (_log)
            {
                using (var client = Manager.GetClient())
                {
                    client.Set(now, message);
                }
            }

            Colorful.Console.WriteLine($" {message}", Red);
        }

        public static void Info(string message)
        {
            Time();
            Colorful.Console.WriteLine($" {message}", Purple);
        }

        public static void Success(string message)
        {
            Time();
            Console.WriteLine($" {message}", Green);
        }


        #endregion
    }
}
