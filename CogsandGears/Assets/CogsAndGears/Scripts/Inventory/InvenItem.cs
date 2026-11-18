using System.Collections.Generic;
using UnityEngine;

public class InvenItem : MonoBehaviour
{
    [SerializeField] private ItemDataSO _itemData;

    public ItemRotation Rotation { get; private set; } = ItemRotation.Angle0;
    public void Rotate() => Rotation = ItemRotationUtility.Next(Rotation);
    public Vector2Int RotatedSize() => ItemRotationUtility.GetRotatedSize(_itemData.Size, Rotation);
    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> cells = new();

        foreach (Vector2Int cell in _itemData.OccupiedCells)
            cells.Add(ItemRotationUtility.RotateCell(cell, _itemData.Size, Rotation));

        return cells;
    }
}