using TMPro;
using UnityEngine;

// 비율 게이지와 선택적 "현재 / 최대" 텍스트를 표시한다.
// 채움은 별도 스프라이트 없이 Fill RectTransform의 anchorMax.x(0~1)로 표현한다.
// 값은 보관하지 않고 데이터 소스(Health, PlayerDash 등)에서 받은 값을 그대로 그린다.
public class GaugeUI : MonoBehaviour
{
    [Tooltip("채움 영역. anchorMin.x = 0 기준으로 anchorMax.x를 비율로 바꾼다")]
    [SerializeField] private RectTransform _fill;
    [Tooltip("비워두면 수치를 표시하지 않는다")]
    [SerializeField] private TMP_Text _valueText;

    private int _shownCurrent = -1;
    private int _shownMax = -1;

    public void SetValue(float current, float max)
    {
        float clamped = Mathf.Clamp(current, 0f, Mathf.Max(0f, max));
        SetRatio(max > 0f ? clamped / max : 0f);

        if (_valueText == null) return;

        // 내림 표시(충전 중 29.9가 가득 찬 것처럼 보이지 않게). 단, 0보다 크면 최소 1로 표시해 살아 있는데 0으로 보이지 않게 한다.
        // 같은 값이면 문자열을 다시 만들지 않는다.
        int shownCurrent = clamped > 0f ? Mathf.Max(1, Mathf.FloorToInt(clamped)) : 0;
        int shownMax = Mathf.FloorToInt(max);
        if (shownCurrent == _shownCurrent && shownMax == _shownMax) return;

        _shownCurrent = shownCurrent;
        _shownMax = shownMax;
        _valueText.text = $"{shownCurrent} / {shownMax}";
    }

    public void SetRatio(float ratio)
    {
        if (_fill == null) return;

        Vector2 anchorMax = _fill.anchorMax;
        anchorMax.x = Mathf.Clamp01(ratio);
        _fill.anchorMax = anchorMax;
    }
}
