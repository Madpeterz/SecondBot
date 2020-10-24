using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using OpenMetaverse.Assets;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;
using System.Threading;

namespace BSB.bottypes
{
    public abstract class CommandsBot : AVstorageBot
    {
        protected CommandsBot()
        {
            CommandsInterface = new Commands.CoreCommandsInterface(this);
        }

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
        public virtual void SendIM(UUID avatar,string message)
        {
            AddToIMchat(avatar, GetClient.Self.Name, message);
            Client.Self.InstantMessage(avatar, message);
        }

        public Commands.CoreCommandsInterface GetCommandsInterface { get { return CommandsInterface; } }
        protected Commands.CoreCommandsInterface CommandsInterface;

        protected Dictionary<string, string> Scripts_content = new Dictionary<string, string>();
        protected Dictionary<string, string> notecards_content = new Dictionary<string, string>();
        protected Dictionary<UUID, long> group_invite_lockout_timer = new Dictionary<UUID, long>();
        protected long last_cleanup_commands;

        public override string GetStatus()
        {
            long dif = helpers.UnixTimeNow() - last_cleanup_commands;
            if (dif > 30)
            {
                last_cleanup_commands = helpers.UnixTimeNow();
                PurgeOldGroupinviteLockouts();
            }
            CommandsInterface.StatusTick();
            return base.GetStatus();
        }
        public bool GetAllowGroupInvite(UUID avatar)
        {
            return !group_invite_lockout_timer.ContainsKey(avatar);
        }

        public void GroupInviteLockoutArm(UUID avatar)
        {
            if (group_invite_lockout_timer.ContainsKey(avatar) == false)
            {
                group_invite_lockout_timer.Add(avatar,0);
            }
            group_invite_lockout_timer[avatar] = helpers.UnixTimeNow();
        }
        protected void PurgeOldGroupinviteLockouts()
        {
            long now = helpers.UnixTimeNow();
            List<UUID> clear_locks = new List<UUID>();
            foreach(KeyValuePair<UUID,long> Glock in group_invite_lockout_timer)
            {
                long dif = now - Glock.Value;
                if(dif >= 120)
                {
                    clear_locks.Add(Glock.Key);
                }
            }
            foreach(UUID glock in clear_locks)
            {
                group_invite_lockout_timer.Remove(glock);
            }
        }

        protected void custom_commands_loop(string command,string arg,UUID caller)
        {
            List<string> customargs = arg.Split(new[] { "~#~" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            while(customargs.Count < 5)
            {
                customargs.Add("notset");
            }
            foreach (string A in custom_commands[command])
            {
                List<string> command_args_split = A.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                int loop = 1;
                string args_passed = "";
                if (command_args_split.Count() == 2)
                {
                    args_passed = command_args_split[1];
                    while (loop <= 5)
                    {
                        args_passed = args_passed.Replace("[C_ARG_"+loop.ToString()+"]", customargs[loop-1]);
                        loop++;
                    }
                }
                CommandsInterface.Call(command_args_split[0], args_passed, caller, "~#~");
            }
        }
        protected virtual void CallCommandLib(string command, string arg)
        {
            CallCommandLib(command, arg, UUID.Zero);
        }
        protected virtual void CallCommandLib(string command, string arg, UUID caller)
        {
            CallCommandLib(command, arg, caller, "~#~");
        }
        protected virtual void CallCommandLib(string command, string arg, UUID caller,string signed_with)
        {
            if(custom_commands.ContainsKey(command) == false)
            {
                CommandsInterface.Call(command, arg, caller, signed_with);
            }
            else
            {
                Thread t = new Thread(() => custom_commands_loop(command, arg, caller));
                t.Start();
            }
        }

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                CommandsInterface = new Commands.CoreCommandsInterface(this);
            }
            Client.Self.RequestBalance();
        }

        protected override void CoreCommandLib(UUID fromUUID, bool from_master, string command, string arg, string signing_code, string signed_with)
        {
            if (CommandsInterface != null)
            {
                if (signing_code != "")
                {
                    string raw = "" + command + "" + arg + "" + myconfig.Security_SignedCommandkey + "";
                    string hashcheck = helpers.GetSHA1(raw);
                    if (hashcheck == signing_code)
                    {
                        CallCommandLib(command, arg, fromUUID, signed_with);
                    }
                }
                else if (from_master == true)
                {
                    CallCommandLib(command, arg, fromUUID, signed_with);
                }
            }
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
        public void ScriptAddContent(string target_Script_storage_id, string content)
        {
            ScriptAddContent(target_Script_storage_id, content, true, "\n\r");
        }
        public void ScriptAddContent(string target_Script_storage_id, string content, bool add_newline)
        {
            ScriptAddContent(target_Script_storage_id, content, add_newline, "\n\r");
        }
        public void ScriptAddContent(string target_Script_storage_id, string content, bool add_newline, string newlinevalue)
        {
            if (Scripts_content.ContainsKey(target_Script_storage_id) == false)
            {
                Scripts_content.Add(target_Script_storage_id, "");
            }
            if (add_newline == true)
            {
                content = "" + newlinevalue + "" + content;
            }
            Scripts_content[target_Script_storage_id] = Scripts_content[target_Script_storage_id] + "" + content;
        }
        public string GetScriptContent(string target_Script_storage_id)
        {
            if (Scripts_content.ContainsKey(target_Script_storage_id) == true)
            {
                return Scripts_content[target_Script_storage_id];
            }
            return null;
        }

        public void ClearScriptStorage(string target_Script_storage_id)
        {
            if (Scripts_content.ContainsKey(target_Script_storage_id) == true)
            {
                Scripts_content.Remove(target_Script_storage_id);
            }
        }

        public bool SendScript(string name, string content, UUID sendToUUID)
        {
            bool returnstatus = true;
            Client.Inventory.RequestCreateItem(
                Client.Inventory.FindFolderForType(AssetType.LSLText),
                name,
                name,
                AssetType.LSLText,
                UUID.Random(),
                InventoryType.LSL,
                PermissionMask.All,
                (bool Success, InventoryItem item) =>
                {
                    if (Success)
                    {
                        AssetScriptText empty = new AssetScriptText { Source = "\n" };
                        empty.Encode();
                        Client.Inventory.RequestUpdateScriptAgentInventory(new byte[] { }, item.UUID, true,
                        (bool uploadSuccess, string uploadStatus, bool compileSuccess, List<string> compileMessages, UUID itemID, UUID assetID) =>
                        {
                            if (uploadSuccess)
                            {
                                empty.AssetData = Encoding.ASCII.GetBytes(content);
                                Client.Inventory.RequestUpdateScriptAgentInventory(empty.AssetData, itemID,true,
                                (bool finaluploadSuccess, string finaluploadStatus, bool finalcompileSuccess, List<string> finalcompileMessages, UUID finalitemID, UUID finalassetID) =>
                                {
                                    if (finalcompileSuccess == false)
                                    {
                                        Warn("Script failed to compile, sending anyway.");
                                    }
                                    Client.Inventory.GiveItem(finalitemID, name, AssetType.LSLText, sendToUUID, false);

                                });
                            }
                            else
                            {
                                Crit("The fuck empty success Script create");
                                returnstatus = false;
                            }
                        });
                    }
                    else
                    {
                        Warn("Unable to find default Scripts folder");
                        returnstatus = false;
                    }
                }
            );
            return returnstatus;
        }
    }
}
