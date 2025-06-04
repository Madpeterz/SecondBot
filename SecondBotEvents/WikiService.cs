using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using RestSharp.Extensions;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;

namespace SecondBotEvents
{
    public class WikiMake
    {
        protected SimpleIO IO = new();
        protected string version = "";
        protected string htmltemplate = "";
        public WikiMake()
        {
            IO.ChangeRoot("Static");
            htmltemplate = IO.ReadFile("template.html");
            version = AssemblyInfo.GetGitHash();
            LogFormater.Info("Creating wiki files");
            IO.ChangeRoot("wiki");
            makehome();
        }

        protected void makehome()
        {
            
            Dictionary<string, Type> commandmodules = http_commands_helper.getCommandModules();
            string content = "<div class=\"card-deck\">";
            foreach (KeyValuePair<string, Type> entry in commandmodules)
            {
                Type endpointtype = entry.Value;
                string namespaceworker = entry.Key.ToLower();
                string url = namespaceworker + ".html";
                int commandcount = 0;
                foreach (MethodInfo M in endpointtype.GetMethods())
                {
                    foreach (CustomAttributeData At in M.CustomAttributes)
                    {
                        if (At.AttributeType.Name == "About")
                        {
                            commandcount++;
                            break;
                        }
                    }
                }
                string about = entry.Value.GetAttribute<ClassInfo>().classinfo;
                string card = "<div class=\"card mt-2 mx-2\" style=\"width: 18rem;\">"
                        + "<div class=\"card-body\">"
                        + "<h5 class=\"card-title\">" + namespaceworker.FirstCharToUpper() + "</h5>"
                        + "<p class=\"card-text\">" + about.Split("<br")[0] + "</p>"
                        + "<a href = \"" + url + "\" class=\"btn btn-primary\">View " + commandcount.ToString() + " commands</a>"
                        + "</div>"
                        + "</div>";
                content = content + card;
                makecommandlist(endpointtype, namespaceworker, about);
            }
            makefile("index", content, "Index");
        }

        protected void makecommandlist(Type endpoint, string namespaceworker, string aboutnamespace)
        {
            string content = "<br/><a href=\"index.html\"><- Back to command sections</a><br/> " +
                "<h4>"+namespaceworker.FirstCharToUpper()+"</h4><p>"+ aboutnamespace + "</p>"
                +"<hr/><div class=\"table-responsive\"><table class=\"table table-hover table-bordered table-striped\">";
            content = content + "<thead><tr><th>Command</th>";
            content = content + "<th>About</th></tr></thead><tbody>";
            foreach (MethodInfo M in endpoint.GetMethods())
            {
                bool isCallable = false;
                string about = "";
                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (At.AttributeType.Name == "About")
                    {
                        about = M.GetAttribute<About>().about;
                        isCallable = true;
                        break;
                    }
                }
                if(isCallable == false)
                {
                    continue;
                }
                string url = "command" + M.Name.ToLower() + ".html";
                content = content + "<tr>";
                content = content + "<td><a href=\""+ url+"\">" + M.Name + "</a></td>";
                content = content + "<td>" + about + "</td></tr>";
                makecommandfile(namespaceworker, M, about);
            }
            content = content + "</tbody></table></div>";
            makefile(namespaceworker, content, "Command list for "+ namespaceworker.FirstCharToUpper());
        }

