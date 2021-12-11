using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Network;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;



namespace Oxide.Plugins
{
    [Info("DisableLoot", "Leon", "1.0.0")]
    [Description("DisableLoot")]
    class DisableLoot : CovalencePlugin
    {
        #region Definitions

        #endregion Definitions

        List<object> prefabs = new List<object>() {
            "assets/bundled/prefabs/radtown/crate_basic.prefab",
            "assets/bundled/prefabs/radtown/crate_elite.prefab",
            "assets/bundled/prefabs/radtown/crate_mine.prefab",
            "assets/bundled/prefabs/radtown/crate_normal.prefab",
            "assets/bundled/prefabs/radtown/crate_normal_2.prefab",
            "assets/bundled/prefabs/radtown/crate_normal_2_food.prefab",
            "assets/bundled/prefabs/radtown/crate_normal_2_medical.prefab",
            "assets/bundled/prefabs/radtown/crate_tools.prefab",
            "assets/bundled/prefabs/radtown/crate_underwater_advanced.prefab",
            "assets/bundled/prefabs/radtown/crate_underwater_basic.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm ammo.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm c4.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm construction resources.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm construction tools.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm food.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm medical.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm res.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm tier1 lootbox.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm tier2 lootbox.prefab",
            "assets/bundled/prefabs/radtown/dmloot/dm tier3 lootbox.prefab",
            "assets/bundled/prefabs/radtown/vehicle_parts.prefab",
            "assets/bundled/prefabs/radtown/foodbox.prefab",
            "assets/bundled/prefabs/radtown/loot_barrel_1.prefab",
            "assets/bundled/prefabs/radtown/loot_barrel_2.prefab",
            "assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-1.prefab",
            "assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-2.prefab",
            "assets/bundled/prefabs/autospawn/resource/loot/trash-pile-1.prefab",
            "assets/bundled/prefabs/radtown/loot_trash.prefab",
            "assets/bundled/prefabs/radtown/minecart.prefab",
            "assets/bundled/prefabs/radtown/oil_barrel.prefab",
            "assets/prefabs/npc/m2bradley/bradley_crate.prefab",
            "assets/prefabs/npc/patrol helicopter/heli_crate.prefab",
            "assets/prefabs/deployable/chinooklockedcrate/codelockedhackablecrate.prefab",
            "assets/prefabs/deployable/chinooklockedcrate/codelockedhackablecrate_oilrig.prefab",
            "assets/prefabs/misc/supply drop/supply_drop.prefab",
            // NPCs/Pickups
            "assets/bundled/prefabs/autospawn/collectable/berry-black/berry-black-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/berry-blue/berry-blue-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/berry-green/berry-green-collectable.prefab",	
            "assets/bundled/prefabs/autospawn/collectable/berry-red/berry-red-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/berry-white/berry-white-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/berry-yellow/berry-yellow-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/corn/corn-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/hemp/hemp-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/mushrooms/mushroom-cluster-5.prefab",
            "assets/bundled/prefabs/autospawn/collectable/mushrooms/mushroom-cluster-6.prefab",
            "assets/bundled/prefabs/autospawn/collectable/potato/potato-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/pumpkin/pumpkin-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/halloween/halloween-bone-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/halloween/halloween-metal-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/halloween/halloween-stone-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/halloween/halloween-sulfur-collectible.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/halloween/halloween-wood-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/metal-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/stone-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/stone/sulfur-collectable.prefab",
            "assets/bundled/prefabs/autospawn/collectable/wood/wood-collectable.prefab",
            "assets/bundled/prefabs/autospawn/phonebooth/phonebooth.static.prefab",
            "assets/bundled/prefabs/autospawn/resource/driftwood/driftwood_1.prefab",
            "assets/bundled/prefabs/autospawn/resource/driftwood/driftwood_2.prefab",
            "assets/bundled/prefabs/autospawn/resource/driftwood/driftwood_3.prefab",
            "assets/bundled/prefabs/autospawn/resource/driftwood/driftwood_4.prefab",
            "assets/bundled/prefabs/autospawn/resource/driftwood/driftwood_5.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_dry/dead_log_a.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_dry/dead_log_b.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_dry/dead_log_c.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_snow/dead_log_a.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_snow/dead_log_b.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_snow/dead_log_c.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_wet/dead_log_a.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_wet/dead_log_b.prefab",
            "assets/bundled/prefabs/autospawn/resource/logs_wet/dead_log_c.prefab",
            "assets/bundled/prefabs/autospawn/resource/loot/trash-pile-1.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores/metal-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores/stone-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores/sulfur-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores_sand/metal-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores_sand/stone-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores_sand/sulfur-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores_snow/metal-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores_snow/stone-ore.prefab",
            "assets/bundled/prefabs/autospawn/resource/ores_snow/sulfur-ore.prefab",

            // NPCs
            "assets/prefabs/npc/bear/bear_full.prefab",
            "assets/prefabs/npc/scientist/groundpatrolpurple.prefab",
            "assets/rust.ai/agents/wolf/wolf.prefab",
            "assets/rust.ai/agents/boar/boar.prefab",
            "assets/rust.ai/agents/chicken/chicken.prefab",
            "assets/rust.ai/agents/bear/bear.prefab",

            // Vehicles
            /* // Hot air balloon. Causes parent issue.
            "assets/prefabs/deployable/hot air balloon/hotairballoon.prefab",
            "assets/prefabs/deployable/hot air balloon/subents/hab_storage.prefab",*/
            
            // Horses
            
            "assets/rust.ai/agents/horse/horse.prefab",
            "assets/rust.ai/agents/stag/stag.prefab",
           // "assets/rust.ai/nextai/testridablehorse.prefab", // Causes error BaseVehicle mount

            // Cars
         
            "assets/content/vehicles/modularcar/module_entities/1module_engine.prefab",
            "assets/content/vehicles/modularcar/module_entities/2module_passengers.prefab",
         //   "assets/content/vehicles/modularcar/module_entities/2module_fuel_tank.prefab",
            "assets/content/vehicles/modularcar/module_entities/2module_flatbed.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_passengers_armored.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_flatbed.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_rear_seats.prefab",
           // "assets/content/vehicles/modularcar/module_entities/1module_storage.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_taxi.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_cockpit_armored.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_cockpit_with_engine.prefab",
            "assets/content/vehicles/modularcar/module_entities/1module_cockpit.prefab",
            
            /*
            "assets/content/vehicles/modularcar/2module_car_spawned.entity.prefab",
            "assets/content/vehicles/modularcar/3module_car_spawned.entity.prefab",
            "assets/content/vehicles/modularcar/4module_car_spawned.entity.prefab",
           // "assets/content/vehicles/modularcar/_base_car_chassis.entity.prefab", errors
            "assets/content/vehicles/modularcar/car_chassis_2module.entity.prefab",
            "assets/content/vehicles/modularcar/car_chassis_3module.entity.prefab",
            "assets/content/vehicles/modularcar/car_chassis_4module.entity.prefab",
            */
            
/*
            "assets/content/vehicles/modularcar/subents/modular_car_1mod_storage.prefab",
            "assets/content/vehicles/modularcar/subents/modular_car_1mod_trade.prefab",
            "assets/content/vehicles/modularcar/subents/modular_car_2mod_fuel_tank.prefab",
            "assets/content/vehicles/modularcar/subents/modular_car_fuel_storage.prefab",
            "assets/content/vehicles/modularcar/subents/modular_car_i4_engine_storage.prefab",
            "assets/content/vehicles/modularcar/subents/modular_car_v8_engine_storage.prefab",*/

            // Scientists

            /*
            "assets/prefabs/npc/scientist/htn/scientist_astar_full_any.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_full_any.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_full_lr300.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_full_mp5.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_full_pistol.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_full_shotgun.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_junkpile_pistol.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_turret_any.prefab",
            "assets/prefabs/npc/scientist/htn/scientist_turret_lr300.prefab",
            "assets/prefabs/npc/scientist/scientist.prefab",
            "assets/prefabs/npc/scientist/scientist_corpse.prefab",
            "assets/prefabs/npc/scientist/scientist_gunner.prefab",
            "assets/prefabs/npc/scientist/scientistjunkpile.prefab",
            "assets/prefabs/npc/scientist/scientistpeacekeeper.prefab",
            "assets/prefabs/npc/scientist/scientiststationary.prefab",*/
        };

        void OnEntitySpawned(BaseNetworkable networkable)
        {
            if (!CheckNetworkable(networkable))
            {
                var container = networkable as LootContainer;
                if (container == null)
                    return;
                if (container.inventory == null || container.inventory.itemList == null)
                {
                    return;
                }

                /* TODO - block certain items from spawning in it if the box is not blocked
                foreach (Item item in container.inventory.itemList)
                {
                    item.Remove(0f);
                    item.RemoveFromContainer();
                }*/
            }
        }

        private bool CheckNetworkable(BaseNetworkable networkable)
        {
            if (isBlocked(networkable.name, networkable.PrefabName, networkable.ShortPrefabName))
            {
                networkable.Kill();
                return true;
            }

            return false;
        }

        private bool isBlocked(string name, string prefab, string shortprefab) {
            return prefabs.Contains(prefab);
        }
    }
}