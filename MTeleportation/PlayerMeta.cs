using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTeleportation
{
    public class PlayerMeta
    {
        public PlayerMeta(EUI uI, EAutoDeny autoDeny, EAutoAccept autoAccept)
        {
            Requests = new Dictionary<ulong, ulong>();
            SendCooldown = 0;
            UI = uI;
            CombatCooldown = 0;
            AutoDeny = autoDeny;
            AutoAccept = autoAccept;
        }

        public Dictionary<ulong, ulong> Requests { get; set; }
        public ulong SendCooldown { get; set; }
        public EUI UI { get; set; }
        public ulong CombatCooldown { get; set; }
        public EAutoDeny AutoDeny { get; set; }
        public EAutoAccept AutoAccept { get; set; }
    }

}
