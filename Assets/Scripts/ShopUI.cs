using MAG_I.ShopCatalogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace MAG_I.ShopCatalogue
{
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

    public enum ESortByCustomOrderType
    {
        None = 0,
        Coins_Gems_Tickets,
        Gems_Tickets_Coins,
        Tickets_Coins_Gems,

        Gems_Coins_Tickets,
        Coins_Tickets_Gems,
        Tickets_Gems_Coins,
    }

    public class ShopUI : MonoBehaviour
    {
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
        private TMPro.TMP_Dropdown _sortByCustomOrderDropDown;
        [SerializeField]
        private TMPro.TMP_Dropdown _sortByValueDropDown;
        [SerializeField]
        private TMPro.TMP_Dropdown _filterDropDown;
        [SerializeField]
        private Button _productsButton;
        [SerializeField]
        private Button _bundlesButton;

        private Queue<GameObject> _pooledShopItems;
        private List<GameObject> _shopItems;
        private CatalogueManager _catalogueManager;
        private EItemTypeForFilter _selectedFilter;
        private ESortByValueType _selectedSortType;
        private bool _isBundleOnDisplay;

        void Start()
        {
            _pooledShopItems = new Queue<GameObject>();
            _shopItems = new List<GameObject>();
            _isBundleOnDisplay = false;
            _selectedFilter = EItemTypeForFilter.All;
            _selectedSortType = ESortByValueType.None;

            // Initialize and load the catalogue.
            _catalogueManager = new CatalogueManager();
            _catalogueManager.LoadCatalogFromJson(_jsonCatalogue.text);
            ShowProducts();

            //Add Listeners
            _productsButton.onClick.AddListener(ShowProducts);
            _bundlesButton.onClick.AddListener(ShowBundles);

            var customSortItemTypes = new List<string>(System.Enum.GetNames(typeof(ESortByCustomOrderType)));
            for (int i = 0; i < customSortItemTypes.Count; i++)
            {
                customSortItemTypes[i] = customSortItemTypes[i].Replace("_", "<");
            }
            _sortByCustomOrderDropDown.AddOptions(customSortItemTypes);

            var sortItemTypes = new List<string>(System.Enum.GetNames(typeof(ESortByValueType)));
            _sortByValueDropDown.ClearOptions();
            _sortByValueDropDown.AddOptions(sortItemTypes);
            _sortByValueDropDown.onValueChanged.AddListener(SortCatalogueView);

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
            var itemType = (EItemTypeForFilter)cataLogueItemType;
            _selectedFilter = itemType;
            List<CatalogueItem> filteredItems = new();
            if (_isBundleOnDisplay)
            {
                //if(_selectedSortType != ESortByValueType.None)
                //{

                //}
                filteredItems.AddRange(_catalogueManager.FilterFromGivenItems(_catalogueManager.GetAllBundles(), item =>
                {
                    Bundle bundle = (Bundle)item;
                    // Return true if any entry in bundle matches.
                    bool finalFilterValidation = false;
                    int andFilter = 0;
                    foreach (var bundleItem in bundle.Items)
                    {
                        if (cataLogueItemType <= (int)EItemTypeForFilter.Gems_or_Tickets)
                        {
                            finalFilterValidation |= IsValidItemTypeFilter(bundleItem.ItemType, itemType);
                        }
                        else
                        {
                            if(IsValidItemTypeFilter(bundleItem.ItemType, itemType))
                            {
                                andFilter++;
                            }
                            if(andFilter >= 2)
                            {
                                finalFilterValidation = true;
                            }
                        }
                    }
                    return finalFilterValidation;
                }));
            }
            else
            {
                filteredItems.AddRange(_catalogueManager.FilterFromGivenItems(_catalogueManager.GetAllProducts(), item =>
                {
                    Product prod = (Product)item;
                    return IsValidItemTypeFilter(prod.ItemType, itemType);
                }));
            }
            UpdateCatalogueView(filteredItems);
        }

        private void SortCatalogueView(int sortType)
        {
            List<CatalogueItem> filteredItems = new();
            _selectedSortType = (ESortByValueType)sortType;
            if (_selectedFilter == EItemTypeForFilter.All)
            {
                if (_isBundleOnDisplay)
                {
                    filteredItems.AddRange(GetSortedItems(_catalogueManager.GetAllBundles(), (ESortByValueType)sortType));
                }
                else
                {
                    filteredItems.AddRange(GetSortedItems(_catalogueManager.GetAllProducts(), (ESortByValueType)sortType));
                }
            }
            UpdateCatalogueView(filteredItems);
        }
        #endregion

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
                _shopItems.RemoveRange(startIndex, endIndex-startIndex);
            }
        }

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
                _shopItems[i].GetComponent<CatalogueItemUI>().PrepView(sp, items[i].Price);
                _shopItems[i].transform.SetParent(_shopScrollRect.content);
                _shopItems[i].transform.localScale = Vector3.one;
            }
        }

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