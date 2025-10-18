using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using RestSharp.Extensions;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;
using static StackExchange.Redis.Role;

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
            makeJsonConfigHelpers();
        }

        protected void makeJsonConfigHelpers()
        {
            var configNamespace = "SecondBotEvents.Config";
            var assembly = typeof(SecondBotEvents.Config.Config).Assembly; // Or Assembly.GetExecutingAssembly() if appropriate
            var excludeNames = new HashSet<string> { "SecondBotEvents.Config.ConfigDescriptor", "SecondBotEvents.Config.Config" };
            var configClasses = assembly.GetTypes()
                .Where(t => t.IsClass
                    && t.Namespace == configNamespace
                    && !excludeNames.Contains(t.FullName))
                .ToList();
            IO.ChangeRoot("json");
            foreach (var type in configClasses)
            {
                try
                {
                    var worker = Activator.CreateInstance(type, new object[] { false, "" }) as SecondBotEvents.Config.Config;
                    if (worker == null)
                    {
                        continue;
                    }
                    var loaded = worker.DescribeConfig();
                    if (loaded == null)
                    {
                        continue;
                    }
                    string info = JsonSerializer.Serialize(loaded, JsonOptions.UnsafeRelaxed);
                    string name = type.FullName.Replace("SecondBotEvents.Config.", "");
                    IO.WriteFile(name + ".json", info);
                }
                catch (Exception ex)
                {
                    LogFormater.Warn("Unable to create json config helper for " + type.FullName + ": " + ex.Message);
                }
            }
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

        protected string makeCommandList(string namespaceworker, List<MethodInfo> commands)
        {
            commands = commands.OrderBy(x => x.Name).ToList();
            string reply = "<div class=\"list-group\">";
            foreach (MethodInfo M in commands)
            {
                string about = "";
                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (At.AttributeType.Name == "About")
                    {
                        about = M.GetAttribute<About>().about;
                        break;
                    }
                }
                if(about == "")
                {
                    about = "No description";
                }
                if(about.Contains("<br>") == true)
                {
                    about = about.Split("<br>")[0];
                }
                if (about.Contains("\n") == true)
                {
                    about = about.Split("\n")[0];
                }
                string url = "command" + M.Name.ToLower() + ".html";
                reply = reply + "<a href=\""+ url+"\" class=\"list-group-item list-group-item-action\" aria-current=\"false\">";
                reply = reply + "<div class=\"d-flex w-100 justify-content-between\">";
                reply = reply + "<span class=\"text-primary fw-bold text-decoration-underline\">" + M.Name+"</span>";
                reply = reply + "</div>";
                reply = reply + "<p class=\"mb-1\"><small>" + about + "</small></p>";
                reply = reply + "</a>";
                makecommandfile(namespaceworker, M, about);
            }
            reply = reply + "</div>";
            return reply;
        }
        protected void makecommandlist(Type endpoint, string namespaceworker, string aboutnamespace)
        {
            List<MethodInfo> getCommands = [];
            List<MethodInfo> setCommands = [];
            List<MethodInfo> doCommands = [];
            foreach (MethodInfo M in endpoint.GetMethods())
            {
                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (typeof(CmdType).IsAssignableFrom(At.AttributeType))
                    {
                        if (At.AttributeType.Name == "CmdTypeGet")
                        {
                            getCommands.Add(M);
                        }
                        else if (At.AttributeType.Name == "CmdTypeSet")
                        {
                            setCommands.Add(M);
                        }
                        else if (At.AttributeType.Name == "CmdTypeDo")
                        {
                            doCommands.Add(M);
                        }
                        break;
                    }
                }
            }
            string content = "";
            if (((getCommands.Count > 0) || (setCommands.Count > 0)) && (doCommands.Count == 0))
            {
                // get set only (no do commands)
                content = "<br/><a href=\"index.html\"><- Back to command sections</a><br/> " +
                    "<h4>" + namespaceworker.FirstCharToUpper() + "</h4><p>" + aboutnamespace + "</p>"
                    + "<hr/>" +
                    "<div class=\"table-responsive\"><table class=\"table table-bordered table-striped\">" +
                    "<thead><tr>" +
                    "<th>Get (" + getCommands.Count.ToString() + ") / Set (" + setCommands.Count.ToString() + ")</th>" +
                    "</tr></thead>";

                content = content + "<tbody><tr>" +
                    "<td>" + makeCommandList(namespaceworker, getCommands) + "" + makeCommandList(namespaceworker, setCommands) + "</td>" +
                    "</tbody></table></div>";
            }
            else if (((getCommands.Count == 0) && (setCommands.Count == 0)) && (doCommands.Count > 0))
            {
                // Do only (no get/set commands)
                content = "<br/><a href=\"index.html\"><- Back to command sections</a><br/> " +
                    "<h4>" + namespaceworker.FirstCharToUpper() + "</h4><p>" + aboutnamespace + "</p>"
                    + "<hr/>" +
                    "<div class=\"table-responsive\"><table class=\"table table-bordered table-striped\">" +
                    "<thead><tr>" +
                    "<th>Do (" + doCommands.Count.ToString() + ")</th>" +
                    "</tr></thead>";

                content = content + "<tbody><tr>" +
                    "<td>" + makeCommandList(namespaceworker, doCommands) + "</td>" +
                    "</tbody></table></div>";
            }
            else if ((getCommands.Count > 4) && (setCommands.Count > 4))
            {
                // mix of both
                content = "<br/><a href=\"index.html\"><- Back to command sections</a><br/> " +
                "<h4>" + namespaceworker.FirstCharToUpper() + "</h4><p>" + aboutnamespace + "</p>"
                + "<hr/>" +
                "<div class=\"table-responsive\"><table class=\"table table-bordered table-striped\">" +
                "<thead><tr>" +
                "<th>Get (" + getCommands.Count.ToString() + ")</th>" +
                "<th>Set (" + setCommands.Count.ToString() + ")</th>" +
                "<th>Do (" + doCommands.Count.ToString() + ")</th>" +
                "</tr></thead>";

                content = content + "<tbody><tr>" +
                    "<td>" + makeCommandList(namespaceworker, getCommands) + "</td>" +
                    "<td>" + makeCommandList(namespaceworker, setCommands) + "</td>" +
                    "<td>" + makeCommandList(namespaceworker, doCommands) + "</td></tr>" +
                    "</tbody></table></div>";
            }
            else
            {     
                // mix of both
                content = "<br/><a href=\"index.html\"><- Back to command sections</a><br/> " +
                "<h4>" + namespaceworker.FirstCharToUpper() + "</h4><p>" + aboutnamespace + "</p>"
                + "<hr/>" +
                "<div class=\"table-responsive\"><table class=\"table table-bordered table-striped\">" +
                "<thead><tr>" +
                "<th>Get (" + getCommands.Count.ToString() + ") / Set (" + setCommands.Count.ToString() + ")</th>" +
                "<th>Do (" + doCommands.Count.ToString() + ")</th>" +
                "</tr></thead>";

                content = content + "<tbody><tr>" +
                    "<td>" + makeCommandList(namespaceworker, getCommands) + "" + makeCommandList(namespaceworker, setCommands) + "</td>" +
                    "<td>" + makeCommandList(namespaceworker, doCommands) + "</td></tr>" +
                    "</tbody></table></div>";
            }
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
            Dictionary<string, string[]> hintacceptvalues = [];

            string examplecall = method.Name;
            foreach (ArgHints At in method.GetCustomAttributes<ArgHints>())
            {
                hints.Add(At.name, At.about);
                hinttypes.Add(At.name, At.defaultValueType);
                hintexamplevalue.Add(At.name, At.exampleValue);
                hintacceptvalues.Add(At.name, At.acceptedValues);
            }

            if (method.GetParameters().Length > 0)
            {
                content = content + "<h5>Args</h5><div class=\"table-responsive\"><table class=\"table table-hover table-bordered table-striped\">";
                content = content + "<thead><tr><th>Name</th>";
                content = content + "<th>Hint</th><th>Type</th><th>Example value</th><th>Supported values</th></tr></thead><tbody>";
                bool hadStartSplit = false;
                string addon = "";
                foreach (ParameterInfo pram in method.GetParameters())
                {
                    if(hadStartSplit == false)
                    {
                        hadStartSplit = true;
                        examplecall = examplecall + "|||";
                    }
                    content = content + "<tr>";
                    content = content + "<td>" + pram.Name + "</a></td>";
                    string hinttext = "";
                    string hinttype = "";
                    string hintexample = "";
                    string hintexamplecmd = "";
                    string hinttextsuggested = "";
                    if(hintacceptvalues.ContainsKey(pram.Name) == true)
                    {
                        if (hintacceptvalues[pram.Name] != null)
                        {
                            hinttextsuggested = "";
                            string addoncsv = "";
                            int loop = 0;
                            foreach(string a in hintacceptvalues[pram.Name])
                            {
                                if (loop == 3)
                                {
                                    hinttextsuggested = hinttextsuggested + "<br/>";
                                    addoncsv = "";
                                    loop = 0;
                                }
                                hinttextsuggested = hinttextsuggested + addoncsv;
                                hinttextsuggested = hinttextsuggested + a;
                                addoncsv = ", ";
                                loop++;
                            }
                        }
                    }
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
                        "<td>" + hinttextsuggested + "</td>" +
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
            content = content + "</ul><br/><h5>Example call</h5><br/>"+examplecall+"<br/><br/>";
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
