using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Image))]
public class InvenItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _iconImage;
    private RectTransform _rectTransform;
    private RectTransform _dragLayer;
    private InvenGrid _invenGrid;
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
    
    public void SetUpDrag(RectTransform dragLayer, InvenGrid invenGrid)
    {
        _dragLayer = dragLayer;
        _invenGrid = invenGrid;
    }
    
    public void Initialize(InvenItem item, Vector2 cellSize, Vector2 spacing)
    {
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_dragLayer == null) return;
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
        
        bool isInsideGrid = _invenGrid.TryGetDropPosition(
            eventData.position,
            eventData.pressEventCamera,
            _item,
            out Vector2Int position
        );

        if (isInsideGrid && _invenGrid.TryPlace(_item, position))
        {
            // 배치 성공
            _rectTransform.SetParent(_dragLayer, false);
            _rectTransform.localScale = Vector3.one;

            _rectTransform.position =
                _invenGrid.GetPlacementWorldPosition(_item, position);

            _isPlaced = true;
            return;
        }

        // 기존에 배치된 아이템이라면 회전 상태 복원
        if (_wasPlacedAtDragStart)
        {
            int undoCount = (4 - _rotationCount % 4) % 4;

            for (int i = 0; i < undoCount; i++)
                _item.Rotate();

            _rectTransform.localRotation = _origiRotation;
        }

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
}
