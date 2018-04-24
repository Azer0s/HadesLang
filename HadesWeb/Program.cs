using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Output;
using Interpreter = Interpreter.Interpreter;

namespace HadesWeb
{
    class Program
    {
        private static readonly global::Interpreter.Interpreter Interpreter =
            new global::Interpreter.Interpreter(new NoOutput(), new NoOutput());

        public static void Main()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5678/");
            Console.WriteLine("Listening...");
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;
                var returnBytes = new byte[1];

                Console.WriteLine(request.RawUrl);
                if (request.RawUrl == "/favicon.ico")
                {
                    returnBytes = File.ReadAllBytes("wwwroot/favicon.ico");
                }
                else if (request.RawUrl == "/" || request.RawUrl == "/#")
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
                        response.StatusCode = 404;
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
            file = $"wwwroot/views/{file}";
            if (File.Exists(file))
            {
                var woutput = new WebOutput();
                Interpreter.SetOutput(woutput,woutput);
                var code = Interpreter.InterpretLine($"with '{file}'", new List<string> { "web" }, null);
                response.StatusCode = int.Parse(code);
                return Encoding.UTF8.GetBytes(woutput.Output.ToString());
            }
            response.StatusCode = 500;
            return new byte[1];
        }
    }
}
