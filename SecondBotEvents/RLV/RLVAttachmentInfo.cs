using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondBotEvents.RLV
{
    public class AttachmentInfo
    {
        public Primitive Prim;
        public InventoryItem Item;
        public UUID InventoryID;
        public UUID PrimID;
        public bool MarkedAttached;
        public AttachmentPoint Point => Prim != null ? Prim.PrimData.AttachmentPoint : AttachmentPoint.Default;
    }
}
