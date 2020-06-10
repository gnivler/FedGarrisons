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

        private static void Postfix(Town town, ref StatExplainer explanation, ref float __result)
        {
            try
            {
                Log("\n");
                Log(town.Name);
                Log(new string('-', 10));
                if (town.IsUnderSiege)
                {
                    Log($"under siege: {__result} food");
                    return;
                }

                var garrisonParty = town.GarrisonParty;
                var troops = garrisonParty?.Party.NumberOfAllMembers ?? 0;
                var food = -troops / 20;
                Log($"{troops} troops food impact {food}");
                Log($"original total food cost {__result}, modified to remove troop food costs {__result - food}");
                __result -= food;
                Log($"not under siege, {__result} used ({-food} saved)");
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

        private static void Log(object input)
        {
            //FileLog.Log($"[Fed Garrisons] {input ?? "null"}");
        }
    }
}
