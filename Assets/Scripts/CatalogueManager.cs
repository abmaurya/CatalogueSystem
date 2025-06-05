using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
        /// Loads catalogue data from a JSON TextAsset.
        /// </summary>
        public void LoadCatalogFromJson(string catalogueJsonData)
        {
            _catalogueData = JsonConvert.DeserializeObject<CatalogueData>(catalogueJsonData);
            if (_catalogueData.Products != null)
                _allItems.AddRange(_catalogueData.Products);
            if (_catalogueData.Bundles != null)
                _allItems.AddRange(_catalogueData.Bundles);
        }

        public List<CatalogueItem> GetAllItems()
        {
            // Return a copy to avoid direct external modification.
            return new List<CatalogueItem>(_allItems);
        }

        public List<CatalogueItem> FilterAlltems(System.Func<CatalogueItem, bool> predicate)
        {
            return _allItems.Where(predicate).ToList();
        }

        public List<CatalogueItem> SortItems<TKey>(System.Func<CatalogueItem, TKey> predicate, bool ascending = true)
        {
            return SortGivenItems(_allItems, predicate, ascending);
        }

        /// <summary>
        /// Sorts catalogue items by a custom item order. For a product, its associated item is used;
        /// for a bundle, the first matching item (lowest index in customOrder) is used.
        /// </summary>
        public List<CatalogueItem> SortItemsByCustomOrder(List<EItemType> customOrder)
        {
            return SortGivenItemsByCustomOrder(_allItems, customOrder);
        }

        public List<CatalogueItem> GetAllProducts()
        {
            List<CatalogueItem> products = new(_catalogueData.Products);
            return products;
        }

        public List<CatalogueItem> GetAllBundles()
        {
            List<CatalogueItem> bundles = new(_catalogueData.Bundles);
            return bundles;
        }

        public List<CatalogueItem> FilterAllItems(System.Func<CatalogueItem, bool> predicate)
        {
            throw new System.NotImplementedException();
        }

        public List<CatalogueItem> FilterAllProducts(System.Func<CatalogueItem, bool> predicate)
        {
            return _catalogueData.Products.Where(predicate).ToList();
        }

        public List<CatalogueItem> FilterAllBundles(System.Func<CatalogueItem, bool> predicate)
        {
            return _catalogueData.Bundles.Where(predicate).ToList();
        }

        public List<CatalogueItem> FilterFromGivenItems(List<CatalogueItem> items, System.Func<CatalogueItem, bool> predicate)
        {
            return items.Where(predicate).ToList();
        }

        public List<CatalogueItem> SortGivenItems<TKey>(List<CatalogueItem> items, System.Func<CatalogueItem, TKey> keySelector, bool ascending = true)
        {
            return ascending ? items.OrderBy(keySelector).ToList()
                             : items.OrderByDescending(keySelector).ToList();
        }

        public List<CatalogueItem> SortGivenItemsByCustomOrder(List<CatalogueItem> items, List<EItemType> customOrder)
        {
            return items.OrderBy(item =>
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