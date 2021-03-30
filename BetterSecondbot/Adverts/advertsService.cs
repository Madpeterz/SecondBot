using BetterSecondBot;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using Newtonsoft.Json;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondbot.Adverts
{


    public class advertsService
    {
        public advertsService(Cli Master, bool LoadFromDocker)
        {
            controler = Master;
            if(LoadFromDocker == true)
            {
                loadFromDockerEnv();
            }
            else
            {
                loadFromDisk();
            }
        }

        protected void loadFromDisk()
        {
            advertsBlob loadedAdverts = new advertsBlob();
            advertConfig demoAdvert = new advertConfig();
            demoAdvert.attachment = UUID.Zero.ToString();
            demoAdvert.content = "Content";
            demoAdvert.title = "Title";
            demoAdvert.groups = new[] { UUID.Zero.ToString(), UUID.Zero.ToString() };
            demoAdvert.days = "0,1,2,3,4,5,6";
            demoAdvert.hour = "12";
            demoAdvert.min = "30";
            demoAdvert.notice = "false";
            demoAdvert.enabled = "true";
            loadedAdverts.adverts = new advertConfig[] { demoAdvert };
            

            string targetfile = "adverts.json";
            SimpleIO io = new SimpleIO();
            if (SimpleIO.FileType(targetfile, "json") == false)
            {
                io.WriteJsonAdverts(loadedAdverts, targetfile);
                return;
            }
            if (io.Exists(targetfile) == false)
            {
                io.WriteJsonAdverts(loadedAdverts, targetfile);
                return;
            }
            string json = io.ReadFile(targetfile);
            if (json.Length > 0)
            {
                try
                {
                    loadedAdverts = JsonConvert.DeserializeObject<advertsBlob>(json);
                    foreach (advertConfig loaded in loadedAdverts.adverts)
                    {
                        Unpack(loaded);
                    }
                }
                catch
                {
                    io.makeOld(targetfile);
                    io.WriteJsonAdverts(loadedAdverts, targetfile);
                }
                return;
            }
        }

        protected void Unpack(advertConfig config)
        {
            titles.Add(config.title);
            UUID attachment = UUID.Zero;
            UUID.TryParse(config.attachment, out attachment);
            attachments.Add(attachment);
            content.Add(config.content);
            bool asGroupNotice = false;
            bool.TryParse(config.notice, out asGroupNotice);
            asNotice.Add(asGroupNotice);
            string[] unpackDays = config.days.Split(',');
            List<int> activeondays = new List<int>();
            foreach(string Uday in unpackDays)
            {
                if(int.TryParse(Uday,out int dayid) == true)
                {
                    activeondays.Add(dayid);
                }
            }
            activeDays.Add(activeondays);
            int activeHourid = -2;
            int.TryParse(config.hour, out activeHourid);
            activeHours.Add(activeHourid);
            int activeMinid = -2;
            int.TryParse(config.min, out activeMinid);
            activeMins.Add(activeMinid);
            bool asEnabled = false;
            bool.TryParse(config.enabled, out asEnabled);
            enabled.Add(asEnabled);
            List<UUID> selectedGroups = new List<UUID>();
            foreach(string groupA in config.groups)
            {
                UUID groupUUID = UUID.Zero;
                UUID.TryParse(groupA, out groupUUID);
                if(groupUUID != UUID.Zero)
                {
                    selectedGroups.Add(groupUUID);
                }
            }
            groups.Add(selectedGroups);
        }

        protected void loadFromDockerEnv()
        {
            int loop = 1;
            bool found = true;
            while (found == true)
            {
                string title = getEnv("advert_" + loop.ToString() + "_title");
                if (helpers.notempty(title) == true)
                {
                    advertConfig config = new advertConfig();
                    config.title = title;
                    config.attachment = getEnv("advert_" + loop.ToString() + "_attachment");
                    config.content = getEnv("advert_" + loop.ToString() + "_content");
                    config.notice = getEnv("advert_" + loop.ToString() + "notice");
                    config.days = getEnv("advert_" + loop.ToString() + "days");
                    config.hour = getEnv("advert_" + loop.ToString() + "hour");
                    config.min = getEnv("advert_" + loop.ToString() + "min");
                    List<string> groupentrys = new List<string>();
                    int loop2 = 1;
                    bool exitNow = false;
                    while(exitNow == false)
                    {
                        exitNow = true;
                        string value = getEnv("advert_" + loop.ToString() + "_group_" + loop2.ToString());
                        if (helpers.notempty(value) == true)
                        {
                            groupentrys.Add(value);
                            exitNow = false;
                        }
                        loop2++;
                    }
                    config.groups = groupentrys.ToArray();
                    Unpack(config);
                }
                else
                {
                    found = false;
                }
                loop++;
            }
        }

        protected string getEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        protected List<string> titles = new List<string>();
        protected List<UUID> attachments = new List<UUID>();
        protected List<string> content = new List<string>();
        protected List<bool> asNotice = new List<bool>();
        protected List<List<int>> activeDays = new List<List<int>>();
        protected List<int> activeHours = new List<int>();
        protected List<int> activeMins = new List<int>();
        protected List<bool> enabled = new List<bool>();
        protected List<List<UUID>> groups = new List<List<UUID>>();
        protected Cli controler = null;

        protected int lastTickMin = -1;
        public void Tick()
        {
            DateTime moment = DateTime.Now;
            if (lastTickMin != moment.Minute)
            {
                lastTickMin = moment.Minute;
                checkForWork();
            }
        }

        protected void checkForWork()
        {
            DateTime moment = DateTime.Now;
            int dayofweek = (int)(moment.DayOfWeek + 6) % 7;
            int loop = 0;
            while (loop < titles.Count)
            {
                if (enabled[loop] == false)
                {
                    continue;
                }
                if (activeDays[loop].Contains(dayofweek) == false)
                {
                    continue;
                }
                if ((activeHours[loop] != moment.Hour) && (activeHours[loop] != -1))
                {
                    continue;
                }
                if ((activeHours[loop] != moment.Minute) && (activeHours[loop] != -1))
                {
                    continue;
                }
                if (activeHours[loop] == -1)
                {
                    if ((moment.Minute != 0) && (moment.Minute != 30))
                    {
                        continue;
                    }
                }
                TriggerAdvert(loop);
                loop++;
            }
        }

        protected void TriggerAdvert(int advertID)
        {

            foreach (UUID group in groups[advertID])
            {
                if (asNotice[advertID] == true)
                {
                    if (attachments[advertID] != UUID.Zero)
                    {
                        controler.getBot().CallAPI(
                            "GroupnoticeWithAttachment",
                            new[] { group.ToString(), titles[advertID], content[advertID], attachments[advertID].ToString() }
                        );
                    }
                    else
                    {
                        controler.getBot().CallAPI(
                            "Groupnotice",
                            new[] { group.ToString(), titles[advertID], content[advertID] }
                        );
                    }
                }
                else
                {
                    controler.getBot().CallAPI(
                        "Groupchat",
                        new[] { group.ToString(), content[advertID] }
                    );
                }
            }
        }
    }
}
