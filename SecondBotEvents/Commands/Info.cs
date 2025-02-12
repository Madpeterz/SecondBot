using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    [ClassInfo("What is going on with the bot")]
    public class Info(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Lists objects that are sculpty type in the current sim that the bot can see")]
        [ReturnHints("A json object")]
        public object ListSculptys()
        {
            Dictionary<uint, Primitive> objects_copy = GetClient().Network.CurrentSim.ObjectsPrimitives.Copy();

            Dictionary<uint, uint> mapLocalID = [];
            Dictionary<uint, Primitive> sculpts = [];
            foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
            {
                mapLocalID.Add(Obj.Value.LocalID, Obj.Key);
                if (PrimType.Sculpt.Equals(Obj.Value.Type) == true)
                {
                    sculpts.Add(Obj.Key, Obj.Value);
                }
            }
            List<SculptysInfo> reply = [];
            foreach (KeyValuePair<uint, Primitive> Obj in sculpts)
            {
                Vector3 pos = Obj.Value.Position;
                string name = "?";
                if (Obj.Value.NameValues != null)
                {
                    name = Obj.Value.NameValues[0].Value.ToString();
                }
                UUID key = Obj.Value.ID;
                UUID owner = Obj.Value.OwnerID;
                bool asLink = false;
                if (Obj.Value.ParentID > 0)
                {
                    if(mapLocalID.ContainsKey(Obj.Value.ParentID) == true)
                    {
                        Primitive wip = objects_copy[mapLocalID[Obj.Value.ParentID]];
                        pos = wip.Position;
                        asLink = true;
                        owner = wip.OwnerID;
                        if (wip.NameValues != null)
                        {
                            name = wip.NameValues[0].Value.ToString();
                        }
                        key = wip.ID;
                    }
                }
                SculptysInfo A = new()
                {
                    isLinked = asLink.ToString(),
                    name = name.ToString(),
                    owner = owner.ToString(),
                    uuid = key.ToString(),
                    pos = pos.ToString()
                };
                reply.Add(A);
            }
            if (reply.Count == 0)
            {
                return Failure("No objects found");
            }
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Fetchs the current bot")]
        [ReturnHints("The build ID of the bot")]
        public object Version()
        {
            return BasicReply(master.GetVersion());

        }

        [About("Fetchs the name of the bot")]
        [ReturnHints("Firstname Lastname")]
        public object Name()
        {
            return BasicReply(GetClient().Self.FirstName + " " + GetClient().Self.LastName);
        }

        [About("Fetchs the current parcels name")]
        [ReturnHints("Parcelname")]
        [ReturnHintsFailure("Error parcel not found")]
        [ReturnHintsFailure("Error not in a sim")]
        public object ParcelName()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return Failure("Error not in a sim");
            }
            int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
            if (GetClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                return Failure("Error parcel not found");
            }
            return BasicReply(GetClient().Network.CurrentSim.Parcels[localid].Name);
        }

        [About("Requests the current unixtime at the bot")]
        [ReturnHints("Unixtime")]
        public object UnixTimeNow()
        {
            return BasicReply(SecondbotHelpers.UnixTimeNow().ToString());
        }

        [About("Fetchs the current region name")]
        [ReturnHints("Regionname")]
        [ReturnHintsFailure("Error not in a sim")]
        public object SimName()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return Failure("Error not in a sim");
            }
            return BasicReply(GetClient().Network.CurrentSim.Name);
        }

        [About("Fetchs the current location of the bot")]
        [ReturnHints("array of X,Y,Z values")]
        [ReturnHintsFailure("Error not in a sim")]
        public object GetPosition()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return Failure("Error not in a sim");
            }
            Dictionary<string, int> pos = new()
            {
                { "x", (int)Math.Round(GetClient().Self.SimPosition.X) },
                { "y", (int)Math.Round(GetClient().Self.SimPosition.Y) },
                { "z", (int)Math.Round(GetClient().Self.SimPosition.Z) }
            };
            return BasicReply(JsonConvert.SerializeObject(pos));
        }
    }

    public class SculptysInfo
    {
        public string name = "";
        public string isLinked = "False";
        public string owner = "?";
        public string uuid = "?";
        public string pos = "<0,0,0>";

    }
}
