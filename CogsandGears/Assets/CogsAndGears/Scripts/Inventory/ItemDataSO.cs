using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public abstract class ItemDataSO : ScriptableObject
{
    [Header("Item Info")]
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _icon;

    [Header("Inventory Info")]
    [SerializeField] private Vector2Int _size;
    
    public string ItemName => _itemName;
    public Sprite Icon => _icon;
    
    public Vector2Int Size => _size;
}
