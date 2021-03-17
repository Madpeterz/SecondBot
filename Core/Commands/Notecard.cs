using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using System.Reflection;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Notecard : WebApiControllerWithTokens
    {
        public HTTP_Notecard(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Adds content to the Collection<br/> Also creates the Collection if it does not exist")]
        [ReturnHints("Collection value is empty")]
        [ReturnHints("Content value is empty")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Post, "/NotecardAdd/{collection}/{token}")]
        public object NotecardAdd(string collection, [FormField] string content, string token)
        {
            if (tokens.Allow(token, "notecard", "NotecardAdd", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "NotecardAdd", new string[] { collection, content });
            }
            if (helpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", "NotecardAdd", new string[] { collection, content });
            }
            if (helpers.notempty(content) == false)
            {
                return Failure("Content value is empty", "NotecardAdd", new string[] { collection, content });
            }
            bot.NotecardAddContent(collection, content);
            return BasicReply("ok", "NotecardAdd", new string[] { collection, content });
        }

        [About("Adds content to the Collection<br/> Also creates the Collection if it does not exist")]
        [ReturnHints("Collection value is empty")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/NotecardClear/{collection}/{token}")]
        public object NotecardClear(string collection, string token)
        {
            if (tokens.Allow(token, "notecard", "NotecardClear", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "NotecardClear", new string[] { collection });
            }
            if (helpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", "NotecardClear", new string[] { collection });
            }
            bot.ClearNotecardStorage(collection);
            return BasicReply("ok", "NotecardClear", new string[] { collection });
        }

        [About("Adds content to the Collection<br/> Also creates the Collection if it does not exist")]
        [ReturnHints("True|False")]
        [ReturnHints("Collection value is empty")]
        [ReturnHints("Notecardname value is empty")]
        [ReturnHints("Invaild avatar uuid")]
        [ReturnHints("No content in notecard storage ?")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/NotecardSend/{avatar}/{collection}/{notecardname}/{token}")]
        public object NotecardSend(string avatar, string collection, string notecardname, string token)
        {
            if (tokens.Allow(token, "notecard", "NotecardSend", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "NotecardSend", new string[] { avatar, collection, notecardname });
            }
            if (helpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", "NotecardSend", new string[] { avatar, collection, notecardname });
            }
            if (helpers.notempty(notecardname) == false)
            {
                return Failure("Notecardname value is empty", "NotecardSend", new string[] { avatar, collection, notecardname });
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", "NotecardSend", new string[] { avatar, collection, notecardname });
            }
            string content = bot.GetNotecardContent(collection);
            if (content == null)
            {
                return Failure("No content in notecard storage " + collection, "NotecardSend", new string[] { avatar, collection, notecardname });
            }
            bot.ClearNotecardStorage(collection);
            return BasicReply(bot.SendNotecard(notecardname, content, avataruuid).ToString(), "NotecardSend", new string[] { avatar, collection, notecardname });
        }
    }
}
