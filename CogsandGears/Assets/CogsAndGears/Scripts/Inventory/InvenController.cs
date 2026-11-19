using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class InvenController : MonoBehaviour
{
    [Header("Item Pool")]
    [SerializeField] private List<ItemDataSO> _itemPool;
    [SerializeField] private InvenUI _invenUI;
    //Temp
    [SerializeField] private InputActionReference _invenAction;

    private readonly List<InvenItem> _candidates = new();
    
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
        _invenUI.ShowCandidates(Candidates);
        
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
        
        _invenUI.gameObject.SetActive(false);
        
        Confirmed?.Invoke();
    }
}
