using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName = "Objet";
    [TextArea] public string description = "";
    public Sprite icon;
    public int maxStack = 1;
}