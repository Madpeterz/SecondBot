﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using OpenMetaverse.Assets;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;
using BetterSecondBot.HttpService;
using Core.Static;
using System.Reflection;
using Newtonsoft.Json;
using System.Net.Http;
using BetterSecondBotShared.Json;

namespace BetterSecondBot.bottypes
{
    public abstract class CommandsBot : StorageBot
    {
        protected List<string> CommandHistory = new List<string>();
        protected int commandid = 1;
        public virtual string CommandHistoryAdd(string command, string arg, bool status,string infomessage)
        {
            string message = "";
            if (infomessage != "--")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(commandid.ToString());
                sb.Append(" - ");
                sb.Append(command);
                sb.Append(" / [" + arg + "]");
                if (status == false)
                {
                    sb.Append(" (FAILED) ");
                }
                else
                {
                    sb.Append(" (OK) ");
                }
                if (helpers.notempty(infomessage) == true)
                {
                    sb.Append("[Info: ");
                    sb.Append(infomessage);
                    sb.Append("]");
                }
                CommandHistory.Add(sb.ToString());
                if (CommandHistory.Count() > 50)
                {
                    CommandHistory.RemoveAt(0);
                }
                message = sb.ToString();
                if (myconfig.Setting_LogCommands == true)
                {
                    Info(message);
                }
                commandid++;
                if (commandid >= 50)
                {
                    commandid = 1;
                }
            }
            return message;
        }

        public string[] GetLastCommands()
        {
            return GetLastCommands(10);
        }
        public string[] GetLastCommands(int amount)
        {
            List<string> commands = new List<string>();
            int loop = 0;
            while((loop < CommandHistory.Count) && (loop < amount))
            {
                commands.Add(CommandHistory.ElementAt(CommandHistory.Count - loop - 1));
                loop++;
            }
            return commands.ToArray();
        }
        public override void SendIM(UUID avatar,string message)
        {
            AddToIMchat(avatar, GetClient.Self.Name, message);
            Client.Self.InstantMessage(avatar, message);
            base.SendIM(avatar, message);
        }

        protected Dictionary<string, string> notecards_content = new Dictionary<string, string>();
        protected Dictionary<UUID, Dictionary<UUID, long>> group_invite_lockout_timer = new Dictionary<UUID, Dictionary<UUID, long>>();
        protected long last_cleanup_commands;

        public override string GetStatus()
        {
            long dif = helpers.UnixTimeNow() - last_cleanup_commands;
            if (dif > 30)
            {
                last_cleanup_commands = helpers.UnixTimeNow();
                PurgeOldGroupinviteLockouts();
            }
            return base.GetStatus();
        }
        public bool GetAllowGroupInvite(UUID avatar, UUID group)
        {
            if (group_invite_lockout_timer.ContainsKey(group) == true)
            {
                return !group_invite_lockout_timer[group].ContainsKey(avatar);
            }
            return true;
        }

        protected void PurgeOldGroupinviteLockouts()
        {
            long now = helpers.UnixTimeNow();
            foreach (UUID group in group_invite_lockout_timer.Keys)
            {
                List<UUID> clear_locks = new List<UUID>();
                foreach (KeyValuePair<UUID, long> Glock in group_invite_lockout_timer[group])
                {
                    long dif = now - Glock.Value;
                    if (dif >= 120)
                    {
                        clear_locks.Add(Glock.Key);
                    }
                }
                foreach (UUID glock in clear_locks)
                {
                    group_invite_lockout_timer[group].Remove(glock);
                }
            }
        }

        public override void AfterBotLoginHandler()
        {
            if (reconnect == false)
            {
                loadCommands();
                LoadCustomCommands();
            }
            Client.Self.RequestBalance();
            base.AfterBotLoginHandler();
        }

        public string[] getFullListOfCommands()
        {
            List<string> output = new List<string>();
            foreach(string A in endpointcommandmap.Keys)
            {
                output.Add(A.ToLowerInvariant());
            }
            return output.ToArray();
        }

        public string[] GetFullListOfCommandsWithCustoms()
        {
            List<string> output = new List<string>();
            foreach (string A in endpointcommandmap.Keys)
            {
                output.Add(A.ToLowerInvariant());
            }
            foreach(string A in custom_commands.Keys)
            {
                output.Add(A.ToLowerInvariant());
            }
            return output.ToArray();
        }

