using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Network;
using Rust;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Oxide.Plugins
{
    [Info("Decay", "Leon", "1.0.0")]
    [Description("Decay settings")]
    class Decay : CovalencePlugin
    {

        // From NoDecay plugin

        public Dictionary<string, float> multipliers = new Dictionary<string, float>() {
                { "wall.external.high.wood", 5.0f },
                { "wall.external.high.stone", 5.0f },
                { "minicopter.entity", 10000.0f },
                { "rhib", 10000.0f },
                { "sedan", 10000.0f },
                { "sedantest.entity", 10000.0f },
                { "rowboat", 10000.0f },
        };

        object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || info.damageTypes == null || entity == null || !info.damageTypes.Has(DamageType.Decay)) return null;

            if (multipliers.ContainsKey(entity.ShortPrefabName)) {
                info.damageTypes.ScaleAll(multipliers[entity.ShortPrefabName]);
                if (!info.hasDamage) return true;
            }

            return null;
        }
    }
}
