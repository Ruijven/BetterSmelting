using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace BetterSmelting
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BetterSmeltingPlugin : BaseUnityPlugin
    {
        internal const string ModName = "BetterSmelting";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "ruijven";
        private const string ModGUID = $"{Author}.{ModName}";
        private static string ConfigFileName = $"{ModGUID}.cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource BetterSmeltingLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            // Uncomment the line below to use the LocalizationManager for localizing your mod.
            // Make sure to populate the English.yml file in the translation folder with your keys to be localized and the values associated before uncommenting!.
            //Localizer.Load(); // Use this to initialize the LocalizationManager (for more information on LocalizationManager, see the LocalizationManager documentation https://github.com/blaxxun-boop/LocalizationManager#example-project).

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            // Smelter configuration
            MaxSmelterOre = config("Smelter", "MaxOre", 10, 
                new ConfigDescription("Maximum amount of ore that can be loaded into a Smelter", 
                    new AcceptableValueRange<int>(10, 100)));

            MaxSmelterFuel = config("Smelter", "MaxFuel", 20, 
                new ConfigDescription("Maximum amount of fuel that can be loaded into a Smelter", 
                    new AcceptableValueRange<int>(20, 100)));

            // Blast Furnace configuration
            MaxBlastFurnaceOre = config("BlastFurnace", "MaxOre", 10, 
                new ConfigDescription("Maximum amount of ore that can be loaded into a Blast Furnace", 
                    new AcceptableValueRange<int>(10, 100)));

            MaxBlastFurnaceFuel = config("BlastFurnace", "MaxFuel", 20, 
                new ConfigDescription("Maximum amount of fuel that can be loaded into a Blast Furnace", 
                    new AcceptableValueRange<int>(20, 100)));

            // Feature configuration
            AllOresInBlastFurnace = config("Features", "AllOresInBlastFurnace", true,
                "If enabled, all ore types can be smelted in the Blast Furnace");
                
            // Smelting speed configuration
            SmeltingSpeedMultiplier = config("Features", "SmeltingSpeedMultiplier", 1.0f,
                new ConfigDescription("Multiplier for smelting speed (higher values = faster smelting)",
                    new AcceptableValueRange<float>(0.1f, 10.0f)));

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                BetterSmeltingLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                BetterSmeltingLogger.LogError($"There was an issue loading your {ConfigFileName}");
                BetterSmeltingLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        
        // Configuration entries
        public static ConfigEntry<int> MaxSmelterOre = null!;
        public static ConfigEntry<int> MaxSmelterFuel = null!;
        public static ConfigEntry<int> MaxBlastFurnaceOre = null!;
        public static ConfigEntry<int> MaxBlastFurnaceFuel = null!;
        public static ConfigEntry<bool> AllOresInBlastFurnace = null!;
        public static ConfigEntry<float> SmeltingSpeedMultiplier = null!;
        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public int? Order { get; set; }
            public bool? Browsable { get; set; }
            public string? Category { get; set; }
            public Action<ConfigEntryBase>? CustomDrawer { get; set; }
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                $"# Acceptable values: {string.Join(", ", UnityInput.Current.SupportedKeyCodes)}";
        }

        #endregion
    }

    public static class KeyboardExtensions
    {
        public static bool IsKeyDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) &&
                   shortcut.Modifiers.All(Input.GetKey);
        }
    }
}