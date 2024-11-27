using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Assets.Gadgets;
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
            var prefab = new CustomPrefab(Info);

            var template = new CloneTemplate(Info, TechType.PlasteelTank);
            template.ModifyPrefab += obj =>
            {
                GameObject.DestroyImmediate(obj.GetComponent<Oxygen>());
                OxygenLink oxygenLink = obj.AddComponent<OxygenLink>();                
            };

            prefab.SetGameObject(template);

            CraftingGadget craftingGadget = prefab.SetRecipe(GetRecipe())
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Equipment");

            EquipmentGadget equipmentGadget = prefab.SetEquipment(EquipmentType.Tank);

            prefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Equipment);

            KnownTechHandler.RemoveDefaultUnlock(Info.TechType);
            KnownTechHandler.UnlockOnStart(Info.TechType);

            prefab.Register();
            Plugin.Logger.LogInfo("Prefab OxygenLink is registered!");
        }
        public static RecipeData GetRecipe()
        {
            List<Ingredient> ingredients = Settings.Current.RecipeDifficulty switch
            {
                Difficulty.Easy => new List<Ingredient>()
                    {
                        new(TechType.Silicone, 1),
                        new(TechType.Titanium, 3),
                        new(TechType.Lubricant, 2)
                    },
                Difficulty.Medium => new List<Ingredient>()
                    {
                        new(TechType.Silicone, 2),
                        new(TechType.Titanium, 4),
                        new(TechType.Lubricant, 2),
                        new(TechType.Glass, 2),
                        new(TechType.ComputerChip, 1),
                        new(TechType.WiringKit, 1)
                    },
                Difficulty.Hard => new List<Ingredient>()
                    {
                        new(TechType.Silicone, 4),
                        new(TechType.TitaniumIngot, 2),
                        new(TechType.Lubricant, 4),
                        new(TechType.EnameledGlass, 2),
                        new(TechType.AdvancedWiringKit, 2),
                        new(TechType.Polyaniline, 2),
                        new(TechType.Aerogel, 2),
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
}
