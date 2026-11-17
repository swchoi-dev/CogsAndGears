using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private SceneReference _nextScene;
    
    private void Awake()
    {
        CacheComponents();
        InitCursor();
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
