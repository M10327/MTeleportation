﻿using Rocket.API;
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
        public ulong CooldownPerGroupMember;
        public ushort smallUI;
        public ushort bigUI;
        public ushort tpaSuccedEffect;
        public bool AllowAutoAccept;
        public ulong combatTimer;
        public int retryAttempts;
        public bool IsAlliesInstalled;
        public int AutoAcceptDefault;
        public int MaxYValueToAutoAccept;
        public ulong DeathTpaSendCooldown;
        public void LoadDefaults()
        {
            verbose = false;
            messageColor = "ffff00";
            tpaExpiration = 60;
            tpDelay = 3;
            tpSendCooldown = 30;
            CooldownPerGroupMember = 5;
            smallUI = 28001;
            bigUI = 28000;
            tpaSuccedEffect = 1956;
            AllowAutoAccept = true;
            combatTimer = 2;
            retryAttempts = 3;
            IsAlliesInstalled = true;
            AutoAcceptDefault = 1;
            MaxYValueToAutoAccept = 150;
            DeathTpaSendCooldown = 30;
        }
    }
}
