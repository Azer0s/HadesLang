using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Colorful;
using Output;
using Interpreter = Interpreter.Interpreter;
using Console = Colorful.Console;

namespace HadesWeb
{
    class Program
    {
        private static global::Interpreter.Interpreter _interpreter;
        private static readonly Color BLUE = Color.FromArgb(240, 6, 153);
        private static readonly Color YELLOW = Color.FromArgb(247, 208, 2);
        private static readonly Color PURPLE = Color.FromArgb(69, 78, 158);
        private static readonly Color RED = Color.FromArgb(191, 26, 47);
        private static readonly Color GREEN = Color.FromArgb(1, 142, 66);

        private static void Time()
        {
            Console.Write($"[{DateTime.UtcNow:o}]", YELLOW);
        }

        private static void Error(string message)
        {
            Time();
            Console.WriteLine($" {message}", RED);
        }

        private static void Info(string message)
        {
            Time();
            Console.WriteLine($" {message}", PURPLE);
        }

        private static void Success(string message)
        {
            Time();
            Console.WriteLine($" {message}",GREEN);
        }

        public static void Main()
        {
            Console.Title = "HadesWeb Server";
            Console.WriteAscii("HadesWeb Server", BLUE);
            Info("Initializing Hades Interpreter...");
            _interpreter = new global::Interpreter.Interpreter(new NoOutput(), new NoOutput());

            var lines = File.ReadAllLines("config").ToList();
            var address = lines.Where(a => a.StartsWith("address"))
                .Select(a => a.Replace("address:", "").Replace(" ", "")).First();
            var port = lines.Where(a => a.StartsWith("port"))
                .Select(a => a.Replace("port:", "").Replace(" ", "")).First();

            var listener = new HttpListener();
            listener.Prefixes.Add($"http://{address}:{port}/");
            Info($"Listening @ {address} on port {port}");
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;
                var returnBytes = new byte[] { };

                Info($"Request - {request.RawUrl}");
                if (request.RawUrl == "/" || request.RawUrl == "/#")
                {
                    returnBytes = InterpretFile("index.hd", response);
                }
                else
                {
                    if (request.RawUrl.EndsWith(".hd"))
                    {
                        returnBytes = InterpretFile(request.RawUrl.TrimStart('/'), response);
                    }
                    else
                    {
                        try
                        {
                            returnBytes = File.ReadAllBytes($"wwwroot{request.RawUrl}");
                            Success("Handled request successfully");
                        }
                        catch (Exception e)
                        {
                            Error($"Error while handling request - file wwwroot{request.RawUrl} does not exist!");
                        }
                    }
                }

                response.ContentLength64 = returnBytes.Length;
                var output = response.OutputStream;
                output.Write(returnBytes, 0, returnBytes.Length);
                output.Close();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public static byte[] InterpretFile(string file, HttpListenerResponse response)
        {
            var fileWithPath = $"wwwroot/views/{file}";
            if (File.Exists(fileWithPath))
            {
                try
                {
                    var woutput = new WebOutput();
                    _interpreter.SetOutput(woutput, woutput);
                    var code = _interpreter.InterpretLine($"with '{fileWithPath}'", new List<string> {"web"}, null);
                    response.StatusCode = int.Parse(code);
                    Success($"Handled request successfully");
                    return Encoding.UTF8.GetBytes(woutput.Output.ToString());
                }
                catch (Exception)
                {
                    Error($"Error while handling request - /{file}");
                }
            }
            Error($"Error while handling request - file {file} does not exist!");
            response.StatusCode = 500;
            return new byte[1];
        }
    }
}
