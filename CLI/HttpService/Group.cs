using BSB.bottypes;
using BSB.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;

namespace BetterSecondBot.HttpService
{
    public class SecondbotGroup : WebApiControllerWithTokens
    {
        public SecondbotGroup(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [Route(HttpVerbs.Get, "/listgroups/{token}")]
        public object listgroups(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                Dictionary<string, string> grouppackage = new Dictionary<string, string>();
                foreach(KeyValuePair<UUID,Group> entry in bot.MyGroups)
                {
                    grouppackage.Add(entry.Value.ID.ToString(), entry.Value.Name);
                }
                return BasicReply(JsonConvert.SerializeObject(grouppackage));
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/listgroupswithunread/{token}")]
        public object listgroupswithunread(string group, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot.UnreadGroupchatGroups()));
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/haveunreadgroupchat/{token}")]
        public object haveunreadgroupchat(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(bot.HasUnreadGroupchats().ToString());
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/getgroupchat/{group}/{token}")]
        public object getgroupchat(string group, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(group, out UUID groupUUID) == true)
                {
                    return BasicReply(JsonConvert.SerializeObject(bot.GetGroupchat(groupUUID)));
                }
                return BasicReply("Group UUID invaild");
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Post, "/sendgroupchat/{group}/{token}")]
        public object sendgroupchat(string group, string token, [FormField] string message)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(group, out UUID groupUUID) == true)
                {
                    bool status = bot.GetCommandsInterface.Call("Groupchat", group + "~#~" + message, UUID.Zero, "~#~");
                    return BasicReply("Processing");
                }
                return BasicReply("Group UUID invaild");
            }
            return BasicReply("Token not accepted");
        }


    }
}
