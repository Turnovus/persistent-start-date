using HarmonyLib;
using RimWorld;
using Verse;

namespace PersistentDate
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            Harmony harmony = new Harmony("Turnovus.RimWorld.PersistentDate");
            harmony.PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(GenDate))]
    [HarmonyPatch(nameof(GenDate.Year))]
    public class YearReadoutPatch
    {
        [HarmonyPrefix]
        public static void AdjustDate(ref long absTicks)
        {
            long yearOffset = Current.Game?.GetComponent<PersistentDate_GameComponent>()?.startYearOffset ?? 0;
            absTicks += yearOffset * GenDate.TicksPerYear;
        }
    }
}