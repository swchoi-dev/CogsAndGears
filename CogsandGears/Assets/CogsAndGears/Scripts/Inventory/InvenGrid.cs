using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvenGrid : MonoBehaviour, IPointerClickHandler
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    [Header("Grid Settings")]
    [SerializeField] private Vector2Int _gridSize;
    [SerializeField] private Vector2Int _initialUnlockPosition;
    [SerializeField] private Vector2Int _initialUnlockSize;

    [SerializeField] private InvenUI _invenUI;
    
    [Header("Cell UI")]
    [SerializeField] private Color _unlockedColor = Color.white;
    [SerializeField] private Color _lockedColor = Color.gray;

    [Header("Expansion")]
    [Tooltip("확장 Phase에서 잠긴 셀 색")]
    [SerializeField] private Color _expansionLockedColor = Color.black;
    [Tooltip("확장 Phase에서 이번에 선택한(아직 확정 전) 셀 색")]
    [SerializeField] private Color _expansionSelectedColor = Color.white;

    [Header("Placement Preview")]
    [SerializeField] private Color _validPreviewColor = Color.green;
    [SerializeField] private Color _invalidPreviewColor = Color.red;
    
    [Header("Default Weapon")]
    [SerializeField] private WeaponDataSO _defaultWeaponData;
    [SerializeField] private Vector2Int _defaultWeaponPosition;
    
    

    private readonly List<Vector2Int> _previewCells = new();
    // 확장 Phase에서 선택한 임시 셀. CommitExpansion 전까지 _unlockedCells에 반영하지 않는다
    private readonly HashSet<Vector2Int> _pendingCells = new();
    private bool _isExpanding;
    private int _expansionLimit;
    private InvenItem _defaultItem;
    
    private bool[,] _unlockedCells;
    private InvenItem[,] _occupiedCells;
    private Image[,] _cellImages;
    private GridLayoutGroup _gridLayoutGroup;
    
    public Vector2 CellSize => _gridLayoutGroup.cellSize;
    public Vector2 Spacing => _gridLayoutGroup.spacing;
    public InvenItem DefaultItem => _defaultItem;
    public bool IsExpanding => _isExpanding;
    public int ExpansionLimit => _expansionLimit;
    public int SelectedExpansionCount => _pendingCells.Count;
    // 남은 확장 칸 = 이번 확장 수량 - 선택한 셀 수
    public int RemainingExpansion => Mathf.Max(0, _expansionLimit - _pendingCells.Count);

    // 확장 선택·취소·확정으로 상태가 바뀌면 호출된다(남은 칸 UI·Confirm 버튼 갱신용)
    public event Action ExpansionChanged;

    private void Awake()
    {
        CacheComponents();
        InitializeGrid();
        InitDefaultWeaponData();
    }
    
    private void InitDefaultWeaponData()
    {
        if (_defaultWeaponData == null) return;

        InvenItem item = new InvenItem(_defaultWeaponData);
        _defaultItem = item;
        
        // 초기 점유 데이터 등록
        foreach (Vector2Int cell in item.GetOccupiedCells())
        {
            Vector2Int position = _defaultWeaponPosition + cell;
            _occupiedCells[position.x, position.y] = item;
        }

        _invenUI.CreateDefaultItem(item, _defaultWeaponPosition);
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
                _cellImages[x, y].color = GetCellColor(x, y);
            }
        }
    }

    private Color GetCellColor(int x, int y)
    {
        if (_unlockedCells[x, y])
            return _unlockedColor;

        if (!_isExpanding)
            return _lockedColor;

        return _pendingCells.Contains(new Vector2Int(x, y))
            ? _expansionSelectedColor
            : _expansionLockedColor;
    }

    // ---------- 확장 Phase ----------

    public int LockedCellCount
    {
        get
        {
            int count = 0;
            foreach (bool unlocked in _unlockedCells)
            {
                if (!unlocked) count++;
            }

            return count;
        }
    }

    // 확장 Phase 시작. 잠긴 셀보다 많이 요청하면 잠긴 셀 수로 제한한다.
    public void BeginExpansion(int count)
    {
        _isExpanding = true;
        _pendingCells.Clear();
        _expansionLimit = Mathf.Clamp(count, 0, LockedCellCount);

        RefreshGrid();
        ExpansionChanged?.Invoke();
    }

    // 검은 셀이면 선택(흰색), 흰 셀이면 취소(검은색). 조건을 만족하지 않으면 변화 없음.
    public bool TryToggleExpansionCell(Vector2Int cell)
    {
        if (!_isExpanding || !IsInside(cell)) return false;

        bool changed;

        if (_pendingCells.Contains(cell))
        {
            changed = CanDeselectExpansionCell(cell) && _pendingCells.Remove(cell);
        }
        else
        {
            changed = CanSelectExpansionCell(cell) && _pendingCells.Add(cell);
        }

        if (!changed) return false;

        RefreshGrid();
        ExpansionChanged?.Invoke();
        return true;
    }

    // 잠긴 셀 + 남은 확장 수 1 이상 + 기존 활성 셀 또는 선택한 셀과 상하좌우 인접
    public bool CanSelectExpansionCell(Vector2Int cell)
    {
        if (!_isExpanding || !IsInside(cell)) return false;
        if (_unlockedCells[cell.x, cell.y] || _pendingCells.Contains(cell)) return false;
        if (_pendingCells.Count >= _expansionLimit) return false;

        foreach (Vector2Int direction in Directions)
        {
            Vector2Int neighbor = cell + direction;
            if (IsInside(neighbor) &&
                (_unlockedCells[neighbor.x, neighbor.y] || _pendingCells.Contains(neighbor)))
                return true;
        }

        return false;
    }

    // 취소해도 나머지 선택 셀이 모두 기존 활성 영역과 연결되어 있을 때만 허용한다
    public bool CanDeselectExpansionCell(Vector2Int cell)
    {
        return _isExpanding &&
               _pendingCells.Contains(cell) &&
               ArePendingCellsConnected(cell);
    }

    // 기존 활성 셀 전체에서 출발해 선택 셀만 따라 BFS. excluded 셀을 제외한 모든 선택 셀에 도달하면 연결됨.
    private bool ArePendingCellsConnected(Vector2Int? excluded = null)
    {
        int target = _pendingCells.Count - (excluded.HasValue && _pendingCells.Contains(excluded.Value) ? 1 : 0);
        if (target <= 0) return true;

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();

        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                if (_unlockedCells[x, y])
                    queue.Enqueue(new Vector2Int(x, y));
            }
        }

        int reached = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = current + direction;

                if (excluded.HasValue && next == excluded.Value) continue;
                if (!_pendingCells.Contains(next) || !visited.Add(next)) continue;

                reached++;
                queue.Enqueue(next);
            }
        }

        return reached >= target;
    }

    // 선택 수량·연결을 재검증한 뒤 선택 셀을 실제 활성 셀로 확정한다. 확정된 셀은 다시 잠기지 않는다.
    public bool CommitExpansion()
    {
        if (!_isExpanding) return false;
        if (_pendingCells.Count != _expansionLimit) return false;
        if (!ArePendingCellsConnected()) return false;

        foreach (Vector2Int cell in _pendingCells)
            _unlockedCells[cell.x, cell.y] = true;

        _pendingCells.Clear();
        _isExpanding = false;
        _expansionLimit = 0;

        RefreshGrid();
        ExpansionChanged?.Invoke();
        return true;
    }

    public bool IsUnlocked(Vector2Int cell) => IsInside(cell) && _unlockedCells[cell.x, cell.y];

    public bool IsSelectedForExpansion(Vector2Int cell) => _pendingCells.Contains(cell);

    // 셀(Image)을 클릭하면 이벤트가 부모인 그리드로 올라온다. 확장 Phase에서만 처리한다.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isExpanding) return;

        GameObject clicked = eventData.pointerCurrentRaycast.gameObject;
        if (clicked == null || clicked.transform.parent != transform) return;

        int index = clicked.transform.GetSiblingIndex();
        TryToggleExpansionCell(new Vector2Int(index % _gridSize.x, index / _gridSize.x));
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
     
     // 첫 번째 셀의 중심 좌표 (Grid 로컬 좌표)
     private Vector2 GetFirstCellCenter()
     {
         RectTransform gridRect = (RectTransform)transform;
         RectTransform cellRect = _cellImages[0, 0].rectTransform;

         Vector3 worldCenter = cellRect.TransformPoint(cellRect.rect.center);

         return gridRect.InverseTransformPoint(worldCenter);
     }

    // 마우스 화면 좌표 → 아이템 배치 시작 셀 좌표
     public bool TryGetDropPosition(
         Vector2 screenPosition,
         Camera uiCamera,
         InvenItem item,
         out Vector2Int position)
     {
         position = default;

         RectTransform gridRect = (RectTransform)transform;

         if (!RectTransformUtility.RectangleContainsScreenPoint(
                 gridRect, screenPosition, uiCamera))
             return false;

         if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                 gridRect, screenPosition, uiCamera, out Vector2 local))
             return false;

         Vector2 first = GetFirstCellCenter();
         Vector2Int size = item.RotatedSize();

         float stepX = CellSize.x + Spacing.x;
         float stepY = CellSize.y + Spacing.y;

         // 드래그 아이콘의 중심을 기준으로 배치 시작 셀 계산
         float offsetX = (size.x - 1) * stepX * 0.5f;
         float offsetY = (size.y - 1) * stepY * 0.5f;

         int x = Mathf.RoundToInt((local.x - first.x - offsetX) / stepX);
         int y = Mathf.RoundToInt((first.y - local.y - offsetY) / stepY);

         position = new Vector2Int(x, y);

         return true;
     }

    // 배치 시작 셀 → 아이템 UI 중심의 월드 좌표
     public Vector3 GetPlacementWorldPosition(
         InvenItem item,
         Vector2Int position)
     {
         RectTransform gridRect = (RectTransform)transform;

         Vector2 first = GetFirstCellCenter();
         Vector2Int size = item.RotatedSize();

         float stepX = CellSize.x + Spacing.x;
         float stepY = CellSize.y + Spacing.y;

         float x = first.x +
                   (position.x + (size.x - 1) * 0.5f) * stepX;

         float y = first.y -
                   (position.y + (size.y - 1) * 0.5f) * stepY;

         return gridRect.TransformPoint(new Vector3(x, y, 0f));
     }
     
     public void ShowPlacementPreview(InvenItem item, Vector2Int position)
     {
         ClearPlacementPreview();

         bool canPlace = CanPlace(item, position);
         Color previewColor = canPlace
             ? _validPreviewColor
             : _invalidPreviewColor;

         foreach (Vector2Int cell in item.GetOccupiedCells())
         {
             Vector2Int target = position + cell;

             if (!IsInside(target))
                 continue;

             _cellImages[target.x, target.y].color = previewColor;
             _previewCells.Add(target);
         }
     }
     
     public void ClearPlacementPreview()
     {
         foreach (Vector2Int cell in _previewCells)
         {
             _cellImages[cell.x, cell.y].color = GetCellColor(cell.x, cell.y);
         }

         _previewCells.Clear();
     }
}
