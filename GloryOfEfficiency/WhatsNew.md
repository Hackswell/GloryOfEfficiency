﻿# Changelog
## Glory of Efficiency 1.0.1
- Fixed bug with flower Color Unifier
- Added proper update key to manifest
- Auto-dig artifact spots works for extra goodies in SVE
  - Actually a Farm Type Manager compatibility issue
- Auto-harvest on IslandWest now collects Golden Walnuts
- Removed accidental personal commit to change window size. Now compatible with "Default Window Size" mod. 

## Glory of Efficiency 1.0.0
- Forked from Joys of Efficiency under GPL.
- Update build files to work with net6.0 / SdV 1.6 [Hackswell + others]
  - Removed all Reflections and use updated public methods
  - Converted all ObjectIDs from Int32 to string.  Still unoptimized, but should be more compatible.
- Added config option "AutoDepositSeedMaker" to NOT auto-drop into Seed Makers. [Hackswell]
- Added new config item "MachineTypes" in config.json.  [Hackswell]
  - All machines are enumerated by default now, and up to date as of SdV 1.6.4.
  - If a new machine is added to SdV, you can manually add the name to the list. No need to recompile GoE!
- Added new config item "RunEveryNthTick".  If BalancedMode is FALSE, then run the GoE loops every "RunEveryNthTick"/60.0 seconds.
- Updated some of the Fishing HUDs and probability code. [Sandman534]
  - New config items:
    - FishingTackleInfo: true / FALSE [default]
    - TackleBoxAttach: true / FALSE [default]
    - TackleBoxCoordinates: [x, y]
    - ProbMaxFish: 10 [maximum number of probabilities to list]
- Added code from "Don't Eat This" mod by [Pyrohead37].  Module hasn't been updated since 2019.
  - New config items, loaded in this order:
    - DontEatThat: true / FALSE [default]
    - DontEatCategories: Which categories of items NOT to eat. Defaults are:
      - Artisan Goods
      - Animal Product
      - Crop
      - Fish
      - Flower
      - Harmful
      - Vegetable
    - DontEat: Which specific ITEMS not to eat. It's okay if they overlap categories.  Defaults:
      - Holly
      - Oil
      - Red Mushroom
      - Sap
      - Truffle
    - DoEat: Which specific ITEMS to ALLOW eating.  These are actually _removed_ from the "don't eat" list that was compiled from the above entries.  Defaults:
      - Mayonnaise
      - Beer
      - Green Tea
- Removed heaps of antiquated code from SdV 1.3/1.4 [Hackswell]
- May still contain inefficient algorithms. Still need to modernize some of the loops with "newly" available methods.


## Joys of Efficiency 1.4.9 [Hackswell fork]
- Updated build files to work with net5.0
- Updated code to work with HarmonyLib
- Moved from Newtonsoft.Json to System.Text.Json
- Reduce number of warnings
- Added Heavy Tapper to machine list for auto-collection
- Added new option to NOT auto-add to Seed Makers

## Joys of Efficiency 1.4.1
- Fixed Harmony related crash.
- Improved usability around menus.

## Joys of Efficiency 1.4.0
- Updated for SDV 1.5 and SMAPI 3.8.


## 1.3.0
- Update for SDV 1.4 and SMAPI 3.0

## 1.3.1
- Fixed AutoFishing moves the bar too quickly.
- Fixed IdlePause set illegal time after loading or when new day started.
- Implemented FarmCleaner function.
- Added DisableConfigLimitation for advanced users.

## 1.3.2
- Fixed code which may cause DivededByZeroException.
- Relocated MineInfoGui, so it won't be hidden by the health bar.
- Fixed CraftingFromChests was not working.

## 1.3.3
- Fixed AutoHarvest harvesting crops infinitely.

## 1.3.4
- Removed CraftingFromChests. If you want, I recommend using Convinient Chests instead.
- Added support for double gates.

## 1.3.5
- Fixed the problem that AutoGate did not close a fence gate when BalancedMode is enabled.
- Added coordinates option for Show Shipping Price feature.

## 1.3.6
- Now FishInformationHud shows correct quality when you are fishing perfectly.
- Fixed the problem that FishingProbabilitiesBox may throw an exception.

## 1.3.7
- Added AFK Fishing function.

## 1.3.8
- Now auto-casting works even if SafeMode is on.
- Fixed a bug that AutoDepositIngredient consume coffee beans if you don't have enough(<5) beans.
- Updated chinese translation by Lake1059. Many thanks!
- Now AutoShakeFruitedPlants supports tea bushes!

## 1.3.9
- Added Korean translation. (provided by wally232. Many thanks!)
- Graphical adjustment for Android.

## 1.3.10
- Adjusted MineInfoGui for Android.
- Adjusted FishingInfo for Android.
- Fixed a problem that GetTruePrice might throw NRE.

## Joys of Efficiency 1.0.22
- Moved Utilities to Util.cs
- Added Balanced mode
- Fixed Collect Collectibles feature couldn't work in coop and mine
- Changed distance determining algorithm
- Changed AutoShakeFruitedTree to AutoShakeFruitedPlants

