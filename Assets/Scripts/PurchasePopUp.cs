using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePopUp : MonoBehaviour
{
    [SerializeField]
    private Image _itemImage;
    [SerializeField]
    private Button _buyButton;
    [SerializeField]
    private Button _closeButton;
    [SerializeField]
    private TMPro.TMP_Text _itemPriceText;
    [SerializeField]
    private TMPro.TMP_Text _itemNameText;
    [SerializeField]
    private TMPro.TMP_Text _itemDescriptionText;

    private Canvas _myCanvas;
    private GraphicRaycaster _graphicRaycaster;

    // Start is called before the first frame update
    void Start()
    {
        _myCanvas = GetComponent<Canvas>();
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
        _buyButton.onClick.AddListener(Hide);
        _closeButton.onClick.AddListener(Hide);
        Hide();
    }

    public void Show(Sprite sp, string description, string itemName, string itemPrice)
    {
        _itemImage.sprite = sp;
        _itemNameText.text = itemName;
        _itemPriceText.text = itemPrice;
        _itemDescriptionText.text = description;
        _myCanvas.enabled = true;
        _graphicRaycaster.enabled = true;
    }

    public void Hide() 
    {
        _myCanvas.enabled = false;
        _graphicRaycaster.enabled = false;
    }
}
