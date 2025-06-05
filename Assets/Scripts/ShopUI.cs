using MAG_I.ShopCatalogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace MAG_I.ShopCatalogue
{
    #region UI Helper Enums 
    public enum ESortByValueType
    {
        None = 0,
        Name_Ascending,
        Name_Descending,
        Price_Ascending,
        Price_Descending,
    }

    public enum EItemTypeForFilter
    {
        All = 0,
        Coins,
        Gems,
        Tickets,

        Coins_or_Gems,
        Coins_or_Tickets,
        Gems_or_Tickets,

        Coins_and_Gems,
        Coins_and_Tickets,
        Gems_and_Tickets,
    }

    public enum ECustomSortType
    {
        None = 0,
        Coins_Gems_Tickets,
        Gems_Tickets_Coins,
        Tickets_Coins_Gems,

        Gems_Coins_Tickets,
        Coins_Tickets_Gems,
        Tickets_Gems_Coins,
    }
    #endregion

    public class ShopUI : MonoBehaviour
    {
        #region Data members
        [Tooltip("Assign the catalogue json file (as a TextAsset) from the Resources folder.")]
        [SerializeField]
        private TextAsset _jsonCatalogue;
        [SerializeField]
        private ScrollRect _shopScrollRect;
        [SerializeField]
        private GameObject _itemPrefab;
        [SerializeField]
        private Sprite _coinsSprite;
        [SerializeField]
        private Sprite _gemsSprite;
        [SerializeField]
        private Sprite _ticketsSprite;
        [SerializeField]
        private Sprite _bundleSprite;
        [SerializeField]
        private TMPro.TMP_Dropdown _customSortDropDown;
        [SerializeField]
        private TMPro.TMP_Dropdown _sortByValueDropDown;
        [SerializeField]
        private TMPro.TMP_Dropdown _filterDropDown;
        [SerializeField]
        private Button _productsButton;
        [SerializeField]
        private Button _bundlesButton;
        [SerializeField]
        private PurchasePopUp _purchasePopUp;

        private Queue<GameObject> _pooledShopItems;
        private List<GameObject> _shopItems;
        private CatalogueManager _catalogueManager;
        private EItemTypeForFilter _selectedFilter;
        private ESortByValueType _selectedSortByValueType;
        private ECustomSortType _selectedCustomSortType;
        private bool _isBundleOnDisplay;
        #endregion

        void Start()
        {
            _pooledShopItems = new Queue<GameObject>();
            _shopItems = new List<GameObject>();
            _isBundleOnDisplay = false;
            _selectedFilter = EItemTypeForFilter.All;
            _selectedSortByValueType = ESortByValueType.None;
            _selectedCustomSortType = ECustomSortType.None;

            // Initialize and load the catalogue.
            _catalogueManager = new CatalogueManager();
            _catalogueManager.LoadCatalogFromJson(_jsonCatalogue.text);
            ShowProducts();

            //Add Listeners
            _productsButton.onClick.AddListener(ShowProducts);
            _bundlesButton.onClick.AddListener(ShowBundles);

            var customSortItemTypes = new List<string>(System.Enum.GetNames(typeof(ECustomSortType)));
            for (int i = 0; i < customSortItemTypes.Count; i++)
            {
                customSortItemTypes[i] = customSortItemTypes[i].Replace("_", " > ");
            }
            _customSortDropDown.ClearOptions();
            _customSortDropDown.AddOptions(customSortItemTypes);
            _customSortDropDown.onValueChanged.AddListener(CustomSortCatalogueItems);

            var sortItemTypes = new List<string>(System.Enum.GetNames(typeof(ESortByValueType)));
            _sortByValueDropDown.ClearOptions();
            _sortByValueDropDown.AddOptions(sortItemTypes);
            _sortByValueDropDown.onValueChanged.AddListener(SortCatalogueByValue);

            var itemTypes = new List<string>(System.Enum.GetNames(typeof(EItemTypeForFilter)));
            _filterDropDown.ClearOptions();
            _filterDropDown.AddOptions(itemTypes);
            _filterDropDown.onValueChanged.AddListener(FilterCatalogueView);
            //RunExamples();
        }

        #region UI elements Event Listeners
        private void ShowProducts()
        {
            _isBundleOnDisplay = false;
            if (_selectedFilter == EItemTypeForFilter.All)
            {
                UpdateCatalogueView(_catalogueManager.GetAllProducts());
            }
            else
            {
                FilterCatalogueView((int)_selectedFilter);
            }
        }

        private void ShowBundles()
        {
            _isBundleOnDisplay = true;
            if (_selectedFilter == EItemTypeForFilter.All)
            {
                UpdateCatalogueView(_catalogueManager.GetAllBundles());
            }
            else
            {
                FilterCatalogueView((int)_selectedFilter);
            }
        }

        private void FilterCatalogueView(int cataLogueItemType)
        {
            _selectedFilter = (EItemTypeForFilter)cataLogueItemType;
            UpdateCatalogueView(SortAndFilterCatalogue());
        }

        private void SortCatalogueByValue(int sortType)
        {
            _selectedSortByValueType = (ESortByValueType)sortType;
            if(_selectedCustomSortType != ECustomSortType.None)
            {
                _customSortDropDown.onValueChanged.RemoveAllListeners();
                _customSortDropDown.value = 0;
                _selectedCustomSortType = ECustomSortType.None;
                _customSortDropDown.onValueChanged.AddListener(CustomSortCatalogueItems);
            }
            UpdateCatalogueView(SortAndFilterCatalogue());
        }

        private void CustomSortCatalogueItems(int customSortType)
        {
            _selectedCustomSortType = (ECustomSortType)customSortType;
            if (_selectedCustomSortType != ECustomSortType.None)
            {
                _sortByValueDropDown.onValueChanged.RemoveAllListeners();
                _sortByValueDropDown.value = 0;
                _selectedSortByValueType = ESortByValueType.None;
                _sortByValueDropDown.onValueChanged.AddListener(SortCatalogueByValue);
            }
            UpdateCatalogueView(SortAndFilterCatalogue());
        }
        #endregion

        #region Sorting and FIltering helper functions

        /// <summary>
        /// Sorts and filters the catalogue items
        /// </summary>
        /// <returns>List of sorted and filtered items</returns>
        private List<CatalogueItem> SortAndFilterCatalogue()
        {
            List<CatalogueItem> filteredItems = null;
            List<CatalogueItem> filteredAndSortedItems = new();

            if (_isBundleOnDisplay)
            {
                filteredItems = _catalogueManager.GetAllBundles();
                if (_selectedFilter != EItemTypeForFilter.All)
                {
                    filteredItems.Clear();
                    filteredItems.AddRange(_catalogueManager.FilterFromGivenItems(_catalogueManager.GetAllBundles(), item =>
                    {
                        Bundle bundle = (Bundle)item;
                        // Return true if any entry in bundle matches.
                        bool finalFilterValidation = false;
                        int andFilter = 0;
                        foreach (var bundleItem in bundle.Items)
                        {
                            if (_selectedFilter <= EItemTypeForFilter.Gems_or_Tickets)
                            {
                                finalFilterValidation |= IsValidItemTypeFilter(bundleItem.ItemType, _selectedFilter);
                            }
                            else
                            {
                                if (IsValidItemTypeFilter(bundleItem.ItemType, _selectedFilter))
                                {
                                    andFilter++;
                                }
                                if (andFilter >= 2)
                                {
                                    finalFilterValidation = true;
                                }
                            }
                        }
                        return finalFilterValidation;
                    }));
                }
                
            }
            else
            {
                filteredItems = _catalogueManager.GetAllProducts();
                if (_selectedFilter != EItemTypeForFilter.All)
                {
                    filteredItems.Clear();
                    filteredItems = _catalogueManager.FilterFromGivenItems(_catalogueManager.GetAllProducts(), item =>
                    {
                        Product prod = (Product)item;
                        return IsValidItemTypeFilter(prod.ItemType, _selectedFilter);
                    });
                }
            }
            //Only One sort can be active at a time
            if (_selectedCustomSortType != ECustomSortType.None)
            {
                filteredAndSortedItems.AddRange(_catalogueManager.SortGivenItemsByCustomOrder(filteredItems, GetCustomOrderList()));
            }
            else
            {
                filteredAndSortedItems.AddRange(GetSortedItems(filteredItems, (ESortByValueType)_selectedSortByValueType));
            }
            return filteredAndSortedItems;
        }

        /// <summary>
        /// Checks if the item selected and the item in the catalogue is valid for filter
        /// </summary>
        /// <param name="itemType">Type of catalogue item</param>
        /// <param name="itemTypesFilter">Type chosen from the filter dropdown</param>
        /// <returns>true if the item type matches the chosen filter type</returns>
        private bool IsValidItemTypeFilter(EItemType itemType, EItemTypeForFilter itemTypesFilter)
        {
            switch (itemTypesFilter)
            {
                case EItemTypeForFilter.All:
                    return true;
                case EItemTypeForFilter.Coins:
                    return itemType == EItemType.Coins;
                case EItemTypeForFilter.Gems:
                    return itemType == EItemType.Gems;
                case EItemTypeForFilter.Tickets:
                    return itemType == EItemType.Tickets;

                case EItemTypeForFilter.Coins_or_Gems:
                    return itemType == EItemType.Coins || itemType == EItemType.Gems;
                case EItemTypeForFilter.Coins_or_Tickets:
                    return itemType == EItemType.Coins || itemType == EItemType.Tickets;
                case EItemTypeForFilter.Gems_or_Tickets:
                    return itemType == EItemType.Gems || itemType == EItemType.Gems;

                case EItemTypeForFilter.Coins_and_Gems:
                    goto case EItemTypeForFilter.Coins_or_Gems;
                case EItemTypeForFilter.Coins_and_Tickets:
                    goto case EItemTypeForFilter.Coins_or_Tickets;
                case EItemTypeForFilter.Gems_and_Tickets:
                    goto case EItemTypeForFilter.Gems_or_Tickets;
            }
            return false;
        }

        /// <summary>
        /// Get sorted list of catalogue items
        /// </summary>
        /// <param name="items">The catalofue items to sort</param>
        /// <param name="sortType">Type of sorting to applye</param>
        /// <returns>List of sorted items</returns>
        private List<CatalogueItem> GetSortedItems(List<CatalogueItem> items, ESortByValueType sortType)
        {
            switch (sortType)
            {
                case ESortByValueType.Name_Ascending:
                    return _catalogueManager.SortGivenItems(items, item => item.Name);
                case ESortByValueType.Name_Descending:
                    return _catalogueManager.SortGivenItems(items, item => item.Name, false);
                case ESortByValueType.Price_Ascending:
                    return _catalogueManager.SortGivenItems(items, item => item.Price);
                case ESortByValueType.Price_Descending:
                    return _catalogueManager.SortGivenItems(items, item => item.Price, false);
            }
            return items;
        }

        /// <summary>
        /// Get list of item types that dictate custom order
        /// </summary>
        /// <returns>List of item types</returns>
        private List<EItemType> GetCustomOrderList()
        {
            List<EItemType> customOrderList = new();
            switch (_selectedCustomSortType)
            {
                case ECustomSortType.Coins_Gems_Tickets:
                    customOrderList.Add(EItemType.Coins);
                    customOrderList.Add(EItemType.Gems);
                    customOrderList.Add(EItemType.Tickets);
                    break;
                case ECustomSortType.Gems_Tickets_Coins:
                    customOrderList.Add(EItemType.Gems);
                    customOrderList.Add(EItemType.Tickets);
                    customOrderList.Add(EItemType.Coins);
                    break;
                case ECustomSortType.Tickets_Coins_Gems:
                    customOrderList.Add(EItemType.Tickets); 
                    customOrderList.Add(EItemType.Coins);
                    customOrderList.Add(EItemType.Gems);
                    break;
                case ECustomSortType.Gems_Coins_Tickets:
                    customOrderList.Add(EItemType.Gems);
                    customOrderList.Add(EItemType.Coins);
                    customOrderList.Add(EItemType.Tickets);
                    break;
                case ECustomSortType.Coins_Tickets_Gems:
                    customOrderList.Add(EItemType.Coins);
                    customOrderList.Add(EItemType.Tickets);
                    customOrderList.Add(EItemType.Gems);
                    break;
                case ECustomSortType.Tickets_Gems_Coins:
                    customOrderList.Add(EItemType.Tickets);
                    customOrderList.Add(EItemType.Gems);
                    customOrderList.Add(EItemType.Coins);
                    break;
            }
            return customOrderList;
        }
        #endregion

        #region UI modification functions
        /// <summary>
        /// Reset catalogue items before prepareing new items for UI 
        /// </summary>
        /// <param name="items">List of items to reset</param>
        private void ResetItemUICollections(ref List<CatalogueItem> items)
        {
            if (items.Count >= _shopItems.Count)
            {
                var endIndex = items.Count;
                for (int i = _shopItems.Count; i < endIndex; i++)
                {
                    var item = GetUICataloguItem();
                    _shopItems.Add(item);
                }
            }
            else if (_shopItems.Count > 0)
            {
                var startIndex = items.Count;
                var endIndex = _shopItems.Count;

                for (int i = startIndex; i < endIndex; i++)
                {
                    _shopItems[i].transform.SetParent(gameObject.transform);
                    _pooledShopItems.Enqueue(_shopItems[i]);
                }
                _shopItems.RemoveRange(startIndex, endIndex - startIndex);
            }
        }

        /// <summary>
        /// Update the shop view with new items
        /// </summary>
        /// <param name="items">List of items to showcase</param>
        private void UpdateCatalogueView(List<CatalogueItem> items)
        {
            ResetItemUICollections(ref items);

            for (int i = 0; i < items.Count; i++)
            {
                Sprite sp = null;
                if (items[i].GetType() == typeof(Product))
                {
                    switch (items[i].GetItemType())
                    {
                        case EItemType.Coins:
                            sp = _coinsSprite;
                            break;
                        case EItemType.Gems:
                            sp = _gemsSprite;
                            break;
                        case EItemType.Tickets:
                            sp = _ticketsSprite;
                            break;
                    }
                }
                else
                {
                    sp = _bundleSprite;
                }
                _shopItems[i].GetComponent<CatalogueItemUI>().PrepView(sp, items[i].Price, items[i].Name, items[i].ShortDescription, _purchasePopUp);
                _shopItems[i].transform.SetParent(_shopScrollRect.content);
                _shopItems[i].transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Get a newly instantiated item or from the pool
        /// </summary>
        /// <returns>A gameobject that represents individual catalogue item in UI</returns>
        private GameObject GetUICataloguItem()
        {
            if (_pooledShopItems.Count > 0)
            {
                return _pooledShopItems.Dequeue();
            }
            else
            {
                return Instantiate<GameObject>(_itemPrefab);
            }
        }
        #endregion

        private void RunExamples()
        {
            // Get all items.
            Debug.Log("=== All Catalogue Items ===");
            foreach (var item in _catalogueManager.GetAllItems())
            {
                Debug.Log(item.Name + " - $" + item.Price);
            }

            // Example: Sort by descending Price.
            var sortedByPriceDesc = _catalogueManager.SortItems(item => item.Price, false);
            Debug.Log("=== Sorted by Price (Descending) ===");
            foreach (var item in sortedByPriceDesc)
            {
                Debug.Log(item.Name + " - $" + item.Price);
            }

            // Filter items to show only those involving Coins or Tickets.
            var filteredItems = _catalogueManager.FilterAlltems(item =>
            {
                if (item is Product prod)
                    return prod.ItemType == EItemType.Coins || prod.ItemType == EItemType.Tickets;
                else if (item is Bundle bundle)
                {
                    // Return true if any entry in bundle matches.
                    foreach (var entry in bundle.Items)
                    {
                        if (entry.ItemType == EItemType.Coins || entry.ItemType == EItemType.Tickets)
                            return true;
                    }
                }
                return false;
            });

            Debug.Log("=== Filtered Items (Coins and Tickets only) ===");
            foreach (var item in filteredItems)
            {
                Debug.Log(item.Name);
            }

            // Custom sort by a custom item ordering.
            var customOrder = new List<EItemType> { EItemType.Gems, EItemType.Coins, EItemType.Tickets };
            var sortedByCustomOrder = _catalogueManager.SortItemsByCustomOrder(customOrder);
            Debug.Log("=== Sorted by Custom Item Order (Gems > Coins > Tickets) ===");
            foreach (var item in sortedByCustomOrder)
            {
                Debug.Log(item.Name);
            }
        }
    }
}