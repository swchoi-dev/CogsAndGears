using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private Texture2D _cursorTexture;
    
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
}
