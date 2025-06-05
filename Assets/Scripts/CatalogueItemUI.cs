using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _myPriceText;
    private Image _myCatalogueItemImage;
    private Button _myButton;
    private string _itemPrice;
    private string _itemName;
    private string _itemDescription;
    private PurchasePopUp _purchasePopUp;
    // Start is called before the first frame update
    void Awake()
    {
        _myCatalogueItemImage = GetComponent<Image>();
        _myButton = GetComponent<Button>();
    }

    public void PrepView(Sprite s, float price, string itemName, string description, PurchasePopUp purchasePopUp)
    {
        if (!_purchasePopUp)
        {
            _purchasePopUp ??= purchasePopUp;
            _myButton.onClick.AddListener(OnClickPreviewButton);
        }
        _myCatalogueItemImage.sprite = s;
        _itemPrice = price.ToString();
        _myPriceText.text = _itemPrice;
        _itemName = itemName;
        _itemDescription = description;
    }

    public void OnClickPreviewButton()
    {
        _purchasePopUp?.Show(_myCatalogueItemImage.sprite, _itemDescription, _itemName, _itemPrice);
    }
}
