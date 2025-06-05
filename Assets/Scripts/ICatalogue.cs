using System.Collections.Generic;

namespace MAG_I.ShopCatalogue
{
    public interface ICatalogue
    {
        /// <summary>
        /// Returns all the items in the catalogue
        /// </summary>
        /// <returns>List of catalogue items</returns>
        List<CatalogueItem> GetAllItems();

        /// <summary>
        /// Filters items from all the catalogue items based on predicate
        /// </summary>
        /// <param name="predicate">The function that dictates the filtering</param>
        /// <returns>List of filtered items</returns>
        List<CatalogueItem> FilterAllItems(System.Func<CatalogueItem, bool> predicate);

        /// <summary>
        /// Sort all the items in ascending or descending order based on a predicate
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="predicate">The function that dictates the filtering</param>
        /// <param name="ascending">Sort in ascending order</param>
        /// <returns>List of sorted items</returns>
        List<CatalogueItem> SortAllItems<TKey>(System.Func<CatalogueItem, TKey> predicate, bool ascending = true);

        /// <summary>
        /// Sort all the items in ascending or descending order based on a predicate
        /// </summary>
        /// <param name="customOrder">List of item type</param>
        /// <returns>List of sorted items</returns>
        List<CatalogueItem> SortAllItemsByCustomOrder(List<EItemType> customOrder);
        
        
        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        List<CatalogueItem> GetAllProducts();
        
        /// <summary>
        /// Get all bundles
        /// </summary>
        /// <returns>List of all bundles</returns>
        List<CatalogueItem> GetAllBundles();

        /// <summary>
        /// Filter products from all the products
        /// </summary>
        /// <param name="predicate">The function that dictates the product filtering</param>
        /// <returns>List of filtered products</returns>
        List<CatalogueItem> FilterAllProducts(System.Func<CatalogueItem, bool> predicate);

        /// <summary>
        /// Filter bundles from all the bundles
        /// </summary>
        /// <param name="predicate">The function that dictates the bundle filtering</param>
        /// <returns>List of filtered bundles</returns>
        List<CatalogueItem> FilterAllBundles(System.Func<CatalogueItem, bool> predicate);



        /// <summary>
        /// Filter items from a given list of items
        /// </summary>
        /// <param name="items">List of items to sort</param>
        /// <param name="predicate">The function that dictates the items filtering</param>
        /// <returns>List of filtered items</returns>
        List<CatalogueItem> FilterFromGivenItems(List<CatalogueItem> items, System.Func<CatalogueItem, bool> predicate);

        /// <summary>
        /// Sort items from a given list of items
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="items">List of items to sort</param>
        /// <param name="predicate">The function that dictates the items sorting</param>
        /// <param name="ascending">Default true, sets order of sorting to ascending if true</param
        /// <returns>List of sorted items</returns>
        List<CatalogueItem> SortGivenItems<TKey>(List<CatalogueItem> items, System.Func<CatalogueItem, TKey> predicate, bool ascending = true);

        /// <summary>
        /// Custom sorting of items from a given list of items
        /// </summary>
        /// <param name="items">List of items to sort</param>
        /// <param name="customOrder">The function that dictates the items sorting</param>
        /// <returns>List of custom sorted items</returns>
        List<CatalogueItem> SortGivenItemsByCustomOrder(List<CatalogueItem> items, List<EItemType> customOrder);
    }
}