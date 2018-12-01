using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace Hades.Core.Tools
{
    public static class ProjectInitializer
    {
        private static readonly Regex simpleName = new Regex("^[^\\.]*$");
        private static readonly Regex orgName = new Regex("^([^\\.]*)\\.([^\\.]*)\\.([^\\.]*)$");
        public static int Run(List<string> args)
        {
            var name = args.First();
            var dir = args[1]; //working name

            Repository.Init(dir); //Init git repository
            dir += Path.DirectorySeparatorChar;

            var src = dir + "src" + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(src);
            
            //SRC folder
            if (simpleName.IsMatch(name))
            {
                src += name;
                Directory.CreateDirectory(src);
            }
            else if (orgName.IsMatch(name))
            {
                foreach (var s in orgName.Match(name).Groups.Select(a => a.Value).Skip(1).Reverse())
                {
                    src += s;
                    Directory.CreateDirectory(src);
                    src += Path.DirectorySeparatorChar;
                }
            }
            else
            {
                Console.Error.WriteLine("Could not create project: name has invalid format!");
                return 1;
            }
            
            var mainhd = src + Path.DirectorySeparatorChar + "main.hd";
            File.WriteAllText(mainhd, "with console from std:io\nconsole->out:\"Hello\"");

            var projectjson = dir + Path.DirectorySeparatorChar + "project.json";
            File.WriteAllText(projectjson,""); //TODO: Fill project.json
            
            Directory.CreateDirectory(dir + "libs");
            
            using (var repo = new Repository(dir))
            {
                repo.Index.Add(projectjson.Substring(dir.Length + 1, projectjson.Length - dir.Length - 1));
                repo.Index.Add(mainhd.Substring(dir.Length, mainhd.Length - dir.Length).Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}",$"{Path.DirectorySeparatorChar}"));
                
                // Create the committer's signature and commit
                var author = new Signature("HadesProjectInitializer", "@hpi", DateTime.Now);
                var committer = author;

                // Commit to the repository
                repo.Commit("Initial commit", author, committer);
            }
            
            return 0;
        }
    }
}