using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Image))]
public class InvenItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image _iconImage;
    private RectTransform _rectTransform;
    private RectTransform _dragLayer;
    private InvenItem _item;
    
    private bool _isDragging;
    private Transform _originParent;
    private Vector2 _originPosition;
    private Vector3 _originScale;
    private int _originSiblingIndex;
    
    
    public InvenItem Item => _item;

    private void Awake() => CacheComponents();

    private void Update()
    {
        if (!_isDragging) return;

        if (PlayerInputHandler.Instance.ISRotatePressed)
        {
            _item.Rotate();
            _rectTransform.Rotate(0f, 0f, -90f, Space.Self);
        }
    }
    
    public void SetDragLayer(RectTransform dragLayer)
    {
        _dragLayer = dragLayer;
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
        
        _originParent = transform.parent;
        _originPosition = transform.position;
        _originScale = transform.localScale;
        _originSiblingIndex = transform.GetSiblingIndex();
        
        _isDragging = true;
        
        _rectTransform.SetParent(_dragLayer, false);
        _rectTransform.SetAsLastSibling();

        _rectTransform.localScale = Vector3.one;

        MoveToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        MoveToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        // Step 4-3 전까지는 원래 슬롯으로 복귀
        _rectTransform.SetParent(_originParent, false);
        _rectTransform.SetSiblingIndex(_originSiblingIndex);
        _rectTransform.position = _originPosition;
        _rectTransform.localScale = _originScale;
        
        _isDragging = false;
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
}
