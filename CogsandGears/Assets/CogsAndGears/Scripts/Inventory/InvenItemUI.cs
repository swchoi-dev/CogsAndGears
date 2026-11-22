using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Image))]
public class InvenItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    private Image _iconImage;
    private RectTransform _rectTransform;
    private RectTransform _dragLayer;
    private InvenGrid _invenGrid;
    private InvenUI _owner;
    private InvenItem _item;
    
    private bool _isDragging;
    private Transform _originParent;
    private Vector2 _originPosition;
    private Vector3 _originScale;
    private int _originSiblingIndex;
    
    private bool _isPlaced;
    private bool _wasPlacedAtDragStart;
    private Quaternion _origiRotation;
    private int _rotationCount;
    private PointerEventData _lastDragEventData;

    public bool IsPlaced => _isPlaced;
    public InvenItem Item => _item;
    // 이번 보상 후보(아직 확정 전)인지. 확정되면 일반 소유 무기가 된다
    public bool IsCandidate { get; private set; }
    // DiscardZone에 놓여 있는지(최종 Confirm 전까지 복구 가능)
    public bool IsDiscarded { get; private set; }
    public bool WasPlacedAtDragStart => _wasPlacedAtDragStart;
    public Transform OriginParent => _originParent;

    private void Awake() => CacheComponents();

    private void Update()
    {
        if (!_isDragging) return;

        if (PlayerInputHandler.Instance.ISRotatePressed)
        {
            _item.Rotate();
            _rectTransform.Rotate(0f, 0f, -90f, Space.Self);
            _rotationCount++;
            
            if (_lastDragEventData != null)
                UpdatePlacementPreview(_lastDragEventData);
        }
        
    }
    
    // owner(InvenUI)가 있으면 드래그 시작 가능 여부·그리드 배치 허용·버리기/후보 반환을 owner의 규칙에 맡긴다
    public void SetupDrag(RectTransform dragLayer, InvenGrid invenGrid, InvenUI owner = null)
    {
        _dragLayer = dragLayer;
        _invenGrid = invenGrid;
        _owner = owner;
    }
    
    public void Initialize(InvenItem item, Vector2 cellSize, Vector2 spacing)
    {
        // 비활성 부모 아래에서 생성되면 Awake가 아직 실행되지 않았을 수 있다
        if (_iconImage == null) CacheComponents();

        _item = item;

        _iconImage.sprite = item.ItemData.Icon;
        _iconImage.preserveAspect = false;

        Vector2Int size = item.RotatedSize();

        _rectTransform.sizeDelta = new Vector2(
            size.x * cellSize.x + (size.x - 1) * spacing.x,
            size.y * cellSize.y + (size.y - 1) * spacing.y);
    }
    
    private void CacheComponents()
    {
        _iconImage = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData) => _owner?.OnItemPointerEnter(this);

    public void OnPointerExit(PointerEventData eventData) => _owner?.OnItemPointerExit(this);

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그를 시도하면 툴팁은 즉시 숨긴다(드래그가 막힌 후보도 포함)
        _owner?.OnItemDragStarted();

        if (_dragLayer == null) return;
        if (_owner != null && !_owner.CanBeginDrag(this)) return;
        _isDragging = true;
        
        _lastDragEventData = eventData;
        
        _originParent = transform.parent;
        _originPosition = transform.position;
        _originScale = transform.localScale;
        _originSiblingIndex = transform.GetSiblingIndex();
     
        _wasPlacedAtDragStart = _isPlaced;
        _origiRotation = _rectTransform.localRotation;
        _rotationCount = 0;
        
        _rectTransform.SetParent(_dragLayer, false);
        _rectTransform.SetAsLastSibling();

        _rectTransform.localScale = Vector3.one;

        MoveToPointer(eventData);
        UpdatePlacementPreview(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _lastDragEventData = eventData;
        
        MoveToPointer(eventData);
        UpdatePlacementPreview(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        _lastDragEventData = null;
        _isDragging = false;
        _invenGrid.ClearPlacementPreview();
        _owner?.OnItemDragEnded();
        
        bool isInsideGrid = _invenGrid.TryGetDropPosition(
            eventData.position,
            eventData.pressEventCamera,
            _item,
            out Vector2Int position
        );

        bool placementAllowed = _owner == null || _owner.CanPlaceInGrid(this);

        if (isInsideGrid && placementAllowed && _invenGrid.TryPlace(_item, position))
        {
            // 배치 성공
            _rectTransform.SetParent(_dragLayer, false);
            _rectTransform.localScale = Vector3.one;

            _rectTransform.position =
                _invenGrid.GetPlacementWorldPosition(_item, position);

            _isPlaced = true;
            IsDiscarded = false;
            _owner?.OnItemPlaced(this);
            return;
        }

        // 그리드 밖: DiscardZone 버리기 또는 후보 슬롯 반환을 owner가 처리
        if (_owner != null && _owner.TryHandleDrop(this, eventData))
            return;

        // 실패 시 회전 상태 복원 (배치·후보·버림 상태 모두 드래그 전 회전으로 되돌린다)
        int undoCount = (4 - _rotationCount % 4) % 4;

        for (int i = 0; i < undoCount; i++)
            _item.Rotate();

        _rectTransform.localRotation = _origiRotation;

        // 배치 실패 → 원래 위치 복귀
        _rectTransform.SetParent(_originParent, false);
        _rectTransform.SetSiblingIndex(_originSiblingIndex);

        _rectTransform.position = _originPosition;
        _rectTransform.localScale = _originScale;
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _dragLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPosition))
        {
            _rectTransform.position =
                _dragLayer.TransformPoint(localPosition);
        }
    }
    
    private void UpdatePlacementPreview(PointerEventData eventData)
    {
        if (_invenGrid.TryGetDropPosition(
                eventData.position,
                eventData.pressEventCamera,
                _item,
                out Vector2Int position))
        {
            _invenGrid.ShowPlacementPreview(_item, position);
        }
        else
        {
            _invenGrid.ClearPlacementPreview();
        }
    }
    
    public void SetPlaced(bool placed)
    {
        _isPlaced = placed;
    }

    public void SetCandidate(bool candidate)
    {
        IsCandidate = candidate;
    }

    public void SetDiscarded(bool discarded)
    {
        IsDiscarded = discarded;
    }

    // 다른 후보가 선택돼 고를 수 없을 때 흐리게 표시한다
    public void SetSelectable(bool selectable)
    {
        _iconImage.color = selectable ? Color.white : new Color(1f, 1f, 1f, 0.3f);
    }

    // 컨테이너(후보 슬롯·DiscardZone) 중앙에 지정 배율로 놓는다. 회전은 유지한다.
    public void MoveToContainer(Transform container, float scale)
    {
        _rectTransform.SetParent(container, false);
        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);
        _rectTransform.anchoredPosition = Vector2.zero;
        _rectTransform.localScale = Vector3.one * scale;
    }
}
