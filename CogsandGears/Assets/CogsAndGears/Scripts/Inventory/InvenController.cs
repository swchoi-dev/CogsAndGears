using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvenController : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryUI;
    
    //Temp
    [SerializeField] private InputActionReference _invenAction;

    public event Action Opened;
    public event Action Confirmed;
    
    private void Awake()
    {
        GameManager.Instance.RegisterInventory(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterInventory(this);
    }
    
    private void OnEnable()
    {
        _invenAction.action.Enable();
        _invenAction.action.performed += _ => SetInvenUIActive(!_inventoryUI.activeSelf);
    }

    private void SetInvenUIActive(bool active)
    {
        _inventoryUI.SetActive(active);
        if (active)
        {
            Opened?.Invoke();
        }
        else
        {
            Confirmed?.Invoke();
        }
    }

    public void OpenInven()
    {
        _inventoryUI.SetActive(true);
        Opened?.Invoke();
    }

    public void ConfirmInven()
    {
        _inventoryUI.SetActive(false);
        Confirmed?.Invoke();
    }
}
