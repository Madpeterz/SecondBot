using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using Newtonsoft.Json;
using OpenMetaverse;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSecondBot.bottypes
{
    public abstract class AVstorageBot : AttachableBot
    {
        protected UUID master_uuid = UUID.Zero;
        public UUID getMaster_uuid { get { return master_uuid; } }

        protected Dictionary<string, long> AvatarStorageLastUsed = new Dictionary<string, long>();
        public Dictionary<string, UUID> AvatarName2Key = new Dictionary<string, UUID>();
        public Dictionary<UUID, string> AvatarKey2Name = new Dictionary<UUID, string>();
        protected Dictionary<string, KeyValuePair<long, int>> PendingAvatarFinds_vianame = new Dictionary<string, KeyValuePair<long, int>>();
        protected Dictionary<UUID, KeyValuePair<long, int>> PendingAvatarFinds_viauuid = new Dictionary<UUID, KeyValuePair<long, int>>();

        protected long max_db_storage_age = 2240; // how long should we keep an avatar in memory before clearing (in secs) [now - last accessed]
        protected int max_retry_attempts = 2;
        protected bool show_av_storage_info_in_status = true;
        protected string AVstorageBot_laststatus = "";


        protected Dictionary<UUID, string> FriendNames = new Dictionary<UUID, string>();
        protected override void AvatarFriendNames(object o, FriendNamesEventArgs E)
        {
            FriendNames = E.Names;
        }

        public string getJsonFriendlist()
        {
            Dictionary<UUID, friendreplyobject> Friends = new Dictionary<UUID, friendreplyobject>();
            Dictionary<UUID, FriendInfo> friendscopy = GetClient.Friends.FriendList.Copy();
            foreach (KeyValuePair<UUID, FriendInfo> av in friendscopy)
            {
                string name = FindAvatarKey2Name(av.Key);
                if (name != "loopup")
                {
                    friendreplyobject entry = new friendreplyobject();
                    entry.name = name;
                    entry.id = av.Key.ToString();
                    entry.online = av.Value.IsOnline;
                    Friends.Add(av.Key, entry);
                }
            }
            return JsonConvert.SerializeObject(Friends);
        }

        public override string GetStatus()
        {
            cleanupAvatarStorage();
            ReTriggerAvLookups();
            UpdateStorageWithAroundMe();
            string reply = "";
            if (show_av_storage_info_in_status == true)
            {
                if ((PendingAvatarFinds_vianame.Count > 0) || (PendingAvatarFinds_viauuid.Count > 0))
                {
                    reply = "Pending: "+PendingAvatarFinds_vianame.Count.ToString() + " Name2Key() - "+ PendingAvatarFinds_viauuid.Count.ToString()+" Key2Name()";
                }
                if (AvatarStorageLastUsed.Count > 0)
                {
                    if (reply != "")
                    {
                        reply += " , ";
                    }
                    reply += AvatarStorageLastUsed.Count.ToString() + " loaded";
                }
                reply += " - " + FriendNames.Count() + " Friends";
                if (reply != "")
                {
                    reply = " (AVStorage: " + reply + ")";
                }
                else
                {
                    reply = " (AVStorage: Empty)";
                }
            }
            if (reply != AVstorageBot_laststatus)
            {
                AVstorageBot_laststatus = reply;
                return base.GetStatus() + reply;
            }
            return base.GetStatus();
        }

        protected void UpdateStorageWithAroundMe()
        {
            if(Client.Network.CurrentSim != null)
            {
                Dictionary<uint,Avatar> avcopy = Client.Network.CurrentSim.ObjectsAvatars.Copy();
                foreach (Avatar av in avcopy.Values)
                {
                    AddAvatarToDB(av.ID, av.Name);
                }
            }
        }

        protected void ReTriggerUUIDtoName()
        {
            lock (PendingAvatarFinds_viauuid)
            {
                long now = helpers.UnixTimeNow();
                List<UUID> retrigger_key2name = new List<UUID>();
                List<UUID> giveup_key2name = new List<UUID>();
                foreach (KeyValuePair<UUID, KeyValuePair<long, int>> pair in PendingAvatarFinds_viauuid)
                {
                    long dif = now - pair.Value.Key;
                    if (dif > 30)
                    {
                        if (pair.Value.Value < max_retry_attempts)
                        {
                            retrigger_key2name.Add(pair.Key);
                        }
                        else
                        {
                            giveup_key2name.Add(pair.Key);
                        }
                    }
                }
                if (retrigger_key2name.Count > 0)
                {
                    foreach (UUID avuuid in retrigger_key2name)
                    {
                        PendingAvatarFinds_viauuid[avuuid] = new KeyValuePair<long, int>(now, PendingAvatarFinds_viauuid[avuuid].Value + 1);
                    }
                    Client.Avatars.RequestAvatarNames(retrigger_key2name);
                }
                foreach (UUID avuuid in giveup_key2name)
                {
                    PendingAvatarFinds_viauuid.Remove(avuuid);
                }
            }
        }

        protected void ReTriggerNameToUUID()
        {
            long now = helpers.UnixTimeNow();
            List<string> retrigger_name2key = new List<string>();
            List<string> giveup_name2key = new List<string>();
            foreach (KeyValuePair<string, KeyValuePair<long, int>> pair in PendingAvatarFinds_vianame)
            {
                long dif = now - pair.Value.Key;
                if (dif > 30)
                {
                    if (pair.Value.Value < max_retry_attempts)
                    {
                        retrigger_name2key.Add(pair.Key);
                    }
                    else
                    {
                        giveup_name2key.Add(pair.Key);
                    }
                }
            }
            
            foreach (string avname in retrigger_name2key)
            {
                int retry_counter = PendingAvatarFinds_vianame[avname].Value + 1;
                PendingAvatarFinds_vianame[avname] = new KeyValuePair<long, int>(now, retry_counter);
                Client.Directory.StartPeopleSearch(avname, 0);
                
            }
            foreach (string avname in giveup_name2key)
            {
                PendingAvatarFinds_vianame.Remove(avname);
            }
        }

        protected void ReTriggerAvLookups()
        {
            ReTriggerUUIDtoName();
            ReTriggerNameToUUID();
        }

        protected void cleanupAvatarStorage()
        {
            List<string> PurgeAvatars = new List<string>();
            long now = helpers.UnixTimeNow();
            foreach (KeyValuePair<string, long> entry in AvatarStorageLastUsed)
            {
                long dif = now - entry.Value;
                if(dif > max_db_storage_age)
                {
                    if (PurgeAvatars.Contains(entry.Key) == false)
                    {
                        PurgeAvatars.Add(entry.Key);
                    }
                }
            }
            lock (AvatarName2Key)
            {
                lock (AvatarStorageLastUsed)
                {
                    foreach (string purge in PurgeAvatars)
                    {
                        if (purge != myconfig.Security_MasterUsername)
                        {
                            if (AvatarStorageLastUsed.ContainsKey(purge) == true)
                            {
                                if (AvatarName2Key.ContainsKey(purge) == true)
                                {
                                    AvatarStorageLastUsed.Remove(purge);
                                    UUID avuuid = AvatarName2Key[purge];
                                    AvatarName2Key.Remove(purge);
                                    AvatarKey2Name.Remove(avuuid);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void AfterBotLoginHandler()
        {
            if (reconnect == false)
            {
                Client.Avatars.UUIDNameReply += Key2NameEvent;
                Client.Directory.DirPeopleReply += Name2KeyEvent;
            }
            if (helpers.notempty(myconfig.Security_MasterUsername) == true)
            {
                FindAvatarName2Key(myconfig.Security_MasterUsername);
            }
            base.AfterBotLoginHandler();
        }

        protected void Key2NameEvent(object sender,UUIDNameReplyEventArgs e)
        {
            foreach(KeyValuePair<UUID,string> pair in e.Names)
            {
                AddAvatarToDB(pair.Key, pair.Value);
            }
        }
        protected void Name2KeyEvent(object sender, DirPeopleReplyEventArgs e)
        {
            foreach (DirectoryManager.AgentSearchData pair in e.MatchedPeople)
            {
                AddAvatarToDB(pair.AgentID, "" + pair.FirstName + " " + pair.LastName + "");
            }
        }

        protected void AddAvatarToDB(UUID av_uuid,string av_name)
        {
            if ((av_name != "null") && (av_name != null) && (av_name.ToLowerInvariant() != "secondlife") && (av_uuid != UUID.Zero))
            {
                lock (AvatarStorageLastUsed) lock (AvatarKey2Name) lock (AvatarName2Key) lock (PendingAvatarFinds_viauuid) lock (PendingAvatarFinds_vianame)
                {
                    if (AvatarStorageLastUsed.ContainsKey(av_name) == false)
                    {
                        if (av_uuid != UUID.Zero)
                        {
                            if ((AvatarKey2Name.ContainsKey(av_uuid) == false) && (AvatarName2Key.ContainsKey(av_name) == false))
                            {
                                AvatarName2Key.Add(av_name, av_uuid);
                                AvatarKey2Name.Add(av_uuid, av_name);
                            }
                            AvatarStorageLastUsed.Add(av_name, helpers.UnixTimeNow());
                            if (PendingAvatarFinds_vianame.ContainsKey(av_name) == true)
                            {
                                PendingAvatarFinds_vianame.Remove(av_name);
                            }
                            if (PendingAvatarFinds_viauuid.ContainsKey(av_uuid) == true)
                            {
                                PendingAvatarFinds_viauuid.Remove(av_uuid);
                            }
                        }
                    }
                    else
                    {
                        AvatarStorageLastUsed[av_name] = helpers.UnixTimeNow();
                    }
                }
                if (helpers.notempty(myconfig.Security_MasterUsername) == true)
                {
                    if(myconfig.Security_MasterUsername == av_name)
                    {
                        if (master_uuid == UUID.Zero)
                        {
                            master_uuid = av_uuid;
                            FoundMasterAvatar();
                        }

                    }
                }
            }
        }

        protected string FindAvatarUUIDInStorage(string avatar_name)
        {
            if (AvatarStorageLastUsed.ContainsKey(avatar_name) == true)
            {
                AvatarStorageLastUsed[avatar_name] = helpers.UnixTimeNow();
                return AvatarName2Key[avatar_name].ToString();
            }
            else if (FriendNames.ContainsValue(avatar_name) == true)
            {
                string returnvalue = "";
                foreach(UUID entry in FriendNames.Keys)
                {
                    if(FriendNames[entry] == avatar_name)
                    {
                        returnvalue = entry.ToString();
                        break;
                    }
                }
                return returnvalue;
            }
            return "";
        }

        public string FindAvatarName2Key(string avatar_name)
        {
            List<string> bits = avatar_name.Split(' ').ToList();
            if(bits.Count() == 1)
            {
                bits.Add("Resident");
            }
            avatar_name = bits[0].FirstCharToUpper() + " " + bits[1].FirstCharToUpper();
            string uuid = FindAvatarUUIDInStorage(avatar_name);
            if (uuid != "")
            {
                return uuid;
            }
            lock (PendingAvatarFinds_vianame)
            {
                if (PendingAvatarFinds_vianame.ContainsKey(avatar_name) == false)
                {
                    PendingAvatarFinds_vianame.Add(avatar_name, new KeyValuePair<long, int>(helpers.UnixTimeNow(), 0));
                    Client.Directory.StartPeopleSearch(avatar_name, 0);
                }
            }
            return "lookup";
        }


        protected string FindAvatarNameInStorage(UUID avatar_uuid)
        {
            if (AvatarKey2Name.ContainsKey(avatar_uuid) == true)
            {
                AvatarStorageLastUsed[AvatarKey2Name[avatar_uuid]] = helpers.UnixTimeNow();
                return AvatarKey2Name[avatar_uuid];
            }
            else if(FriendNames.ContainsKey(avatar_uuid) == true)
            {
                return FriendNames[avatar_uuid];
            }
            return "";
        }


        public string FindAvatarKey2Name(UUID avatar_uuid)
        {
            string avatar_name = FindAvatarNameInStorage(avatar_uuid);
            if (avatar_name != "")
            {
                return avatar_name;
            }
            lock (PendingAvatarFinds_viauuid)
            {
                if (PendingAvatarFinds_viauuid.ContainsKey(avatar_uuid) == false)
                {
                    LogFormater.Info("Looking up: " + avatar_uuid);
                    PendingAvatarFinds_viauuid.Add(avatar_uuid, new KeyValuePair<long, int>(helpers.UnixTimeNow(), 0));
                    Client.Avatars.RequestAvatarName(avatar_uuid);
                }
            }
            return "lookup";
        }

        protected virtual void FoundMasterAvatar()
        {

        }
    }

    public class friendreplyobject
    {
        public string name;
        public string id;
        public bool online;
    }
}
