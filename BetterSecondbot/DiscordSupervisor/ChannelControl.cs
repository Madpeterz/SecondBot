using Discord;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordChannelControl : DiscordFunctions
    {

        protected async Task<IUserMessage> SendMessageToChannelAsync(string channelname, string message, string catmapid, UUID sender_id, string TopicType)
        {
            if (AllowNewOutbound() == true)
            {
                try
                {
                    channelname = channelname.ToLowerInvariant();
                    channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
                    ITextChannel Channel = await FindTextChannel(channelname, catmap[catmapid], sender_id, TopicType, false).ConfigureAwait(true);
                    if (Channel != null)
                    {
                        return await Channel.SendMessageAsync(message).ConfigureAwait(false);
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        protected async Task<ITextChannel> FindTextChannel(string channelname, ICategoryChannel cat, UUID sender, string TopicType, bool CleanChannel)
        {
            return await FindTextChannel(DiscordServer, channelname, cat, sender, TopicType, true, CleanChannel);
        }


        protected async Task<ITextChannel> FindTextChannel(IGuild onserver, string channelname)
        {
            return await FindTextChannel(onserver, channelname, null, UUID.Zero, "notUSED");
        }

        protected async Task<ITextChannel> FindTextChannel(IGuild onserver, string channelname, ICategoryChannel cat)
        {
            return await FindTextChannel(onserver, channelname, cat, UUID.Zero, "notUSED", true);
        }

        protected async Task<ITextChannel> FindTextChannel(IGuild onserver, string channelname, ICategoryChannel cat, UUID sender, string TopicType)
        {
            return await FindTextChannel(onserver, channelname, cat, sender, TopicType, true);
        }

        protected async Task<ITextChannel> FindTextChannel(IGuild onserver, string channelname, ICategoryChannel cat, UUID sender, string TopicType, bool create_channel)
        {
            return await FindTextChannel(onserver, channelname, cat, sender, TopicType, create_channel, true);
        }

        protected async Task<ITextChannel> FindTextChannel(IGuild onserver,string channelname, ICategoryChannel cat, UUID sender, string TopicType,bool create_channel, bool CleanChannel)
        {
            await WaitForUnlock().ConfigureAwait(false);
            channelname = channelname.ToLowerInvariant();
            channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
            DiscordLock = true;
            IReadOnlyCollection<ITextChannel> found_chans = await onserver.GetTextChannelsAsync(CacheMode.AllowDownload);
            ITextChannel result = null;
            foreach (ITextChannel ITC in found_chans)
            {
                if (create_channel == false)
                {
                    if (ITC.Name == channelname)
                    {
                        result = ITC;
                        break;
                    }
                }
                else
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
            }
            if (create_channel == true)
            {
                if (result == null)
                {
                    result = await CreateChannel(onserver, channelname, TopicType, sender.ToString()).ConfigureAwait(false);
                }
                else
                {
                    if (CleanChannel == true)
                    {
                        await CleanDiscordChannel(result, 24).ConfigureAwait(false);
                    }
                }
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
            long nowunix = DateTimeOffset.Now.ToUnixTimeSeconds();
            IEnumerable<IMessage> messages;
            bool empty = false;
            while (empty == false)
            {
                empty = true;
                messages = await chan.GetMessagesAsync(50).FlattenAsync();
                List<ulong> deleteMessages = new List<ulong>();
                List<IMessage> slowDeleteMessages = new List<IMessage>();
                
                foreach (IMessage mess in messages)
                {
                    long messageunix = mess.Timestamp.ToUnixTimeSeconds();
                    long dif = nowunix - messageunix;
                    var hours = (dif / 60)/60;
                    bool slowdel = false;
                    
                    if (hours > (24 * 13))
                    {
                        slowdel = true;
                    }
                    if ((hours > HistoryHours) || (forceempty == true))
                    {
                        empty = false;
                        if (slowdel == false)
                        {
                            deleteMessages.Add(mess.Id);
                        }
                        else
                        {
                            slowDeleteMessages.Add(mess);
                        }
                    }
                }
                if ((deleteMessages.Count > 0) || (slowDeleteMessages.Count > 0))
                {
                    try
                    {
                        if (deleteMessages.Count > 0)
                        {
                            await chan.DeleteMessagesAsync(deleteMessages).ConfigureAwait(true);
                        }
                        if (slowDeleteMessages.Count > 0)
                        {
                            foreach (IMessage slowdelmessage in slowDeleteMessages)
                            {
                                await chan.DeleteMessageAsync(slowdelmessage).ConfigureAwait(true);
                            }
                        }
                    }
                    catch
                    {

                    }
                    empty = false;
                }
            }
        }

        protected async Task<ITextChannel> CreateChannel(IGuild onserver, string channelname, string channeltopictype, string sender_id)
        {
            channelname = channelname.ToLowerInvariant();
            channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
            string display_topic = "" + channeltopictype + ":" + sender_id + "";
            if (channelname == "interface")
            {
                display_topic = "Actions -> !clear, !commands";
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
                if (controler.getBot() != null)
                {
                    display_topic = "" + myconfig.Basic_BotUserName + " #" + controler.getBot().MyVersion + "";
                }
            }
            else if (channeltopictype == "Group")
            {
                display_topic = "" + channeltopictype + ":" + sender_id + ": Actions -> !clear, !notice title|||message";
            }
            else if (channeltopictype == "IM")
            {
                display_topic = "" + channeltopictype + ":" + sender_id + ": Actions -> !clear, !close";
            }
            try
            {
                IGuildChannel channel = await onserver.CreateTextChannelAsync(channelname, X => DiscordGetNewChannelProperies(X, channelname, display_topic, channeltopictype.ToLowerInvariant()));
                ITextChannel Txtchan = await onserver.GetTextChannelAsync(channel.Id);
                return Txtchan;
            }
            catch
            {
                return null;
            }
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
            if (DiscordServer != null)
            {
                List<string> required_cats = new List<string>() { "bot", "group", "im" };
                IReadOnlyCollection<ICategoryChannel> found_cats = await DiscordServer.GetCategoriesAsync(CacheMode.AllowDownload);
                foreach (ICategoryChannel fcat in found_cats)
                {
                    if (catmap.ContainsKey(fcat.Name) == false)
                    {
                        if (required_cats.Contains(fcat.Name) == true)
                        {
                            required_cats.Remove(fcat.Name);
                            catmap.Add(fcat.Name, fcat);
                        }
                    }
                    else
                    {
                        required_cats.Remove(fcat.Name);
                    }
                }
                foreach (string A in required_cats)
                {
                    ICategoryChannel newcat = await DiscordServer.CreateCategoryAsync(A).ConfigureAwait(true);
                    catmap.Add(A, newcat);
                }
                List<string> required_channels = new List<string>() { "status", "interface", "localchat" };
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
                        if (GroupChannels.Contains(chan.Name) == false)
                        {
                            if (chan.CategoryId == catmap["group"].Id)
                            {
                                GroupChannels.Add(chan.Name);
                            }
                        }
                    }
                }
                foreach (string A in required_channels)
                {
                    _ = await FindTextChannel(A, catmap["bot"], UUID.Zero, "bot", true).ConfigureAwait(false);
                }

                if (HasBot() == true)
                {
                    foreach (Group G in controler.getBot().MyGroups.Values)
                    {
                        string groupname = G.Name.ToLowerInvariant();
                        groupname = String.Concat(groupname.Where(char.IsLetterOrDigit));
                        if (GroupChannels.Contains(groupname) == false)
                        {
                            _ = await FindTextChannel(groupname, catmap["group"], G.ID, "Group", !myconfig.DiscordFull_Keep_GroupChat).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}
