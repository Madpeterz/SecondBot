using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.Commands.Prims
{
    class TransferInventoryToObject : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "UUID","True" }; } }
        public override string[] ArgHints { get { return new[] { "Object we are sending to", "The item we are sending","Running script (Optional)" }; } }
        public override string Helpfile { get { return "Transfers a item [ARG 2] to a objects inventory [ARG 1] (And if set with the script running state [ARG 3])"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[1], out UUID targetitem) == true)
                {
                    if (UUID.TryParse(args[0], out UUID targetobject) == true)
                    {
                        InventoryItem itm = bot.GetClient.Inventory.FetchItem(targetitem, bot.GetClient.Self.AgentID, (5 * 1000));
                        if (itm != null)
                        {
                            Dictionary<uint, Primitive> objects_copy = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();
                            bool found = false;
                            foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
                            {
                                if (Obj.Value.ID == targetobject)
                                {
                                    bool script_state = false;
                                    if (args.Length == 3)
                                    {
                                        if (itm.AssetType != AssetType.LSLText)
                                        {
                                            script_state = false;
                                            InfoBlob = "Please omit the 3rd arg if not sending a script!";
                                        }
                                        else
                                        {
                                            if (bool.TryParse(args[2], out script_state) == false)
                                            {
                                                return Failed("Unable to process arg 3");
                                            }
                                        }
                                    }
                                    if (script_state == false)
                                    {
                                        bot.GetClient.Inventory.UpdateTaskInventory(Obj.Key, itm);
                                    }
                                    else
                                    {
                                        bot.GetClient.Inventory.CopyScriptToTask(Obj.Key, itm, true);
                                    }
                                    found = true;
                                    break;
                                }
                            }
                            if (found == false)
                            {
                                return true;
                            }
                            return Failed("Unable to see object (Please wait and try again)");
                        }
                        return Failed("Unable to find item");
                    }
                    return Failed("Invaild object UUID");
                }
                return Failed("Invaild item UUID");
            }
            return false;
        }
    }
}
