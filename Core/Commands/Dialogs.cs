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

namespace BetterSecondBot.HttpService
{
    public class HTTP_Dialogs : WebApiControllerWithTokens
    {
        public HTTP_Dialogs(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Updates the relay target (you can have 1 of each type)<br/>Clear will disable them all")]
        [ReturnHints("cleared")]
        [ReturnHints("set/avatar")]
        [ReturnHints("set/http")]
        [ReturnHints("set/channel")]
        [ReturnHints("Not a vaild option")]
        [ArgHints("target","URLARG", "Options: Channel (Any number),Avatar UUID,HTTPurl<br/>Clear")]
        [Route(HttpVerbs.Get, "/DialogRelay/{target}/{token}")]
        public object DialogRelay(string target,string token)
        {
            if (tokens.Allow(token, "dialogs", "DialogRelay", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (target == "clear")
            {
                bot.SetRelayDialogsAvatar(UUID.Zero);
                bot.SetRelayDialogsChannel(0);
                bot.SetRelayDialogsHTTP("");
                return BasicReply("cleared");
            }
            ProcessAvatar(target);
            if (avataruuid != UUID.Zero)
            {
                bot.SetRelayDialogsAvatar(avataruuid);
                return BasicReply("set/avatar");
            }
            if (target.StartsWith("http") == true)
            {
                bot.SetRelayDialogsHTTP(target);
                return BasicReply("set/http");
            }
            if (int.TryParse(target, out int channel) == true)
            {
                bot.SetRelayDialogsChannel(channel);
                return BasicReply("set/channel");
            }
            return BasicReply("Not a vaild option");
            
        }

        [About("Makes the bot interact with the dialog [dialogid] with the button [buttontext]")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ReturnHints("bad dialog id")]
        [ArgHints("target", "URLARG", "Options: Channel (Any number),Avatar UUID,HTTPurl<br/>Clear")]
        [Route(HttpVerbs.Get, "/DialogResponce/{dialogid}/{buttontext}/{token}")]
        public object DialogResponce(string dialogid, string buttontext, string token)
        {
            if (tokens.Allow(token, "dialogs", "DialogResponce", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (int.TryParse(dialogid, out int dialogidnum) == false)
            {
                return BasicReply("bad dialog id");
            }
            return BasicReply(bot.DialogReply(dialogidnum, buttontext).ToString());
        }

        [About("Should the bot track dialogs and send them to the relays setup?")]
        [ReturnHints("updated")]
        [ReturnHints("bad status")]
        [ArgHints("status", "URLARG", "true or false")]
        [Route(HttpVerbs.Get, "/DialogTrack/{status}/{token}")]
        public object DialogTrack(string status, string token)
        {
            if (tokens.Allow(token, "dialogs", "DialogTrack", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (bool.TryParse(status, out bool statuscode) == false)
            {
                return BasicReply("bad status");
            }
            bot.SetTrackDialogs(statuscode);
            return BasicReply("updated");
        }

    }
}
