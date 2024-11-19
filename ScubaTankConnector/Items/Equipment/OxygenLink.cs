using System.Collections.Generic;
using UnityEngine;

namespace OxygenLink
{
    public class OxygenLink : Oxygen, IOxygenSource, IEquippable, ISecondaryTooltip
    {
        private bool eventsRegistered;
        private List<Oxygen> linkedSources;

        public OxygenLink() : base()
        {
            eventsRegistered = false;
            linkedSources = new List<Oxygen>();
        }

        public new float oxygenAvailable => this.GetOxygenAvailable();
        public new float oxygenCapacity => this.GetOxygenCapacity();
        public new float oxygenValue => this.GetOxygenAvailable();
        public new bool isPlayer => false;

        private List<InventoryItem> GetAllItems()
        {
            var allItems = new List<InventoryItem>();
            foreach (var techType in Inventory.main.container.GetItemTypes())
            {
                allItems.AddRange(Inventory.main.container.GetItems(techType));
            }
            return allItems;
        }

        private void LinkOxygenSource(Oxygen source)
        {
            if (!linkedSources.Contains(source))
            {
                linkedSources.Add(source);
            }
        }

        private void UnlinkOxygenSource(Oxygen source)
        {
            if (linkedSources.Contains(source))
            {
                linkedSources.Remove(source);
            }
        }

        private void ProcessItem(InventoryItem item, bool remove = false)
        {
            if (item.item.gameObject.TryGetComponent(out Oxygen oxygen))
            {
                if (remove)
                    UnlinkOxygenSource(oxygen);
                else
                    LinkOxygenSource(oxygen);
            }
        }

        private void FindAndLinkAllSources()
        {
            foreach (var item in GetAllItems())
            {
                ProcessItem(item);
            }
        }

        private void UnlinkAllSources()
        {
            foreach (var item in GetAllItems())
            {
                ProcessItem(item, true);
            }
        }

        private void RegisterEvents()
        {
            if (!eventsRegistered)
            {
                eventsRegistered = true;
                Inventory.main.container.onAddItem += OnItemAdded;
                Inventory.main.container.onRemoveItem += OnItemRemoved;
                Events.OnPlayerDeath += OnDeath;
            }
        }

        private void UnregisterEvents()
        {
            if (eventsRegistered)
            {
                Inventory.main.container.onAddItem -= OnItemAdded;
                Inventory.main.container.onRemoveItem -= OnItemRemoved;
                Events.OnPlayerDeath -= OnDeath;
                eventsRegistered = false;
            }
        }

        public void OnItemAdded(InventoryItem item) => ProcessItem(item);

        public void OnItemRemoved(InventoryItem item) => ProcessItem(item, true);

        public void OnDeath(object sender, Extensions.EventArgs<Player> e)
        {
            if (Settings.Current.DestroyOnDeath)
            {
                UnregisterEvents();
                UnlinkAllSources();
                Destroy(gameObject);
            }
        }

        public new void OnDestroy()
        {
            UnregisterEvents();
            UnlinkAllSources();
        }

        public new void OnEquip(GameObject sender, string slot)
        {
            FindAndLinkAllSources();
            RegisterEvents();
            Player.main.oxygenMgr.RegisterSource(this);
        }

        public new void OnUnequip(GameObject sender, string slot)
        {
            UnregisterEvents();
            UnlinkAllSources();
            Player.main.oxygenMgr.UnregisterSource(this);
        }

        public new void UpdateEquipped(GameObject sender, string slot)
        {
        }

        public new float GetOxygenAvailable()
        {
            float totalOxygen = 0f;
            foreach (var source in linkedSources)
            {
                totalOxygen += source.GetOxygenAvailable();
            }
            return totalOxygen;
        }

        public new float GetOxygenCapacity()
        {
            float totalCapacity = 0f;
            foreach (var source in linkedSources)
            {
                totalCapacity += source.GetOxygenCapacity();
            }
            return totalCapacity;
        }

        public new float AddOxygen(float amount)
        {
            float remaining = amount;
            foreach (var source in linkedSources)
            {
                if (remaining <= 0f) break;
                remaining -= source.AddOxygen(remaining);
            }
            var res = amount - remaining;
            return res;
        }

        public new float RemoveOxygen(float amount)
        {
            float remaining = amount;
            foreach (var source in linkedSources)
            {
                if (remaining <= 0f) break;
                remaining -= source.RemoveOxygen(remaining);
            }
            var res = amount - remaining;
            return res;
        }

        public new bool IsPlayer() => false;

        public new bool IsBreathable() => linkedSources.Count > 0 && GetOxygenAvailable() > 0;

        public new string GetSecondaryTooltip()
        {
            return $"Oxygen: {GetOxygenAvailable()}s\n" +
                   $"Capacity: {GetOxygenCapacity()}s\n";
        }
    }
}
