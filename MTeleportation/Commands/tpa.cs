using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MTeleportation.Commands
{
    internal class tpa : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "tpa";

        public string Help => MTeleportation.Instance.Translate("TPAHelp");

        public string Syntax => MTeleportation.Instance.Translate("TPAHelp");

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer p = caller as UnturnedPlayer;
            ulong currentTime = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            if (command.Length >= 1)
            {
                RemoveOldTpas((ulong)p.CSteamID);
                if (command[0].ToLower() == "accept" || command[0].ToLower() == "a")
                {
                    if (MTeleportation.tpaRequests[(ulong)p.CSteamID].Count >= 1)
                    {
                        if (MTeleportation.combatCooldown[(ulong)p.CSteamID] + MTeleportation.Instance.Configuration.Instance.combatTimer > (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds())
                        {
                            UnturnedChat.Say(caller, MTeleportation.Instance.Translate("InCombatNoAccept", MTeleportation.combatCooldown[(ulong)p.CSteamID] + MTeleportation.Instance.Configuration.Instance.combatTimer - (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()), MTeleportation.Instance.MessageColor);
                            return;
                        }
                        var x = MTeleportation.tpaRequests[(ulong)p.CSteamID].First();
                        UnturnedPlayer requester = null;
                        string requesterDisplayName = "Name not found";
                        try
                        {
                            requester = UnturnedPlayer.FromCSteamID((CSteamID)x.Key);
                            if (requester != null)
                            {
                                if (MTeleportation.activeTpas.ContainsKey((ulong)p.CSteamID))
                                {
                                    UnturnedChat.Say(requester, MTeleportation.Instance.Translate("AlreadyTeleporting"), MTeleportation.Instance.MessageColor);
                                    return;
                                }
                                MTeleportation.activeTpas.Add((ulong)requester.CSteamID, (ulong)p.CSteamID);
                                TpToPlayer(p, requester, false, MTeleportation.Instance.Configuration.Instance.retryAttempts);
                                UnturnedChat.Say(requester, MTeleportation.Instance.Translate("TPAAccepted", p.DisplayName), MTeleportation.Instance.MessageColor);
                                requesterDisplayName = requester.CharacterName;
                            }
                        }
                        catch { }
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAAccept", requesterDisplayName), MTeleportation.Instance.MessageColor);
                        MTeleportation.tpaRequests[(ulong)p.CSteamID].Remove(x.Key);
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPANoRequests"), MTeleportation.Instance.MessageColor);
                        return;
                    }
                    
                }
                else if (command[0].ToLower() == "deny" || command[0].ToLower() == "d")
                {
                    if (MTeleportation.tpaRequests[(ulong)p.CSteamID].Count >= 1)
                    {
                        var x = MTeleportation.tpaRequests[(ulong)p.CSteamID].First();
                        UnturnedPlayer requester = null;
                        string requesterDisplayName = "Name not found";
                        try
                        {
                            requester = UnturnedPlayer.FromCSteamID((CSteamID)x.Key);
                            if (requester != null)
                            {
                                UnturnedChat.Say(requester, MTeleportation.Instance.Translate("TPADenied", p.DisplayName), MTeleportation.Instance.MessageColor);
                                requesterDisplayName = requester.CharacterName;
                            }
                        }
                        catch { }
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPADeny", requesterDisplayName), MTeleportation.Instance.MessageColor);
                        MTeleportation.tpaRequests[(ulong)p.CSteamID].Remove(x.Key);
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPANoRequests"), MTeleportation.Instance.MessageColor);
                        return;
                    }
                }
                else if (command[0].ToLower() == "cancel" || command[0].ToLower() == "c")
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPACancel"), MTeleportation.Instance.MessageColor);
                    CancelOutgoingTpas((ulong)p.CSteamID);
                }
                else if (command[0].ToLower() == "ui")
                {
                    if (command.Length >= 2)
                    {
                        switch (command[1].ToLower())
                        {
                            case "big":
                                MTeleportation.tpaUI[(ulong)p.CSteamID] = "big";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("UISet", MTeleportation.tpaUI[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "small":
                                MTeleportation.tpaUI[(ulong)p.CSteamID] = "small";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("UISet", MTeleportation.tpaUI[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "off":
                                MTeleportation.tpaUI[(ulong)p.CSteamID] = "off";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("UISet", MTeleportation.tpaUI[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            default:
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("UIHelp"), MTeleportation.Instance.MessageColor);
                                break;
                        }
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("UIHelp"), MTeleportation.Instance.MessageColor);
                        return;
                    }
                }
                else if (command[0].ToLower() == "autodeny")
                {
                    if (command.Length >= 2)
                    {
                        switch (command[1].ToLower())
                        {
                            case "all":
                                MTeleportation.autoDeny[(ulong)p.CSteamID] = "all";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenySet", MTeleportation.autoDeny[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "nongroup":
                                MTeleportation.autoDeny[(ulong)p.CSteamID] = "nongroup";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenySet", MTeleportation.autoDeny[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "off":
                                MTeleportation.autoDeny[(ulong)p.CSteamID] = "off";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenySet", MTeleportation.autoDeny[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "nonally":
                                MTeleportation.autoDeny[(ulong)p.CSteamID] = "nonally";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenySet", MTeleportation.autoDeny[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            default:
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenyOptions"), MTeleportation.Instance.MessageColor);
                                break;
                        }
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenyOptions"), MTeleportation.Instance.MessageColor);
                    }
                }
                else if (command[0].ToLower() == "autoaccept")
                {
                    if (command.Length >= 2)
                    {
                        switch (command[1].ToLower())
                        {
                            case "group":
                                MTeleportation.autoAccept[(ulong)p.CSteamID] = "group";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoAcceptSet", MTeleportation.autoAccept[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "none":
                                MTeleportation.autoAccept[(ulong)p.CSteamID] = "none";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoAcceptSet", MTeleportation.autoAccept[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            case "ally":
                                MTeleportation.autoAccept[(ulong)p.CSteamID] = "ally";
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoAcceptSet", MTeleportation.autoAccept[(ulong)p.CSteamID]), MTeleportation.Instance.MessageColor);
                                break;
                            default:
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoAcceptOptions"), MTeleportation.Instance.MessageColor);
                                break;
                        }
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoAcceptOptions"), MTeleportation.Instance.MessageColor);
                    }
                }
                else if (command[0].ToLower() == "blacklist" || command[0].ToLower() == "bl")
                {
                    if (command.Length >= 2)
                    {
                        if (!MTeleportation.Instance.blacklists.data.ContainsKey((ulong)p.CSteamID))
                        {
                            MTeleportation.Instance.blacklists.data[(ulong)p.CSteamID] = new List<ulong>();
                        }
                        switch (command[1].ToLower())
                        {
                            case "add":
                                if (command.Length >= 3)
                                {
                                    try
                                    {
                                        UnturnedPlayer tPlayer = UnturnedPlayer.FromName(command[2]);
                                        MTeleportation.Instance.blacklists.data[(ulong)p.CSteamID].Add((ulong)tPlayer.CSteamID);
                                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("BAdd", tPlayer.DisplayName), MTeleportation.Instance.MessageColor);
                                    }
                                    catch { UnturnedChat.Say(p, MTeleportation.Instance.Translate("TargetNotFound"), MTeleportation.Instance.MessageColor); }
                                }
                                else { UnturnedChat.Say(p, MTeleportation.Instance.Translate("BlacklistOptions"), MTeleportation.Instance.MessageColor); }
                                break;
                            case "remove":
                                if (command.Length >= 3)
                                {
                                    try
                                    {
                                        UnturnedPlayer tPlayer = UnturnedPlayer.FromName(command[2]);
                                        MTeleportation.Instance.blacklists.data[(ulong)p.CSteamID].Remove((ulong)tPlayer.CSteamID);
                                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("BRemove", tPlayer.DisplayName), MTeleportation.Instance.MessageColor);
                                    }
                                    catch { UnturnedChat.Say(p, MTeleportation.Instance.Translate("TargetNotFound"), MTeleportation.Instance.MessageColor); }
                                }
                                else { UnturnedChat.Say(p, MTeleportation.Instance.Translate("BlacklistOptions"), MTeleportation.Instance.MessageColor); }
                                break;
                            case "list":
                                string playerList = "";
                                foreach (ulong x in MTeleportation.Instance.blacklists.data[(ulong)p.CSteamID])
                                {
                                    UnturnedPlayer listTarget = null;
                                    bool foundPlayer = false;
                                    try
                                    {
                                        listTarget = UnturnedPlayer.FromCSteamID((Steamworks.CSteamID)x);
                                        if (listTarget != null)
                                        {
                                            playerList = playerList + listTarget.DisplayName + "(" + x.ToString() + "), ";
                                            foundPlayer = true;
                                        }
                                    }
                                    catch { }
                                    if (!foundPlayer)
                                    {
                                        playerList = playerList + x.ToString() + ", ";
                                    }
                                }
                                UnturnedChat.Say(caller, MTeleportation.Instance.Translate("BList", playerList), MTeleportation.Instance.MessageColor);
                                break;
                            case "clear":
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("BClear"), MTeleportation.Instance.MessageColor);
                                MTeleportation.Instance.blacklists.data.Remove((ulong)p.CSteamID);
                                break;
                            default:
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("BlacklistOptions"), MTeleportation.Instance.MessageColor);
                                break;
                        }
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("BlacklistOptions"), MTeleportation.Instance.MessageColor);
                    }
                }
                else
                {
                    UnturnedPlayer target = null;
                    try
                    {
                        target = UnturnedPlayer.FromName(command[0]);
                    }
                    catch
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TargetNotFound"), MTeleportation.Instance.MessageColor);
                        return;
                    }
                    if (target == null || p == null)
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TargetNotFound"), MTeleportation.Instance.MessageColor);
                        return;

                    }
                    RemoveOldTpas((ulong)target.CSteamID);
                    if (target.CSteamID == p.CSteamID) // checks if player is self and cancels. I comment this out for testing
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAYourself"), MTeleportation.Instance.MessageColor);
                        return;
                    }
                    if (MTeleportation.tpaRequests[(ulong)target.CSteamID].ContainsKey((ulong)p.CSteamID))
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPADuplicate", target.DisplayName), MTeleportation.Instance.MessageColor);
                        return;
                    }

                    if ((ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() - MTeleportation.tpaCooldown[(ulong)p.CSteamID] < MTeleportation.Instance.Configuration.Instance.tpSendCooldown)
                    {
                        long cooldown = (long)MTeleportation.Instance.Configuration.Instance.tpSendCooldown - Math.Abs((long)MTeleportation.tpaCooldown[(ulong)p.CSteamID] - (long)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds());
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPACooldown", cooldown), MTeleportation.Instance.MessageColor);
                        return;
                    }

                    if (MTeleportation.Instance.blacklists.data.ContainsKey((ulong)target.CSteamID))
                    {
                        if (MTeleportation.Instance.blacklists.data[(ulong)target.CSteamID].Contains((ulong)p.CSteamID)){
                            UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAIgnored"), MTeleportation.Instance.MessageColor);
                            return;
                        }
                    }

                    switch (MTeleportation.autoDeny[(ulong)target.CSteamID]) // auto deny stuff
                    {
                        case "all":
                            UnturnedChat.Say(p, MTeleportation.Instance.Translate("AutoDenied"), MTeleportation.Instance.MessageColor);
                            return;
                        case "nongroup":
                            if (p.Player.quests.groupID != target.Player.quests.groupID)
                            {
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAIgnored"), MTeleportation.Instance.MessageColor);
                                return;
                            }
                            break;
                        case "nonally":
                            if (p.Player.quests.groupID != target.Player.quests.groupID && !CheckIfAllied((ulong)p.CSteamID, (ulong)target.CSteamID))
                            {
                                UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAIgnored"), MTeleportation.Instance.MessageColor);
                                return;
                            }
                            break;
                    }

                    // if players are in same group
                    if (MTeleportation.Instance.Configuration.Instance.AllowAutoAccept
                        && ((p.Player.quests.groupID == target.Player.quests.groupID && (ulong)p.Player.quests.groupID != 0
                            && (MTeleportation.autoAccept[(ulong)target.CSteamID] == "group" || MTeleportation.autoAccept[(ulong)target.CSteamID] == "ally"))
                        || (MTeleportation.autoAccept[(ulong)target.CSteamID] == "ally") && CheckIfAllied((ulong)p.CSteamID, (ulong)target.CSteamID)))
                    {
                        if (MTeleportation.activeTpas.ContainsKey((ulong)p.CSteamID))
                        {
                            UnturnedChat.Say(p, MTeleportation.Instance.Translate("AlreadyTeleporting"), MTeleportation.Instance.MessageColor);
                            return;
                        }
                        MTeleportation.activeTpas.Add((ulong)p.CSteamID, (ulong)target.CSteamID);
                        TpToPlayer(target, p, false, MTeleportation.Instance.Configuration.Instance.retryAttempts);
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAAccepted", target.CharacterName), MTeleportation.Instance.MessageColor);
                        UnturnedChat.Say(target, MTeleportation.Instance.Translate("IsTping", p.DisplayName), MTeleportation.Instance.MessageColor);
                    }
                    else // regular tpa req functionality
                    {
                        MTeleportation.tpaRequests[(ulong)target.CSteamID].Add((ulong)p.CSteamID, currentTime);
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPASend", target.CharacterName), MTeleportation.Instance.MessageColor);
                        UnturnedChat.Say(target, MTeleportation.Instance.Translate("TPARecieve", p.DisplayName), MTeleportation.Instance.MessageColor);
                        switch (MTeleportation.tpaUI[(ulong)target.CSteamID])
                        {
                            case "big":
                                EffectManager.askEffectClearByID(MTeleportation.Instance.Configuration.Instance.bigUI, target.Player.channel.owner.transportConnection);
                                EffectManager.sendUIEffect(MTeleportation.Instance.Configuration.Instance.bigUI, (short)(MTeleportation.Instance.Configuration.Instance.bigUI + 10), target.Player.channel.owner.transportConnection, true, MTeleportation.Instance.Translate("UITitle"), MTeleportation.Instance.Translate("TPARecieve", p.DisplayName));
                                break;
                            case "small":
                                EffectManager.askEffectClearByID(MTeleportation.Instance.Configuration.Instance.smallUI, target.Player.channel.owner.transportConnection);
                                EffectManager.sendUIEffect(MTeleportation.Instance.Configuration.Instance.smallUI, (short)(MTeleportation.Instance.Configuration.Instance.smallUI + 10), target.Player.channel.owner.transportConnection, true, MTeleportation.Instance.Translate("TPARecieve", p.DisplayName));
                                break;
                            case "off":
                                break;
                        }
                    }
                    return;
                }
            }
            else
            {
                UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAHelp"), MTeleportation.Instance.MessageColor);
            }
        }

        public void CancelOutgoingTpas(ulong canceler)
        {
            foreach (var steamPlayer in MTeleportation.playerList)
            {
                UnturnedPlayer target = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                if (MTeleportation.Instance.Configuration.Instance.verbose) Rocket.Core.Logging.Logger.Log("working on " + target.CharacterName);
                foreach (var x in MTeleportation.tpaRequests[(ulong)target.CSteamID])
                {
                    if (MTeleportation.Instance.Configuration.Instance.verbose) Rocket.Core.Logging.Logger.Log("removing User " + canceler + " from " + target.CharacterName);
                    if (x.Key == canceler)
                    {
                        MTeleportation.tpaRequests[(ulong)target.CSteamID].Remove(x.Key);
                        return;
                    }
                }
            }
        }

        public void RemoveOldTpas(ulong target)
        {
            ulong currentTime = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            Dictionary<ulong,ulong> tempDict = MTeleportation.tpaRequests[target];
            foreach (var x in MTeleportation.tpaRequests[target].ToArray())
            {
                if (currentTime - x.Value >= MTeleportation.Instance.Configuration.Instance.tpaExpiration)
                {
                    MTeleportation.tpaRequests[target].Remove(x.Key);
                }
            }
        }

        public async void TpToPlayer(UnturnedPlayer target, UnturnedPlayer p, bool instant, int retries)
        {
            try
            {
                bool canceled = false;
                if (target == null || p == null)
                {
                    RemoveActiveTP(p.CSteamID);
                    return;
                }
                if (!instant) await Task.Delay((MTeleportation.Instance.Configuration.Instance.tpDelay * 1000));
                if (target == null || p == null)
                {
                    RemoveActiveTP(p.CSteamID);
                    return;
                }
                if (!MTeleportation.activeTpas.ContainsKey((ulong)p.CSteamID))
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("TpaWasCanceledUnknown"), MTeleportation.Instance.MessageColor);
                    return; 
                }
                if (target.Player.life.isDead)
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPADeadTarget"), MTeleportation.Instance.MessageColor);
                    canceled = true;
                    retries = 0;
                }
                else if (p.Player.life.isDead)
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPADeadSelf"), MTeleportation.Instance.MessageColor);
                    canceled = true;
                    retries = 0;
                }
                else if (target.IsInVehicle)
                {
                    var vehicle = target.CurrentVehicle;
                    bool hasSeats = false;
                    foreach (var pas in vehicle.passengers)
                    {
                        if (pas.player == null)
                        {
                            hasSeats = true;
                            break;
                        }
                    }
                    if (hasSeats)
                    {
                        VehicleManager.ServerForcePassengerIntoVehicle(p.Player, vehicle);
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAVehicleTarget"), MTeleportation.Instance.MessageColor);
                        canceled = true;

                    }
                }
                else if (p.IsInVehicle)
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAVehicleSelf"), MTeleportation.Instance.MessageColor);
                    canceled = true;
                }
                else if (MTeleportation.combatCooldown[(ulong)target.CSteamID] + MTeleportation.Instance.Configuration.Instance.combatTimer > (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds())
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("InCombatTPAFail", target.DisplayName), MTeleportation.Instance.MessageColor);
                    canceled = true;
                }

                if (!canceled)
                {
                    p.Teleport(target);
                    CancelOutgoingTpas((ulong)p.CSteamID);
                    await Task.Delay(10);
                    double distance = Vector3.Distance(p.Position, target.Position);
                    if (distance < 5)
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPASuccess", target.CharacterName), MTeleportation.Instance.MessageColor);
                    }
                    else if (p.IsInVehicle && target.IsInVehicle)
                    {
                        if (p.CurrentVehicle != target.CurrentVehicle)
                        {
                            UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAFail"), MTeleportation.Instance.MessageColor);
                            canceled = true;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPAFail"), MTeleportation.Instance.MessageColor);
                        canceled = true;
                    }
                    if (!canceled)
                    {
                        MTeleportation.tpaCooldown[(ulong)p.CSteamID] = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                        if (MTeleportation.Instance.Configuration.Instance.tpaSuccedEffect != 0) EffectManager.sendEffect(MTeleportation.Instance.Configuration.Instance.tpaSuccedEffect, 200, target.Position);
                    }
                }
                if (canceled && retries > 0)
                {
                    UnturnedChat.Say(p, MTeleportation.Instance.Translate("TPARetry", retries.ToString()), MTeleportation.Instance.MessageColor);
                    retries--;
                    await Task.Delay(1000);
                    TpToPlayer(target, p, true, retries);
                }
                else RemoveActiveTP(p.CSteamID);
            }
            catch { }
        }

        public void RemoveActiveTP(CSteamID playerID)
        {
            ulong id = (ulong)playerID;
            if (MTeleportation.activeTpas.ContainsKey(id))
            {
                MTeleportation.activeTpas.Remove(id);
            }
        }

        public bool CheckIfAllied(ulong teleporter, ulong target)
        {
            if (MTeleportation.Instance.Configuration.Instance.IsAlliesInstalled)
            {
                return CheckIfAllies2(teleporter, target);
            }
            return false;
        }

        public bool CheckIfAllies2(ulong teleporter, ulong target)
        {
            var allies = MAllies.MAllies.Instance.allies.data;
            if (allies.ContainsKey(teleporter))
            {
                if (allies[teleporter].Contains(target))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
