using BetterSecondBot.HttpService;
using BetterSecondBotShared.API;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using EmbedIO;
using System;
using System.Collections.Generic;
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
        protected Dictionary<string, string> seen_command_names = new Dictionary<string, string>();


        public string CreateRoot()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(html_header);
            sb.Append("<h3>Build: ");
            sb.Append(buildVersion);
            sb.Append("</h3><br/>");
            sb.Append("<table class='table table-striped table-bordered'><thead><tr><th>Interface</th><th>Commands</th><th>Status</th></tr></thead><tbody>");
            foreach (KeyValuePair<string, KeyValuePair<int, string>> entry in api_reports)
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
            sb = DebugModeCreateWiki.MenuActive(sb, "Home");
            sb.Append(html_footer);
            return sb.ToString();
        }

        protected static StringBuilder MenuActive(StringBuilder index, string area)
        {
            Dictionary<string, string> active_swaps = new Dictionary<string, string>
            {
                { "CoreActive", "" },
                { "CLIActive", "" },
                { "JSONActive", "" },
                { "RLVapiActive", "" },
                { "HTTPActive", "" }
            };
            active_swaps["" + area + "Active"] = "Active";
            foreach (KeyValuePair<string, string> pair in active_swaps)
            {
                index = index.Replace("[[" + pair.Key + "]]", pair.Value);
            }
            return index;
        }

        protected void InterfaceCommands(string area, API_supported_interface shared_interface, bool track_commands = false)
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
                        LogFormater.Debug("command " + c + " from " + interface_name + " overlaps an ready loaded command from " + seen_command_names[c] + "");
                    }
                }
                string workspace = shared_interface.GetCommandWorkspace(c);
                StringBuilder sb = new StringBuilder();
                sb.Append(html_header);
                sb.Append("<h3>Build: ");
                sb.Append(buildVersion);
                sb.Append("</h3><br/>");
                sb.Append("<h4>Interface: <a href='[[AREA]].html'>[[AREA]]</a>");
                if (workspace != "")
                {
                    sb.Append(" / <a href='[[AREA]][[WORKSPACE]].html'>[[WORKSPACE]]</a>");
                }
                sb.Append("</h4><hr/><h3>[[COMMAND]]</h3>");
                int loop = 0;
                int minargs = shared_interface.GetCommandArgs(c);
                string[] arg_types = shared_interface.GetCommandArgTypes(c);
                string[] arg_hints = shared_interface.GetCommandArgHints(c);
                if (area == "Core")
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
                            LogFormater.Debug("[WikiMake] " + area + " Command " + c + " missing some required hint values");
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
                sb = DebugModeCreateWiki.MenuActive(sb, area);
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
                foreach (string c in cmds)
                {
                    if (shared_interface.GetCommandWorkspace(c) == workspace)
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
                sb = DebugModeCreateWiki.MenuActive(sb, area);
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
            sb = DebugModeCreateWiki.MenuActive(sb, area);
            io.writefile("" + area + ".html", sb.ToString());
        }
        protected void InterfaceWiki(string area, API_supported_interface shared_interface, bool track_commands = false)
        {
            LogFormater.Info("[WIKI] Starting area " + area + "");
            // create index 
            InterfaceIndex(area, shared_interface);

            // create workspaces
            InterfaceWorkspaces(area, shared_interface);

            // create commands
            InterfaceCommands(area, shared_interface, track_commands);
            LogFormater.Info("[WIKI] Done with area");
        }

        public DebugModeCreateWiki(string Version, SimpleIO shareio)
        {
            io = shareio;
            buildVersion = Version;
            html_header = helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "wiki_header.txt");
            html_footer = helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "wiki_footer.txt");
            io.ChangeRoot("wiki");
            io.ChangeRoot("wiki/files");

            // JSONcfg
            MakeJsonConfig defaultJson = new MakeJsonConfig();
            api_reports.Add("JSON", new KeyValuePair<int, string>(defaultJson.ApiCommandsCount, "Ready"));
            InterfaceWiki("JSON", defaultJson);

            // CMD 
            BSB.Commands.CoreCommandsInterface cmd = new BSB.Commands.CoreCommandsInterface(null,true);
            api_reports.Add("Core", new KeyValuePair<int, string>(cmd.ApiCommandsCount, "Good"));
            InterfaceWiki("Core", cmd, true);

            // RLVapi
            BSB.RLV.RLVcontrol RLVapi = new BSB.RLV.RLVcontrol(null,true);
            api_reports.Add("RLVapi", new KeyValuePair<int, string>(RLVapi.ApiCommandsCount, "Limited"));
            InterfaceWiki("RLVapi", RLVapi, true);

            // HTTP
            HTTPWiki();

            io.ChangeRoot("wiki");
            io.writefile("index.html", CreateRoot());
        }

        protected void HTTPWiki()
        {
            LogFormater.Info("[WIKI] Starting area HTTP endpoint");
            HTTPendpoint HTTP = new HTTPendpoint();

            HTTPmenu("HTTP", HTTP);

            // create workspaces
            HTTPWorkspaces("HTTP", HTTP);

            // create commands
            HTTPCommands("HTTP", HTTP);

            HTTP = null;
            LogFormater.Info("[WIKI] Done with area");
        }

        protected string getURLargs(Dictionary<string, KeyValuePair<string, string>> values)
        {
            List<string> urlargs = new List<string>();
            foreach (KeyValuePair<string, KeyValuePair<string, string>> entry in values)
            {
                if (entry.Value.Key == "URLARG")
                {
                    urlargs.Add("{"+entry.Key+"}");
                }
            }
            return String.Join('/', urlargs);
        }

        protected void HTTPCommands(string area, HTTPendpoint HTTP)
        {
            foreach (string workspace in HTTP.getEndPoints())
            {

                foreach (string c in HTTP.getEndpointCommands(workspace))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(html_header);
                    sb.Append("<h3>Build: ");
                    sb.Append(buildVersion);
                    sb.Append("</h3><br/>");
                    sb.Append("<h4>Interface: <a href='[[AREA]].html'>[[AREA]]</a>");
                    if (workspace != "")
                    {
                        sb.Append(" / <a href='[[AREA]][[WORKSPACE]].html'>[[WORKSPACE]]</a>");
                    }
                    sb.Append("</h4><hr/><h3>[[COMMAND]]</h3>");
                    sb.Append("[[URLENDPOINT]]/[[WORKSPACE]]/[[COMMAND]]/[[URLADDON]]<br/>");
                    sb.Append("Method: [[APIMETHOD]]");
                    sb.Append("<hr style='border-top: 1px dashed #dcdcdc;'>");
                    sb.Append("[[HELP]]");
                    Dictionary<string, KeyValuePair<string, string>> values = HTTP.getCommandArgs(workspace, c);
                    int ValueCount = values.Count;

                    if (ValueCount > 0)
                    {
                        sb.Append("<hr/><h4>Args helper</h4>");
                        sb.Append("<table class='table table-striped table-bordered'><thead><tr><td>Name</td><th>Type</th><th>Hint</th></tr></thead><tbody>");
                        foreach (KeyValuePair<string, KeyValuePair<string, string>> entry in values)
                        {
                            string hint = entry.Value.Value;
                            string type = entry.Value.Key;
                            if (entry.Value.Key.Contains("Optional") == true)
                            {
                                type = type.Replace("Optional", "{Optional} ");
                            }
                            else if (entry.Value.Key == "URLARG")
                            {
                                type = "URL arg";
                            }
                            sb.Append("<tr><td>" + entry.Key + "</td><td>" + type + "</td><td>" + hint + "</td></tr>");
                        }
                        sb.Append("</tbody></table>");
                    }

                    string[] returnvalues = HTTP.getReturnsValues(workspace, c);
                    if (returnvalues.Length > 0)
                    {
                        sb.Append("<hr/><h4>Possible replys</h4>");
                        sb.Append("<ul>");
                        foreach (string entry in returnvalues)
                        {
                            sb.Append("<li>"+entry+"</li>");
                        }
                        sb.Append("</ul>");
                    }



                    sb = DebugModeCreateWiki.MenuActive(sb, area);
                    sb.Append(html_footer);
                    sb.Replace("[[COMMAND]]", c);
                    sb.Replace("[[URLENDPOINT]]", "http://localhost:8080");
                    sb.Replace("[[APIMETHOD]]", HTTP.getCommandMethod(workspace, c));
                    sb.Replace("[[URLADDON]]", getURLargs(values));
                    sb.Replace("[[HELP]]", HTTP.getCommandAbout(workspace, c));
                    sb.Replace("[[MINARGS]]", ValueCount.ToString());
                    sb.Replace("[[WORKSPACE]]", workspace);
                    sb.Replace("[[AREA]]", area);
                    sb.Replace("[[SUBFOLDER]]", "");
                    sb.Replace("[[RETURNROOT]]", "../");
                    string target_file = "" + area + "" + workspace + "" + c + ".html";
                    io.writefile(target_file, sb.ToString());
                }
            }
        }

        protected void HTTPmenu(string area, HTTPendpoint HTTP)
        {
            api_reports.Add(area, new KeyValuePair<int, string>(HTTP.getFullCount(), "Basic"));

            // create index 
            StringBuilder sb = new StringBuilder();
            sb.Append(html_header);
            sb.Append("<h3>Interface:");
            sb.Append("HTTP");
            sb.Append("</h3><br/>");
            sb.Append("<h4>Build: ");
            sb.Append(buildVersion);
            sb.Append("</h4><br/>");
            sb.Append("<table class='datatable table table-striped table-bordered'><thead><tr><th>Workspace</th><th>Commands</th></tr></thead><tbody>");
            foreach (string workspace in HTTP.getEndPoints())
            {
                string workspace_link = "<a href='[[AREA]]" + workspace + ".html'>" + workspace + "</a>";
                sb.Append("<tr><td>");
                sb.Append(workspace_link);
                sb.Append("</td><td>");
                sb.Append(HTTP.getEndpointCount(workspace).ToString());
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Replace("[[SUBFOLDER]]", "");
            sb.Replace("[[RETURNROOT]]", "../");
            sb.Replace("[[AREA]]", area);
            sb.Append(html_footer);
            sb = DebugModeCreateWiki.MenuActive(sb, area);
            io.writefile("" + area + ".html", sb.ToString());
        }

        protected void HTTPWorkspaces(string area, HTTPendpoint HTTP)
        {
            foreach (string workspace in HTTP.getEndPoints())
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
                foreach (string c in HTTP.getEndpointCommands(workspace))
                {
                    int args = HTTP.getCommandArgCount(workspace, c);
                    sb.Append("<tr><td><a href='[[AREA]]");
                    sb.Append(workspace);
                    sb.Append(c);
                    sb.Append(".html'>");
                    sb.Append(c);
                    sb.Append("</a></td><td>");
                    sb.Append(args.ToString());
                    sb.Append("</td></tr>");
                }
                sb.Append("</tbody></table>");
                sb.Replace("[[SUBFOLDER]]", "");
                sb.Replace("[[RETURNROOT]]", "../");
                sb.Replace("[[WORKSPACE]]", workspace);
                sb.Replace("[[AREA]]", area);
                sb.Append(html_footer);
                sb = DebugModeCreateWiki.MenuActive(sb, area);
                string target_file = "" + area + "" + workspace + ".html";
                io.writefile(target_file, sb.ToString());
            }
        }
    }

    public class HTTPendpoint
    {
        protected List<Endpoint> endpoints = new List<Endpoint>();


        public void createEndpoint(string name, Type Api)
        {
            Endpoint Endpoint = new Endpoint();
            Endpoint.name = name;
            foreach (MethodInfo M in Api.GetMethods())
            {
                APIcall C = new APIcall();
                CustomAttributeData httpverb = null;
                CustomAttributeData NeedsToken = null;
                CustomAttributeData About = null;
                List<CustomAttributeData> ArgHints = new List<CustomAttributeData>();
                List<CustomAttributeData> ReturnValues = new List<CustomAttributeData>();

                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (At.AttributeType.Name == "RouteAttribute")
                    {
                        httpverb = At;
                    }
                    else if (At.AttributeType.Name == "NeedsToken")
                    {
                        NeedsToken = At;
                    }
                    else if (At.AttributeType.Name == "About")
                    {
                        About = At;
                    }
                    else if (At.AttributeType.Name == "ArgHints")
                    {
                        ArgHints.Add(At);
                    }
                    else if (At.AttributeType.Name == "ReturnHints")
                    {
                        ReturnValues.Add(At);
                    }
                }
                if (httpverb != null)
                {
                    C.name = M.Name.ToLowerInvariant();
                    C.type = "Post";
                    C.about = "Http interface command about missing";
                    if (About != null)
                    {
                        C.about = About.ConstructorArguments[0].Value.ToString();
                    }
                    if ((int)httpverb.ConstructorArguments[0].Value == 2)
                    {
                        C.type = "Get";
                    }
                    C.RequiresToken = true;
                    if (NeedsToken != null)
                    {
                        C.RequiresToken = (bool)NeedsToken.ConstructorArguments[0].Value;
                    }
                    foreach (CustomAttributeData cad in ArgHints)
                    {
                        C.values.Add(cad.ConstructorArguments[0].Value.ToString(), new KeyValuePair<string, string>(cad.ConstructorArguments[1].Value.ToString(), cad.ConstructorArguments[2].Value.ToString()));
                    }
                    foreach (CustomAttributeData cad in ReturnValues)
                    {
                        C.returns.Add(cad.ConstructorArguments[0].Value.ToString());
                    }
                    C.Setup();
                    Endpoint.callable.Add(C);
                }
            }
            endpoints.Add(Endpoint);
        }
        public HTTPendpoint()
        {
            createEndpoint("core", typeof(HttpApiCore));
            createEndpoint("inventory", typeof(HttpApiInventory));
            createEndpoint("im", typeof(HttpApiIM));
            createEndpoint("groups", typeof(HttpApiGroup));
            createEndpoint("chat", typeof(HttpApiLocalchat));
            createEndpoint("parcelestate", typeof(HttpApiParcelEstate));
        }

        public string getCommandMethod(string endpoint, string command)
        {
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpoint)
                {
                    foreach (APIcall api in End.callable)
                    {
                        if (api.name == command)
                        {
                            return api.type;
                        }
                    }
                }
            }
            return "?";
        }

        public string getCommandAbout(string endpoint, string command)
        {
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpoint)
                {
                    foreach (APIcall api in End.callable)
                    {
                        if (api.name == command)
                        {
                            return api.about;
                        }
                    }
                }
            }
            return "?";
        }

        public string[] getReturnsValues(string endpoint, string command)
        {
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpoint)
                {
                    foreach (APIcall api in End.callable)
                    {
                        if (api.name == command)
                        {
                            return api.returns.ToArray();
                        }
                    }
                }
            }
            return new string[] { };
        }



        public int getCommandArgCount(string endpoint, string command)
        {
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpoint)
                {
                    foreach (APIcall api in End.callable)
                    {
                        if (api.name == command)
                        {
                            return api.values.Count;
                        }
                    }
                }
            }
            return 0;
        }

        public string[] getEndpointCommands(string endpointname)
        {
            List<string> commands = new List<string>();
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpointname)
                {
                    foreach (APIcall api in End.callable)
                    {
                        commands.Add(api.name);
                    }
                }
            }
            return commands.ToArray();
        }

        public string[] getEndPoints()
        {
            List<string> names = new List<string>();
            foreach (Endpoint End in endpoints)
            {
                names.Add(End.name);
            }
            return names.ToArray();
        }

        public int getEndpointCount(string endpointname)
        {
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpointname)
                {
                    return End.callable.Count;
                }
            }
            return 0;
        }

        public int getFullCount()
        {
            int count = 0;
            foreach (Endpoint End in endpoints)
            {
                count += End.callable.Count;
            }
            return count;
        }

        public Dictionary<string, KeyValuePair<string, string>> getCommandArgs(string endpoint, string command)
        {
            Dictionary<string, KeyValuePair<string, string>> empty = new Dictionary<string, KeyValuePair<string, string>>();
            foreach (Endpoint End in endpoints)
            {
                if (End.name == endpoint)
                {
                    foreach (APIcall api in End.callable)
                    {
                        if (api.name == command)
                        {
                            return api.values;
                        }
                    }
                }
            }
            return empty;

        }

    }

    public class Endpoint
    {
        public string name = "Notset";
        public List<APIcall> callable = new List<APIcall>();
    }

    public class APIcall
    {
        public string name { get; set; }
        public string type = "get";
        public string about = "A API call that is missing its about value";
        public Dictionary<string, KeyValuePair<string, string>> values = new Dictionary<string, KeyValuePair<string, string>>();
        public List<string> returns = new List<string>();
        public bool RequiresToken = true;

        public virtual void Setup()
        {
            if (RequiresToken == true)
            {
                returns.Add("Token not accepted");
                values.Add(
                    "token",
                    new KeyValuePair<string, string>(
                        "URLARG",
                        "the api access token")
                );
            }
        }
    }
}
