using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class ClickObject : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Object UUID" }; } }
        public override string Helpfile { get { return "Makes the bot attempt to click a object"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID ObjectUUID) == true)
                {
                    Dictionary<uint, Primitive> objectsentrys = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();
                    bool found_object = false;
                    foreach (KeyValuePair<uint,Primitive> entry in objectsentrys)
                    {
                        if(entry.Value.ID == ObjectUUID)
                        {
                            bot.GetClient.Objects.ClickObject(bot.GetClient.Network.CurrentSim, entry.Key);
                            found_object = true;
                            break;
                        }
                    }
                    if(found_object == true)
                    {
                        return true;
                    }
                    else
                    {
                        return Failed("Unable to see object");
                    }
                }
                else
                {
                    return Failed("UUID not vaild");
                }
            }
            return false;
        }
    }
}
