using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    public class Estate : CommandsAPI
    {
        public Estate(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Sends the message to the current sim")]
        [ReturnHints("restarting")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("canceled")]
        [ArgHints("delay", "How long to delay the restart for (30 to 240 secs) - defaults to 240 if out of bounds \n" +
            "set to 0 if your canceling!")]
        [ArgHints("mode", "true to start a restart, false to cancel")]
        public object SimRestart(string delay, string mode)
        {
            if (getClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", new [] { delay, mode });
            }
            bool.TryParse(mode, out bool modeflag);
            if (modeflag == false)
            {
                getClient().Estate.CancelRestart();
                return BasicReply("canceled", new [] { delay, mode });
            }
            int delay_restart = 60;
            int.TryParse(delay, out delay_restart);
            if((delay_restart < 30) || (delay_restart > 240))
            {
                delay_restart = 240;
            }
            getClient().Estate.RestartRegion(delay_restart);
            return BasicReply("restarting", new [] { delay, mode });
        }

        [About("Sends the message to the current sim")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHints("ok")]
        [ArgHints("message", "What the message is")]
        public object SimMessage(string message, string token)
        {
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", new [] { message });
            }
            if (getClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", new [] { message });
            }
            getClient().Estate.SimulatorMessage(message);
            return BasicReply("ok", new [] { message });
        }

        [About("Fetchs the regions map tile")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("Texture UUID")]
        [ArgHints("regionname", "the name of the region we are fetching")]
        public object GetSimTexture(string regionname)
        {
            if (getClient().Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", new [] { regionname });
            }
            return BasicReply(region.MapImageID.ToString(), new [] { regionname });
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("ok")]
        public object EstateParcelReclaim()
        {
            if (getClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            int localid = getClient().Parcels.GetParcelLocalID(getClient().Network.CurrentSim, getClient().Self.SimPosition);
            getClient().Parcels.Reclaim(getClient().Network.CurrentSim, localid);
            return BasicReply("ok");
        }

        [About("Gets the global location of a sim [region]")]
        [ArgHints("regionname", "the region we want")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("a json object with the x,y and region name")]
        public object GetSimGlobalPos(string regionname)
        {
            if (getClient().Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", new [] { regionname });
            }
            Dictionary<string, string> reply = new Dictionary<string, string>();
            reply.Add("region", regionname);
            reply.Add("X", region.X.ToString());
            reply.Add("Y", region.Y.ToString());
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Requests the estate banlist")]
        [ReturnHints("ban list json")]
        public object GetEstateBanList()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetEstateBans()));
        }

        [About("Attempts to add/remove the avatar to/from the Estate banlist")]
        [ReturnHints("Unban request accepted")]
        [ReturnHints("Ban request accepted")]
        [ReturnHintsFailure("Unable to find avatar UUID")]
        [ReturnHintsFailure("Unable to process global value please use true or false")]
        [ReturnHintsFailure("Not an estate manager on region {REGIONNAME}")]
        [ArgHints("avatar", "the uuid avatar you wish to ban")]
        [ArgHints("mode", "What action would you like to take<br/>Defaults to remove if not given \"add\"")]
        [ArgHints("global", "if true this the ban/unban will be applyed to all estates the bot has access to")]

        public object UpdateEstateBanlist(string avatar, string mode, string global)
        {
            if (getClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager on region " + getClient().Network.CurrentSim.Name, new [] { avatar, mode, global });
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID", new [] { avatar, mode, global });
            }
            bool globalban = false;
            if (bool.TryParse(global, out globalban) == false)
            {
                return Failure("Unable to process global value please use true or false", new [] { avatar, mode, global });
            }
            if (mode != "add")
            {
                getClient().Estate.UnbanUser(avataruuid, globalban);
                return BasicReply("Unban request accepted", new [] { avatar, mode, global });
            }
            getClient().Estate.BanUser(avataruuid, globalban);
            return BasicReply("Ban request accepted", new [] { avatar, mode, global });
        }
    }
}
