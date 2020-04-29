using BetterSecondBotShared.API;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BSB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterSecondBot.WikiMake
{
    class DebugModeCreateWiki
    {
        protected Dictionary<string, KeyValuePair<int, string>> api_reports = new Dictionary<string, KeyValuePair<int, string>>();
        protected string html_header = "";
        protected string html_footer = "";
        protected string buildVersion = "";
        protected SimpleIO io;
        protected Dictionary<string,string> seen_command_names = new Dictionary<string, string>();

        public static string ReadResourceFile(string filename)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
                string result = "";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string CreateRoot()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(html_header);
            sb.Append("<h3>Build: ");
            sb.Append(buildVersion);
            sb.Append("</h3><br/>");
            sb.Append("<table class='table table-striped table-bordered'><thead><tr><th>Interface</th><th>Commands</th><th>Status</th></tr></thead><tbody>");
            foreach (KeyValuePair<string,KeyValuePair<int, string>> entry in api_reports)
            {
                sb.Append("<tr>");
                sb.Append("<td><a href='[[SUBFOLDER]]");
                sb.Append(entry.Key);
                sb.Append(".html'>");
                sb.Append(entry.Key);
                sb.Append("</a></td>");
                sb.Append("<td>");
                sb.Append(entry.Value.Key.ToString());
                sb.Append("</td><td>");
                sb.Append(entry.Value.Value);
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Replace("[[AREA]]", "Home");
            sb.Replace("[[SUBFOLDER]]", "files/");
            sb.Replace("[[RETURNROOT]]", "");
            sb = MenuActive(sb, "Home");
            sb.Append(html_footer);
            return sb.ToString();
        }

        protected StringBuilder MenuActive(StringBuilder index,string area)
        {
            Dictionary<string, string> active_swaps = new Dictionary<string, string>
            {
                { "CoreActive", "" },
                { "CLIActive", "" },
                { "JSONActive", "" },
                { "RLVapiActive", "" },
                { "HTTPgetActive", "" },
                { "HTTPpostActive", "" }
            };
            active_swaps["" + area + "Active"] = "Active";
            foreach(KeyValuePair<string,string> pair in active_swaps)
            {
                index = index.Replace("[["+ pair.Key+"]]", pair.Value);
            }
            return index;
        }

        protected void InterfaceCommands(string area, API_supported_interface shared_interface,bool track_commands=false)
        {
            string[] cmds = shared_interface.GetCommandsList();
            string interface_name = shared_interface.GetType().Name;
            foreach (string c in cmds)
            {
                if (track_commands == true)
                {
                    if (seen_command_names.ContainsKey(c) == false)
                    {
                        seen_command_names.Add(c, interface_name);
                    }
                    else
                    {
                        ConsoleLog.Debug("command " + c + " from " + interface_name + " overlaps an ready loaded command from "+ seen_command_names[c]+"");
                    }
                }
                string workspace = shared_interface.GetCommandWorkspace(c);
                StringBuilder sb = new StringBuilder();
                sb.Append(html_header);
                sb.Append("<h3>Build: ");
                sb.Append(buildVersion);
                sb.Append("</h3><br/>");
                sb.Append("<h4>Interface: <a href='[[AREA]].html'>[[AREA]]</a>");
                if(workspace != "")
                {
                    sb.Append(" / <a href='[[AREA]][[WORKSPACE]].html'>[[WORKSPACE]]</a>");
                }
                sb.Append("</h4><hr/><h3>[[COMMAND]]</h3>");
                int loop = 0;
                int minargs = shared_interface.GetCommandArgs(c);
                string[] arg_types = shared_interface.GetCommandArgTypes(c);
                string[] arg_hints = shared_interface.GetCommandArgHints(c);
                if(area == "Core")
                {
                    StringBuilder ExampleCall = new StringBuilder();
                    ExampleCall.Append("Example: [[COMMAND]]");
                    ExampleCall.Append("|||");
                    string addon = "";
                    while (loop < minargs)
                    {
                        ExampleCall.Append(addon);
                        addon = "~#~";
                        string hint_value = "";
                        if (arg_hints.Length > loop)
                        {
                            if (arg_hints[loop] != null)
                            {
                                string[] bits = arg_hints[loop].Split("<br/>", StringSplitOptions.RemoveEmptyEntries);
                                if (bits.Length >= 1)
                                {
                                    hint_value = bits[0];
                                }
                            }
                        }
                        if (hint_value != "")
                        {
                            ExampleCall.Append(hint_value);
                        }
                        else
                        {
                            ExampleCall.Append("?");
                            ConsoleLog.Debug("[WikiMake] " + area + " Command " + c + " missing some required hint values");
                        }
                        loop++;
                    }
                    sb.Append(ExampleCall.ToString());
                    sb.Append("<hr style='border-top: 1px dashed #dcdcdc;'>");
                }
                sb.Append("[[HELP]]");
                if (arg_types.Length > 0)
                {
                    sb.Append("<hr/><h4>Args helper</h4>");
                    loop = 0;

                    sb.Append("<table class='table table-striped table-bordered'><thead><tr><td>Num</td><th>Type</th><th>Required</th><th>Hint</th></tr></thead><tbody>");
                    while (loop < arg_types.Length)
                    {
                        string hint = "";
                        if (arg_hints.Length > loop)
                        {
                            hint = arg_hints[loop];
                        }
                        string Required = "X";
                        if ((loop + 1) > minargs)
                        {
                            Required = "";
                        }
                        sb.Append("<tr><td>" + (loop + 1).ToString() + "</td><td>" + arg_types[loop] + "</td><td>" + Required + "</td><td>" + hint + "</td></tr>");
                        loop++;
                    }
                    sb.Append("</tbody></table>");
                }
                sb.Append(html_footer);

                sb.Replace("[[COMMAND]]", c);
                sb.Replace("[[HELP]]", shared_interface.GetCommandHelp(c));
                sb.Replace("[[MINARGS]]", minargs.ToString());
                sb.Replace("[[WORKSPACE]]", workspace);
                sb.Replace("[[AREA]]", area);
                sb.Replace("[[SUBFOLDER]]", "");
                sb.Replace("[[RETURNROOT]]", "../");
                sb = MenuActive(sb, area);
                string target_file = "" + area + "" + workspace + "" + c + ".html";
                io.writefile(target_file, sb.ToString());
            }
        }
        protected void InterfaceWorkspaces(string area, API_supported_interface shared_interface)
        {
            string[] cmds = shared_interface.GetCommandsList();
            foreach (string workspace in shared_interface.GetAllWorkspaces())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(html_header);
                sb.Append("<h3>Interface: <a href='[[AREA]].html'>[[AREA]]</a>");
                if (workspace != "")
                {
                    sb.Append(" / [[WORKSPACE]]");
                }
                sb.Append("</h3><br/>");
                sb.Append("<h4>Build: ");
                sb.Append(buildVersion);
                sb.Append("</h4><br/>");
                sb.Append("<table class='datatable table table-striped table-bordered'><thead><tr><th>Command</th><th>Min args</th></tr></thead><tbody>");
                foreach(string c in cmds)
                {
                    if(shared_interface.GetCommandWorkspace(c) == workspace)
                    {
                        int args = shared_interface.GetCommandArgs(c);
                        sb.Append("<tr><td><a href='[[AREA]]");
                        sb.Append(workspace);
                        sb.Append(c);
                        sb.Append(".html'>");
                        sb.Append(c);
                        sb.Append("</a></td><td>");
                        sb.Append(args.ToString());
                        sb.Append("</td></tr>");
                    }
                }
                sb.Append("</tbody></table>");
                sb.Replace("[[SUBFOLDER]]", "");
                sb.Replace("[[RETURNROOT]]", "../");
                sb.Replace("[[WORKSPACE]]", workspace);
                sb.Replace("[[AREA]]", area);
                sb.Append(html_footer);
                sb = MenuActive(sb, area);
                string target_file = "" + area + "" + workspace + ".html";
                io.writefile(target_file, sb.ToString());
            }
        }

        protected void InterfaceIndex(string area, API_supported_interface shared_interface)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(html_header);
            sb.Append("<h3>Interface:");
            sb.Append(area);
            sb.Append("</h3><br/>");
            sb.Append("<h4>Build: ");
            sb.Append(buildVersion);
            sb.Append("</h4><br/>");
            string[] cmds = shared_interface.GetCommandsList();
            sb.Append("<table class='datatable table table-striped table-bordered'><thead><tr><th>Workspace</th><th>Commands</th></tr></thead><tbody>");
            foreach (string workspace in shared_interface.GetAllWorkspaces())
            {
                int commands_count = 0;
                foreach (string c in cmds)
                {
                    if (shared_interface.GetCommandWorkspace(c) == workspace)
                    {
                        commands_count++;
                    }
                }

                string workspace_link = "<a href='[[AREA]]" + workspace + ".html'>" + workspace + "</a>";
                sb.Append("<tr><td>");
                sb.Append(workspace_link);
                sb.Append("</td><td>");
                sb.Append(commands_count.ToString());
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Replace("[[SUBFOLDER]]", "");
            sb.Replace("[[RETURNROOT]]", "../");
            sb.Replace("[[AREA]]", area);
            sb.Append(html_footer);
            sb = MenuActive(sb, area);
            io.writefile(""+ area+".html", sb.ToString());
        }
        protected void InterfaceWiki(string area,API_supported_interface shared_interface, bool track_commands = false)
        {
            ConsoleLog.Info("[WIKI] Starting area " + area + "");
            // create index 
            InterfaceIndex(area, shared_interface);

            // create workspaces
            InterfaceWorkspaces(area, shared_interface);

            // create commands
            InterfaceCommands(area, shared_interface, track_commands);
            ConsoleLog.Info("[WIKI] Done with area");
        }

        public DebugModeCreateWiki(string Version, SimpleIO shareio)
        {
            io = shareio;
            buildVersion = Version;
            html_header = ReadResourceFile("wiki_header.txt");
            html_footer = ReadResourceFile("wiki_footer.txt");
            io.ChangeRoot("wiki");
            io.ChangeRoot("wiki/files");

            // JSONcfg
            MakeJsonConfig defaultJson = new MakeJsonConfig();
            api_reports.Add("JSON", new KeyValuePair<int, string>(defaultJson.ApiCommandsCount, "Ready"));
            InterfaceWiki("JSON", defaultJson);

            // CMD 
            BSB.Commands.CoreCommandsInterface cmd = new BSB.Commands.CoreCommandsInterface(null);
            api_reports.Add("Core", new KeyValuePair<int, string>(cmd.ApiCommandsCount, "Basic"));
            InterfaceWiki("Core", cmd,true);

            // RLVapi
            BSB.RLV.RLVcontrol RLVapi = new BSB.RLV.RLVcontrol(null);
            api_reports.Add("RLVapi", new KeyValuePair<int, string>(RLVapi.ApiCommandsCount, "Basic"));
            InterfaceWiki("RLVapi", RLVapi, true);


            // HTTP get
            BetterSecondBot.HttpServer.HTTPCommandsInterfaceGet http_get = new BetterSecondBot.HttpServer.HTTPCommandsInterfaceGet(null,null);
            api_reports.Add("HTTPget", new KeyValuePair<int, string>(http_get.ApiCommandsCount, "Limited"));
            InterfaceWiki("HTTPget", http_get);

            // HTTP post
            BetterSecondBot.HttpServer.HTTPCommandsInterfacePost http_post = new BetterSecondBot.HttpServer.HTTPCommandsInterfacePost(null,null);
            api_reports.Add("HTTPpost", new KeyValuePair<int, string>(http_post.ApiCommandsCount, "Limited"));
            InterfaceWiki("HTTPpost", http_post);

            io.ChangeRoot("wiki");
            io.writefile("index.html", CreateRoot());
        }
    }
}
