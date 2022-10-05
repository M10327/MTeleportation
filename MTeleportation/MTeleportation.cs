using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MTeleportation
{
    public class MTeleportation : RocketPlugin<MTeleportationConfig>
    {
        public static MTeleportation Instance { get; private set; }
        public static Dictionary<ulong, Dictionary<ulong, ulong>> tpaRequests;
        public static Dictionary<ulong, ulong> tpaCooldown;
        public static Dictionary<ulong, string> tpaUI;
        public static List<SteamPlayer> playerList;
        public static Dictionary<ulong, ulong> combatCooldown;
        public static Dictionary<ulong, string> autoDeny;
        public UnityEngine.Color MessageColor { get; set; }
        public BlacklistDB blacklists;

        protected override void Load()
        {
            Instance = this;
            tpaRequests = new Dictionary<ulong, Dictionary<ulong, ulong>>();
            tpaCooldown = new Dictionary<ulong, ulong>();
            tpaUI = new Dictionary<ulong, string>();
            combatCooldown = new Dictionary<ulong, ulong>();
            autoDeny = new Dictionary<ulong, string>();
            MessageColor = (Color)UnturnedChat.GetColorFromHex(Configuration.Instance.messageColor);
            blacklists = new BlacklistDB();
            blacklists.Reload();
            if (Configuration.Instance.combatTimer > 0)
            {
                DamageTool.onPlayerAllowedToDamagePlayer += PlayerDamagePlayer;
            }

            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            foreach (var steamPlayer in Provider.clients)
            {
                UnturnedPlayer p = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                Events_OnPlayerConnected(p);
            }
            foreach (var x in blacklists.data.ToArray()) // remove ppls blacklists that just have id 0, because those are functionally empty
            {
                if (x.Value.Count == 1)
                {
                    if (x.Value[0] == 0)
                    {
                        blacklists.data.Remove(x.Key);
                    }
                }
                else if (x.Value.Count == 0){
                    blacklists.data.Remove(x.Key);
                }
            }
        }

        private void PlayerDamagePlayer(Player instigator, Player victim, ref bool isAllowed)
        {
            if (isAllowed)
            {
                combatCooldown[(ulong)UnturnedPlayer.FromPlayer(instigator).CSteamID] = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                combatCooldown[(ulong)UnturnedPlayer.FromPlayer(victim).CSteamID] = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            }
        }

        private void Events_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer p)
        {
            if (!tpaRequests.ContainsKey((ulong)p.CSteamID))
            {
                tpaRequests[(ulong)p.CSteamID] = new Dictionary<ulong, ulong>();
            }
            if (!tpaCooldown.ContainsKey((ulong)p.CSteamID))
            {
                tpaCooldown[(ulong)p.CSteamID] = 0;
            }
            if (!tpaUI.ContainsKey((ulong)p.CSteamID))
            {
                tpaUI[(ulong)p.CSteamID] = "big";
            }
            if (!combatCooldown.ContainsKey((ulong)p.CSteamID))
            {
                combatCooldown[(ulong)p.CSteamID] = 0;
            }
            if (!autoDeny.ContainsKey((ulong)p.CSteamID))
            {
                autoDeny[(ulong)p.CSteamID] = "none";
            }
            playerList = Provider.clients;
        }

        private void Events_OnPlayerDisconnected(Rocket.Unturned.Player.UnturnedPlayer p)
        {
            foreach (var steamPlayer in Provider.clients)
            {
                UnturnedPlayer t = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                if (tpaRequests.ContainsKey((ulong)t.CSteamID))
                {
                    if (tpaRequests[(ulong)t.CSteamID].ContainsKey((ulong)p.CSteamID)){
                        tpaRequests[(ulong)t.CSteamID].Remove((ulong)p.CSteamID);
                    }
                }

            }
            if (tpaRequests.ContainsKey((ulong)p.CSteamID))
            {
                tpaRequests.Remove((ulong)p.CSteamID);
            }
            if (tpaCooldown.ContainsKey((ulong)p.CSteamID))
            {
                tpaCooldown.Remove((ulong)p.CSteamID);
            }
            if (tpaUI.ContainsKey((ulong)p.CSteamID))
            {
                tpaUI.Remove((ulong)p.CSteamID);
            }
            if (combatCooldown.ContainsKey((ulong)p.CSteamID))
            {
                combatCooldown.Remove((ulong)p.CSteamID);
            }
            if (autoDeny.ContainsKey((ulong)p.CSteamID))
            {
                autoDeny.Remove((ulong)p.CSteamID);
            }
            playerList = Provider.clients;
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "TargetNotFound", "Target was not found" },
            { "TPAHelp", "Use: /tpa playername/accept/deny/cancel/ui/autodeny/blacklist" },
            { "TPADuplicate", "You've already sent a tpa request to {0}" },
            { "TPASend", "You sent a tpa request to {0}" },
            { "TPARecieve", "You've recieved a tpa request from {0}" },
            { "TPANoRequests", "You do not have any pending tpa requests" },
            { "TPACancel", "You have cancelled all outgoing tpa requests" },
            { "TPAAccept", "You accepted {0}'s tpa request" },
            { "TPAAccepted", "{0} accepted your tpa request" },
            { "TPADeny", "You denied {0}'s tpa request" },
            { "TPADenied", "{0} denied your tpa request" },
            { "TPASuccess", "You have been teleported to {0}" },
            { "TPAYourself", "You cannot tpa to yourself" },
            { "TPAVehicleTarget", "Cannot tpa, target is in vehicle" },
            { "TPAVehicleSelf", "Cannot tpa, you are in a vehicle" },
            { "TPADeadTarget", "Cannot tpa, target is dead" },
            { "TPADeadSelf", "Cannot tpa, you are dead" },
            { "TPAFail", "Tpa failed. Unknown reason" },
            { "TPACooldown", "Your tpa is still on cooldown for {0} seconds!" },
            { "UITitle", "TPA REQ RECEIVED!" },
            { "UIHelp", "/tpa ui <big/small/off>" },
            { "UISet", "Set ui to {0}" },
            { "IsTping", "{0} is teleporting to you!" },
            { "InCombatNoAccept", "You cannot accept a tpa during combat! Please wait {0} seconds" },
            { "InCombatTPAFail", "Tpa failed, {0} is in combat" },
            { "TPARetry", "Tpa failed, retrying {0} more times" },
            { "AutoDenyOptions", "Use /tpa autodeny <all/nongroup/off>" },
            { "AutoDenySet", "Auto deny set to {0}" },
            { "TPAIgnored", "Your tpa request has been ignored" },
            { "BlacklistOptions", "Use /tpa blacklist <add/remove/list/clear> (player name)" },
            { "BList", "Blacklisted players: {0}" },
            { "BAdd", "Added {0} to your tpa blacklist" },
            { "BRemove", "Removed {0} from your tpa blacklist" },
            { "BClear", "Cleared your tpa blacklist" }
        };

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            DamageTool.onPlayerAllowedToDamagePlayer -= PlayerDamagePlayer;
            blacklists.CommitToFile();
        }
    }
}
