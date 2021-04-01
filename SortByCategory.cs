using System;
using System.Linq;

namespace SortByCategory
{
    public static class SortByCategory
    {
        enum CustomItemType
        {
            None,
            Junk,
        }

        private static CustomItemType GetCustomItemType(Kingmaker.Items.ItemEntity item)
        {
            if (!Main.Settings.SortJunk || item.Blueprint.IsNotable || (item.Cost == 0 && item.TotalWeight == 0))
            {
                return CustomItemType.None;
            }

            if (item.Blueprint.MiscellaneousType != Kingmaker.Blueprints.Items.BlueprintItem.MiscellaneousItemType.None)
            {
                return CustomItemType.Junk;
            }

            if (!(item.Blueprint is Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipment))
            {
                if (item.Blueprint.TrashLootTypes.Length > 0)
                {
                    return CustomItemType.Junk;
                }
            }

            return CustomItemType.None;
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.UI.Common.UIUtilityItem), "GetItemType")]
        public static class UIUtilityItem_GetItemType_Patch
        {
            public static void Postfix(Kingmaker.Items.ItemEntity item, ref string __result)
            {
                if (Main.Enabled && __result == "")
                {
                    if (GetCustomItemType(item) == CustomItemType.Junk)
                    {
                        __result = "Junk";
                        return;
                    }

                    if (Main.Settings.MoreCategories)
                    {
                        if (Kingmaker.Blueprints.BlueprintExtenstions.GetComponent<Kingmaker.Blueprints.Items.Components.CopyRecipe>(item.Blueprint))
                        {
                            __result = "Recipe";
                        }
                        else if (item.Blueprint is Kingmaker.Blueprints.Items.BlueprintItemKey)
                        {
                            __result = "Key";
                        }
                        else if (item.Blueprint.name.StartsWith("Artifact_"))
                        {
                            __result = "Antique artifact";
                        }
                        else if (item.Blueprint.name.StartsWith("Antique_"))
                        {
                            __result = "Antique relic";
                        }
                        else if (item.Blueprint.name.StartsWith("Memento_"))
                        {
                            __result = "Antique";
                        }
                        else if (item.Blueprint.IsNotable)
                        {
                            return;
                        }

                        if (item.Blueprint.FlavorText.Contains("cooking ingredient"))
                        {
                            __result = "Cooking ingredient";
                        }
                        else if (item.Blueprint is Kingmaker.Blueprints.Items.BlueprintItemNote)
                        {
                            __result = "Book";
                        }
                        else
                        {
                            switch (item.Blueprint.ItemType)
                            {
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Weapon:
                                    __result = "Weapon";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Shield:
                                    __result = "Shield";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Armor:
                                    __result = "Armor";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Ring:
                                    __result = "Ring";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Belt:
                                    __result = "Belt";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Feet:
                                    __result = "Boots";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Gloves:
                                    __result = "Gloves";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Head:
                                    __result = "Headgear";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Neck:
                                    __result = "Amulet";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Shoulders:
                                    __result = "Cloak";
                                    break;
                                case Kingmaker.UI.Common.ItemsFilter.ItemType.Wrist:
                                    __result = "Bracelet";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.UI.Common.ItemsFilter), "CompareByTypeAndName")]
        public static class ItemsFilter_CompareByTypeAndName_Patch
        {
            public static bool Prefix(Kingmaker.Items.ItemEntity a, Kingmaker.Items.ItemEntity b, Kingmaker.UI.Common.ItemsFilter.FilterType filter, ref int __result)
            {
                if (Main.Enabled)
                {
                    if (Kingmaker.UI.Common.ItemsFilter.GetItemType(a, filter) >= Kingmaker.UI.Common.ItemsFilter.ItemType.Other ||
                        Kingmaker.UI.Common.ItemsFilter.GetItemType(b, filter) >= Kingmaker.UI.Common.ItemsFilter.ItemType.Other)
                    {
                        return true;
                    }

                    int customCompare = GetCustomItemType(a).CompareTo(GetCustomItemType(b));
                    if (customCompare != 0)
                    {
                        __result = customCompare;
                        return false;
                    }

                    if (Kingmaker.UI.Common.ItemsFilter.GetItemType(a, filter) == Kingmaker.UI.Common.ItemsFilter.GetItemType(b, filter))
                    {
                        int categoryCompare = Kingmaker.UI.Common.UIUtilityItem.GetItemType(a).CompareTo(Kingmaker.UI.Common.UIUtilityItem.GetItemType(b));
                        if (categoryCompare == 0)
                        {
                            int priceCompare = Main.Settings.SortByPrice ? a.Cost.CompareTo(b.Cost) : 0;
                            if (priceCompare == 0)
                            {
                                __result = string.Compare(a.Name, b.Name, StringComparison.Ordinal);

                            }
                            else
                            {
                                __result = priceCompare;

                            }
                        }
                        else
                        {
                            __result = categoryCompare;
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(Kingmaker.UI.Vendor.VendorMassSale), "WantToBeSold")]
        public static class VendorMassSale_WantToBeSold_Patch
        {
            public static void Postfix(Kingmaker.UI.Vendor.VendorMassSale __instance, Kingmaker.Items.ItemEntity item, ref bool __result)
            {
                if (Main.Enabled && Main.Settings.JunkMassSale && !__result && item != null && GetCustomItemType(item) == CustomItemType.Junk)
                {
                    __result = true;
                }
            }
        }
    }
}
