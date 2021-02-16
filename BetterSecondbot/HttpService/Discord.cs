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
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Discord : WebApiControllerWithTokens
    {
        public HTTP_Discord(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Adds a discord server role to the selected member")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ArgHints("serverid","URLARG","the server id to apply this action to")]
        [ArgHints("roleid", "URLARG", "the role id we are giving")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [Route(HttpVerbs.Get, "/Discord_AddRole/{serverid}/{roleid}/{memberid}/{token}")]
        public object Discord_AddRole(string serverid,string roleid,string memberid,string token)
        {
            if (tokens.Allow(token, "discord", "Discord_AddRole", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return BasicReply("Discord client not ready");
            }
            return BasicReply(addRoleToMember(serverid, roleid, memberid).Result.ToString());
        }

        [About("Adds the selected user to the ban list - Disallows rejoining untill they are removed from the list")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [ArgHints("why", "string", "why they are being banned")]
        [Route(HttpVerbs.Get, "/Discord_BanMember/{serverid}/{memberid}/{token}")]
        public object Discord_BanMember(string serverid, string memberid, [FormField] string why, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_BanMember", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return BasicReply("Discord client not ready");
            }
            return BasicReply(BanMember(serverid, memberid, why).Result.ToString());
        }


        protected async Task<bool> BanMember(string givenserverid, string givenmemberid, string why)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketGuildUser user = server.GetUser(memberid);
            await user.BanAsync(7, why);
            return true;
        }


        protected async Task<bool> addRoleToMember(string givenserverid, string givenroleid, string givenmemberid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            SocketGuildUser user = server.GetUser(memberid);
            await user.AddRoleAsync(role); // ? Irole seems to accept SocketRole ?
            return true;
        }
    }
}
