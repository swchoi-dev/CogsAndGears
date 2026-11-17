using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private SceneReference _nextScene;
    [SerializeField] private GameObject _inventoryUI;
    
    //Temp
    [SerializeField] private InputActionReference _invenAction;
    
    private bool _isInvenActive;
    
    private void Awake()
    {
        CacheComponents();
        InitCursor();
        _invenAction.action.Enable();
        _invenAction.action.performed += _ => _inventoryUI.SetActive(!_inventoryUI.activeSelf);
    }

    private void CacheComponents()
    {
        
    }

    private void InitCursor()
    {
        if (_cursorTexture != null)
        {
            Vector2 hotspot = new Vector2(
                _cursorTexture.width / 2f,
                _cursorTexture.height / 2f
                );
            
            Cursor.SetCursor(
                _cursorTexture,
                hotspot, 
                CursorMode.ForceSoftware);
        }        
    }
    
    public void LoadNextScene()
    {
        if (!_nextScene.IsValid)
        {
            Debug.LogWarning("SceneReferenceSample : 씬이 연결되지 않았음");
            return;
        }

        SceneManager.LoadScene(_nextScene.SceneName);
    }
    
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