        public void NotecardAddContent(string target_notecard_storage_id, string content)
        {
            NotecardAddContent(target_notecard_storage_id, content, true, "\n\r");
        }
        public void NotecardAddContent(string target_notecard_storage_id, string content, bool add_newline)
        {
            NotecardAddContent(target_notecard_storage_id, content, add_newline, "\n\r");
        }
        public void NotecardAddContent(string target_notecard_storage_id,string content,bool add_newline,string newlinevalue)
        {
            if(notecards_content.ContainsKey(target_notecard_storage_id) == false)
            {
                notecards_content.Add(target_notecard_storage_id, "");
            }
            if (add_newline == true)
            {
                content = "" + newlinevalue + "" + content;
            }
            notecards_content[target_notecard_storage_id] = notecards_content[target_notecard_storage_id] + ""+content;
        }
        public string GetNotecardContent(string target_notecard_storage_id)
        {
            if (notecards_content.ContainsKey(target_notecard_storage_id) == true)
            {
                return notecards_content[target_notecard_storage_id];
            }
            return null;
        }

        public void ClearNotecardStorage(string target_notecard_storage_id)
        {
            if (notecards_content.ContainsKey(target_notecard_storage_id) == true)
            {
                notecards_content.Remove(target_notecard_storage_id);
            }
        }

        public bool SendNotecard(string name, string content, UUID sendToUUID)
        {
            bool returnstatus = true;
            name = name + " " + DateTime.Now;
            Client.Inventory.RequestCreateItem(
                Client.Inventory.FindFolderForType(AssetType.Notecard),
                name,
                name + " Created via SecondBot notecard API",
                AssetType.Notecard,
                UUID.Random(),
                InventoryType.Notecard,
                PermissionMask.All,
                (bool Success, InventoryItem item) =>
                {
                    if (Success)
                    {
                        AssetNotecard empty = new AssetNotecard { BodyText = "\n" };
                        empty.Encode();
                        Client.Inventory.RequestUploadNotecardAsset(empty.AssetData, item.UUID,
                        (bool emptySuccess, string emptyStatus, UUID emptyItemID, UUID emptyAssetID) =>
                        {
                            if (emptySuccess)
                            {
                                empty.BodyText = content;
                                empty.Encode();
                                Client.Inventory.RequestUploadNotecardAsset(empty.AssetData, emptyItemID,
                                (bool finalSuccess, string finalStatus, UUID finalItemID, UUID finalID) =>
                                {
                                    if (finalSuccess)
                                    {
                                        Info("Sending notecard now");
                                        Client.Inventory.GiveItem(finalItemID, name, AssetType.Notecard, sendToUUID, false);
                                    }
                                    else
                                    {
                                        returnstatus = false;
                                        Warn("Unable to request notecard upload");
                                    }

                                });
                            }
                            else
                            {
                                Crit("The fuck empty success notecard create");
                                returnstatus = false;
                            }
                        });
                    }
                    else
                    {
                        Warn("Unable to find default notecards folder");
                        returnstatus = false;
                    }
                }
            );
            return returnstatus;
        }


