using System.Collections.Generic;
using System.Linq;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace JoysOfEfficiency.Automation
{
    internal class AnimalAutomation
    {
        private static Config Config => InstanceHolder.Config;

        private static readonly Logger Logger = new Logger("AnimalAutomation");

        public static void LetAnimalsInHome()
        {
            Farm farm = Game1.getFarm();
            foreach (KeyValuePair<long, FarmAnimal> kv in farm.animals.Pairs.ToArray())
            {
                FarmAnimal animal = kv.Value;
                Logger.Log(
                    $"Warped {animal.displayName}({animal.shortDisplayType()}) to {animal.displayHouse}@[{animal.home.animalDoor.X}, {animal.home.animalDoor.Y}]");
                animal.warpHome();
            }
        }

        public static void AutoOpenAnimalDoor()
        {
            if (Game1.IsWinter)
            {
                Logger.Log("Don't open the animal door because it's winter");
                return;
            }

            if (Game1.isRaining || Game1.isSnowing)
            {
                Logger.Log("Don't open the animal door because of rainy/snowy weather.");
                return;
            }


            Farm farm = Game1.getFarm();
            foreach (Building building in farm.buildings)
            {
                switch (building.buildingType.Value)
                {
                    case "Coop":
                    case "Barn":
                    {
                        if (building.indoors.Value is AnimalHouse house)
                        {
                            if (house.animals.Any() && !building.animalDoorOpen.Value)
                            {
                                building.ToggleAnimalDoor(Game1.player);
                            }
                        }
                        break;
                    }
                }
            }
        }

        public static void AutoCloseAnimalDoor()
        {
            Farm farm = Game1.getFarm();
            foreach (Building building in farm.buildings)
            {
                switch (building.buildingType.Value)
                {
                    case "Coop":
                    case "Barn":
                    {
                        if (building.indoors.Value is AnimalHouse house)
                        {
                            if (house.animals.Any() && building.animalDoorOpen.Value)
                            {
                                building.ToggleAnimalDoor(Game1.player);
                            }
                        }
                        break;
                    }
                }
            }
        }

        public static void PetNearbyPets()
        {
            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;

            Rectangle bb = Util.Expand(player.GetBoundingBox(), Config.AutoPetRadius * Game1.tileSize);

            foreach (Pet pet in location.characters.OfType<Pet>().Where(pet => pet.GetBoundingBox().Intersects(bb)))
            {
                bool wasPet = WasPetToday(pet);
                if (!wasPet)
                {
                    Logger.Log($"Petted {(pet.petType.Value == "Dog" ? "Dog" : "Cat")}'{pet.Name}' @{pet.position}");
                    pet.checkAction(player, location); // Pet pet... lol

                }
            }
        }

        public static void PetNearbyAnimals()
        {
            int radius = Config.AutoPetRadius * Game1.tileSize;
            Rectangle bb = Util.Expand(Game1.player.GetBoundingBox(), radius);
            foreach (FarmAnimal animal in GetAnimalsList(Game1.player))
            {
                if (Game1.timeOfDay >= 1900 && !animal.isMoving())
                {
                    continue;
                }
                if (!bb.Contains((int) animal.Position.X, (int) animal.Position.Y) || animal.wasPet.Value)
                {
                    continue;
                }

                Logger.Log($"Petted {animal.displayType}'{animal.Name}' @{animal.position}");
                animal.pet(Game1.player);
            }
        }

        public static void ShearingAndMilking(Farmer player)
        {
            int radius = InstanceHolder.Config.AnimalHarvestRadius * Game1.tileSize;
            Rectangle bb = Util.Expand(player.GetBoundingBox(), radius);
            foreach (FarmAnimal animal in GetAnimalsList(player))
            {
                string lowerType = animal.type.Value.ToLower();
                if (animal.currentProduce.Value is null || animal.isBaby() ||
                    player.CurrentTool == null || !animal.GetBoundingBox().Intersects(bb))
                {
                    continue;
                }

                if ((!lowerType.Contains("sheep") || !(player.CurrentTool is Shears) || !(player.Stamina >= 4f)) &&
                    (!lowerType.Contains("cow") || !(player.CurrentTool is MilkPail) || !(player.Stamina >= 4f)) &&
                    (!lowerType.Contains("goat") || !(player.CurrentTool is MilkPail) || !(player.Stamina >= 4f)))
                    continue;

                if (!player.addItemToInventoryBool(
                    new StardewValley.Object(animal.currentProduce.Value,
                        animal.hasEatenAnimalCracker.Value ? 2 : 1,
                        false,
                        -1,
                        animal.produceQuality.Value)))
                {
                    continue;
                }

                switch (player.CurrentTool)
                {
                    case Shears:
                        Shears.playSnip(player);
                        break;
                    case MilkPail:
                        player.currentLocation.localSound("Milking");
                        DelayedAction.playSoundAfterDelay("fishingRodBend", 300);
                        DelayedAction.playSoundAfterDelay("fishingRodBend", 1200);
                        break;
                    default: continue;
                }

                animal.doEmote(20);
                Game1.playSound("coin");
                animal.currentProduce.Value = null;
                animal.ReloadTextureIfNeeded();
                player.gainExperience(0, 5);
            }
        }

        private static bool WasPetToday(Pet pet)
        {
            return pet.lastPetDay.ContainsKey(Game1.player.UniqueMultiplayerID) &&
                   pet.lastPetDay[Game1.player.UniqueMultiplayerID] == Game1.Date.TotalDays;
        }


        private static IEnumerable<FarmAnimal> GetAnimalsList(Character player)
        {
            HashSet<FarmAnimal> list = new HashSet<FarmAnimal>();
            switch (player.currentLocation)
            {
                case Farm farm:
                {
                    list.AddRange(farm.animals.Values);
                    break;
                }

                case AnimalHouse house:
                {
                    list.AddRange(house.animals.Values);
                    break;
                }
            }
            return list;
        }

    }
}
