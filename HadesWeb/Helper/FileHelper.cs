using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HadesWeb.Server;
using HadesWeb.Util;
using Variables;

namespace HadesWeb.Helper
{
    class FileHelper
    {
        public static byte[] GetFile(string file)
        {
            var returnBytes = new byte[] { };
            try
            {
                returnBytes = File.ReadAllBytes($"wwwroot{file}");
                Log.Success("Handled request successfully");
            }
            catch (Exception e)
            {
                Log.Error($"Error while handling request - file wwwroot{file} does not exist!");
            }

            return returnBytes;
        }

        public static byte[] InterpretFile(string file, HttpListenerResponse response, Interpreter.Interpreter interpreter, string controllerBasePath = "wwwroot/controller/{0}.hd", string viewBasePath = "wwwroot/view/{0}.hdhtml")
        {
            var fileWithPath = string.Format(controllerBasePath,file);
            if (File.Exists(fileWithPath))
            {
                try
                {
                    var code = ViewEngine.RenderFile(file, interpreter,controllerBasePath,viewBasePath);
                    response.StatusCode = code.Item2;

                    //Cleanup
                    Cache.Instance.Variables = new Dictionary<Meta, IVariable>();

                    Log.Success($"Handled request successfully");
                    return code.Item1;
                }
                catch (Exception e)
                {
                    Log.Error($"Error while handling request - /{file}");
                    response.StatusCode = 500;
                    return new byte[1];
                }
            }
            Log.Error($"Error while handling request - file {file} does not exist!");
            response.StatusCode = 500;
            return new byte[1];
        }
    }
}
