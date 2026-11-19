using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class InvenController : MonoBehaviour
{
    //Temp
    [SerializeField] private InputActionReference _invenAction;
    
    [Header("Item Pool")]
    [SerializeField] private List<ItemDataSO> _itemPool;
    [SerializeField] private InvenUI _invenUI;
    
    [SerializeField] private InvenGrid _invenGrid;
    [SerializeField] private PlayerWeapon _playerWeapon;

    private readonly List<InvenItem> _candidates = new();
    private readonly Dictionary<InvenItem, WeaponRuntime> _equippedWeapons = new();
    
    public IReadOnlyList<InvenItem> Candidates => _candidates;
    
    public event Action Opened;
    public event Action Closed;
    public event Action Confirmed;
    
    private void OnEnable()
    {
        _invenAction.action.Enable();
        _invenAction.action.performed += OnInvenPerformed;
    }

    private void OnDisable()
    {
        _invenAction.action.performed -= OnInvenPerformed;
        _invenAction.action.Disable();
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
    

    private void OnInvenPerformed(InputAction.CallbackContext context)
    {
        if (_invenUI.gameObject.activeSelf)
        {
            CloseInven();
        }
        else
        {
            OpenInven();
        }
    }

    public void OpenInven()
    {
        if (_invenUI.gameObject.activeSelf) return;
        
        GenerateCandidates();
        
        _invenUI.gameObject.SetActive(true);
        _invenUI.CreateCandidates(Candidates);
        
        Opened?.Invoke();
    }

    public void CloseInven()
    {
        if (!_invenUI.gameObject.activeSelf) return;
        
        _invenUI.gameObject.SetActive(false);
        
        Closed?.Invoke();
    }

    public void ConfirmInven()
    {
        if (!_invenUI.gameObject.activeSelf) return;
        
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
