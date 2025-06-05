using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAG_I.ShopCatalogue.Tests
{
    public class TestCatalogue
    {

        // A Test behaves as an ordinary method
        [Test]
        public void RunExamples()
        {
            // Initialize and load the catalogue.
            var _jsonCatalogue = Resources.Load<TextAsset>("Data/CatalogueData");
            var _catalogueManager = new CatalogueManager();
            _catalogueManager.LoadCatalogueFromJson(_jsonCatalogue.text);
            // Get all items.
            Debug.Log("=== All Catalogue Items ===");
            var allitems = _catalogueManager.GetAllItems();
            Assert.AreEqual(4, allitems.Count);
            foreach (var item in allitems)
            {
                Debug.Log(item.Name + " - $" + item.Price);
            }

            // Example: Sort by descending Price.
            var sortedByPriceDesc = _catalogueManager.SortAllItems(item => item.Price, false);
            Debug.Log("=== Sorted by Price (Descending) ===");
            Assert.IsTrue(sortedByPriceDesc[0].Price > sortedByPriceDesc[1].Price, "Sorting didn't work");
            foreach (var item in sortedByPriceDesc)
            {
                Debug.Log(item.Name + " - $" + item.Price);
            }

            // Filter items to show only those involving Coins or Tickets.
            var filteredItems = _catalogueManager.FilterAlltems(item =>
            {
                if (item is Product prod)
                {
                    return prod.ItemType == EItemType.Coins || prod.ItemType == EItemType.Tickets;
                }
                else if (item is Bundle bundle)
                {
                    // Return true if any entry in bundle matches.
                    foreach (var bundleItem in bundle.Items)
                    {
                        if (bundleItem.ItemType == EItemType.Coins || bundleItem.ItemType == EItemType.Tickets)
                            return true;
                    }
                }

                return false;
            });

            Debug.Log("=== Filtered Items (Coins and Tickets only) ===");
            bool bundleContainsItem = false;
            foreach (var item in filteredItems)
            {
                if (item is Product prod)
                {
                    Assert.IsTrue(prod.ItemType == EItemType.Coins || prod.ItemType == EItemType.Tickets);
                }
                else if (item is Bundle bundle)
                {
                    // Return true if any entry in bundle matches.
                    foreach (var bundleItem in bundle.Items)
                    {
                        if (bundleItem.ItemType == EItemType.Coins || bundleItem.ItemType == EItemType.Tickets)
                        {
                            bundleContainsItem = true;
                        }
                    }
                    Assert.IsTrue(bundleContainsItem);
                }
                Debug.Log(item.Name);
            }

            // Custom sort by a custom item ordering.
            var customOrder = new List<EItemType> { EItemType.Gems, EItemType.Coins, EItemType.Tickets };
            var sortedByCustomOrder = _catalogueManager.SortAllItemsByCustomOrder(customOrder);
            Debug.Log("=== Sorted by Custom Item Order (Gems > Coins > Tickets) ===");
            Assert.AreEqual(sortedByCustomOrder[0].GetItemType(), EItemType.Gems);
            foreach (var item in sortedByCustomOrder)
            {
                Debug.Log(item.Name);
            }
        }

    }
}