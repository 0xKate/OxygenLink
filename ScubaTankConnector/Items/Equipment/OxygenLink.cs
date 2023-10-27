using Nautilus.Assets.PrefabTemplates;
using Nautilus.Assets;
using Nautilus.Crafting;
using System;
using System.Collections.Generic;
using Nautilus.Utility;
using UnityEngine;
using Nautilus.Assets.Gadgets;
using static CraftData;

namespace OxygenLink
{
    public static class OxygenLinkPrefab
    {
        public static PrefabInfo Info { get; } = PrefabInfo
            .WithTechType("OxygenLink", "Oxygen Link", "Links all connected tanks in the inventory.")
            .WithIcon(ImageUtils.LoadSpriteFromFile(System.IO.Path.Combine(Plugin.ASSETS_FOLDER_LOCATION, "OxygenLink.png")))
            .WithSizeInInventory(new Vector2int() { x = 1, y = 1 });

        public static void Register()
        {
            var oxygenLinkPrefab = new CustomPrefab(Info);

            var oxygenLinkObj = new CloneTemplate(Info, TechType.PlasteelTank);
            oxygenLinkObj.ModifyPrefab += obj =>
            {
                GameObject.DestroyImmediate(obj.GetComponent<Oxygen>());
                obj.AddComponent<OxygenLink>();
            };

            oxygenLinkPrefab.SetGameObject(oxygenLinkObj);
            oxygenLinkPrefab
                .SetRecipe(GetRecipe())
                .WithFabricatorType(CraftTree.Type.Fabricator)
                .WithStepsToFabricatorTab("Personal", "Equipment");
            oxygenLinkPrefab.SetEquipment(EquipmentType.Tank);
            oxygenLinkPrefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Equipment);
            oxygenLinkPrefab.Register();
        }

        public static RecipeData GetRecipe()
        {
            var numIngredients = (int)Config.RecipeDifficulty;
            List<Ingredient> ingredients = Config.RecipeDifficulty switch
            {
                Difficulty.Easy => new List<Ingredient>()
                    {
                        new Ingredient(TechType.Silicone, 1),
                        new Ingredient(TechType.Titanium, 3),
                        new Ingredient(TechType.Lubricant, 2)
                    },
                Difficulty.Medium => new List<Ingredient>()
                    {
                        new Ingredient(TechType.Silicone, 2),
                        new Ingredient(TechType.Titanium, 4),
                        new Ingredient(TechType.Lubricant, 2),
                        new Ingredient(TechType.Glass, 2),
                        new Ingredient(TechType.ComputerChip, 1),
                        new Ingredient(TechType.WiringKit, 1)
                    },
                Difficulty.Hard => new List<Ingredient>()
                    {
                        new Ingredient(TechType.Silicone, 4),
                        new Ingredient(TechType.TitaniumIngot, 2),
                        new Ingredient(TechType.Lubricant, 4),
                        new Ingredient(TechType.EnameledGlass, 2),
                        new Ingredient(TechType.AdvancedWiringKit, 2),
                        new Ingredient(TechType.Polyaniline, 2),
                        new Ingredient(TechType.Aerogel, 2),
                    },
                _ => throw new InvalidOperationException("Invalid difficulty level") { },
            };

            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = ingredients
            };
        }
    }


    public class OxygenLink : MonoBehaviour, IEquippable
    {
        public List<Oxygen> LinkedSources = new List<Oxygen>();
        public void OnEquip(GameObject sender, string slot)
        {
            GetOxygenSources().ForEach(source => LinkOxygenSource(source));
            Inventory.main.container.onAddItem += OnItemAdded;
            Inventory.main.container.onRemoveItem += OnItemRemoved;
            Plugin.Logger.LogDebug("OxygenLink Equipped!");
        }
        public void OnUnequip(GameObject sender, string slot)
        {
            Inventory.main.container.onAddItem -= OnItemAdded;
            Inventory.main.container.onRemoveItem -= OnItemRemoved;
            LinkedSources.ForEach(source => UnlinkOxygenSource(source));
            Plugin.Logger.LogDebug("OxygenLink Un-Equipped!");
        }

        public List<Oxygen> GetOxygenSources()
        {
            List<Oxygen> oxygenSources = new List<Oxygen>();

            foreach (TechType techType in Inventory.main.container.GetItemTypes())
            {
                foreach (InventoryItem item in Inventory.main.container.GetItems(techType))
                {
                    if (item.item.gameObject.TryGetComponent<Oxygen>(out Oxygen oxygen))
                    {
                        oxygenSources.Add(oxygen);
                    }
                }
            }

            return oxygenSources;
        }

        public void LinkOxygenSource(Oxygen source)
        {
            this.LinkedSources.Add(source);
            Player.main.oxygenMgr.RegisterSource(source);
        }

        public void UnlinkOxygenSource(Oxygen source)
        {
            this.LinkedSources.Remove(source);
            Player.main.oxygenMgr.UnregisterSource(source);
        }

        public void UpdateEquipped(GameObject sender, string slot)
        {
        }
        private void OnItemAdded(InventoryItem item)
        {
            if (item.item.gameObject.TryGetComponent<Oxygen>(out Oxygen source))
            {
                if (!Player.main.oxygenMgr.sources.Contains(source))
                {
                    LinkOxygenSource(source);
                    Plugin.Logger.LogDebug($"Detected new oxygen source! [{source.name}]");
                };
            }
        }
        private void OnItemRemoved(InventoryItem item)
        {
            if (item.item.gameObject.TryGetComponent<Oxygen>(out Oxygen source))
            {
                if (Player.main.oxygenMgr.sources.Contains(source))
                {
                    UnlinkOxygenSource(source);
                    Plugin.Logger.LogDebug($"Detected removed oxygen source! [{source.name}]");
                };
            }
        }
        private void OnDestroy()
        {
            Inventory.main.container.onAddItem -= OnItemAdded;
            Inventory.main.container.onRemoveItem -= OnItemRemoved;
            LinkedSources.ForEach(source => UnlinkOxygenSource(source));
            Plugin.Logger.LogDebug("OxygenLink Component Destroyed!");
        }
    }
}