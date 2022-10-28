using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTeleportation
{
    public class MTeleportationConfig : IRocketPluginConfiguration
    {
        public bool verbose;
        public string messageColor;
        public ulong tpaExpiration;
        public int tpDelay;
        public ulong tpSendCooldown;
        public ushort smallUI;
        public ushort bigUI;
        public ushort tpaSuccedEffect;
        public bool autoAcceptSameGroupTpas;
        public ulong combatTimer;
        public int retryAttempts;
        public void LoadDefaults()
        {
            verbose = false;
            messageColor = "ffff00";
            tpaExpiration = 60;
            tpDelay = 3;
            tpSendCooldown = 30;
            smallUI = 28001;
            bigUI = 28000;
            tpaSuccedEffect = 0;
            autoAcceptSameGroupTpas = true;
            combatTimer = 2;
            retryAttempts = 3;
        }
    }
}
