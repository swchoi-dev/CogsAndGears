using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum InventoryPhase
{
    Expansion,
    Reward
}

// 인벤토리 창 UI. 같은 창 안에서 ExpansionLayer(가방 확장)와 RewardLayer(보상·버리기·정비)를 전환한다.
// 아이템 드래그 규칙(시작 가능 여부, 후보 1개 제한, 버리기·후보 반환)을 InvenItemUI 대신 판단한다.
public class InvenUI : MonoBehaviour
{
    [Header("Candidate Slots")]
    [SerializeField] private Transform[] _slotPanels = new Transform[3];

    [Header("Item UI")]
    [SerializeField] private InvenItemUI _itemPrefab;
    [SerializeField] private InvenGrid _invenGrid;

    [SerializeField] private RectTransform _itemLayer;

    [Header("Phase Layers")]
    [SerializeField] private GameObject _expansionLayer;
    [SerializeField] private GameObject _rewardLayer;

    [Header("Expansion UI")]
    [SerializeField] private TMP_Text _expansionRemainingText;
    [SerializeField] private Button _expansionConfirmButton;

    [Header("Reward UI")]
    [SerializeField] private Button _rewardConfirmButton;
    [Tooltip("기존 무기를 버리는 영역(드롭 판정 범위)")]
    [SerializeField] private RectTransform _discardZone;
    [Tooltip("버린 무기를 쌓아 두는 부모")]
    [SerializeField] private RectTransform _discardContainer;
    [SerializeField] private float _discardItemScale = 0.5f;
    [SerializeField] private float _discardItemSpacing = 12f;

    [Header("Read Only (Pause)")]
    [Tooltip("일시정지 중 가방 보기에서 표시하는 레이어(제목·뒤로 버튼)")]
    [SerializeField] private GameObject _readOnlyLayer;

    [Header("Tooltip")]
    [SerializeField] private ItemTooltipUI _tooltip;

    private const float CandidatePreviewScale = 0.7f;

    private readonly List<InvenItemUI> _candidateUIs = new();
    private readonly List<InvenItemUI> _discardedUIs = new();
    private bool _isDraggingItem;

    public InventoryPhase Phase { get; private set; } = InventoryPhase.Reward;
    // 일시정지 중 가방 보기. 드래그·회전·버리기·확장·확정을 모두 막는다
    public bool IsReadOnly { get; private set; }

    // 인벤토리에 배치된 후보(이번 보상으로 선택한 무기). 최대 1개
    public InvenItemUI SelectedCandidate => _candidateUIs.Find(ui => ui != null && ui.IsPlaced);

    // 최종 Confirm 조건: 인벤토리(그리드)에 무기가 1개 이상. DiscardZone의 무기는 포함하지 않는다
    public bool CanConfirmReward => _invenGrid.GetPlacedItems().Count >= 1;

    private void OnEnable()
    {
        _invenGrid.ExpansionChanged += RefreshExpansionUI;
    }

    private void OnDisable()
    {
        _invenGrid.ExpansionChanged -= RefreshExpansionUI;
        HideTooltip();
    }

    public void ShowReadOnly()
    {
        IsReadOnly = true;
        _expansionLayer.SetActive(false);
        _rewardLayer.SetActive(false);
        SetReadOnlyLayer(true);
        HideTooltip();
    }

    public void HideReadOnly()
    {
        IsReadOnly = false;
        SetReadOnlyLayer(false);
    }

    private void SetReadOnlyLayer(bool active)
    {
        if (_readOnlyLayer != null)
            _readOnlyLayer.SetActive(active);
    }

    public void ShowExpansionPhase()
    {
        HideReadOnly();
        HideTooltip();
        Phase = InventoryPhase.Expansion;
        _expansionLayer.SetActive(true);
        _rewardLayer.SetActive(false);
        RefreshExpansionUI();
    }

    public void ShowRewardPhase()
    {
        HideReadOnly();
        HideTooltip();
        Phase = InventoryPhase.Reward;
        _expansionLayer.SetActive(false);
        _rewardLayer.SetActive(true);
        RefreshRewardUI();
    }

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

            itemUI.Initialize(
                candidates[i],
                _invenGrid.CellSize,
                _invenGrid.Spacing
            );

            itemUI.SetupDrag(_itemLayer, _invenGrid, this);
            itemUI.SetCandidate(true);
            itemUI.MoveToContainer(_slotPanels[i], CandidatePreviewScale);

