using System;
using JoysOfEfficiency.Automation;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Huds;
using JoysOfEfficiency.Misc;
using JoysOfEfficiency.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace JoysOfEfficiency.EventHandler
{
    internal class UpdateEvents
    {
        public static bool DayEnded { get; set; }

        private int _ticks;


        private static Config Conf => InstanceHolder.Config;

        private static readonly Logger Logger = new Logger("UpdateEvent");

        // This updates every GameTick (approx 60x / second)
        public void OnGameUpdateEvent(object sender, UpdateTickedEventArgs args)
        {
            OnEveryUpdate();
            if (args.IsMultipleOf(6))
            {
                OnGameSixthUpdate();
            }
        }

        public void OnEveryUpdate()
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            IdlePause.OnTickUpdate();

            Farmer player = Game1.player;
            if (Conf.AutoGate)
            {
                FenceGateAutomation.TryToggleGate(player);
            }

            if (player.CurrentTool is FishingRod rod)
            {
                FishingProbabilitiesBox.UpdateProbabilities(rod);
                AutoFisher.AfkFishing();
            }

            GiftInformationTooltip.UpdateTooltip();
        }

        // Every sixth tick.  Approx 0.10 seconds
        private void OnGameSixthUpdate()
        {
            if (Game1.currentGameTime == null || !Context.IsWorldReady || !Context.IsPlayerFree)
            {
                return;
            }

            if (Conf.CloseTreasureWhenAllLooted && Game1.activeClickableMenu is ItemGrabMenu menu)
            {
                InventoryAutomation.TryCloseItemGrabMenu(menu);
            }

            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;
            try
            {
                if (Game1.currentLocation is MineShaft { isFallingDownShaft: true })
                {
                    return;
                }
                if (!Context.CanPlayerMove)
                {
                    return;
                }
                if (Conf.AutoReelRod)
                {
                    AutoFisher.AutoReelRod();
                }

                // Balanced mode tries to keep JoE actions to once every 1.0 second
                _ticks = (_ticks + 1) % 6;
                if (Conf.BalancedMode && _ticks != 0)
                {
                    return;
                }


                FarmCleaner.OnSixthUpdate();
                if (Conf.UnifyFlowerColors)
                {
                    FlowerColorUnifier.UnifyFlowerColors();
                }
                if (Conf.AutoEat)
                {
                    FoodAutomation.TryToEatIfNeeded(player);
                }
                if (Conf.AutoPickUpTrash)
                {
                    TrashCanScavenger.ScavengeTrashCan();
                }
                if (Conf.AutoWaterNearbyCrops)
                {
                    HarvestAutomation.WaterNearbyCrops();
                }
                if (Conf.AutoPetNearbyAnimals)
                {
                    AnimalAutomation.PetNearbyAnimals();
                }
                if (Conf.AutoShearingAndMilking)
                {
                    AnimalAutomation.ShearingAndMilking(player);
                }
                if (Conf.AutoPullMachineResult)
                {
                    MachineOperator.PullMachineResult();
                }
                if (Conf.AutoDepositIngredient)
                {
                    MachineOperator.DepositIngredientsToMachines();
                }
                if (Conf.AutoHarvest)
                {
                    HarvestAutomation.HarvestNearbyCrops(player);
                }
                if (Conf.AutoDestroyDeadCrops)
                {
                    HarvestAutomation.DestroyNearDeadCrops(player);
                }
                if (Conf.AutoRefillWateringCan)
                {
                    WateringCanRefiller.RefillWateringCan();
                }
                if (Conf.AutoCollectCollectibles)
                {
                    CollectibleCollector.CollectNearbyCollectibles(location);
                }
                if (Conf.AutoDigArtifactSpot)
                {
                    ArtifactSpotDigger.DigNearbyArtifactSpots();
                }
                if (Conf.AutoShakeFruitedPlants)
                {
                    HarvestAutomation.ShakeNearbyFruitedTree();
                    HarvestAutomation.ShakeNearbyFruitedBush();
                }
                if (Conf.AutoAnimalDoor && !DayEnded && Game1.timeOfDay >= 1900)
                {
                    DayEnded = true;
                    EventHolder.Save.OnBeforeSave(null, null);
                }
                if (Conf.AutoPetNearbyPets)
                {
                    AnimalAutomation.PetNearbyPets();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Source);
                Logger.Error(ex.ToString());
            }
        }
    }
}
