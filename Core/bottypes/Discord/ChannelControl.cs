using Discord;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSB.bottypes
{
    public abstract class DiscordBotChannelControl : DiscordBotFunctions
    {

        protected async Task SendMessageToChannelAsync(string channelname, string message, string catmapid, UUID sender_id, string TopicType)
        {
            if (AllowNewOutbound() == true)
            {
                channelname = channelname.ToLowerInvariant();
                channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
                ITextChannel Channel = await FindTextChannel(channelname, catmap[catmapid], sender_id, TopicType).ConfigureAwait(false);
                if (Channel != null)
                {
                    await Channel.SendMessageAsync(message);
                }
            }
        }

        protected async Task<ITextChannel> FindTextChannel(string channelname, ICategoryChannel cat, UUID sender, string TopicType)
        {
            await WaitForUnlock().ConfigureAwait(false);
            channelname = channelname.ToLowerInvariant();
            channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
            DiscordLock = true;
            IReadOnlyCollection<ITextChannel> found_chans = await DiscordServer.GetTextChannelsAsync(CacheMode.AllowDownload);
            ITextChannel result = null;
            foreach (ITextChannel ITC in found_chans)
            {
                if (ITC.CategoryId == cat.Id)
                {
                    if (ITC.Name == channelname)
                    {
                        result = ITC;
                        break;
                    }
                }
            }
            if (result == null)
            {
                result = await CreateChannel(channelname, TopicType, sender.ToString()).ConfigureAwait(false);
            }
            else
            {
                await CleanDiscordChannel(result, 24).ConfigureAwait(false);
            }
            DiscordLock = false;
            return result;
        }

        protected async static Task CleanDiscordChannel(ITextChannel chan)
        {
            await CleanDiscordChannel(chan, 48).ConfigureAwait(false);
        }
        protected async static Task CleanDiscordChannel(ITextChannel chan, int HistoryHours)
        {
            await CleanDiscordChannel(chan, HistoryHours, false).ConfigureAwait(false);
        }

        protected async static Task CleanDiscordChannel(ITextChannel chan, int HistoryHours, bool forceempty)
        {
            DateTimeOffset Now = new DateTimeOffset(new DateTime());
            IEnumerable<IMessage> messages;
            bool empty = false;
            while (empty == false)
            {
                empty = true;
                messages = await chan.GetMessagesAsync(50).FlattenAsync();
                List<ulong> deleteMessages = new List<ulong>();
                foreach (IMessage mess in messages)
                {
                    var hours = ((Now.ToUnixTimeSeconds() - mess.Timestamp.ToUnixTimeSeconds()) / 60) / 60;
                    if ((hours > HistoryHours) || (forceempty == true))
                    {
                        empty = false;
                        deleteMessages.Add(mess.Id);
                    }
                }
                if (deleteMessages.Count > 0)
                {
                    await chan.DeleteMessagesAsync(deleteMessages);
                }
            }
        }

        protected async Task<ITextChannel> CreateChannel(string channelname, string channeltopictype, string sender_id)
        {
            channelname = channelname.ToLowerInvariant();
            channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
            string display_topic = "" + channeltopictype + ":" + sender_id + "";
            if (channelname == "interface")
            {
                display_topic = "Actions -> !clear, !commands, !help command or command";
            }
            else if (channelname == "status")
            {
                display_topic = "Actions -> !clear";
            }
            else if (channelname == "localchat")
            {
                display_topic = "Actions -> !clear";
            }
            else if (sender_id == UUID.Zero.ToString())
            {
                display_topic = "" + myconfig.Basic_BotUserName + " #" + MyVersion + "";
            }
            else if (channeltopictype == "Group")
            {
                display_topic = "" + channeltopictype + ":" + sender_id + ": Actions -> !clear, !notice title|||message";
            }
            else if (channeltopictype == "IM")
            {
                display_topic = "" + channeltopictype + ":" + sender_id + ": Actions -> !clear, !close";
            }
            IGuildChannel channel = await DiscordServer.CreateTextChannelAsync(channelname, X => DiscordGetNewChannelProperies(X, channelname, display_topic, channeltopictype.ToLowerInvariant()));
            ITextChannel Txtchan = await DiscordServer.GetTextChannelAsync(channel.Id);
            return Txtchan;
        }

        protected void DiscordGetNewChannelProperies(TextChannelProperties C, string channelname, string channeltopic, string catname)
        {
            if (catname != null)
            {
                if (catmap.ContainsKey(catname) == true)
                {
                    C.CategoryId = catmap[catname].Id;
                }
            }
            C.Name = channelname;
            C.Topic = channeltopic;
        }

        protected async Task DiscordRebuildChannels()
        {
            List<string> required_cats = new List<string>() { "bot", "group", "im" };
            IReadOnlyCollection<ICategoryChannel> found_cats = await DiscordServer.GetCategoriesAsync(CacheMode.AllowDownload);
            foreach (ICategoryChannel fcat in found_cats)
            {
                if (required_cats.Contains(fcat.Name) == true)
                {
                    required_cats.Remove(fcat.Name);
                    catmap.Add(fcat.Name, fcat);
                }
            }
            foreach (string A in required_cats)
            {
                ICategoryChannel newcat = await DiscordServer.CreateCategoryAsync(A).ConfigureAwait(true);
                catmap.Add(A, newcat);
            }
            List<string> required_channels = new List<string>() { "status", "interface","localchat" };
            IReadOnlyCollection<ITextChannel> found_chans = await DiscordServer.GetTextChannelsAsync(CacheMode.AllowDownload);
            List<string> GroupChannels = new List<string>();
            foreach (ITextChannel chan in found_chans)
            {
                if (chan.CategoryId == catmap["bot"].Id)
                {
                    required_channels.Remove(chan.Name);
                }
                else
                {
                    if (chan.CategoryId == catmap["group"].Id)
                    {
                        GroupChannels.Add(chan.Name);
                    }
                }
            }
            foreach (string A in required_channels)
            {
                _ = await FindTextChannel(A, catmap["bot"], UUID.Zero, "bot").ConfigureAwait(false);
            }
            foreach (Group G in mygroups.Values)
            {
                string groupname = G.Name.ToLowerInvariant();
                groupname = String.Concat(groupname.Where(char.IsLetterOrDigit));
                if (GroupChannels.Contains(groupname) == false)
                {
                    _ = await FindTextChannel(groupname, catmap["group"], G.ID, "Group").ConfigureAwait(false);
                }
            }
        }
    }
}
