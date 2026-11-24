using TMPro;
using UnityEngine;

// 피해 숫자 1개. 피격 순간의 월드 위치를 기준으로 매 프레임 화면 좌표로 변환해(카메라 이동 반영)
// 위로 떠오르며 사라진다. DamageNumberLayer가 풀로 재사용한다.
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private DamageNumberLayer _layer;
    private RectTransform _rectTransform;
    private RectTransform _area;
    private Camera _camera;
    private Vector3 _worldPosition;
    private float _startRealtime;
    private float _duration;
    private float _riseDistance;
    private float _popScale;
    private Color _color;

    public void Play(DamageNumberLayer layer, RectTransform area, Camera worldCamera, Vector3 worldPosition, int amount, Color color, float duration, float riseDistance, float popScale)
    {
        _layer = layer;
        _area = area;
        _camera = worldCamera;
        _rectTransform = (RectTransform)transform;
        _worldPosition = worldPosition;
        _color = color;
        _duration = Mathf.Max(0.05f, duration);
        _riseDistance = riseDistance;
        _popScale = popScale;
        _startRealtime = Time.unscaledTime;

        _text.text = amount.ToString();
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        UpdateVisual(0f);
    }

    private void LateUpdate()
    {
        float t = (Time.unscaledTime - _startRealtime) / _duration;
        if (t >= 1f)
        {
            _layer.Release(this);
            return;
        }

        UpdateVisual(t);
    }

    private void UpdateVisual(float t)
    {
        // 월드 위치 기준으로 위로 떠오른다(ease-out)
        float rise = 1f - (1f - t) * (1f - t);
        Vector3 world = _worldPosition + Vector3.up * (_riseDistance * rise);
        Vector3 screen = _camera.WorldToScreenPoint(world);

        // 카메라 뒤쪽이면 표시하지 않는다
        _text.enabled = screen.z > 0f;
        if (!_text.enabled) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local);
        _rectTransform.anchoredPosition = local;

        // 처음엔 크게 튀어나왔다가 원래 크기로, 후반 40%에서 페이드아웃
        float scale = Mathf.Lerp(_popScale, 1f, Mathf.Clamp01(t / 0.15f));
        _rectTransform.localScale = Vector3.one * scale;

        Color color = _color;
        color.a *= t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        _text.color = color;
    }
}
