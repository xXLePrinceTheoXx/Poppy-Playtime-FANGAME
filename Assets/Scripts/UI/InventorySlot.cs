using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public Text quantityText;
    public Image borderImage;

    public void SetItem(ItemData data, int quantity)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        iconImage.sprite = data.icon;
        iconImage.enabled = true;
        quantityText.text = quantity > 1 ? quantity.ToString() : "";

        // Bordure accent quand l'item est présent
        if (borderImage != null) borderImage.enabled = true;
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        quantityText.text = "";

        // Enlever la bordure
        if (borderImage != null) borderImage.enabled = false;
    }
}