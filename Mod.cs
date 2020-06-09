using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.MountAndBlade;

// ReSharper disable InconsistentNaming

// inspired by V's Garrisons Don't Eat Nor Starve - v2
// https://www.nexusmods.com/mountandblade2bannerlord/mods/281
// just zeroes out garrison food consumption unless under siege
// vanilla needs to fix this for real and the mod/patch will be deprecated

namespace Fed_Garrisons
{
    public class Mod : MBSubModuleBase
    {
        private static readonly Harmony harmony = new Harmony("ca.gnivler.bannerlord.FedGarrisons");

        protected override void OnSubModuleLoad()
        {
            try
            {
                //Harmony.DEBUG = true;
                FileLog.Log("\n");
                Log("Startup " + DateTime.Now.ToShortTimeString());
                ManualPatches();
                //Harmony.DEBUG = false;
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void ManualPatches()
        {
            try
            {
                var CalculateTownFoodStocksChangeMi = AccessTools.Method(typeof(DefaultSettlementFoodModel),
                    "CalculateTownFoodStocksChange", new[] {typeof(Town), typeof(StatExplainer)});
                var postfix = AccessTools.Method(typeof(Mod), nameof(Postfix));
                Log("Patching CalculateTownFoodStocksChange");
                harmony.Patch(CalculateTownFoodStocksChangeMi, null, new HarmonyMethod(postfix));
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void Postfix(Town town, ref float __result)
        {
            try
            {
                if (town.IsUnderSiege)
                {
                    Log($"{town.Name} under siege: {__result} food");
                    return;
                }

                var garrisonParty = town.GarrisonParty;
                var troops = garrisonParty?.Party.NumberOfAllMembers ?? 0;
                var food = -troops / 20;
                __result = 0;
                Log($"{town.Name} not under siege, garrison wages include food, {__result} used ({-troops} saved)");
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void Log(object input)
        {
            //FileLog.Log($"[Fed Garrisons] {input ?? "null"}");
        }
    }
}
