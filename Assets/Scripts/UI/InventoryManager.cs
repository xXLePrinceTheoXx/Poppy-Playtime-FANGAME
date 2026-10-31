using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuration")]
    public int slotCount = 20;
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("UI Références")]
    public InventorySlot[] slots;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// Ajoute un objet à l'inventaire
    /// </summary>
    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null) return false;

        // Si empilable et déjà présent, on ajoute la quantité
        if (item.maxStack > 1)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].data == item && items[i].quantity < item.maxStack)
                {
                    items[i].quantity += quantity;
                    RefreshUI();
                    return true;
                }
            }
        }

        // Sinon on crée une nouvelle entrée si de la place
        if (items.Count < slotCount)
        {
            items.Add(new InventoryItem { data = item, quantity = quantity });
            RefreshUI();
            return true;
        }

        Debug.Log("Inventaire plein !");
        return false;
    }

    /// <summary>
    /// Retire un objet de l'inventaire
    /// </summary>
    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].data == item)
            {
                items[i].quantity -= quantity;
                if (items[i].quantity <= 0)
                    items.RemoveAt(i);
                RefreshUI();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Vérifie si le joueur possède un objet
    /// </summary>
    public bool HasItem(ItemData item)
    {
        foreach (var it in items)
            if (it.data == item) return true;
        return false;
    }

    /// <summary>
    /// Met à jour l'affichage visuel des slots
    /// </summary>
    public void RefreshUI()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count)
                slots[i].SetItem(items[i].data, items[i].quantity);
            else
                slots[i].Clear();
        }
    }
}