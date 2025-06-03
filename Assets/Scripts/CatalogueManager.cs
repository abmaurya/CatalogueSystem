using System.Collections.Generic;
using System.Linq;
//This following is for JsonUtility, can be replaced by
//the one Provided by Microsoft -> JsonSerializer from System.Text.Json
using UnityEngine;

namespace MAG_I.ShopCatalogue
{
    public class CatalogueManager : ICatalogue
    {
        // Holds the catalogue data all products and bundles
        private List<CatalogueItem> _allItems = new List<CatalogueItem>();
        private CatalogueData _catalogueData;

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// Add new items to the catalogue with new entries
        /// </summary>
        /// <param name="items"></param>
        private void AddToCatalogue(List<CatalogueItem> items)
        {
            if (items == null)
            {
                return;
            }
            int totalItems = items.Count;
            for (int i = 0; i < totalItems; i++)
            {
                if (items[i] is Product product)
                {
                    _catalogueData.Products.Add(product);
                }
                else
                {
                    _catalogueData.Bundles.Add((Bundle)items[i]);
                }
            }

        }
        #endregion 

        /// <summary>
        /// Loads catalog data from a JSON TextAsset.
        /// </summary>
        public void LoadCatalogFromJson(string catalogueJsonData)
        {
            _catalogueData = JsonUtility.FromJson<CatalogueData>(catalogueJsonData);
            if (_catalogueData.Products != null)
                _allItems.AddRange(_catalogueData.Products);
            if (_catalogueData.Bundles != null)
                _allItems.AddRange(_catalogueData.Bundles);
        }

        public string GetCatalogueAsJson()
        {
            return JsonUtility.ToJson(_catalogueData);
        }

        public List<CatalogueItem> GetAllItems()
        {
            // Return a copy to avoid direct external modification.
            return new List<CatalogueItem>(_allItems);
        }

        public List<CatalogueItem> FilterItems(System.Func<CatalogueItem, bool> predicate)
        {
            return _allItems.Where(predicate).ToList();
        }

        public List<CatalogueItem> SortItems<TKey>(System.Func<CatalogueItem, TKey> keySelector, bool ascending = true)
        {
            return ascending ? _allItems.OrderBy(keySelector).ToList()
                             : _allItems.OrderByDescending(keySelector).ToList();
        }

        /// <summary>
        /// Sorts catalog items by a custom item order. For a product, its associated item is used;
        /// for a bundle, the first matching item (lowest index in customOrder) is used.
        /// </summary>
        public List<CatalogueItem> SortItemsByCustomOrder(List<EItemType> customOrder)
        {
            return _allItems.OrderBy(item =>
            {
                int index = int.MaxValue;
                if (item is Product prod)
                {
                    index = customOrder.IndexOf(prod.ItemType);
                }
                else if (item is Bundle bundle)
                {
                    // For bundles, determine the best index among all contained items.
                    int foundIndex = int.MaxValue;
                    foreach (BundleItem entry in bundle.Items)
                    {
                        int idx = customOrder.IndexOf(entry.ItemType);
                        if (idx != -1 && idx < foundIndex)
                            foundIndex = idx;
                    }
                    index = foundIndex;
                }
                return index;
            }).ToList();
        }
    }
}