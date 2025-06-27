using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BetterSmelting
{
    [HarmonyPatch(typeof(Smelter))]
    public static class SmelterPatcher
    {
        // Patch for the max ore capacity field and conversion list
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake_Postfix(Smelter __instance)
        {
            // Set max ore capacity based on the smelter type
            if (__instance.m_name.ToLower().Contains("blastfurnace"))
            {
                __instance.m_maxOre = BetterSmeltingPlugin.MaxBlastFurnaceOre.Value;
                __instance.m_maxFuel = BetterSmeltingPlugin.MaxBlastFurnaceFuel.Value;
                
                // If AllOresInBlastFurnace is enabled, modify the conversion list to include all ore types
                if (BetterSmeltingPlugin.AllOresInBlastFurnace.Value)
                {
                    // Store the original conversions
                    var originalConversions = new List<Smelter.ItemConversion>(__instance.m_conversion);
                    
                    // Find a regular smelter to get its conversions
                    var smelters = Resources.FindObjectsOfTypeAll<Smelter>();
                    foreach (var smelter in smelters)
                    {
                        if (smelter.m_name.ToLower().Contains("smelter") && !smelter.m_name.ToLower().Contains("blastfurnace"))
                        {
                            // Add all conversions from the regular smelter to the blast furnace
                            foreach (var conversion in smelter.m_conversion)
                            {
                                // Check if this conversion is already in the blast furnace
                                bool alreadyExists = false;
                                foreach (var existingConversion in originalConversions)
                                {
                                    if (existingConversion.m_from.m_itemData.m_shared.m_name == conversion.m_from.m_itemData.m_shared.m_name)
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }
                                
                                // If it doesn't exist, add it
                                if (!alreadyExists)
                                {
                                    __instance.m_conversion.Add(conversion);
                                }
                            }
                            break; // We only need one regular smelter
                        }
                    }
                }
            }
            else if (__instance.m_name.ToLower().Contains("smelter") || __instance.m_name.ToLower().Contains("furnace"))
            {
                __instance.m_maxOre = BetterSmeltingPlugin.MaxSmelterOre.Value;
                __instance.m_maxFuel = BetterSmeltingPlugin.MaxSmelterFuel.Value;
            }
        }

        // Patch IsItemAllowed to allow all ores in blast furnace
        [HarmonyPatch("IsItemAllowed", new Type[] { typeof(ItemDrop.ItemData) })]
        [HarmonyPrefix]
        public static bool IsItemAllowed_ItemData_Prefix(Smelter __instance, ItemDrop.ItemData item, ref bool __result)
        {
            // Only apply if the feature is enabled
            if (!BetterSmeltingPlugin.AllOresInBlastFurnace.Value)
                return true; // Run original method

            // Only apply to blast furnaces
            if (!__instance.m_name.ToLower().Contains("blastfurnace"))
                return true; // Run original method for non-blast furnaces

            // Check if this is an ore item
            if (item.m_shared.m_name.ToLower().Contains("ore"))
            {
                __result = true; // Allow any ore
                return false; // Skip original method
            }

            return true; // Run original method for non-ore items
        }

        // Also patch the string version of IsItemAllowed
        [HarmonyPatch("IsItemAllowed", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        public static bool IsItemAllowed_String_Prefix(Smelter __instance, string itemName, ref bool __result)
        {
            // Only apply if the feature is enabled
            if (!BetterSmeltingPlugin.AllOresInBlastFurnace.Value)
                return true; // Run original method

            // Only apply to blast furnaces
            if (!__instance.m_name.ToLower().Contains("blastfurnace"))
                return true; // Run original method for non-blast furnaces

            // Check if this is an ore item
            if (itemName.ToLower().Contains("ore"))
            {
                __result = true; // Allow any ore
                return false; // Skip original method
            }

            return true; // Run original method for non-ore items
        }

        // Additional patch for OnAddOre to enforce our custom limits
        [HarmonyPatch("OnAddOre")]
        [HarmonyPrefix]
        public static bool OnAddOre_Prefix(Smelter __instance, ref bool __result, Switch sw, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return true; // Let the original method handle finding an item
            }
            
            if (!__instance.IsItemAllowed(item.m_dropPrefab.name))
            {
                return true; // Let the original method handle invalid items
            }
            
            int queueSize = __instance.GetQueueSize();
            int maxOre = __instance.m_maxOre; // This will already be our modified value from Awake_Postfix
            
            if (queueSize >= maxOre)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                __result = false;
                return false; // Skip original method
            }
            
            return true; // Let the original method continue
        }
        
        // Additional patch for OnAddFuel to enforce our custom limits
        [HarmonyPatch("OnAddFuel")]
        [HarmonyPrefix]
        public static bool OnAddFuel_Prefix(Smelter __instance, ref bool __result, Switch sw, Humanoid user, ItemDrop.ItemData item)
        {
            float currentFuel = __instance.GetFuel();
            int maxFuel = __instance.m_maxFuel; // This will already be our modified value from Awake_Postfix
            
            if ((double)currentFuel > (double)(maxFuel - 1))
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                __result = false;
                return false; // Skip original method
            }
            
            return true; // Let the original method continue
        }
        
        // Patch to modify smelting speed by patching GetDeltaTime
        [HarmonyPatch("GetDeltaTime")]
        [HarmonyPostfix]
        public static void GetDeltaTime_Postfix(ref double __result)
        {
            // Apply smelting speed multiplier if configured to be different from default
            if (BetterSmeltingPlugin.SmeltingSpeedMultiplier.Value != 1.0f)
            {
                __result *= BetterSmeltingPlugin.SmeltingSpeedMultiplier.Value;
            }
        }
    }
}
