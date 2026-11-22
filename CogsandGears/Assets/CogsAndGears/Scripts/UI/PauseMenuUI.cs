using UnityEngine;
using UnityEngine.InputSystem;

// ESC 일시정지 메뉴. 정지·재개 판정과 시간 제어는 GameFlowManager / GameManager가 담당하고,
// 이 컴포넌트는 ESC 입력·메뉴 표시·읽기 전용 가방 열고 닫기만 처리한다.
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameFlowManager _gameFlow;
    [SerializeField] private InvenController _inventory;
    [SerializeField] private GameObject _menuPanel;

    private bool _isLeaving;

    private void Awake()
    {
        _menuPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _gameFlow.ChangedState += OnChangedState;
    }

    private void OnDisable()
    {
        _gameFlow.ChangedState -= OnChangedState;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        HandleEscape();
    }

    // 가방 보기 중이면 가방만 닫고, 정지 중이면 재개, 플레이 중이면 정지. 그 외(인벤토리 정비·종료 등)는 무시한다.
    public void HandleEscape()
    {
        if (_inventory.IsReadOnlyOpen)
        {
            CloseBag();
            return;
        }

        switch (_gameFlow.FlowState)
        {
            case GameFlowState.GAME_PAUSE:
                Resume();
                break;
            case GameFlowState.PLAYING:
                Pause();
                break;
        }
    }

    public void Pause()
    {
        if (_gameFlow.Pause())
            _menuPanel.SetActive(true);
    }

    public void Resume()
    {
        _inventory.CloseReadOnly();
        _menuPanel.SetActive(false);
        _gameFlow.Resume();
    }

    public void OpenBag()
    {
        if (_gameFlow.FlowState != GameFlowState.GAME_PAUSE) return;

        if (_inventory.OpenReadOnly())
            _menuPanel.SetActive(false);
    }

    public void CloseBag()
    {
        _inventory.CloseReadOnly();

        if (_gameFlow.FlowState == GameFlowState.GAME_PAUSE)
            _menuPanel.SetActive(true);
    }

    public void GoToTitle()
    {
        if (_isLeaving) return;
        _isLeaving = true;

        _inventory.CloseReadOnly();
        GameManager.Instance.LoadTitle();
    }

    // 정지가 아닌 상태로 바뀌면(종료 등) 메뉴를 남기지 않는다
    private void OnChangedState(GameFlowState state)
    {
        if (state != GameFlowState.GAME_PAUSE && _menuPanel.activeSelf)
            _menuPanel.SetActive(false);
    }
}
