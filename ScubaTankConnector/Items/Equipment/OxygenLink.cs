using System.Collections.Generic;
using UnityEngine;

namespace OxygenLink
{
    public class OxygenLink : MonoBehaviour, IEquippable
    {
        private bool EventsRegistered = false;
        public List<Oxygen> LinkedSources = new List<Oxygen>();
        public void OnEquip(GameObject sender, string slot)
        {
            FindAndLinkAllSources();
            RegisterEvents();
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
            if (!this.LinkedSources.Contains(source))
            {
                this.LinkedSources.Add(source);
                Player.main.oxygenMgr.RegisterSource(source);
                Plugin.Logger.LogDebug($"Detected new oxygen source! [{source.name}]");
            };
        }
        public void UnlinkOxygenSource(Oxygen source)
        {
            if (this.LinkedSources.Contains(source))
            {
                this.LinkedSources.Remove(source);
                Player.main.oxygenMgr.UnregisterSource(source);
            };
        }
        public void FindAndLinkAllSources()
        {
            foreach (Oxygen source in GetOxygenSources())
            {
                LinkOxygenSource(source);
            }
        }
        public void UnlinkAllSources()
        {
            List<Oxygen> sourcesCopy = new List<Oxygen>(this.LinkedSources);
            foreach (Oxygen source in sourcesCopy)
            {
                UnlinkOxygenSource(source);
            }
        }

        public void OnItemAdded(InventoryItem item)
        {
            if (item.item.gameObject.TryGetComponent<Oxygen>(out Oxygen source))
            {
                LinkOxygenSource(source);
            }
        }
        public void OnItemRemoved(InventoryItem item)
        {
            if (item.item.gameObject.TryGetComponent<Oxygen>(out Oxygen source))
            {
                if (this.LinkedSources.Contains(source))
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