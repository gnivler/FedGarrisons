using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
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
                Log("Startup " + DateTime.Now.ToShortTimeString());
                //Harmony.DEBUG = false;
                var OnGameStartMi = AccessTools.Method(typeof(SandBoxManager), "OnGameStart", new[] {typeof(CampaignGameStarter)});
                var onGameStartPostfix = AccessTools.Method(typeof(Mod), nameof(OnGameStartPostfix));
                Trace("Patching OnGameStart");
                harmony.Patch(OnGameStartMi, null, new HarmonyMethod(onGameStartPostfix));
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void Trace(object input)
        {
            //FileLog.Log($"[Fed Garrisons] {input ?? "null"}");
        }

        private static void Log(object input)
        {
            //FileLog.Log($"[Fed Garrisons] {input ?? "null"}");
        }

        private static void OnGameStartPostfix()
        {
            try
            {
                var calculateTownFoodStocksChangeMi = AccessTools.Method(typeof(DefaultSettlementFoodModel),
                    "CalculateTownFoodStocksChange", new[] {typeof(Town), typeof(StatExplainer)});
                var calculateTownFoodStocksChangePostfix = AccessTools.Method(typeof(Mod), nameof(CalculateTownFoodStocksChangePostfix));
                Trace("Patching CalculateTownFoodStocksChange");
                harmony.Patch(calculateTownFoodStocksChangeMi, null, new HarmonyMethod(calculateTownFoodStocksChangePostfix));
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void CalculateTownFoodStocksChangePostfix(Town town, ref StatExplainer explanation, ref float __result)
        {
            try
            {
                Trace("\n");
                Trace(town.Name);
                Trace(new string('-', 10));
                if (town.IsUnderSiege)
                {
                    Trace($"under siege: {__result} food");
                    return;
                }

                var garrisonParty = town.GarrisonParty;
                var troops = garrisonParty?.Party.NumberOfAllMembers ?? 0;
                var food = -troops / 20;
                Trace($"{troops} troops food impact {food}");
                Trace($"original total food cost {__result}, modified to remove troop food costs {__result - food}");
                __result -= food;
                Trace($"not under siege, {__result} used ({-food} saved)");
                if (explanation == null)
                {
                    return;
                }

                for (var i = 0; i < explanation.Lines.Count; i++)
                {
                    if (explanation.Lines[i].Name.Contains("Garrison"))
                    {
                        explanation.Lines[i] = new StatExplainer.ExplanationLine("Garrison (free) ", 0f, StatExplainer.OperationType.Add);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e);
            }
        }
    }
}
