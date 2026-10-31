using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null &&
                InventoryManager.Instance.AddItem(itemData, quantity))
            {
                Destroy(gameObject);
            }
        }
    }
}