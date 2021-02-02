using BetterSecondBotShared.API;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
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
            BSB.Commands.CoreCommandsInterface cmd = new BSB.Commands.CoreCommandsInterface(null);
            api_reports.Add("Core", new KeyValuePair<int, string>(cmd.ApiCommandsCount, "Good"));
            InterfaceWiki("Core", cmd, true);

            // RLVapi
            BSB.RLV.RLVcontrol RLVapi = new BSB.RLV.RLVcontrol(null);
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
        public HTTPendpoint()
        {
            Endpoint core = new Endpoint();
            core.name = "core";
            core.callable.Add(new gettoken());
            core.callable.Add(new logout());
            core.callable.Add(new version());
            core.callable.Add(new name());
            core.callable.Add(new command());
            core.callable.Add(new friends());
            core.callable.Add(new nearme());
            core.callable.Add(new hello());
            endpoints.Add(core);
            Endpoint inventory = new Endpoint();
            inventory.name = "inventory";
            inventory.callable.Add(new contents());
            inventory.callable.Add(new folders());
            inventory.callable.Add(new rename());
            inventory.callable.Add(new realuuid());
            inventory.callable.Add(new send());
            inventory.callable.Add(new delete());
            endpoints.Add(inventory);
            Endpoint im = new Endpoint();
            im.name = "im";
            im.callable.Add(new chatwindows());
            im.callable.Add(new listwithunread());
            im.callable.Add(new haveunreadims());
            im.callable.Add(new getimchat());
            im.callable.Add(new sendimchat());
            endpoints.Add(im);
            Endpoint group = new Endpoint();
            group.name = "group";
            group.callable.Add(new listgroups());
            group.callable.Add(new listgroupswithunread());
            group.callable.Add(new haveunreadgroupchat());
            group.callable.Add(new getgroupchat());
            group.callable.Add(new sendgroupchat());
            endpoints.Add(group);
            Endpoint chat = new Endpoint();
            chat.name = "chat";
            chat.callable.Add(new localchathistory());
            chat.callable.Add(new localchatsay());
            endpoints.Add(chat);
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

    abstract public class APIcall
    {
        public string name { get { return GetType().Name.ToLowerInvariant(); } }
        public string type = "get";
        public string about = "A API call that is missing its about value";
        public Dictionary<string, KeyValuePair<string, string>> values = new Dictionary<string, KeyValuePair<string, string>>();
        public List<string> returns = new List<string>();
        public bool RequiresToken = true;

        public APIcall()
        {
            Setup();
        }
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

    abstract public class postAPIcall : APIcall
    {
        public override void Setup()
        {
            type = "post";
            base.Setup();
        }
    }

    public class gettoken : postAPIcall
    {
        public override void Setup()
        {
            RequiresToken = false;
            about = "Requests a new token (Vaild for 10 mins) <br/>to use with all other requests";
            values.Add("Authcode", new KeyValuePair<string, string>("string", "the first 10 chars of SHA1(unixtime+WebUIkey)<br/>unixtime can be +- 30 of the bots time."));
            returns = new List<string>();
            returns.Add("Authcode not accepted");
            returns.Add("New API Token");
            base.Setup();
        }
    }

    public class logout : APIcall
    {
        public override void Setup()
        {
            about = "A API call that is missing its about value";
            returns.Add("ok");
            returns.Add("Failed to remove token");
            base.Setup();
        }
    }

    public class version : APIcall
    {
        public override void Setup()
        {
            about = "Gets the bots build version";
            returns.Add("Bot build version");
            base.Setup();
        }
    }

    public class friends : APIcall
    {
        public override void Setup()
        {
            about = "Gets the friendslist <br/>Formated as follows<br/>friendreplyobject<br/><ul><li>name: String</li><li>id: String</li><li>online: bool</li></ul>";
            returns.Add("array UUID = friendreplyobject");
            base.Setup();
        }
    }

    public class nearme : APIcall
    {
        public override void Setup()
        {
            about = "returns a list of all known avatars near (same sim)>";
            returns.Add("array UUID = Name");
            base.Setup();
        }
    }

    public class hello : APIcall
    {
        public override void Setup()
        {
            RequiresToken = false;
            about = "used to test you can talk to the api but mostly pointless.";
            returns.Add("world");
            base.Setup();
        }
    }

    public class name : APIcall
    {
        public override void Setup()
        {
            about = "Gets the name of the bot";
            returns.Add("Fistname Lastname");
            base.Setup();
        }
    }

    public class command : postAPIcall
    {
        public override void Setup()
        {
            about = "Makes a request to the core commands lib";
            values.Add(
                "body",
                new KeyValuePair<string, string>(
                    "JsonObject",
                    "A JSON object formated as follows<br/>Command: string<br/>Args: string[]<br/>AuthCode: string<br/>========<br/>See LSL example on how to create a core command auth code")
            );
            returns.Add("accepted");
            base.Setup();
        }
    }

    public class contents : APIcall
    {
        public override void Setup()
        {
            about = "Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>";
            values.Add(
                "folderUUID",
                new KeyValuePair<string, string>(
                    "URLARG",
                    "the folder to fetch (Found via: inventory/folders)")
            );
            returns.Add("array of InventoryMapItem");
            base.Setup();
        }
    }

    public class folders : APIcall
    {
        public override void Setup()
        {
            about = "Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>";
            returns.Add("array of InventoryMapFolder");
            base.Setup();
        }
    }

    public class rename : postAPIcall
    {
        public override void Setup()
        {
            about = "renames a folder or inventory item";
            returns.Add("true|false");
            values.Add("item", new KeyValuePair<string, string>("URLARG", "UUID of the item/folder we are working on"));
            values.Add("newname", new KeyValuePair<string, string>("string", "What we are setting"));
            base.Setup();
        }
    }

    public class realuuid : APIcall
    {
        public override void Setup()
        {
            about = "converts a inventory uuid to a realworld uuid<br/>Needed for texture preview";
            returns.Add("Failed");
            returns.Add("UUID");
            values.Add("item", new KeyValuePair<string, string>("URLARG", "UUID of the item/folder we are working on"));
            base.Setup();
        }
    }

    public class send : APIcall
    {
        public override void Setup()
        {
            about = "sends a item to an avatar";
            returns.Add("Failed");
            returns.Add("UUID");
            values.Add("item", new KeyValuePair<string, string>("URLARG", "UUID of the item we are working on"));
            values.Add("avatar", new KeyValuePair<string, string>("URLARG", "a UUID or Firstname Lastname"));
            base.Setup();
        }
    }

    public class delete : APIcall
    {
        public override void Setup()
        {
            about = "Removes a item/folder from inventory (Make sure you set the isfolder flag correctly!)";
            returns.Add("Failed");
            returns.Add("UUID");
            values.Add("item", new KeyValuePair<string, string>("URLARG", "UUID of the item/folder we are working on"));
            values.Add("isfolder", new KeyValuePair<string, string>("URLARG", "true or false if this is a folder"));
            base.Setup();
        }
    }

    public class chatwindows : APIcall
    {
        public override void Setup()
        {
            about = "gets a full list of all chat windows";
            returns.Add("array UUID = Name");
            base.Setup();
        }
    }

    public class listwithunread : APIcall
    {
        public override void Setup()
        {
            about = "gets a list of chat windows with unread messages";
            returns.Add("array of UUID");
            base.Setup();
        }
    }

    public class haveunreadims : APIcall
    {
        public override void Setup()
        {
            about = "gets if there are any unread im messages at all";
            returns.Add("true|false");
            base.Setup();
        }
    }

    public class getimchat : APIcall
    {
        public override void Setup()
        {
            about = "gets the chat from the selected window";
            values.Add("window", new KeyValuePair<string, string>("URLARG", "the UUID of the chat window"));
            returns.Add("Chat contents");
            returns.Add("Window UUID invaild");
            base.Setup();
        }
    }

    public class sendimchat : postAPIcall
    {
        public override void Setup()
        {
            about = "sends a im to the selected avatar";
            values.Add("avatar", new KeyValuePair<string, string>("URLARG", "a UUID or Firstname Lastname"));
            values.Add("message", new KeyValuePair<string, string>("string", "the message to send"));
            returns.Add("ok");
            base.Setup();
        }
    }

    public class listgroups : APIcall
    {
        public override void Setup()
        {
            about = "fetchs a list of all groups known to the bot";
            returns.Add("array UUID=name");
            base.Setup();
        }
    }

    public class listgroupswithunread : APIcall
    {
        public override void Setup()
        {
            about = "fetchs a list of all groups with unread messages";
            returns.Add("array UUID");
            base.Setup();
        }
    }

    public class haveunreadgroupchat : APIcall
    {
        public override void Setup()
        {
            about = "checks if there are any groups with unread messages";
            returns.Add("true|false");
            base.Setup();
        }
    }

    public class getgroupchat : APIcall
    {
        public override void Setup()
        {
            about = "fetchs the groupchat history";
            values.Add("group ", new KeyValuePair<string, string>("URLARG", "UUID of the group"));
            returns.Add("Group UUID invaild");
            returns.Add("Group Chat");
            base.Setup();
        }
    }

    public class sendgroupchat : postAPIcall
    {
        public override void Setup()
        {
            about = "sends a message to the groupchat";
            values.Add("group ", new KeyValuePair<string, string>("URLARG", "UUID of the group"));
            values.Add("message ", new KeyValuePair<string, string>("string", "the message to send"));
            returns.Add("Group UUID invaild");
            returns.Add("Processing");
            base.Setup();
        }
    }

    public class localchathistory : APIcall
    {
        public override void Setup()
        {
            about = "fetchs the last 20 localchat messages";
            returns.Add("array string");
            base.Setup();
        }
    }

    public class localchatsay : postAPIcall
    {
        public override void Setup()
        {
            about = "sends a message to localchat";
            values.Add("message ", new KeyValuePair<string, string>("string", "the message to send"));
            returns.Add("see->localchathistory");
            base.Setup();
        }
    }

}
