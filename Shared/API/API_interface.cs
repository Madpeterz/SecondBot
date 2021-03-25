using BetterSecondBotShared.logs;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterSecondBotShared.API
{
    public abstract class API_interface
    {
        public string CommandName { get { return GetType().Name.ToLowerInvariant(); ; } }
        public virtual int Min_Required_args { get { return 0; } }
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
                            string name = D.CommandName;
                            if (subtype_map.ContainsKey(name) == false)
                            {
                                string worknamespace = com.Namespace;
                                worknamespace = worknamespace.Replace("BetterSecondBot.Commands.", "");
                                worknamespace = worknamespace.Replace("BetterSecondBot.RLV.", "");
                                worknamespace = worknamespace.Replace("BetterSecondbot.HttpServer.View.", "");
                                worknamespace = worknamespace.Replace("BetterSecondbot.HttpServer.view.", "");
                                worknamespace = worknamespace.Replace("BetterSecondbot.HttpServer.Control.", "");
                                worknamespace = worknamespace.Replace("BetterSecondbot.HttpServer.control.", "");
                                worknamespace = worknamespace.Replace("CMD_", "");
                                subtype_map.Add(name, D.GetType());
                                subtype_workspace_map.Add(name, worknamespace);
                            }
                            else
                            {
                                LogFormater.Crit("[CMD] command: " + D.CommandName + " already defined!");
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

                Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();

                List<Assembly> GoodLooking = new List<Assembly>();
                foreach (Assembly Ass in asses)
                {
                    if (Ass.FullName.StartsWith("BetterSecondbot") == true) 
                    {
                        GoodLooking.Add(Ass);
                    }
                    else if (Ass.FullName.StartsWith("Core") == true)
                    {
                        GoodLooking.Add(Ass);
                    }
                    else if (Ass.FullName.StartsWith("Shared") == true) 
                    {
                        GoodLooking.Add(Ass);
                    }
                }

                List<Type> reply = new List<Type>();
                foreach (Assembly Ass in GoodLooking)
                {
                    foreach(Type ClassType in Ass.GetTypes())
                    {
                        if(ClassType.IsSubclassOf(API_type) == true)
                        {
                            reply.Add(ClassType);
                        }
                    }
                }
                return reply;
            }
            else
            {
                LogFormater.Crit(this.GetType() + " Attempted to call GetSubTypes without setting API_TYPE first!");
            }
            return null;
        }
        public virtual string[] GetCommandsList()
        {
            if (subtype_map.Count() == 0)
            {
                LoadCommandsList();
            }
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
                return cmdGet.Min_Required_args;
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
