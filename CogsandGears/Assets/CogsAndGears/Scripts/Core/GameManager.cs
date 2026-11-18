using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private SceneReference _nextScene;
    [SerializeField] private SceneReference _gameOverScene;
    [SerializeField] private SceneReference _gameClearScene;
    
    private InvenController _inventory;
    private GameFlowManager _flowManager;
    
    private void Awake()
    {
        InitCursor();
    }

    public void RegisterFlowManager(GameFlowManager flowManager)
    {
        _flowManager = flowManager;
        _flowManager.ChangedState += OnChangedState;
    }

    public void UnRegisterFlowManager(GameFlowManager flowManager)
    {
        if (_flowManager != flowManager) return;
        
        _flowManager.ChangedState -= OnChangedState;
        _flowManager = null;
    }
    
    public void RegisterInventory(InvenController inventory)
    {
        _inventory = inventory;
        _inventory.Opened += OnInvenOpened;
        _inventory.Confirmed += OnInvenClosed;
    }

    public void UnregisterInventory(InvenController inventory)
    {
        if (_inventory != inventory) return;

        _inventory.Opened -= OnInvenOpened;
        _inventory.Confirmed -= OnInvenClosed;
        _inventory = null;
    }

    private void OnInvenOpened()
    {
        Time.timeScale = 0;
    }

    private void OnInvenClosed()
    {
        Time.timeScale = 1;
    }

    private void OnChangedState(GameFlowState state)
    {
        switch (state)
        {
            case GameFlowState.GAME_CLEAR:
                SceneManager.LoadScene(_gameClearScene.SceneName);
                break;
            case GameFlowState.GAME_OVER:
                SceneManager.LoadScene(_gameOverScene.SceneName);
                break;
        }
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
