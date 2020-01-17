﻿using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using Harmony;

namespace Fans
{
    public class HighPressureGasFan : IBuildingConfig
    {
        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            private static void Prefix()
            {
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{Id.ToUpperInvariant()}.NAME", DisplayName);
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{Id.ToUpperInvariant()}.DESC", Description);
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{Id.ToUpperInvariant()}.EFFECT", Effect);

                ModUtil.AddBuildingToPlanScreen("HVAC", Id);
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public class Db_Initialize_Patch
        {
            private static void Prefix()
            {
                var improvedGasPiping = new List<string>(Database.Techs.TECH_GROUPING["ImprovedGasPiping"]) { Id };
                Database.Techs.TECH_GROUPING["ImprovedGasPiping"] = improvedGasPiping.ToArray();
            }
        }

        public const string Id = "HighPressureFanBlock";
        public static string DisplayName = UI.FormatAsLink("High Pressure Fan Block", Id.ToUpper());
        public static string Description = $"Moves {UI.FormatAsLink("Gasses", "ELEMENTS_GAS")} from one side to the other.";
        public static string Effect = $"Blows around {UI.FormatAsLink("Gasses", "ELEMENTS_GAS")} in high pressure areas.";

        private const float SuckRate = 0.5f;
        private const ConduitType Type = ConduitType.Gas;
        private const float OverPressureThreshold = 20.0f;

        public override BuildingDef CreateBuildingDef()
        {
            return BaseFanConfig.CreateBuildingDef(Id, "highgasfan_kanim");
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            BaseFanConfig.ConfigureBuildingTemplate(go, prefab_tag);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            BaseFanConfig.DoPostConfigureComplete(go, SuckRate, Type, OverPressureThreshold);
        }
    }
}