            _candidateUIs.Add(itemUI);
        }

        RefreshRewardUI();
    }

    public void CreateDefaultItem(InvenItem item, Vector2Int position)
    {
        InvenItemUI itemUI = Instantiate(_itemPrefab, _itemLayer);

        itemUI.Initialize(
            item,
            _invenGrid.CellSize,
            _invenGrid.Spacing
        );

        itemUI.SetupDrag(_itemLayer, _invenGrid, this);

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

    // ---------- 드래그 규칙 (InvenItemUI가 호출) ----------

    // 확장 Phase에서는 모든 드래그 금지. 보상 Phase에서 다른 후보가 이미 선택돼 있으면 나머지 후보는 드래그 불가.
    public bool CanBeginDrag(InvenItemUI itemUI)
    {
        if (IsReadOnly || Phase != InventoryPhase.Reward) return false;
        if (!itemUI.IsCandidate || itemUI.IsPlaced) return true;

        InvenItemUI selected = SelectedCandidate;
        return selected == null || selected == itemUI;
    }

    // 실제 배치 단계에서도 후보 1개 제한을 다시 검증한다(UI 비활성화만으로 막지 않음)
    public bool CanPlaceInGrid(InvenItemUI itemUI)
    {
        if (IsReadOnly || Phase != InventoryPhase.Reward) return false;
        if (!itemUI.IsCandidate) return true;

        InvenItemUI selected = SelectedCandidate;
        return selected == null || selected == itemUI;
    }

    public void OnItemPlaced(InvenItemUI itemUI)
    {
        // DiscardZone에서 복구된 무기
        if (_discardedUIs.Remove(itemUI))
            LayoutDiscardZone();

        RefreshRewardUI();
    }

    // 그리드 밖에 놓았을 때: 기존 무기 → DiscardZone, 배치된 후보 → 원래 후보 슬롯(선택 취소). 처리하면 true.
    public bool TryHandleDrop(InvenItemUI itemUI, PointerEventData eventData)
    {
        if (IsReadOnly || Phase != InventoryPhase.Reward) return false;

        Camera eventCamera = eventData.pressEventCamera;

        // 인벤토리에 배치돼 있던 기존 무기만 버릴 수 있다(후보 슬롯의 무기·이번 보상 후보는 제외)
        if (!itemUI.IsCandidate &&
            itemUI.WasPlacedAtDragStart &&
            RectTransformUtility.RectangleContainsScreenPoint(_discardZone, eventData.position, eventCamera))
        {
            _invenGrid.Remove(itemUI.Item);
            itemUI.SetPlaced(false);
            itemUI.SetDiscarded(true);
            _discardedUIs.Add(itemUI);
            LayoutDiscardZone();
            RefreshRewardUI();
            return true;
        }

        // 배치했던 후보를 원래 슬롯에 되돌리면 선택 취소
        if (itemUI.IsCandidate && itemUI.WasPlacedAtDragStart)
        {
            int slotIndex = _candidateUIs.IndexOf(itemUI);

            if (slotIndex >= 0 &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    (RectTransform)_slotPanels[slotIndex], eventData.position, eventCamera))
            {
                _invenGrid.Remove(itemUI.Item);
                itemUI.SetPlaced(false);
                itemUI.MoveToContainer(_slotPanels[slotIndex], CandidatePreviewScale);
                RefreshRewardUI();
                return true;
            }
        }

        return false;
    }

    // ---------- 후보 툴팁 (InvenItemUI가 호출) ----------

    // 보상 Phase의 후보에만 표시한다. 선택 불가(흐림) 후보도 정보는 볼 수 있다. 드래그 중에는 표시하지 않는다.
    public void OnItemPointerEnter(InvenItemUI itemUI)
    {
        if (_tooltip == null || _isDraggingItem) return;
        if (IsReadOnly || Phase != InventoryPhase.Reward || !itemUI.IsCandidate) return;

        _tooltip.Show(itemUI.Item.ItemData);
    }

    public void OnItemPointerExit(InvenItemUI itemUI)
    {
        if (_tooltip != null && _tooltip.Current == itemUI.Item.ItemData)
            _tooltip.Hide();
    }

    public void OnItemDragStarted()
    {
        _isDraggingItem = true;
        HideTooltip();
    }

    public void OnItemDragEnded()
    {
        _isDraggingItem = false;
    }

    private void HideTooltip()
    {
        if (_tooltip != null)
            _tooltip.Hide();
    }

    // ---------- 최종 확정 ----------

    // 보상 확정: 버린 무기·선택하지 않은 후보 UI를 제거하고, 선택한 후보를 일반 소유 무기로 전환한다.
    // (실제 장착 무기 갱신은 InvenController.SyncPlayerWeapons가 그리드 배치 기준으로 처리)
    public void FinalizeReward()
    {
        HideTooltip();

        foreach (InvenItemUI discarded in _discardedUIs)
        {
            if (discarded != null)
                Destroy(discarded.gameObject);
        }

        _discardedUIs.Clear();

        InvenItemUI selected = SelectedCandidate;
        if (selected != null)
        {
            selected.SetCandidate(false);
            selected.SetSelectable(true);
            _candidateUIs.Remove(selected);
        }

        ClearCandidates();
    }

    // ---------- UI 갱신 ----------

    private void RefreshExpansionUI()
    {
        if (_expansionRemainingText != null)
            _expansionRemainingText.text = $"Expansion Left: {_invenGrid.RemainingExpansion}";

        if (_expansionConfirmButton != null)
            _expansionConfirmButton.interactable =
                _invenGrid.IsExpanding && _invenGrid.RemainingExpansion == 0;
    }

    private void RefreshRewardUI()
    {
        InvenItemUI selected = SelectedCandidate;

        foreach (InvenItemUI candidate in _candidateUIs)
        {
            if (candidate != null)
                candidate.SetSelectable(selected == null || selected == candidate);
        }

        if (_rewardConfirmButton != null)
            _rewardConfirmButton.interactable = CanConfirmReward;
    }

    // 버린 무기를 위에서부터 세로로 쌓는다(회전 상태에 따른 실제 높이 반영)
    private void LayoutDiscardZone()
    {
        float y = _discardContainer.rect.height * 0.5f;

        foreach (InvenItemUI itemUI in _discardedUIs)
        {
            itemUI.MoveToContainer(_discardContainer, _discardItemScale);

            RectTransform rect = (RectTransform)itemUI.transform;
            bool sideways = Mathf.RoundToInt(rect.localEulerAngles.z / 90f) % 2 != 0;
            float height = (sideways ? rect.sizeDelta.x : rect.sizeDelta.y) * _discardItemScale;

            y -= height * 0.5f;
            rect.anchoredPosition = new Vector2(0f, y);
            y -= height * 0.5f + _discardItemSpacing;
        }
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
