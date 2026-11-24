using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 아이템 상세 정보 패널(하나만 재사용). 마우스 근처에 표시하고 화면 밖으로 나가지 않게 위치를 조정한다.
// 표시 값은 ItemDataSO / WeaponDataSO에 실제로 있는 데이터만 사용한다. 패널은 레이캐스트를 막지 않는다.
public class ItemTooltipUI : MonoBehaviour
{
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _statsText;
    [Tooltip("마우스 위치 기준 패널 오프셋(화면 픽셀)")]
    [SerializeField] private Vector2 _offset = new Vector2(24f, -24f);
    [SerializeField] private float _screenMargin = 12f;

    public ItemDataSO Current { get; private set; }

    private Canvas _rootCanvas;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        _rootCanvas = canvas != null ? canvas.rootCanvas : null;
        Hide();
    }

    private void Update()
    {
        if (Current != null && Mouse.current != null)
            UpdatePosition(Mouse.current.position.ReadValue());
    }

    public void Show(ItemDataSO data)
    {
        if (data == null) return;

        Current = data;
        _nameText.text = data.ItemName;
        _statsText.text = BuildStats(data);
        _panel.gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);

        if (Mouse.current != null)
            UpdatePosition(Mouse.current.position.ReadValue());
    }

    public void Hide()
    {
        Current = null;

        if (_panel != null)
            _panel.gameObject.SetActive(false);
    }

    private const string ValueColumn = "<pos=58%>";

    private static string BuildStats(ItemDataSO data)
    {
        string stats = "";

        if (data is WeaponDataSO weapon)
        {
            stats += $"Damage{ValueColumn}{weapon.Damage:0.##}\n";
            stats += $"Fire Rate{ValueColumn}{weapon.FireRate:0.##} /s\n";
            stats += $"Projectile Speed{ValueColumn}{weapon.ProjectileSpeed:0.##}\n";
            stats += $"Spread{ValueColumn}±{weapon.SpreadAngle:0.#}°\n";
        }

        int cellCount = data.OccupiedCells != null ? data.OccupiedCells.Count : 0;
        stats += $"Size{ValueColumn}{data.Size.x} × {data.Size.y} ({cellCount} cells)";
        return stats;
    }

    // 기본은 마우스 오른쪽 아래. 화면 오른쪽·아래로 넘치면 반대쪽으로 뒤집고, 마지막으로 화면 안으로 제한한다.
    // 화면 범위는 Screen Space Overlay 루트 캔버스의 pixelRect(실제 게임 화면)를 기준으로 한다.
    private void UpdatePosition(Vector2 mouse)
    {
        Rect screen = _rootCanvas != null ? _rootCanvas.pixelRect : new Rect(0f, 0f, Screen.width, Screen.height);
        Vector2 size = _panel.rect.size * _panel.lossyScale;
        Vector2 position = mouse + _offset;

        if (position.x + size.x > screen.xMax - _screenMargin)
            position.x = mouse.x - _offset.x - size.x;

        if (position.y - size.y < screen.yMin + _screenMargin)
            position.y = mouse.y - _offset.y + size.y;

        position.x = Mathf.Clamp(position.x, screen.xMin + _screenMargin, Mathf.Max(screen.xMin + _screenMargin, screen.xMax - _screenMargin - size.x));
        position.y = Mathf.Clamp(position.y, Mathf.Min(screen.yMax - _screenMargin, screen.yMin + _screenMargin + size.y), screen.yMax - _screenMargin);

        // 패널 pivot은 좌상단(0,1)
        _panel.position = position;
    }
}
