using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;

namespace BetterSecondBotShared.IO
{
    public class SimpleIO
    {
        protected string root_folder = "";
        public void ChangeRoot(string A)
        {

            root_folder = A + "/";
            create_dir(root_folder);
        }
        public void create_dir(string target_folder)
        {
            if (dir_exists(root_folder) == false)
            {
                System.IO.Directory.CreateDirectory(root_folder);
            }
        }
        public static bool dir_exists(string target_folder)
        {
            return System.IO.Directory.Exists(target_folder);
        }
        public void Delete(string filename)
        {
            System.IO.File.Delete(root_folder+filename);
        }
        public void writefile(string filename,string content)
        {
            if(Exists(filename))
            {
                Delete(filename);
            }
            System.IO.File.WriteAllText(@""+root_folder + "" + filename, content);
        }
        public void WriteJsonConfig(JsonConfig Config, string targetfile)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@""+root_folder + "" + targetfile))
            using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(writer, Config);
            }
        }
        public void WriteJsonCommands(JsonCommandsfile commandsfile, string targetfile)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@"" + root_folder + "" + targetfile))
            using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(writer, commandsfile);
            }
        }

        public void makeOld(string targetfile)
        {
            System.IO.File.Move(@""+root_folder + "" + targetfile, @"" + root_folder + "" + targetfile+".old");
        }
        public bool Exists(string targetfile)
        {
            return File.Exists(@""+root_folder+"" + targetfile);
        }
        public static bool FileType(string targetfile, string matchtype)
        {
            string[] bits = targetfile.Split('.');
            if (bits[^1] == matchtype)
            {
                return true;
            }
            return false;
        }
        public string ReadFile(string targetfile)
        {
            if (Exists(targetfile) == true)
            {
                string return_text = "";
                using (var Stream = new FileStream(@"" + root_folder + "" + targetfile, FileMode.Open, FileAccess.Read))
                using (var StreamReader = new StreamReader(Stream))
                {
                    return_text = StreamReader.ReadToEnd();
                }
                return return_text;
            }
            return "";
        }
    }
}
