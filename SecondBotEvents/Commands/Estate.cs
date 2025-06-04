using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Look after a sim as the estate manager")]
    public class Estate(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Sends the message to the current sim")]
        [ReturnHints("restarting")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("canceled")]
        [ArgHints("delay", "How long to delay the restart for (30 to 240 secs) - defaults to 240 if out of bounds \n" +
            "set to 0 if your canceling!", "Number", "60")]
        [ArgHints("mode", "true to start a restart, false to cancel", "BOOL")]
        public object SimRestart(string delay, string mode)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [delay, mode]);
            }
            bool.TryParse(mode, out bool modeflag);
            if (modeflag == false)
            {
                GetClient().Estate.CancelRestart();
                return BasicReply("canceled", [delay, mode]);
            }
            int.TryParse(delay, out int delay_restart);
            if ((delay_restart < 30) || (delay_restart > 240))
            {
                delay_restart = 240;
            }
            GetClient().Estate.RestartRegion(delay_restart);
            return BasicReply("restarting", [delay, mode]);
        }

        [About("Sends the message to the current sim")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHints("ok")]
        [ArgHints("message", "What the message is", "Text", "Hi everyone I need to restart this sim")]
        public object SimMessage(string message)
        {
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", [message]);
            }
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [message]);
            }
            GetClient().Estate.SimulatorMessage(message);
            return BasicReply("ok", [message]);
        }

        [About("Fetchs the regions map tile")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("Texture UUID")]
        [ArgHints("regionname", "the name of the region we are fetching", "Text", "Lostworld")]
        public object GetSimTexture(string regionname)
        {
            if (GetClient().Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", [regionname]);
            }
            return BasicReply(region.MapImageID.ToString(), [regionname]);
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("ok")]
        public object EstateParcelReclaim()
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
            GetClient().Parcels.Reclaim(GetClient().Network.CurrentSim, localid);
            return BasicReply("ok");
        }

        [About("Gets the global location of a sim [region]")]
        [ArgHints("regionname", "the region we want", "Text", "Lostworld")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("a json object with the x,y and region name")]
        public object GetSimGlobalPos(string regionname)
        {
            if (GetClient().Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", [regionname]);
            }
            Dictionary<string, string> reply = new()
            {
                { "region", regionname },
                { "X", region.X.ToString() },
                { "Y", region.Y.ToString() }
            };
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
        [ArgHints("avatar", "avatar you wish to ban", "AVATAR")]
        [ArgHints("mode", "What action would you like to take<br/>Defaults to remove if not given \"add\"", "Text", "add", new string[] {"add","remove"})]
        [ArgHints("global", "if true this the ban/unban will be applyed to all estates the bot has access to", "BOOL")]

        public object UpdateEstateBanlist(string avatar, string mode, string global)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager on region " + GetClient().Network.CurrentSim.Name, [avatar, mode, global]);
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID", [avatar, mode, global]);
            }
            if (bool.TryParse(global, out bool globalban) == false)
            {
                return Failure("Unable to process global value please use true or false", [avatar, mode, global]);
            }
            if (mode != "add")
            {
                GetClient().Estate.UnbanUser(avataruuid, globalban);
                return BasicReply("Unban request accepted", [avatar, mode, global]);
            }
            GetClient().Estate.BanUser(avataruuid, globalban);
            return BasicReply("Ban request accepted", [avatar, mode, global]);
        }
    }
}
