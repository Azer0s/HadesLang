using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Variables;
using static HadesWeb.Log;

namespace HadesWeb
{
    class FileHelper
    {
        public static byte[] GetFile(string file)
        {
            var returnBytes = new byte[] { };
            try
            {
                returnBytes = File.ReadAllBytes($"wwwroot{file}");
                Success("Handled request successfully");
            }
            catch (Exception e)
            {
                Error($"Error while handling request - file wwwroot{file} does not exist!");
            }

            return returnBytes;
        }

        public static byte[] InterpretFile(string file, HttpListenerResponse response, Interpreter.Interpreter interpreter)
        {
            var fileWithPath = $"wwwroot/controller/{file}.hd";
            if (File.Exists(fileWithPath))
            {
                try
                {
                    var code = ViewEngine.RenderFile(file, interpreter);
                    response.StatusCode = code.Item2;

                    //Cleanup
                    Cache.Instance.Variables = new Dictionary<Meta, IVariable>();

                    Success($"Handled request successfully");
                    return code.Item1;
                }
                catch (Exception e)
                {
                    Error($"Error while handling request - /{file}");
                    response.StatusCode = 500;
                    return new byte[1];
                }
            }
            Error($"Error while handling request - file {file} does not exist!");
            response.StatusCode = 500;
            return new byte[1];
        }
    }
}
