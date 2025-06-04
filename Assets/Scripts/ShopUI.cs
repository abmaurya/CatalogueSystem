using MAG_I.ShopCatalogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace MAG_I.ShopCatalogue
{
    public enum ESortType
    {
        None = 0,
        Name_Ascending,
        Name_Descending,
        Price_Ascending,
        Price_Descending,
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
        private TMPro.TMP_Dropdown _sortDropDown;
        [SerializeField]
        private TMPro.TMP_Dropdown _filterDropDown;
        [SerializeField]
        private Button _productsButton;
        [SerializeField]
        private Button _bundlesButton;

        private Queue<GameObject> _pooledShopItems;
        private List<GameObject> _shopItems;
        private CatalogueManager _catalogueManager;
        private EItemType _selectedFilter;
        private bool _isBundleOnDisplay;
        void Start()
        {
            _pooledShopItems = new Queue<GameObject>();
            _shopItems = new List<GameObject>();
            _isBundleOnDisplay = false;
            // Initialize and load the catalogue.
            _catalogueManager = new CatalogueManager();
            _catalogueManager.LoadCatalogFromJson(_jsonCatalogue.text);
            ShowProducts();

            //Add Listeners
            _productsButton.onClick.AddListener(ShowProducts);
            _bundlesButton.onClick.AddListener(ShowBundles);
            
            var itemTypes = new List<string>(System.Enum.GetNames(typeof(EItemType)));
            
            _sortDropDown.ClearOptions();
            _sortDropDown.AddOptions(itemTypes);
            
            _filterDropDown.ClearOptions();
            _filterDropDown.AddOptions(itemTypes);
            _filterDropDown.onValueChanged.AddListener(FilterCatalogueView);
            //RunExamples();
        }

        #region UI elements Event Listeners
        private void ShowProducts()
        {
            _isBundleOnDisplay = false;
            UpdateCatalogueView(_catalogueManager.GetAllProducts());
        }

        private void ShowBundles()
        {
            UpdateCatalogueView(_catalogueManager.GetAllBundles());
            _isBundleOnDisplay = true;
        }

        private void FilterCatalogueView(int cataLogueType)
        {
            var itemType = (EItemType)cataLogueType;
            List<CatalogueItem> filteredItems = new();
            if (_isBundleOnDisplay)
            {
                filteredItems.AddRange(_catalogueManager.FilterIAlltems(item =>
                {
                    if (item is Product prod)
                        return prod.ItemType == itemType;
                    return false;
                }));
            }
            else
            {
                filteredItems.AddRange(_catalogueManager.FilterIAlltems(item =>
                {
                    if (item is Bundle bundle)
                    {
                        // Return true if any entry in bundle matches.
                        foreach (var entry in bundle.Items)
                        {
                            if (entry.ItemType == itemType)
                                return true;
                        }
                    }
                    return false;
                }));
            }

            UpdateCatalogueView(filteredItems);
        }
        #endregion

        private void ResetItemUICollections(ref List<CatalogueItem> items)
        {
            if (items.Count > _shopItems.Count)
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
                var endIndex = _shopItems.Count - 1;

                for (int i = startIndex; i <= endIndex; i++)
                {
                    _shopItems[i].transform.SetParent(gameObject.transform);
                    _pooledShopItems.Enqueue(_shopItems[i]);
                }
                _shopItems.RemoveRange(startIndex, endIndex);
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
            var filteredItems = _catalogueManager.FilterIAlltems(item =>
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