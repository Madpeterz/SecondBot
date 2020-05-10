using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Prims
{
    class UnRezObject : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "The object to unrez"}; } }
        public override string Helpfile { get { return "Returns a rezzed object"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetobject) == true)
                {
                    bool found = false;
                    Dictionary<uint, Primitive> objects_copy = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();
                    foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
                    {
                        if (Obj.Value.ID == targetobject)
                        {
                            bot.GetClient.Inventory.RequestDeRezToInventory(Obj.Key);
                            found = true;
                            break;
                        }
                    }
                    if(found == true)
                    {
                        return true;
                    }
                    return Failed("Unable to see object");
                }
                else
                {
                    return Failed("Invaild object UUID");
                }
            }
            return false;
        }
    }
}
