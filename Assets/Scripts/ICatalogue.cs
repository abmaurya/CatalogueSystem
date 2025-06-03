using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAG_I.ShopCatalogue
{
    public interface ICatalogue
    {
        string GetCatalogueAsJson();
        List<CatalogueItem> GetAllItems();
        List<CatalogueItem> FilterItems(System.Func<CatalogueItem, bool> predicate);
        List<CatalogueItem> SortItems<TKey>(System.Func<CatalogueItem, TKey> keySelector, bool ascending = true);
        List<CatalogueItem> SortItemsByCustomOrder(List<EItemType> customOrder);
    }
}