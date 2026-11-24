using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

// Stage 클리어별 가방 확장 칸 수. StageIndex는 0부터(0 = Stage 1 클리어)
[Serializable]
public struct StageExpansionData
{
    public int StageIndex;
    public int ExpansionCount;
}

// Stage Clear 후 인벤토리 흐름: Expansion Phase(가방 확장) → Reward Phase(보상 1개·버리기·정비) → 확정 → 다음 Stage.
// Confirmed는 최종 Reward 확정에서만 발생한다(Expansion 확정은 Phase 전환만 한다).
public class InvenController : MonoBehaviour
{
    //Temp
    [SerializeField] private InputActionReference _invenAction;
    
    [Header("Item Pool")]
    [SerializeField] private List<ItemDataSO> _itemPool;
    [SerializeField] private InvenUI _invenUI;
    
    [SerializeField] private InvenGrid _invenGrid;
    [SerializeField] private PlayerWeapon _playerWeapon;

    [Header("Expansion")]
    [SerializeField] private List<StageExpansionData> _stageExpansions = new()
    {
        new StageExpansionData { StageIndex = 0, ExpansionCount = 4 },
        new StageExpansionData { StageIndex = 1, ExpansionCount = 6 },
        new StageExpansionData { StageIndex = 2, ExpansionCount = 8 },
    };

    // 같은 Stage 보상 중복 지급 방지
    private int _lastOpenedStageIndex = -1;

    private readonly List<InvenItem> _candidates = new();
    private readonly Dictionary<InvenItem, WeaponRuntime> _equippedWeapons = new();
    
    public IReadOnlyList<InvenItem> Candidates => _candidates;
    
    public event Action Opened;
    public event Action Closed;
    public event Action Confirmed;
    
    private void OnEnable()
    {
        GameFlowManager.Instance.ChangedState += OnChangedState;
        
    }

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ChangedState -= OnChangedState;
        }
    }
    

    private void GenerateCandidates()
    {
        _candidates.Clear();

        List<ItemDataSO> availableItem = new();

        foreach (var data in _itemPool)
        {
            if (data != null && !availableItem.Contains(data))
            {
                availableItem.Add(data);
            }
        }
        
        int count = Mathf.Min(3, availableItem.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, availableItem.Count); 
            var item = new InvenItem(availableItem[index]);
            
            _candidates.Add(item);
            availableItem.RemoveAt(index);
        }
    }
    
    private void OnChangedState(GameFlowState state)
    {
        switch (state)
        {
            case GameFlowState.STAGE_CLEAR:
                OpenInven();
                break;
        }
    }

    public void OpenInven()
    {
        if (_invenUI.gameObject.activeSelf) return;

        int stageIndex = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentStageIndex : -1;
        if (stageIndex >= 0 && stageIndex == _lastOpenedStageIndex) return;
        _lastOpenedStageIndex = stageIndex;

        GenerateCandidates();
        
        // 첫 활성화 시 InvenGrid.Awake가 기본 무기를 만든다
        _invenUI.gameObject.SetActive(true);
        RegisterDefaultWeapon();
        _invenUI.CreateCandidates(Candidates);

        int expansionCount = GetExpansionCount(stageIndex);
        _invenGrid.BeginExpansion(expansionCount);

        // 확장할 칸이 없으면(설정 0 또는 잠긴 셀 없음) 바로 보상 Phase
        if (_invenGrid.ExpansionLimit > 0)
            _invenUI.ShowExpansionPhase();
        else
        {
            _invenGrid.CommitExpansion();
            _invenUI.ShowRewardPhase();
        }
        
        Opened?.Invoke();
    }

    // Expansion Confirm: 확장을 확정하고 보상 Phase로만 이동한다(다음 Stage로 진행하지 않음)
    public void ConfirmExpansion()
    {
        if (!_invenUI.gameObject.activeSelf || _invenUI.Phase != InventoryPhase.Expansion) return;
        if (!_invenGrid.CommitExpansion()) return;

        _invenUI.ShowRewardPhase();
    }

    private int GetExpansionCount(int stageIndex)
    {
        foreach (StageExpansionData data in _stageExpansions)
        {
            if (data.StageIndex == stageIndex)
                return Mathf.Max(0, data.ExpansionCount);
        }

        return 0;
    }

    // 그리드의 기본 무기를 PlayerWeapon에 이미 있는 같은 무기와 연결한다.
    // 연결하지 않으면 첫 확정 때 같은 무기가 중복 장착되고, 기본 무기를 버려도 발사 목록에서 빠지지 않는다.
    private void RegisterDefaultWeapon()
    {
        InvenItem defaultItem = _invenGrid.DefaultItem;
        if (defaultItem == null || _equippedWeapons.ContainsKey(defaultItem)) return;

        foreach (WeaponRuntime runtime in _playerWeapon.Weapons)
        {
            if (runtime.Data != defaultItem.ItemData || _equippedWeapons.ContainsValue(runtime)) continue;

            _equippedWeapons.Add(defaultItem, runtime);
            return;
        }
    }

    public void CloseInven()
    {
        if (!_invenUI.gameObject.activeSelf) return;
        
        _invenUI.gameObject.SetActive(false);
        
        Closed?.Invoke();
    }

    // ---------- 일시정지 중 읽기 전용 가방 ----------

    public bool IsReadOnlyOpen => _invenUI.gameObject.activeSelf && _invenUI.IsReadOnly;

    // 정비 중(Expansion/Reward)이면 열지 않는다. 후보 생성·확장·Opened 이벤트 없이 현재 가방만 보여 준다.
    public bool OpenReadOnly()
    {
        if (_invenUI.gameObject.activeSelf) return false;

        _invenUI.gameObject.SetActive(true);
        RegisterDefaultWeapon();
        _invenUI.ShowReadOnly();
        return true;
    }

    public void CloseReadOnly()
    {
        if (!IsReadOnlyOpen) return;

        _invenUI.HideReadOnly();
        _invenUI.gameObject.SetActive(false);
    }

    public InventoryLayout CaptureInventoryLayout() => _invenGrid.CaptureLayout();

    // Reward Confirm(최종): 인벤토리에 무기가 1개 이상일 때만 확정하고 다음 Stage로 진행한다
    public void ConfirmInven()
    {
        if (!_invenUI.gameObject.activeSelf) return;
        if (_invenUI.Phase != InventoryPhase.Reward || !_invenUI.CanConfirmReward) return;

        _invenUI.FinalizeReward();
        SyncPlayerWeapons();
        _invenUI.gameObject.SetActive(false);
        
        Confirmed?.Invoke();
    }
    
    private void SyncPlayerWeapons()
    {
        HashSet<InvenItem> placedItems =
            new(_invenGrid.GetPlacedItems());

        // 1. 그리드에서 제거된 무기 해제
        List<InvenItem> equippedItems =
            new(_equippedWeapons.Keys);

        foreach (InvenItem item in equippedItems)
        {
            if (placedItems.Contains(item))
                continue;

            WeaponRuntime runtime = _equippedWeapons[item];

            if (_playerWeapon.RemoveWeapon(runtime))
                _equippedWeapons.Remove(item);
        }

        // 2. 새롭게 배치된 무기 장착
        foreach (InvenItem item in placedItems)
        {
            if (_equippedWeapons.ContainsKey(item))
                continue;

            if (item.ItemData is not WeaponDataSO weaponData)
                continue;

            WeaponRuntime runtime =
                _playerWeapon.AddWeapon(weaponData);

            _equippedWeapons.Add(item, runtime);
        }
    }
}
