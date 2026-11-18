using UnityEngine;

public static class ItemRotationUtility
{
    public static ItemRotation Next(ItemRotation rotation)
    {
        return rotation switch
        {
            ItemRotation.Angle0 => ItemRotation.Angle90,
            ItemRotation.Angle90 => ItemRotation.Angle180,
            ItemRotation.Angle180 => ItemRotation.Angle270,
            ItemRotation.Angle270 => ItemRotation.Angle0,
            _ => ItemRotation.Angle0
        };
    }

    public static Vector2Int GetRotatedSize(
        Vector2Int size,
        ItemRotation rotation)
    {
        return rotation switch
        {
            ItemRotation.Angle90 or ItemRotation.Angle270
                => new Vector2Int(size.y, size.x),
            _ => size
        };
    }

    public static Vector2Int RotateCell(
        Vector2Int cell,
        Vector2Int size,
        ItemRotation rotation)
    {
        int x = cell.x;
        int y = cell.y;
        int width = size.x;
        int height = size.y;

        return rotation switch
        {
            ItemRotation.Angle0 => new Vector2Int(x, y),
            ItemRotation.Angle90 => new Vector2Int(height - 1 - y, x),
            ItemRotation.Angle180 => new Vector2Int(width - 1 - x, height - 1 - y),
            ItemRotation.Angle270 => new Vector2Int(y, width - 1 - x),
            _ => cell
        };
    }
}
