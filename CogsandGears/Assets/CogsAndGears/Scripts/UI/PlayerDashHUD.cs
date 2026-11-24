using TMPro;
using UnityEngine;

// 대시 충전 게이지와 사용 가능 횟수(xN). PlayerDash가 단일 데이터 소스이며,
// 충전 진행도가 시간에 따라 변하므로 Update에서 읽어 표시만 한다(별도 타이머 없음).
public class PlayerDashHUD : MonoBehaviour
{
    [SerializeField] private PlayerDash _dash;
    [SerializeField] private GaugeUI _gauge;
    [SerializeField] private TMP_Text _countText;

    private int _shownCount = -1;

    private void OnEnable()
    {
        _shownCount = -1;
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_dash == null) return;

        // 모두 충전돼 있으면 가득, 아니면 다음 1회 충전 진행도
        _gauge.SetRatio(_dash.CurrentDashCount >= _dash.MaxDashCount ? 1f : _dash.RechargeProgress);

        int count = _dash.CurrentDashCount;
        if (count == _shownCount || _countText == null) return;

        _shownCount = count;
        _countText.text = $"x{count}";
    }
}