        protected void JsonChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            if((group == true) || (localchat == true) || (fromme == true))
            {
                return;
            }
            try
            {
                bool frommaster = true;
                if (sender_uuid != master_uuid)
                {
                    frommaster = false;
                }
                if (frommaster == true)
                {
                    if (message == "fakerestart")
                    {
                        AlertMessageEventArgs Alertargs = new AlertMessageEventArgs("this is a fake restart message",new Random().Next(99999).ToString(), new OpenMetaverse.StructuredData.OSDMap());
                        AlertEvent(this, Alertargs);
                        return;
                    }

                }
                JsonApiDefine APIE = JsonConvert.DeserializeObject<JsonApiDefine>(message);

                CoreCommandLib(sender_uuid, frommaster, APIE.cmd, APIE.args, APIE.signing, APIE.reply);
            }
            catch (Exception e)
            {
                LogFormater.Crit("[CoreCommandLib] exploded: " + e.Message + "");
            }
        }


        protected Dictionary<string, WebApiControllerWithTokens> commandEndpoints = new Dictionary<string, WebApiControllerWithTokens>();
        protected Dictionary<string, string> endpointcommandmap = new Dictionary<string, string>(); // command = endpoint
        protected Dictionary<string, string> commandnameLowerToReal = new Dictionary<string, string>();
        protected TokenStorage Tokens = new TokenStorage();
        protected bool commandsLoaded = false;

        protected override void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            base.BotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
            if ((group == true) || (localchat == true) || (fromme == true))
            {
                return;
            }
            try
            {
                JsonApiDefine APIE = JsonConvert.DeserializeObject<JsonApiDefine>(message);
                JsonChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
            }
            catch
            {
                // Old style [non json] to json converter [to be phased out]
                string signing_code = "";
                string[] S1 = message.Split(new[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
                if (S1.Length == 2)
                {
                    signing_code = S1[1];
                }
                S1 = S1[0].Split(new[] { "#|#" }, StringSplitOptions.RemoveEmptyEntries);
                string outputto = "none";
                if (S1.Length == 2)
                {
                    outputto = S1[1];
                }
                S1 = S1[0].Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);
                string[] args = new string[] { };
                if (S1.Length == 2)
                {
                    args = S1[1].Split(new[] { "~#~" }, StringSplitOptions.RemoveEmptyEntries);
                }
                JsonApiDefine Apihandoff = new JsonApiDefine();
                Apihandoff.cmd = S1[0];
                Apihandoff.args = args;
                Apihandoff.reply = outputto;
                Apihandoff.signing = signing_code;
                message = JsonConvert.SerializeObject(Apihandoff);
                JsonChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
            }
        }

        protected KeyValuePair<bool,string> callAPIcommand(string command, string[] args)
        {

        }

        protected void CoreCommandLib(UUID sender, bool Master, string command, string[] args, string signingcode, string targetreply)
        {
            bool accepted = Master;
            if(accepted == false)
            {
                string raw = "" + command + "" + string.Join("~#~", args) + "" + myconfig.Security_SignedCommandkey + "";
                string hashcheck = helpers.GetSHA1(raw);
                if (hashcheck == signingcode)
                {
                    accepted = true;
                }
            }
            if (accepted == true)
            {
                CallAPI(command, args, targetreply);
            }
        }

        public string CallAPI(string command, string[] args)
        {
            return CallAPI(command, args, "None");
        }
        public string CallAPI(string command, string[] args, string replyvia)
        {
            return CallAPI(command, args, replyvia, false);
        }

        protected string customCommand(string command, string[] args, string replyvia, bool customcommand)
        {
            if (customcommand == true)
            {
                SmartCommandReply(false, replyvia, "Custom command lockout", command);
                return "Custom command lockout";
            }
            int commands_issued = 0;
            foreach (string A in custom_commands[command])
            {
                List<string> command_args_split = A.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                int loop = 1;
                string args_passed = "";
                if (command_args_split.Count() == 2)
                {
                    args_passed = command_args_split[1];
                    while ((loop <= 5) && (loop <= args.Length))
                    {
                        args_passed = args_passed.Replace("[C_ARG_" + loop.ToString() + "]", args[loop - 1]);
                        loop++;
                    }
                }
                CallAPI(command_args_split[0], args_passed.Split("~#~"), "None", true);
                commands_issued++;
            }
            return "Issued "+ commands_issued.ToString()+" commands";
        }
        public string CallAPI(string command, string[] args, string replyvia, bool customcommand)
        {
            if (command == null)
            {
                return "No command";
            }
            if (replyvia == null)
            {
                replyvia = "none";
            }
            KeyValuePair<bool, string> statusreply = new KeyValuePair<bool, string>(false, "Not processed");
            if (commandnameLowerToReal.ContainsKey(command.ToLowerInvariant()) == false)
            {
                if (custom_commands.ContainsKey(command) == false)
                {
                    SmartCommandReply(false, replyvia, "Unknown command", command);
                    return "{ status: \"false\", message: \"Unknown\" }";
                }
                statusreply = new KeyValuePair<bool, string>(true, customCommand(command, args, replyvia, customcommand));
            }
            else
            {
                statusreply = callAPIcommand(command, args);
            }
            SmartCommandReply(statusreply.Key, replyvia, statusreply.Value, command);
            return "{ status: \"" + statusreply.Key.ToString() + "\", message: \""+ statusreply.Value+"\" command: \""+command+"\", args: \""+string.Join("~#~",args)+"\" }";
        }

        





        protected void loadCommands()
        {
            commandsLoaded = true;
            Dictionary<string, Type> commandmodules = http_commands_helper.getCommandModules();
            foreach (Type entry in commandmodules.Values)
            {
                LoadCommandEndpoint(entry);
            }
        }

        protected virtual void LoadCommandEndpoint(Type endpointtype)
        {
            WebApiControllerWithTokens controler = (WebApiControllerWithTokens)Activator.CreateInstance(endpointtype, args: new object[] { this, Tokens });
            controler.disableIPlockout();
            commandEndpoints.Add(endpointtype.Name, controler);
            foreach (MethodInfo M in endpointtype.GetMethods())
            {
                bool isCallable = false;
                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (At.AttributeType.Name == "RouteAttribute")
                    {
                        isCallable = true;
                        break;
                    }
                }
                if (isCallable == true)
                {
                    if (endpointcommandmap.ContainsKey(M.Name) == true)
                    {
                        Warn("Namespace: "+endpointtype.Name+" / Command: " + M.Name + " already found in " + endpointcommandmap[M.Name]);
                        continue;
                    }
                    if(commandnameLowerToReal.ContainsKey(M.Name.ToLowerInvariant()) == true)
                    {
                        Warn("Namespace: " + endpointtype.Name + " / Command: " + M.Name + " already found in " + endpointcommandmap[M.Name]);
                        continue;
                    }
                    endpointcommandmap.Add(M.Name, endpointtype.Name);
                    commandnameLowerToReal.Add(M.Name.ToLowerInvariant(), M.Name);
                }
            }
        }
    }
}
