using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public abstract class ItemDataSO : ScriptableObject
{
    [Header("Item Info")]
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _icon;

    [Header("Inventory Info")]
    [SerializeField] private Vector2Int _size;
    [SerializeField] private List<Vector2Int> _occupiedCells;
    
    public string ItemName => _itemName;
    public Sprite Icon => _icon;
    
    public Vector2Int Size => _size;
    public List<Vector2Int> OccupiedCells => _occupiedCells;

    private void OnValidate()
    {
        if (_size.x <= 0 || _size.y <= 0)
        {
            Debug.LogWarning(
                $"[{name}] Invalid inventory size: {_size}", this);
            return;
        }

        if (_occupiedCells == null || _occupiedCells.Count == 0)
        {
            Debug.LogWarning(
                $"[{name}] OccupiedCells is empty.", this);
            return;
        }

        HashSet<Vector2Int> uniqueCells = new();

        foreach (Vector2Int cell in _occupiedCells)
        {
            if (cell.x < 0 || cell.y < 0 ||
                cell.x >= _size.x || cell.y >= _size.y)
            {
                Debug.LogWarning(
                    $"[{name}] OccupiedCell out of bounds: {cell}", this);
            }

            if (!uniqueCells.Add(cell))
            {
                Debug.LogWarning(
                    $"[{name}] Duplicate OccupiedCell: {cell}", this);
            }
        }
    }
}
