using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MAG_I.ShopCatalogue
{
    public enum EItemType
    {
        Coins,
        Gems,
        Tickets
    }

    [System.Serializable]
    public abstract class CatalogueItem
    {
        public string Id;
        public string Name;
        public string ShortDescription;
        public float Price;
        public virtual EItemType? GetItemType()
        {
            return null;
        }
    }

    [System.Serializable]
    public class Product : CatalogueItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EItemType ItemType;
        public uint Amount;

        public override EItemType? GetItemType()
        {
            return ItemType;
        }
    }

    [System.Serializable]
    public class BundleItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EItemType ItemType;
        public uint Amount;
    }

    [System.Serializable]
    public class Bundle : CatalogueItem
    {
        private readonly int _totalPossibleItems = Enum.GetNames(typeof(EItemType)).Length;
        private List<BundleItem> _items = new();
        public IReadOnlyList<BundleItem> Items => _items;
        public Exception AddToBundle(BundleItem item)
        {
            if (_items.Count >= _totalPossibleItems)
            {
                throw new IndexOutOfRangeException($"Trying to add more items than the limit({_totalPossibleItems}).");
            }
            _items.Add(item);
            return null;
        }

        public void RemoveFromBundle(BundleItem item)
        {
            _items.Remove(item);
        }

        public void RemoveAtIndexFromBundle(int index)
        {
            _items.RemoveAt(index);
        }
    }


    [System.Serializable]
    public sealed class CatalogueData
    {
        public List<Product> Products;
        public List<Bundle> Bundles;
    }
}