        protected void makecommandfile(string namespaceworker, MethodInfo method, string methodabout)
        {
            string content = "<br/><a href=\"" + namespaceworker + ".html\"><- Back to "+namespaceworker.FirstCharToUpper()+"</a><br/> " +
                "<h4>" + method.Name + "</h4></a><p>" + methodabout + "</p>"
                + "<hr/>";

            
            Dictionary<string,string> hints = [];
            Dictionary<string, string> hinttypes = [];
            Dictionary<string, string> hintexamplevalue = [];

            string examplecall = method.Name;
            foreach (ArgHints At in method.GetCustomAttributes<ArgHints>())
            {
                hints.Add(At.name, At.about);
                hinttypes.Add(At.name, At.defaultValueType);
                hintexamplevalue.Add(At.name, At.exampleValue);
            }

            if (method.GetParameters().Length > 0)
            {
                content = content + "<h5>Args</h5><div class=\"table-responsive\"><table class=\"table table-hover table-bordered table-striped\">";
                content = content + "<thead><tr><th>Name</th>";
                content = content + "<th>Hint</th><th>Type</th><th>Example</th></tr></thead><tbody>";
                bool hadStartSplit = false;
                string addon = "";
                foreach (ParameterInfo pram in method.GetParameters())
                {
                    if(hadStartSplit == false)
                    {
                        hadStartSplit = true;
                        examplecall = examplecall + "###";
                    }
                    content = content + "<tr>";
                    content = content + "<td>" + pram.Name + "</a></td>";
                    string hinttext = "";
                    string hinttype = "";
                    string hintexample = "";
                    string hintexamplecmd = "";
                    if (hintexamplevalue.ContainsKey(pram.Name) == true)
                    {
                        if (hintexamplevalue[pram.Name] != null)
                        {
                            hintexample = hintexamplevalue[pram.Name];
                            hintexamplecmd = hintexamplevalue[pram.Name];
                        }
                    }
                    if (hinttypes.ContainsKey(pram.Name) == true)
                    {
                        hinttype = hinttypes[pram.Name];
                        if (hinttype == "UUID")
                        {
                            hintexample = UUID.Random().ToString();
                            hintexamplecmd = hintexample;
                        }
                        else if (hinttype == "AVATAR")
                        {
                            hintexample = "Firstname Lastname or UUID";
                            hintexamplecmd = "Madpeter Zond";
                        }
                        else if(hinttype == "BOOL")
                        {
                            hintexample = "false";
                            hintexamplecmd = "true";
                        }
                        else if(hinttype == "SMART")
                        {
                            hintexample = "HTTP URL, Avatar UUID or chat chanel";
                            hintexamplecmd = "1234";
                        }
                    }
                    if (hints.ContainsKey(pram.Name) == true)
                    {
                        if (hints[pram.Name] != null)
                        {
                            hinttext = hints[pram.Name];
                        }
                    }
                    examplecall = examplecall + addon + "" + hintexamplecmd;
                    addon = "~#~";
                    content = content + "" +
                        "<td>" + hinttext + "</td>" +
                        "<td>"+ hinttype+"</td>" +
                        "<td>"+ hintexample +"</td>" +
                        "</tr>";
                }
                content = content + "</tbody></table></div>";
            }

            content = content + "<h5>Return hints</h5><ul class=\"list-group\">";
            foreach (ReturnHints At in method.GetCustomAttributes<ReturnHints>())
            {
                content = content + "<li class=\"list-group-item text-success\">☑ "+At.hint+"</li>";
            }
            foreach (ReturnHintsFailure At in method.GetCustomAttributes<ReturnHintsFailure>())
            {
                content = content + "<li class=\"list-group-item text-danger\">❌ "+ At.hint+"</li>";
            }
            content = content + "</ul><br/><h5>Example call</h5><br/>"+examplecall+"";
            makefile("command"+method.Name.ToLower(), content, "Command info for " + method.Name);
        }

        protected void makefile(string file, string content, string title)
        {
            string pagecontent = htmltemplate;
            pagecontent = pagecontent.Replace("[[PAGETITLE]]", "Secondbot [Events] command wiki / "+title);
            pagecontent = pagecontent.Replace("[[BUILDVERSION]]", version);
            pagecontent = pagecontent.Replace("[[BUILDDATE]]", "Wiki created:"+DateTime.Now.ToString("dd/MM/yyyy"));
            pagecontent = pagecontent.Replace("[[CONTENT]]", content);
            IO.WriteFile(file+".html", pagecontent);
        }
    }
}
