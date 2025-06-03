using System;
using System.Collections;
using System.Collections.Generic;

namespace MAG_I.ShopCatalogue
{
    public enum EItemType
    {
        Coins,
        Gems,
        Tickets
    }

    [System.Serializable]
    public class CatalogueItem
    {
        public string Id;
        public string Name;
        public string ShortDescription;
        public float Price;
    }

    [System.Serializable]
    public class Product : CatalogueItem
    {
        public EItemType ItemType;
        public uint Amount;
    }

    [System.Serializable]
    public class BundleItem
    {
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
    }


    [System.Serializable]
    public sealed class CatalogueData
    {
        public List<Product> Products = new();
        public List<Bundle> Bundles = new();
    }
}