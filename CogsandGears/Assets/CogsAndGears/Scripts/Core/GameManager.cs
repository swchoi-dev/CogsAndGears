using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private SceneReference _mainScene;
    [SerializeField] private SceneReference _overScene;
    [SerializeField] private SceneReference _clearScene;
    
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

    private void OnChangedState(GameFlowState state)
    {
        switch (state)
        {
            case GameFlowState.GAME_CLEAR:
                SceneManager.LoadScene(_clearScene.SceneName);
                break;
            case GameFlowState.GAME_OVER:
                SceneManager.LoadScene(_overScene.SceneName);
                break;
            case GameFlowState.INVENTORY:
                Time.timeScale = 0;
                break;
            case GameFlowState.PLAYING:
                Time.timeScale = 1;
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
        if (!_mainScene.IsValid)
        {
            Debug.LogWarning("SceneReferenceSample : 씬이 연결되지 않았음");
            return;
        }

        SceneManager.LoadScene(_mainScene.SceneName);
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
