﻿using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CraftData;

namespace OxygenLink
{
    public static class OxygenLinkPrefab
    {
        public static PrefabInfo Info { get; } = PrefabInfo
            .WithTechType("OxygenLink", "Oxygen Link", "Shares oxygen with all tanks in the inventory while equipped.")
            .WithIcon(ImageUtils.LoadSpriteFromFile(System.IO.Path.Combine(Plugin.AssetFolder, "OxygenLink.png")))
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
            var numIngredients = (int)Settings.Current.RecipeDifficulty;
            List<Ingredient> ingredients = Settings.Current.RecipeDifficulty switch
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
        private bool EventsRegistered = false;
        public List<Oxygen> LinkedSources = new List<Oxygen>();
        public void OnEquip(GameObject sender, string slot)
        {
            RegisterEvents();
            GetOxygenSources().ForEach(source => LinkOxygenSource(source));
            Plugin.Logger.LogDebug("OxygenLink Equipped!");
        }
        public void OnUnequip(GameObject sender, string slot)
        {
            UnregisterEvents();
            UnlinkAllSources();
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
        public void UnlinkAllSources()
        {
            new List<Oxygen>(LinkedSources).ForEach(UnlinkOxygenSource);
        }
        public void OnItemAdded(InventoryItem item)
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
        public void OnItemRemoved(InventoryItem item)
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
        public void OnDeath(object sender, Extensions.EventArgs<Player> e)
        {
            if (Settings.Current.DestroyOnDeath)
            {
                UnregisterEvents();
                UnlinkAllSources();
                Destroy(gameObject);
            }

        }
        public void OnDestroy()
        {
            UnregisterEvents();
            UnlinkAllSources();
        }
        public void RegisterEvents()
        {
            if (!EventsRegistered)
            {
                EventsRegistered = true;
                Inventory.main.container.onAddItem += OnItemAdded;
                Inventory.main.container.onRemoveItem += OnItemRemoved;
                Events.OnPlayerDeath += OnDeath;
            }
        }
        public void UnregisterEvents()
        {
            if (EventsRegistered)
            {
                Inventory.main.container.onAddItem -= OnItemAdded;
                Inventory.main.container.onRemoveItem -= OnItemRemoved;
                Events.OnPlayerDeath -= OnDeath;
                EventsRegistered = false;
            }
        }
        public void UpdateEquipped(GameObject sender, string slot) {}
    }
}