## Joys of Efficiency 1.0.23
- Adjusted GiftInfo Window size
- Added FasterRunningSpeed Function

## Joys of Efficiency 1.0.24
- Tweaked Auto Gate Function

## Joys of Efficiency 1.0.25
- AddedSpeedMultiplier will be capped from 1 to 19
- Fixed Bug that continuously throws NullReferenceException in barn, coop, etc.
- Added AutoDepositIngredient and AutoPullMachineResult function.
- Removed collecting crub pot from AutoHarvest. Use AutoPullMachineResult instead.

## Joys of Efficiency 1.0.26
- Adjusted some default settings to be more fair.
- Added AutoPetNearbyPets feature.
- Added CJBCheatsMenu Detection.
- Fixed AutoReelRod issue.

## Joys of Efficiency 1.0.27
- Added ForceMaxCasting option

## Joys of Efficiency 1.0.28
- Changed Machine Detection algorithm
- Added ProtectNectarProducingFlower option to AutoHarvest.
- Removed ForceMaxCasting

## Joys of Efficiency 1.0.29
- Fixed bug that AutoDepositIngredient couldn't work well with furnace
- You must face to/back-to-back with fence to activate AutoGate

## Joys of Efficiency 1.0.30
- Reduced lag when using ProtectNectarProducingFlower.
- Fixed CanMax value did not changed by its upgrade level.
- Added FPS Counter.
- Fixed auto things couldn't work when not holding watering can

## Joys of Efficiency 1.0.31
- Updated languages

## Joys of Efficiency 1.0.32
- Removed FPSCounter and FastToolUpgrade because I added it to debug and don't need them anymore. (also, they're a bit cheaty)
- Removed FasterRunningSpeed because it causes many confliction with other mods or even game itselef.
- Re-adjusted BalancedMode to be more balanced.

## 1.0.33
- Fixed some sliders appear on inappropriate position.
- AutoEat won't execute when fishing or in menu.

## 1.0.34
- Added Harvest Exception

## 1.0.35
- Auto things won't work when falling down shaft or player can't move.
- Updated default.json

## 1.0.36
- Fixed casks absorbs ingredient infinitely when not in cellar.

## 1.0.37
- Added CasksAnywhere support.

## 1.0.38
- Pickup motion won't be played when riding.

## 1.0.39
- Added FishingProbabilitiesInfo feature.

## 1.0.40
- Updated de.json and es.json
- Added Bee house, mushroom box, etc. support.

## 1.0.41
- Added 'CraftingFromChests' feature.

## 1.0.42
- Show the reason why you can't gift when using GiftInfo.
- Added Wells support for auto-refill watering can.

## 1.2.0
- Updated for SDV 1.3(non-beta)
- Removed debugging feature.
- Moved UI settings to UIs Tab.
- Added ShowShippingPrice feature.

## 1.2.1
- added UnifyFlowerColors feature.

## 1.2.2
- added AutoLootTreasures feature.

## 1.2.3
- added compatibility for ChestsAnywhere created by Pathoschild.
- added game pad support in config menu.

## 1.2.4
- added scroll bar to Config menu.
- added CollectLetterAttachmentsAndQuests feature.
- added FilterBackgroundInMenu config to ConfigMenu.

## 1.2.5
- Removed MuchFasterBiting feature and Cheats tab from the menu.
- Added PauseWhenIdle feature.
- Added AutoPickUpTrash feature.

## 1.2.6
- Fixed problem couldn't assigning KeyToggleBlacklist from Config Menu.

## 1.2.7
- Added AutoShearingAndMilking feature.
- Fixed CloseTreasureWhenAllLooted may crush the dialogue in events.

## 1.2.8
- SButton will be used in config instead of Keys and Buttons enum.
- Fixed CollectLetterAttachmentsAndQuests was not appeared in config menu.

## 1.2.9
- Updated for SDV 1.3.32 and SMAPI 2.8
- Changed bee flower protection to be a area protection.

## 1.2.10
- Updated Russian Text.
- Updated for the latest SMAPI (>=2.9) events.

## 1.2.11
- Added support for CasksAnywhere mod.
- Fixed a bug around the config menu.
- Added Cordination Giving System.

## 1.2.12
- Edited FeaturesAndConfigs.md, so you can now see full list of features.
- Fixed Charcole Kiln consumed 1 extra wood when you used Auto Deposit Ingredients option.
- Rearranged folders and namespaces.
- Fixed some configs did not documented on FeaturesAndConfigs.md .
- Use Point type in config.json .
- Added MorePreciseProbabilities and TrialOfExamine option to FishingProbabilitiesInfo.

## 1.2.13
- Fixed indegredients were not consumed from fridge when cooking.
- Added SafeMode.

## 1.2.14
- Removed redundant yellow box on screen.
- Added selling price info on FishingInfo.

## 1.2.15
- Added Flower Color Registration menu.
- Fixed ProtectNectarProducingFlower was not working

## 1.2.16
- Added customized flower color unification.
