using HarmonyLib;
using OxygenLink;
using System;
using static OxygenLink.Extensions;

namespace OxygenLink
{
    public class Events
    {
        public static Event<Player> PlayerDeath = new();
        public static event EventHandler<EventArgs<Player>> OnPlayerDeath
        {
            add { PlayerDeath.OnEvent += value; }
            remove { PlayerDeath.OnEvent -= value; }
        }
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.ResetPlayerOnDeath))]
public static class PlayerOnDeath
{
    [HarmonyPostfix]
    public static void Postfix(ref Player __instance, float waitTime)
    {
        Plugin.Logger.LogDebug("Caught player death, raising event.");
        Events.PlayerDeath.Raise(__instance);
    }
}