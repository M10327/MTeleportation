using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;

namespace MTeleportation
{
    public class MTeleportation : RocketPlugin<MTeleportationConfig>
    {
        public static MTeleportation Instance { get; private set; }
        public static Dictionary<ulong, PlayerMeta> meta;
        public static List<SteamPlayer> playerList;
        public static Dictionary<ulong, ulong> activeTpas;
        private static System.Timers.Timer CoolDownManager;
        public UnityEngine.Color MessageColor { get; set; }
        public BlacklistDB blacklists;

        protected override void Load()
        {
            Instance = this;
            meta = new Dictionary<ulong, PlayerMeta>();
            activeTpas = new Dictionary<ulong, ulong>();

            MessageColor = (Color)UnturnedChat.GetColorFromHex(Configuration.Instance.messageColor);
            blacklists = new BlacklistDB();
            blacklists.Reload();
            if (Configuration.Instance.combatTimer > 0)
            {
                DamageTool.onPlayerAllowedToDamagePlayer += PlayerDamagePlayer;
            }

            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
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

            CoolDownManager = new System.Timers.Timer(1000);
            CoolDownManager.Elapsed += ReduceCooldowns;
            CoolDownManager.AutoReset = true;
            CoolDownManager.Enabled = true;
        }

        private void ReduceCooldowns(object sender, ElapsedEventArgs e)
        {
            foreach (var pl in playerList)
            {
                ulong id = (ulong)pl.playerID.steamID;
                if (meta[id].SendCooldown > 0) meta[id].SendCooldown -= 1;
                if (meta[id].CombatCooldown > 0) meta[id].CombatCooldown -= 1;
                foreach(var req in meta[id].Requests.ToArray())
                {
                    if (meta[id].Requests[req.Key] > 0) meta[id].Requests[req.Key]--;
                    else meta[id].Requests.Remove(req.Key);
                }
            }
        }

        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            ulong id = (ulong)player.CSteamID;
            foreach (var kvp in activeTpas.ToArray())
            {
                if (kvp.Key == id || kvp.Value == id)
                {
                    activeTpas.Remove(kvp.Key);
                    return;
                }
            }
        }

        private void PlayerDamagePlayer(Player instigator, Player victim, ref bool isAllowed)
        {
            if (isAllowed)
            {
                meta[(ulong)UnturnedPlayer.FromPlayer(instigator).CSteamID].CombatCooldown = Configuration.Instance.combatTimer;
                meta[(ulong)UnturnedPlayer.FromPlayer(victim).CSteamID].CombatCooldown = Configuration.Instance.combatTimer;
            }
        }

        private void Events_OnPlayerConnected(Rocket.Unturned.Player.UnturnedPlayer p)
        {
            if (!meta.ContainsKey((ulong)p.CSteamID)){
                meta[(ulong)p.CSteamID] = new PlayerMeta(EUI.big, EAutoDeny.off, (EAutoAccept)Configuration.Instance.AutoAcceptDefault);
            }
            playerList = Provider.clients;
        }

        private void Events_OnPlayerDisconnected(Rocket.Unturned.Player.UnturnedPlayer p)
        {
            foreach (var steamPlayer in Provider.clients)
            {
                UnturnedPlayer t = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                if (meta[(ulong)t.CSteamID].Requests.ContainsKey((ulong)p.CSteamID))
                {
                    meta[(ulong)t.CSteamID].Requests.Remove((ulong)p.CSteamID);
                }

            }
            if (meta.ContainsKey((ulong)p.CSteamID))
            {
                meta.Remove((ulong)p.CSteamID);
            }
            playerList = Provider.clients;
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "TargetNotFound", "Target was not found" },
            { "TPAHelp", "Use: /tpa playername/accept/deny/cancel/ui/autodeny/autoaccept/blacklist" },
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
            { "TPAVehicleTarget", "Cannot tpa, target's vehicle is full" },
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
            { "AutoDenyOptions", "Use /tpa autodeny <all/nongroup/nonally/off>" },
            { "AutoDenySet", "Auto deny set to {0}" },
            { "TPAIgnored", "Your tpa request has been ignored" },
            { "BlacklistOptions", "Use /tpa blacklist <add/remove/list/clear> (player name)" },
            { "BList", "Blacklisted players: {0}" },
            { "BAdd", "Added {0} to your tpa blacklist" },
            { "BRemove", "Removed {0} from your tpa blacklist" },
            { "BClear", "Cleared your tpa blacklist" },
            { "AlreadyTeleporting", "You are already teleporting. Cannot tp again!" },
            { "AutoAcceptSet", "Set autoaccept to {0}" },
            { "AutoAcceptOptions", "Use /tpa autoaccept <group/ally/none>" },
            { "TpaWasCanceledUnknown", "The tpa was canceled" },
            { "TpaTargetTooHighSelf", "You are too high up to accept tpas" },
            { "TpaTargetTooHigh", "{0} is too high to accept" }
        };

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            DamageTool.onPlayerAllowedToDamagePlayer -= PlayerDamagePlayer;
            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            blacklists.CommitToFile();
            if (CoolDownManager != null)
            {
                CoolDownManager.Stop();
                CoolDownManager.Elapsed -= ReduceCooldowns;
            }
        }
    }
}
