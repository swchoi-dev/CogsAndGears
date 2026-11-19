using System.Collections.Generic;
using UnityEngine;

public class InvenUI : MonoBehaviour
{
    [Header("Candidate Slots")]
    [SerializeField] private Transform[] _slotPanels = new Transform[3];

    [Header("Item UI")]
    [SerializeField] private InvenItemUI _itemPrefab;
    [SerializeField] private InvenGrid _invenGrid;

    [SerializeField] private RectTransform _itemLayer;
    
    private readonly List<InvenItemUI> _candidateUIs = new();

    public void CreateCandidates(IReadOnlyList<InvenItem> candidates)
    {
        ClearCandidates();

        int count = Mathf.Min(candidates.Count, _slotPanels.Length);

        for (int i = 0; i < count; i++)
        {
            InvenItemUI itemUI = Instantiate(
                _itemPrefab,
                _slotPanels[i]
            );
            RectTransform rect = itemUI.GetComponent<RectTransform>();
            
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            
            itemUI.Initialize(
                candidates[i],
                _invenGrid.CellSize,
                _invenGrid.Spacing
            );

            itemUI.SetupDrag(_itemLayer, _invenGrid);
            
            RectTransform itemRect = itemUI.GetComponent<RectTransform>();

            const float previewScale = 0.7f;
            itemRect.localScale = Vector3.one * previewScale;
            
            _candidateUIs.Add(itemUI);
        }
    }
    
    public void CreateDefaultItem(InvenItem item, Vector2Int position)
    {
        InvenItemUI itemUI = Instantiate(_itemPrefab, _itemLayer);

        itemUI.Initialize(
            item,
            _invenGrid.CellSize,
            _invenGrid.Spacing
        );

        itemUI.SetupDrag(_itemLayer, _invenGrid);

        RectTransform rect = itemUI.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.localScale = Vector3.one;
        rect.position =
            _invenGrid.GetPlacementWorldPosition(item, position);

        // 기존 InvenItemUI에서 배치 상태로 인식하도록 설정
        itemUI.SetPlaced(true);
    }

    private void ClearCandidates()
    {
        foreach (InvenItemUI itemUI in _candidateUIs)
        {
            if (itemUI != null && !itemUI.IsPlaced)
                Destroy(itemUI.gameObject);
        }

        _candidateUIs.Clear();
    }
}