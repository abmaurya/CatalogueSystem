using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _myPriceText;
    private Image _myCatalogueItemImage;
    // Start is called before the first frame update
    void Awake()
    {
        _myCatalogueItemImage = GetComponent<Image>();
    }

    public void PrepView(Sprite s, float price)
    {
        _myCatalogueItemImage.sprite = s;
        _myPriceText.text = price.ToString();
    }
}
