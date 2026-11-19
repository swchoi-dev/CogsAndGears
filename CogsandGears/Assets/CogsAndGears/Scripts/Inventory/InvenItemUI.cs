using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InvenItemUI : MonoBehaviour
{
    private Image _iconImage;
    private RectTransform _rectTransform;
    private InvenItem _item;
    
    public InvenItem Item => _item;

    private void Awake() => CacheComponents();

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
}
