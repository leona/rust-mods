using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Kits", "Leon", "1.0.0")]
    [Description("Automatically gives selected kit")]
    class AutoKits : CovalencePlugin
    {
        #region Definitions

        [PluginReference]
        private Plugin PlayerDatabase, RateLimit;

        private KitData kitData;
        private DynamicConfigFile kitdata;

        float saveInterval = 60f * 10f;

        #endregion Definitions

        private void SaveKitData() => kitdata.WriteObject(kitData);
        void OnServerShutdown() => SaveKitData();
        void OnServerSave() => SaveKitData();
        
        private void LoadData()
        {
            kitdata = Interface.Oxide.DataFileSystem.GetFile("AutoKits/kits_data");

            try {
                kitData = kitdata.ReadObject<KitData>();
            } catch {
                kitData = new KitData();
            }
        }

        void OnPluginLoaded(Plugin plugin) {
            LoadData();
            InvokeHandler.Instance.InvokeRepeating(() => SaveKitData(), saveInterval, saveInterval); 
        }

        object OnPlayerRespawn(BasePlayer player) {
            string playerKit = getPlayerKit(player.IPlayer);

            InvokeHandler.Instance.Invoke(() => changeLoadout(player.IPlayer, playerKit), 0.5f);
            
            return null;
        }


        [Command("clearinv")]
        void clearInventory(IPlayer _player) {
            var player = _player.Object as BasePlayer;
            Puts("Clearing inventory");

            foreach(var item in player.inventory.containerBelt.itemList) {
                item.Remove();
            }
            foreach (var item in player.inventory.containerMain.itemList) {
                item.Remove();
            }
            foreach (var item in player.inventory.containerWear.itemList) {
                item.Remove();
            }	
        }

        // Save loadout order for player
        [Command("loadoutsave")]
        void saveLoadoutCommand(IPlayer player, string command, string[] args) {
            string name = getPlayerKit(player);
            var basePlayer = player.Object as BasePlayer;
            var copy = CopyItemsFrom(basePlayer);
            kitData.storeKit(copy, name, basePlayer);
        }

        // Store loadout for all players
        [Command("loadoutstore"), Permission("autokits.admin")]
        void storeLoadoutCommand(IPlayer player, string command, string[] args) {
            var basePlayer = player.Object as BasePlayer;
            var name = args[0];

            var copy = CopyItemsFrom(basePlayer);
            kitData.storeKit(copy, name);
            SaveKitData();
            player.Reply("Stored loadout");
        }

        [HookMethod("clearAndBlock")]
        public void clearAndBlock(IPlayer player) {
            clearInventory(player);
            RateLimit.Call("blockPlayer", player.Id);
        }
        
        // Redeem loadout
        [Command("loadout")]
        void loadoutCommand(IPlayer player, string command, string[] args) {
            var name = args[0].ToLower();

            if (!kitData.kits.ContainsKey(name)) {
                player.Reply("Invalid loadout name");
                return;
            }

            if ((bool) RateLimit.Call("afterLife", player.Id, 30)) {
                player.Reply("You can only change your loadout within 30 seconds of respawning");
                return;
            }

            string playerKit = getPlayerKit(player).ToLower();

            Puts($"Current kit: {playerKit}");
            
            if (name == playerKit) {
                player.Reply("This loadout is already selected");
                return;
            }

            player.Reply($"New loadout: {name} - Old loadout: {playerKit}");
            PlayerDatabase.Call("SetPlayerData", player.Id, "default_kit", name);
            changeLoadout(player, name);
        }

        // Get saved player kit name or default
        string getPlayerKit(IPlayer player) {
            var kit = PlayerDatabase.Call("GetPlayerData", player.Id, "default_kit");

            if (kit == null) {
                return "ak";
            } else {
                return (string) kit;
            }
        }

        // Clear inventory and give items
        void changeLoadout(IPlayer player, string name) {
            var basePlayer = player.Object as BasePlayer;

            clearInventory(player);
            var kit = kitData.getKit(name, basePlayer);

            timer.In(1f, () => { // minimum 2 for fixing it bugging out
                if (kit != null) {
                    GiveItemsTo(basePlayer, kit);
                } else {
                    player.Reply("Kit does not exist");
                }
            });
        }

        // Below mostly from the Kits plugin
        private void CopyItems(ref ItemData[] array, ItemContainer container, int limit)
        {
            limit = Mathf.Min(container.itemList.Count, limit);

            Array.Resize(ref array, limit);

            for (int i = 0; i < limit; i++)                    
                array[i] = new ItemData(container.itemList[i]);                    
        }

        private void GiveItems(ItemData[] items, ItemContainer container, ref List<ItemData> leftOverItems, bool isWearContainer = false) {
            for (int i = 0; i < items.Length; i++)
            {
                ItemData itemData = items[i];
                if (itemData.Amount < 1)
                    continue;

                if (container.GetSlot(itemData.Position) != null)
                    leftOverItems.Add(itemData);
                else
                {
                    Item item = CreateItem(itemData);
                    if (!isWearContainer || (isWearContainer && item.info.isWearable))
                    {
                        item.position = itemData.Position;
                        item.SetParent(container);
                    }
                    else
                    {
                        leftOverItems.Add(itemData);
                        item.Remove(0f);
                    }
                }
            }
        }

        Kit CopyItemsFrom(BasePlayer player)
        {
            var kitData = new Kit();
            ItemData[] array = kitData.MainItems;
            CopyItems(ref array, player.inventory.containerMain, 24);
            kitData.MainItems = array;

            array = kitData.WearItems;
            CopyItems(ref array, player.inventory.containerWear, 7);
            kitData.WearItems = array;

            array = kitData.BeltItems;
            CopyItems(ref array, player.inventory.containerBelt, 6);
            kitData.BeltItems = array;
            return kitData;
        }

        private static Item CreateItem(ItemData itemData)
        {
            Item item = ItemManager.CreateByItemID(itemData.ItemID, itemData.Amount, itemData.Skin);
            item.condition = itemData.Condition;
            item.maxCondition = itemData.MaxCondition;

            if (itemData.Frequency > 0)
            {
                ItemModRFListener rfListener = item.info.GetComponentInChildren<ItemModRFListener>();
                if (rfListener != null)
                    (BaseNetworkable.serverEntities.Find(item.instanceData.subEntity) as PagerEntity)?.ChangeFrequency(itemData.Frequency);  
            }

            if (itemData.BlueprintItemID != 0)
            {
                if (item.instanceData == null)
                    item.instanceData = new ProtoBuf.Item.InstanceData();

                item.instanceData.ShouldPool = false;

                item.instanceData.blueprintAmount = 1;
                item.instanceData.blueprintTarget = itemData.BlueprintItemID;

                item.MarkDirty();
            }

            BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
            if (weapon != null)
            {
                if (!string.IsNullOrEmpty(itemData.Ammotype))
                    weapon.primaryMagazine.ammoType = ItemManager.FindItemDefinition(itemData.Ammotype);
                weapon.primaryMagazine.contents = itemData.Ammo;
            }

            FlameThrower flameThrower = item.GetHeldEntity() as FlameThrower;
            if (flameThrower != null)
                flameThrower.ammo = itemData.Ammo;

            if (itemData.Contents != null)
            {
                foreach (ItemData contentData in itemData.Contents)
                {
                    Item newContent = ItemManager.CreateByItemID(contentData.ItemID, contentData.Amount);
                    if (newContent != null)
                    {
                        newContent.condition = contentData.Condition;
                        newContent.MoveToContainer(item.contents);
                    }
                }
            }

            item.MarkDirty();

            return item;
        }

        void GiveItemsTo(BasePlayer player, Kit kit)
        {
            List<ItemData> list = Facepunch.Pool.GetList<ItemData>();

            GiveItems(kit.MainItems, player.inventory.containerMain, ref list);
            GiveItems(kit.WearItems, player.inventory.containerWear, ref list, true);
            GiveItems(kit.BeltItems, player.inventory.containerBelt, ref list);
        }

    }


    class Kit {
        public ItemData[] MainItems { get; set; } = new ItemData[0];
        public ItemData[] WearItems { get; set; } = new ItemData[0];
        public ItemData[] BeltItems { get; set; } = new ItemData[0];
        
        [JsonIgnore]
        private JObject _jObject;

        [JsonIgnore]
        internal JObject ToJObject
        {
            get
            {
                if (_jObject == null)
                {
                    _jObject = new JObject {
                        ["Name"] = "ak",
                        ["MainItems"] = new JArray(),
                        ["WearItems"] = new JArray(),
                        ["BeltItems"] = new JArray()
                    };

                    for (int i = 0; i < MainItems.Length; i++)                            
                        (_jObject["MainItems"] as JArray).Add(MainItems[i].ToJObject);

                    for (int i = 0; i < WearItems.Length; i++)
                        (_jObject["WearItems"] as JArray).Add(WearItems[i].ToJObject);

                    for (int i = 0; i < BeltItems.Length; i++)
                        (_jObject["BeltItems"] as JArray).Add(BeltItems[i].ToJObject);
                }

                return _jObject;
            }
        }

        internal void All(ref List<ItemData> list) {
            foreach(var item in MainItems) list.Add(item);
            foreach(var item in BeltItems) list.Add(item);
            foreach(var item in WearItems) list.Add(item);
        }

        internal Dictionary<string, int> Amounts() {
            List<ItemData> all = new List<ItemData>();
            All(ref all);
            List<ItemData> combined = new List<ItemData>();
            Dictionary<string, int> amounts = new Dictionary<string, int>();
    
            foreach(var item in all) {
                combined.Add(item);

                if (item.Contents == null) continue;

                foreach (var _item in item.Contents.ToList()) {
                    combined.Add(_item);
                }
            }

            foreach(var item in combined) {
                if (item.Shortname == null || item.Amount == null) {
                    continue;
                }

                if (!amounts.ContainsKey(item.Shortname)) {
                    amounts[item.Shortname] = 0;
                }

                amounts[item.Shortname] += item.Amount;
            }

            return amounts;
        }
    }

    class KitData {
        [JsonProperty]
        public Dictionary<string, Kit> kits = new Dictionary<string, Kit>();

        [JsonProperty]
        public Dictionary<ulong, Dictionary<string, Kit>> playerKits = new Dictionary<ulong, Dictionary<string, Kit>>();

        public void storeKit(Kit kit, string name, BasePlayer player = null) {
            if (player == null) {
                kits[name] = kit;
                return;
            }
    
            if (!kits.ContainsKey(name)) {
                Console.WriteLine("Kit does not exist to version");
                return;
            }
            
            var amount1 = kits[name].Amounts();
            var amount2 = kit.Amounts();
            
            if (!Compare(amount1, amount2)) {
                Console.WriteLine("Kits are not equal");
                player.IPlayer.Reply("Cannot save loadout after picking up or using items");
                return;
            }


            if (!playerKits.ContainsKey(player.userID)) {
                playerKits[player.userID] = new Dictionary<string, Kit>();
            }

            playerKits[player.userID][name] = kit;
            Console.WriteLine("Saved kit order");
            player.IPlayer.Reply("Saved loadout");
        }

        public Kit getKit(string name, BasePlayer player) {
            if (!kits.ContainsKey(name)) {
                Console.WriteLine("Kit does not exist");
                return null;
            }

            if (playerKits.ContainsKey(player.userID) && playerKits[player.userID].ContainsKey(name)) {
                return playerKits[player.userID][name];
            } else {
                return kits[name];
            }
        }

        public bool Compare(Dictionary<string, int> dict, Dictionary<string, int> dict2) {
            var equal = false;

            if (dict.Count == dict2.Count) {
                equal = true;
                foreach (var pair in dict)
                {
                    int value;
                    if (dict2.TryGetValue(pair.Key, out value))
                    {
                        if (value != pair.Value) {
                            equal = false;
                            break;
                        }
                    } else {
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }
    }

    class ItemData
    {
        public string Shortname { get; set; }

        public ulong Skin { get; set; }

        public int Amount { get; set; }

        public float Condition { get; set; }

        public float MaxCondition { get; set; }

        public int Ammo { get; set; }

        public string Ammotype { get; set; }

        public int Position { get; set; }

        public int Frequency { get; set; }

        public string BlueprintShortname { get; set; }

        public ItemData[] Contents { get; set; }

        private const string BLUEPRINT_BASE = "blueprintbase";

        [JsonIgnore]
        private int _itemId = 0;

        [JsonIgnore]
        private int _blueprintItemId = 0;

        [JsonIgnore]
        private JObject _jObject;

        
        [JsonIgnore]
        internal int ItemID
        {
            get
            {
                if (_itemId == 0)
                    _itemId = ItemManager.itemDictionaryByName[Shortname].itemid;
                return _itemId;
            }
        }

        [JsonIgnore]
        internal bool IsBlueprint => Shortname.Equals(BLUEPRINT_BASE);

        [JsonIgnore]
        internal int BlueprintItemID
        {
            get
            {
                if (_blueprintItemId == 0 && !string.IsNullOrEmpty(BlueprintShortname))
                    _blueprintItemId = ItemManager.itemDictionaryByName[BlueprintShortname].itemid;
                return _blueprintItemId;
            }
        }

        [JsonIgnore]
        internal JObject ToJObject
        {
            get
            {
                if (_jObject == null)
                {
                    _jObject = new JObject
                    {
                        ["Shortname"] = Shortname,
                        ["SkinID"] = Skin,
                        ["Amount"] = Amount,
                        ["Condition"] = Condition,
                        ["MaxCondition"] = MaxCondition,
                        ["IsBlueprint"] = BlueprintItemID != 0,
                        ["Ammo"] = Ammo,
                        ["AmmoType"] = Ammotype,
                        ["Contents"] = new JArray()
                    };

                    for (int i = 0; i < Contents?.Length; i++)                        
                        (_jObject["Contents"] as JArray).Add(Contents[i].ToJObject);
                }

                return _jObject;
            }
        }

        internal ItemData Clone()
        {
            ItemData othercopy = (ItemData)this.MemberwiseClone();
            return othercopy;
        }

        internal ItemData() { }

        internal ItemData(Item item)
        {
            Shortname = item.info.shortname;
            Amount = item.amount;

            Ammotype = (item.GetHeldEntity() as BaseProjectile)?.primaryMagazine.ammoType.shortname ?? null;
            Ammo = item.GetHeldEntity() is BaseProjectile ? (item.GetHeldEntity() as BaseProjectile).primaryMagazine.contents :
                            item.GetHeldEntity() is FlameThrower ? (item.GetHeldEntity() as FlameThrower).ammo : 0;

            Position = item.position;
            Skin = item.skin;

            Condition = item.condition;
            MaxCondition = item.maxCondition;

            Frequency = ItemModAssociatedEntity<PagerEntity>.GetAssociatedEntity(item)?.GetFrequency() ?? -1;

            if (item.instanceData != null && item.instanceData.blueprintTarget != 0)
                BlueprintShortname = ItemManager.FindItemDefinition(item.instanceData.blueprintTarget).shortname;

            Contents = item.contents?.itemList.Select(item1 => new ItemData(item1)).ToArray();
        }

        public class InstanceData
        {
            public int DataInt { get; set; }
            public int BlueprintTarget { get; set; }
            public int BlueprintAmount { get; set; }
            public uint SubEntityNetID { get; set; }

            internal InstanceData() { }

            internal InstanceData(Item item)
            {
                if (item.instanceData == null)
                    return;

                DataInt = item.instanceData.dataInt;
                BlueprintAmount = item.instanceData.blueprintAmount;
                BlueprintTarget = item.instanceData.blueprintTarget;
            }

            internal void Restore(Item item)
            {
                if (item.instanceData == null)
                    item.instanceData = new ProtoBuf.Item.InstanceData();

                item.instanceData.ShouldPool = false;

                item.instanceData.blueprintAmount = BlueprintAmount;
                item.instanceData.blueprintTarget = BlueprintTarget;
                item.instanceData.dataInt = DataInt;

                item.MarkDirty();
            }

            internal bool IsValid => DataInt != 0 || BlueprintAmount != 0 || BlueprintTarget != 0;                
        }
    }

}
