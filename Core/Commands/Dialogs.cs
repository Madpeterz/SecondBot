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
        [ReturnHints("set/avatar [ok]")]
        [ReturnHints("set/http [ok]")]
        [ReturnHints("set/channel [ok]")]
        [ReturnHintsFailure("Not a vaild option")]
        [ArgHints("target","URLARG", "Options: Channel (Any number),Avatar UUID,HTTPurl<br/>Clear")]
        [Route(HttpVerbs.Get, "/DialogRelay/{target}/{token}")]
        public object DialogRelay(string target,string token)
        {
            if (tokens.Allow(token, "dialogs", "DialogRelay", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "DialogRelay", new [] { target });
            }
            if (target == "clear")
            {
                bot.SetRelayDialogsAvatar(UUID.Zero);
                bot.SetRelayDialogsChannel(0);
                bot.SetRelayDialogsHTTP("");
                return BasicReply("cleared", "DialogRelay", new [] { target });
            }
            ProcessAvatar(target);
            if (avataruuid != UUID.Zero)
            {
                bot.SetRelayDialogsAvatar(avataruuid);
                return BasicReply("set/avatar [ok]", "DialogRelay", new [] { target });
            }
            if (target.StartsWith("http") == true)
            {
                bot.SetRelayDialogsHTTP(target);
                return BasicReply("set/http [ok]", "DialogRelay", new [] { target });
            }
            if (int.TryParse(target, out int channel) == true)
            {
                bot.SetRelayDialogsChannel(channel);
                return BasicReply("set/channel [ok]", "DialogRelay", new [] { target });
            }
            return Failure("Not a vaild option", "DialogRelay", new [] { target });
            
        }

        [About("Makes the bot interact with the dialog [dialogid] with the button [buttontext]")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ReturnHintsFailure("bad dialog id")]
        [ArgHints("dialogid", "URLARG", "The ID for the dialog")]
        [ArgHints("buttontext", "URLARG", "The button text to push")]
        [Route(HttpVerbs.Get, "/DialogResponce/{dialogid}/{buttontext}/{token}")]
        public object DialogResponce(string dialogid, string buttontext, string token)
        {
            if (tokens.Allow(token, "dialogs", "DialogResponce", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "DialogResponce", new [] { dialogid, buttontext });
            }
            if (int.TryParse(dialogid, out int dialogidnum) == false)
            {
                return Failure("bad dialog id", "DialogResponce", new [] { dialogid, buttontext });
            }
            return BasicReply(bot.DialogReply(dialogidnum, buttontext).ToString(), "DialogResponce", new [] { dialogid, buttontext });
        }

        [About("Should the bot track dialogs and send them to the relays setup?")]
        [ReturnHints("updated")]
        [ReturnHintsFailure("bad status")]
        [ArgHints("status", "URLARG", "true or false")]
        [Route(HttpVerbs.Get, "/DialogTrack/{status}/{token}")]
        public object DialogTrack(string status, string token)
        {
            if (tokens.Allow(token, "dialogs", "DialogTrack", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "DialogTrack", new [] { status });
            }
            if (bool.TryParse(status, out bool statuscode) == false)
            {
                return Failure("bad status", "DialogTrack", new [] { status });
            }
            bot.SetTrackDialogs(statuscode);
            return BasicReply("updated", "DialogTrack", new [] { status });
        }

    }
}
