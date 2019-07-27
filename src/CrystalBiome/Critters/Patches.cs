﻿using System;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

namespace CrystalBiome.Critters
{
    public class Patches
    {
        [HarmonyPatch(typeof(KSerialization.Manager))]
        [HarmonyPatch("GetType")]
        [HarmonyPatch(new[] { typeof(string) })]
        public static class Manager_GetType_Patch
        {
            private static void Postfix(string type_name, ref Type __result)
            {
                if (type_name == "CrystalBiome.LivingCrystal")
                {
                    __result = typeof(LivingCrystal);
                }
            }
        }

        [HarmonyPatch(typeof(GasAndLiquidConsumerMonitor.Instance))]
        [HarmonyPatch("OnMassConsumed")]
        public class GasAndLiquidConsumerMonitorInstance_OnMassConsumed_Patch
        {
            private static void Prefix(GasAndLiquidConsumerMonitor.Instance __instance, Sim.MassConsumedCallback mcd)
            {
                LivingCrystal livingCrystal = __instance.GetComponent<LivingCrystal>();
                if (livingCrystal == null)
                {
                    return;
                }

                if (mcd.mass > 0.0f)
                {
                    livingCrystal.AccumulateMass(mcd.mass, mcd.temperature);
                }
            }
        }

        [HarmonyPatch(typeof(CreatureCalorieMonitor.Stomach))]
        [HarmonyPatch("Poop")]
        public class Stomach_Poop_Patch
        {
            private static float temperature = 100.0f;

            private static void Prefix(GameObject ___owner)
            {
                temperature = ___owner.GetComponent<PrimaryElement>().Temperature;
                LivingCrystal livingCrystal = ___owner.GetComponent<LivingCrystal>();
                if (livingCrystal == null)
                {
                    return;
                }
                if (livingCrystal.CanConsumeTemperature())
                {
                    float decreasedTemperature = Math.Max(livingCrystal.ConsumeTemperature() + LivingCrystalConfig.OutputTemperatureDelta, LivingCrystalConfig.LethalLowTemperature);
                    ___owner.GetComponent<PrimaryElement>().Temperature = decreasedTemperature;
                }
            }

            private static void Postfix(GameObject ___owner)
            {
                LivingCrystal livingCrystal = ___owner.GetComponent<LivingCrystal>();
                if (livingCrystal == null)
                {
                    return;
                }

                ___owner.GetComponent<PrimaryElement>().Temperature = temperature;
            }
        }

        [HarmonyPatch(typeof(CodexEntryGenerator))]
        [HarmonyPatch("GenerateCreatureEntries")]
        public class CodexEntryGenerator_GenerateCreatureEntries_Patch
        {
            private static void Postfix(Dictionary<string, CodexEntry> __result)
            {
                Strings.Add($"STRINGS.CREATURES.FAMILY.{LivingCrystalConfig.Id.ToUpperInvariant()}", LivingCrystalConfig.Name);
                Strings.Add($"STRINGS.CREATURES.FAMILY_PLURAL.{LivingCrystalConfig.Id.ToUpperInvariant()}", LivingCrystalConfig.PluralName);
                Action(LivingCrystalConfig.Id, LivingCrystalConfig.Name, __result);
            }
        }

        private static void Action(Tag speciesTag, string name, Dictionary<string, CodexEntry> results)
        {
            List<GameObject> brains = Assets.GetPrefabsWithComponent<CreatureBrain>();
            CodexEntry entry = new CodexEntry("CREATURES", new List<ContentContainer>()
            {
                new ContentContainer(new List<ICodexWidget>()
                {
                    new CodexSpacer(),
                    new CodexSpacer()
                }, ContentContainer.ContentLayout.Vertical)
            }, name);
            entry.parentId = "CREATURES";
            CodexCache.AddEntry(speciesTag.ToString(), entry, null);
            results.Add(speciesTag.ToString(), entry);
            foreach (GameObject gameObject in brains)
            {
                CreatureBrain component = gameObject.GetComponent<CreatureBrain>();
                if (component.species == speciesTag)
                {
                    List<ContentContainer> contentContainerList = new List<ContentContainer>();
                    string symbolPrefix = component.symbolPrefix;
                    Sprite first = Def.GetUISprite(gameObject, symbolPrefix + "ui", false).first;
                    contentContainerList.Add(new ContentContainer(new List<ICodexWidget>()
                    {
                      new CodexImage(128, 128, first)
                    }, ContentContainer.ContentLayout.Vertical));
                    Traverse.Create(typeof(CodexEntryGenerator)).Method("GenerateCreatureDescriptionContainers", new[] { typeof(GameObject), typeof(List<ContentContainer>) }).GetValue(gameObject, contentContainerList);
                    entry.subEntries.Add(new SubEntry(component.PrefabID().ToString(), speciesTag.ToString(), contentContainerList, component.GetProperName())
                    {
                        icon = first,
                        iconColor = UnityEngine.Color.white
                    });
                }
            }
        }
    }
}
