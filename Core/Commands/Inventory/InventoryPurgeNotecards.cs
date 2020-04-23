using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using BSB.Static;
using BetterSecondBotShared.Static;

namespace BSB.Commands.Inventory
{
    public class InventoryPurgeNotecards : CoreCommand
    {
        public override string Helpfile { get { return "Searchs the notecards folder for notecards, any older than 31 days are deleted.<br/>Depending on the number of notecards this might require multiple calls!"; } }
        public override int MinArgs { get { return 0; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(bot.GetClient.Inventory.Store.RootFolder);
                InventoryBase NotecardFolder = null;
                foreach (InventoryBase R in T)
                {
                    if(R.Name == "Notecards")
                    {
                        NotecardFolder = R;
                        break;
                    }
                }
                if(NotecardFolder != null)
                {
                    DateTime Now = DateTime.Now;
                    List<UUID> purge_notecards = new List<UUID>();
                    List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(NotecardFolder.UUID, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000);
                    foreach(InventoryBase C in contents)
                    {
                        InventoryItem A = (InventoryItem)C;
                        if(A.AssetType == AssetType.Notecard)
                        {
                            TimeSpan Dif = DateTime.Now - A.CreationDate;
                            if(Dif.Days >= 31)
                            {
                                purge_notecards.Add(C.UUID);
                            }
                        }
                    }
                    if(purge_notecards.Count() > 0)
                    {
                        bot.GetClient.Inventory.Remove(purge_notecards, new List<UUID>());
                    }
                    return true;
                }
                else
                {
                    return Failed("Unable to find notecard folder");
                }
            }
            return false;
        }
    }
}
