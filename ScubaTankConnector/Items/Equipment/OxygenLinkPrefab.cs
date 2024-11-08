using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Assets.Gadgets;

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

            var oxygenLinkObj = new CloneTemplate(Info, TechType.Tank);
            oxygenLinkObj.ModifyPrefab += obj =>
            {
                GameObject.DestroyImmediate(obj.GetComponent<Oxygen>());
                obj.AddComponent<OxygenLink>();
            };

            oxygenLinkPrefab.SetGameObject(oxygenLinkObj);

            CraftingGadget craftingGadget = oxygenLinkPrefab.SetRecipe(GetRecipe())
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "Equipment");

            EquipmentGadget equipmentGadget = oxygenLinkPrefab.SetEquipment(EquipmentType.Tank);

            oxygenLinkPrefab.SetPdaGroupCategory(TechGroup.Personal, TechCategory.Equipment);

            KnownTechHandler.RemoveDefaultUnlock(Info.TechType);
            KnownTechHandler.AddRequirementForUnlock(Info.TechType, TechType.Welder);

            oxygenLinkPrefab.Register();
            Plugin.Logger.LogInfo("Prefab OxygenLink is registered!");
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
}
