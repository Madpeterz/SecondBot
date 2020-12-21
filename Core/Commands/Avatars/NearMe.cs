using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using Newtonsoft.Json;

namespace BSB.Commands.Avatars
{
    class NearMe : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Gets a list of avatars near the bot"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                Dictionary<string, string> collection = new Dictionary<string, string>();
                foreach(Avatar av in bot.GetClient.Network.CurrentSim.ObjectsAvatars.Copy().Values)
                {
                    Vector3 pos = new Vector3(1, 1, 1);
                    if(bot.GetClient.Network.CurrentSim.AvatarPositions.ContainsKey(av.ID) == true)
                    {
                        pos = bot.GetClient.Network.CurrentSim.AvatarPositions[av.ID];
                    }
                    Dictionary<string, string> entrys = new Dictionary<string, string>();
                    entrys.Add("Username",av.Name);
                    entrys.Add("Pos", JsonConvert.SerializeObject(pos));
                    collection.Add(av.ID.ToString(), JsonConvert.SerializeObject(entrys));
                }
                return bot.GetCommandsInterface.SmartCommandReply(true, args[0], "ok", CommandName, collection);
            }
            return false;
        }
    }
}
