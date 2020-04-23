using BetterSecondBotShared.Static;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSB.bottypes
{
    public abstract class AVstorageBot : AtHome
    {
        protected Dictionary<string, long> AvatarStorageLastUsed = new Dictionary<string, long>();
        protected Dictionary<string, UUID> AvatarName2Key = new Dictionary<string, UUID>();
        protected Dictionary<UUID, string> AvatarKey2Name = new Dictionary<UUID, string>();
        protected Dictionary<string, KeyValuePair<long, int>> PendingAvatarFinds_vianame = new Dictionary<string, KeyValuePair<long, int>>();
        protected Dictionary<UUID, KeyValuePair<long, int>> PendingAvatarFinds_viauuid = new Dictionary<UUID, KeyValuePair<long, int>>();

        protected long max_wait_for_retry_name2key = 30; // How long to wait before we attempt a name to key lookup again for the same avatar (in secs)
        protected long max_wait_for_retry_key2name = 10; // How long to wait before we attempt a key to name lookup again for the same avatar (in secs)
        protected int max_retry_attempts = 2; // how many times should we attempt to get the avatar before giving up.
        protected long max_db_storage_age = 240; // how long should we keep an avatar in memory before clearing (in secs) [now - last accessed]
        protected bool show_av_storage_info_in_status = true;

        public override string GetStatus()
        {
            cleanupAvatarStorage();
            ReTriggerAvLookups();
            string reply = "";
            if (show_av_storage_info_in_status == true)
            {
                int count = PendingAvatarFinds_vianame.Count + PendingAvatarFinds_viauuid.Count;
                if (count > 0)
                {
                    reply = count.ToString() + " pending";
                }
                if (AvatarStorageLastUsed.Count > 0)
                {
                    if (reply != "") reply += " , ";
                    reply += AvatarStorageLastUsed.Count.ToString() + " loaded";
                }
                if (reply != "")
                {
                    reply = " (AVStorage: " + reply + ")";
                }
                else
                {
                    reply = " (AVStorage: Empty)";
                }
            }
            return base.GetStatus() + reply;
        }

        protected void ReTriggerUUIDtoName()
        {
            long now = helpers.UnixTimeNow();
            List<UUID> retrigger_key2name = new List<UUID>();
            List<UUID> giveup_key2name = new List<UUID>();
            foreach (KeyValuePair<UUID, KeyValuePair<long, int>> pair in PendingAvatarFinds_viauuid)
            {
                long dif = now - pair.Value.Key;
                if (dif > max_wait_for_retry_key2name)
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
            if(retrigger_key2name.Count > 0)
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

        protected void ReTriggerNameToUUID()
        {
            long now = helpers.UnixTimeNow();
            List<string> retrigger_name2key = new List<string>();
            List<string> giveup_name2key = new List<string>();
            foreach (KeyValuePair<string, KeyValuePair<long, int>> pair in PendingAvatarFinds_vianame)
            {
                long dif = now - pair.Value.Key;
                if (dif > max_wait_for_retry_key2name)
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
                Client.Directory.StartPeopleSearch(avname, 0);
                PendingAvatarFinds_vianame[avname] = new KeyValuePair<long, int>(now, PendingAvatarFinds_vianame[avname].Value + 1);
                
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
                    PurgeAvatars.Add(entry.Key);
                }
            }
            foreach(string purge in PurgeAvatars)
            {
                AvatarStorageLastUsed.Remove(purge);
                UUID avuuid = AvatarName2Key[purge];
                AvatarName2Key.Remove(purge);
                AvatarKey2Name.Remove(avuuid);
            }
        }

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                Client.Avatars.UUIDNameReply += Key2NameEvent;
                Client.Directory.DirPeopleReply += Name2KeyEvent;
            }
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
            if ((av_name != "null") && (av_name != null))
            {
                if (AvatarStorageLastUsed.ContainsKey(av_name) == false)
                {
                    AvatarName2Key.Add(av_name, av_uuid);
                    AvatarKey2Name.Add(av_uuid, av_name);
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
        }

        public string FindAvatarName2Key(string avatar_name)
        {
            List<string> bits = avatar_name.Split(' ').ToList();
            if(bits.Count() == 1)
            {
                bits.Add("Resident");
            }
            avatar_name = bits[0].FirstCharToUpper() + " " + bits[1].FirstCharToUpper();
            long now = helpers.UnixTimeNow();
            if (AvatarStorageLastUsed.ContainsKey(avatar_name) == true)
            {
                AvatarStorageLastUsed[avatar_name] = now;
                return AvatarName2Key[avatar_name].ToString();
            }
            else
            {
                if(PendingAvatarFinds_vianame.ContainsKey(avatar_name) == false)
                {
                    PendingAvatarFinds_vianame.Add(avatar_name, new KeyValuePair<long, int>(now, 0));
                    Client.Directory.StartPeopleSearch(avatar_name, 0);
                }
            }
            return "lookup";
        }
        public string FindAvatarKey2Name(UUID avatar_uuid)
        {
            long now = helpers.UnixTimeNow();
            if (AvatarKey2Name.ContainsKey(avatar_uuid) == true)
            {
                AvatarStorageLastUsed[AvatarKey2Name[avatar_uuid]] = now;
                return AvatarKey2Name[avatar_uuid];
            }
            else
            {
                if (PendingAvatarFinds_viauuid.ContainsKey(avatar_uuid) == false)
                {
                    PendingAvatarFinds_viauuid.Add(avatar_uuid, new KeyValuePair<long, int>(now, 0));
                    Client.Avatars.RequestAvatarName(avatar_uuid);
                }
                return "lookup";
            }
        }
    }
}
