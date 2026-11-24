using TMPro;
using UnityEngine;

// 화면 밖 몬스터 경고 마커 1개. 화살표는 대상 방향으로 회전하고, 거리 텍스트는 화살표 바깥쪽(대상 방향)에 둔다.
// 예: 오른쪽 "→ 14m", 왼쪽 "14m ←". 텍스트가 화면을 벗어나지 않도록 OffscreenMonsterHUD 여백을 _textOffset보다 크게 둔다.
public class OffscreenWarningMarker : MonoBehaviour
{
    [Tooltip("오른쪽(+X)을 가리키도록 만든 화살표. 대상 방향으로 회전한다")]
    [SerializeField] private RectTransform _arrow;
    [SerializeField] private TMP_Text _distanceText;
    [Tooltip("화살표 중심에서 거리 텍스트 중심까지의 거리(대상 방향)")]
    [SerializeField] private float _textOffset = 42f;

    private int _shownDistance = -1;

    public RectTransform RectTransform => (RectTransform)transform;

    public void Set(Vector2 localPosition, Vector2 direction, float distance)
    {
        RectTransform.anchoredPosition = localPosition;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _arrow.localRotation = Quaternion.Euler(0f, 0f, angle);
        _distanceText.rectTransform.anchoredPosition = direction.normalized * _textOffset;

        int shown = Mathf.RoundToInt(distance);
        if (shown == _shownDistance) return;

        _shownDistance = shown;
        _distanceText.text = $"{shown}m";
    }
}
