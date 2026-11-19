using System.Collections.Generic;
using UnityEngine;

public class InvenUI : MonoBehaviour
{
    [Header("Candidate Slots")]
    [SerializeField] private Transform[] _slotPanels = new Transform[3];

    [Header("Item UI")]
    [SerializeField] private InvenItemUI _itemPrefab;
    [SerializeField] private InvenGrid _invenGrid;

    private readonly List<InvenItemUI> _candidateUIs = new();

    public void ShowCandidates(IReadOnlyList<InvenItem> candidates)
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

            RectTransform slotRect = _slotPanels[i] as RectTransform;
            RectTransform itemRect = itemUI.GetComponent<RectTransform>();

            const float previewScale = 0.7f;
            
            // float scaleX = slotRect.rect.width / itemRect.rect.width;
            // float scaleY = slotRect.rect.height / itemRect.rect.height;

            // float scale = Mathf.Min(scaleX, scaleY) * 0.9f;

            itemRect.localScale = Vector3.one * previewScale;
            
            _candidateUIs.Add(itemUI);
        }
    }

    private void ClearCandidates()
    {
        foreach (InvenItemUI itemUI in _candidateUIs)
        {
            if (itemUI != null)
                Destroy(itemUI.gameObject);
        }

        _candidateUIs.Clear();
    }
}