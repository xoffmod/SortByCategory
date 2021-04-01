using System;
using UnityModManagerNet;

namespace SortByCategory
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Sort by price within a category")]
        public bool SortByPrice = true;

        [Draw("More categories")]
        public bool MoreCategories = true;

        [Draw("Sort junk (bottom)")]
        public bool SortJunk = true;

        [Draw("Sell junk in mass sale")]
        public bool JunkMassSale = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        void IDrawable.OnChange()
        {
        }
    }

#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        public static HarmonyLib.Harmony HarmonyInstance;

        public static bool Enabled = true;

        public static Settings Settings = new Settings();

        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
            modEntry.OnUnload = new Func<UnityModManager.ModEntry, bool>(Main.OnUnload);

            HarmonyInstance = new HarmonyLib.Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(typeof(Main).Assembly);

            return true;
        }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
#if DEBUG
            HarmonyInstance.UnpatchAll(null);
#endif
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Draw(modEntry);
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
    }
}
