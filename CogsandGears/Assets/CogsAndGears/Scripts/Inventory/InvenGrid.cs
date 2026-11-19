using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2Int _gridSize;
    [SerializeField] private Vector2Int _initialUnlockPosition;
    [SerializeField] private Vector2Int _initialUnlockSize;

    [Header("Cell UI")]
    [SerializeField] private Color _unlockedColor = Color.white;
    [SerializeField] private Color _lockedColor = Color.gray;

    private bool[,] _unlockedCells;
    private InvenItem[,] _occupiedCells;
    private Image[,] _cellImages;
    private GridLayoutGroup _gridLayoutGroup;
    
    public Vector2 CellSize => _gridLayoutGroup.cellSize;
    public Vector2 Spacing => _gridLayoutGroup.spacing;

    private void Awake()
    {
        CacheComponents();
        InitializeGrid();
    }
    
    private void InitializeGrid()
    {
        _unlockedCells = new bool[_gridSize.x, _gridSize.y];
        _occupiedCells = new InvenItem[_gridSize.x, _gridSize.y];
        _cellImages = new Image[_gridSize.x, _gridSize.y];

        // Hierarchy에 배치한 Cell_00 ~ Cell_31 참조
        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                int index = y * _gridSize.x + x;
                _cellImages[x, y] = transform.GetChild(index)
                    .GetComponent<Image>();
            }
        }

        // 초기 4×4 영역 해금
        for (int y = 0; y < _initialUnlockSize.y; y++)
        {
            for (int x = 0; x < _initialUnlockSize.x; x++)
            {
                Vector2Int position =
                    _initialUnlockPosition + new Vector2Int(x, y);

                if (IsInside(position))
                    _unlockedCells[position.x, position.y] = true;
            }
        }

        RefreshGrid();
    }

    private bool IsInside(Vector2Int position)
    {
        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < _gridSize.x &&
               position.y < _gridSize.y;
    }
    
    private void RefreshGrid()
    {
        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                _cellImages[x, y].color = _unlockedCells[x, y]
                        ? _unlockedColor
                        : _lockedColor;
            }
        }
    }
    
     // 아이템 배치 가능 여부
     public bool CanPlace(InvenItem item, Vector2Int position)
     {
         List<Vector2Int> occupiedCells = item.GetOccupiedCells();
         if (occupiedCells.Count == 0) return false;
         
         foreach (Vector2Int cell in occupiedCells)
         {
             Vector2Int target = position + cell;

             if (!IsInside(target))
                 return false;

             if (!_unlockedCells[target.x, target.y])
                 return false;

             if (_occupiedCells[target.x, target.y] != null &&
                 _occupiedCells[target.x, target.y] != item)
                 return false;
         }

         return true;
     }

     // 아이템 배치 확정
     public bool TryPlace(InvenItem item, Vector2Int position)
     {
         if (!CanPlace(item, position))
             return false;

         // 기존 배치 정보 제거
         Remove(item);

         foreach (Vector2Int cell in item.GetOccupiedCells())
         {
             Vector2Int target = position + cell;
             _occupiedCells[target.x, target.y] = item;
         }

         return true;
     }

     // 아이템 배치 해제
     public void Remove(InvenItem item)
     {
         for (int y = 0; y < _gridSize.y; y++)
         {
             for (int x = 0; x < _gridSize.x; x++)
             {
                 if (_occupiedCells[x, y] == item)
                     _occupiedCells[x, y] = null;
             }
         }
     }

     // 현재 배치된 아이템 목록 (중복 제거)
     public List<InvenItem> GetPlacedItems()
     {
         HashSet<InvenItem> items = new();

         for (int y = 0; y < _gridSize.y; y++)
         {
             for (int x = 0; x < _gridSize.x; x++)
             {
                 InvenItem item = _occupiedCells[x, y];

                 if (item != null)
                     items.Add(item);
             }
         }

         return new List<InvenItem>(items);
     }

     private void CacheComponents()
     {
         _gridLayoutGroup = GetComponent<GridLayoutGroup>();
     }
}
