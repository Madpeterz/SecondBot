using BetterSecondBotShared.logs;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSecondBotShared.API
{
    public abstract class API_interface
    {
        public string CommandName { get { return GetType().Name.ToLowerInvariant(); } }
        public virtual int MinArgs { get { return 0; } }
        public virtual string Helpfile { get { return "No help given"; } }
        public virtual string[] ArgTypes { get { return new string[] { }; } }
        public virtual string[] ArgHints { get { return new string[] { }; } }

        protected string InfoBlob = "";
        public string GetInfoBlob { get { return InfoBlob; } }
    }

    public class API_supported_interface
    {
        protected Type API_type;
        protected Dictionary<string, Type> subtype_map = new Dictionary<string, Type>();
        protected Dictionary<string, string> subtype_workspace_map = new Dictionary<string, string>();
        public virtual int ApiCommandsCount { get { return subtype_map.Count(); } }

        protected API_interface GetCommand(string command)
        {
            if (KnownCommand(command) == true)
            {
                return Activator.CreateInstance(subtype_map[command]) as API_interface;
            }
            return null;
        }
        protected virtual void LoadCommandsList()
        {
            foreach (var com in GetAPICommandClasses())
            {
                if (com != null)
                {
                    if (com.IsAbstract == false)
                    {
                        API_interface D = (API_interface)Activator.CreateInstance(com);
                        if (D != null)
                        {
                            if (subtype_map.ContainsKey(D.CommandName) == false)
                            {
                                string worknamespace = com.Namespace;
                                worknamespace = worknamespace.Replace("BSB.Commands.", "");
                                worknamespace = worknamespace.Replace("BSB.RLV.", "");
                                worknamespace = worknamespace.Replace("BetterSecondBot.HttpServer.View.", "");
                                worknamespace = worknamespace.Replace("BetterSecondBot.HttpServer.view.", "");
                                worknamespace = worknamespace.Replace("BetterSecondBot.HttpServer.Control.", "");
                                worknamespace = worknamespace.Replace("BetterSecondBot.HttpServer.control.", "");
                                worknamespace = worknamespace.Replace("CMD_", "");
                                subtype_map.Add(D.CommandName, D.GetType());
                                subtype_workspace_map.Add(D.CommandName, worknamespace);
                            }
                            else
                            {
                                ConsoleLog.Crit("[CMD] command: " + D.CommandName + " already defined!");
                            }
                        }
                    }
                }
            }
        }
        protected IEnumerable<Type> GetAPICommandClasses()
        {
            if (API_type != null)
            { 
                return AppDomain.CurrentDomain.GetAssemblies().Where(Ass => {
                    bool reply = false;
                    if (Ass.FullName.StartsWith("BSB") == true) { reply = true; }
                    else if (Ass.FullName.StartsWith("BetterSecondBot") == true){ reply = true; }
                    return reply;
                    }).SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(API_type));
            }
            else
            {
                ConsoleLog.Crit(this.GetType() + " Attempted to call GetSubTypes without setting API_TYPE first!");
            }
            return null;
        }
        public virtual string[] GetCommandsList()
        {
            return subtype_map.Keys.ToArray<string>();
        }
        public virtual string GetCommandWorkspace(string cmd)
        {
            if (subtype_workspace_map.ContainsKey(cmd) == true)
            {
                return subtype_workspace_map[cmd];
            }
            return "";
        }
        protected virtual bool KnownCommand(string command)
        {
            if (subtype_map.Count() == 0)
            {
                LoadCommandsList();
            }
            return subtype_map.ContainsKey(command);
        }

        public virtual string GetCommandHelp(string cmd)
        {
            API_interface cmdGet = GetCommand(cmd);
            if (cmdGet != null)
            {
                return cmdGet.Helpfile;
            }
            return "None given";
        }
        public virtual int GetCommandArgs(string cmd)
        {
            API_interface cmdGet = GetCommand(cmd);
            if (cmdGet != null)
            {
                return cmdGet.MinArgs;
            }
            return 0;
        }
        public virtual string[] GetCommandArgTypes(string cmd)
        {
            API_interface CmdGet = GetCommand(cmd);
            if (CmdGet != null)
            {
                return CmdGet.ArgTypes;
            }
            return new string[] { };
        }
        public virtual string[] GetCommandArgHints(string cmd)
        {
            API_interface CmdGet = GetCommand(cmd);
            if (CmdGet != null)
            {
                return CmdGet.ArgHints;
            }
            return new string[] { };
        }

        public virtual List<string> GetAllWorkspaces()
        {
            List<string> reply = new List<string>();
            foreach (string c in GetCommandsList())
            {
                string workspace = GetCommandWorkspace(c);
                if (reply.Contains(workspace) == false)
                {
                    reply.Add(workspace);
                }
            }
            return reply;
        }
    }
}
