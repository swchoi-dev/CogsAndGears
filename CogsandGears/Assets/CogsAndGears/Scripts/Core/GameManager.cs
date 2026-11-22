using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private Texture2D _cursorTexture;
    // Title 씬에는 이전 필드명(_nextScene)으로 저장돼 있어, 이름 변경 후에도 값을 읽도록 연결한다
    [FormerlySerializedAs("_nextScene")]
    [SerializeField] private SceneReference _mainScene;
    [SerializeField] private SceneReference _overScene;
    [SerializeField] private SceneReference _clearScene;
    [SerializeField] private SceneReference _titleScene;
    
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
                LoadScene(_clearScene);
                break;
            case GameFlowState.GAME_OVER:
                LoadScene(_overScene);
                break;
            case GameFlowState.INVENTORY:
            case GameFlowState.GAME_PAUSE:
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
    
    public void LoadTitle()
    {
        LoadScene(_titleScene);
    }

    // Scene을 떠날 때는 정지·Hit Stop 등으로 바뀐 시간 배율을 되돌린다
    private void LoadScene(SceneReference scene)
    {
        if (scene == null || !scene.IsValid)
        {
            Debug.LogWarning("GameManager : 이동할 씬이 연결되지 않았음");
            return;
        }

        Time.timeScale = 1;
        SceneManager.LoadScene(scene.SceneName);
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
