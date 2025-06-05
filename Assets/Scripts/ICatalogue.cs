using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG_I.ShopCatalogue
{
    public interface ICatalogue
    {
        List<CatalogueItem> GetAllItems();
        List<CatalogueItem> FilterAllItems(System.Func<CatalogueItem, bool> predicate);
        List<CatalogueItem> SortItems<TKey>(System.Func<CatalogueItem, TKey> keySelector, bool ascending = true);
        List<CatalogueItem> SortItemsByCustomOrder(List<EItemType> customOrder);
        
        
        
        List<CatalogueItem> GetAllProducts();
        List<CatalogueItem> GetAllBundles();
        List<CatalogueItem> FilterAllProducts(System.Func<CatalogueItem, bool> predicate);
        List<CatalogueItem> FilterAllBundles(System.Func<CatalogueItem, bool> predicate);

        List<CatalogueItem> FilterFromGivenItems(List<CatalogueItem> items, System.Func<CatalogueItem, bool> predicate);
        List<CatalogueItem> SortGivenItems<TKey>(List<CatalogueItem> items, System.Func<CatalogueItem, TKey> keySelector, bool ascending = true);
        List<CatalogueItem> SortGivenItemsByCustomOrder(List<CatalogueItem> items, List<EItemType> customOrder);
    }
}