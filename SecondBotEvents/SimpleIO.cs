using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SecondBotEvents
{
    public class SimpleIO
    {
        protected string root_folder = "";
        public void ChangeRoot(string A)
        {
            root_folder = A;
            if (root_folder != "")
            {
                if (DirExists(root_folder) == false)
                {
                    System.IO.Directory.CreateDirectory(root_folder);
                }
            }
        }
        public static bool DirExists(string target_folder)
        {
            return System.IO.Directory.Exists(target_folder);
        }
        public void Delete(string filename)
        {
            System.IO.File.Delete(root_folder+"/"+filename);
        }
        public void WriteFile(string filename,string content)
        {
            if(Exists(filename) == true)
            {
                Delete(filename);
            }
            System.IO.File.WriteAllText(@""+root_folder + "/" + filename, content);
        }

        public void MarkOld(string targetfile)
        {
            System.IO.File.Move(@""+root_folder + "/" + targetfile, @"" + root_folder + "/" + targetfile+".old");
        }
        public bool Exists(string targetfile)
        {
            return File.Exists(@""+root_folder+"/" + targetfile);
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
                using (var Stream = new FileStream(@"" + root_folder + "/" + targetfile, FileMode.Open, FileAccess.Read))
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
