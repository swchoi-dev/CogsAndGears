using System;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Serialization;

public class GameFlowManager : SingletonBehaviour<GameFlowManager>
{
    private const int FIRST_STAGE = 0;
    [SerializeField] private List<StageController> _stages;
    [SerializeField] private InvenController _inventory;
    [SerializeField] private Transform _cogs;
    [SerializeField] private CameraFollow2D _cameraFollow2D;
    
    private GameFlowState _flowState;
    private StageController _currentStage;
    private StageController _prevStage;
    private int _currentStageIndex;
    private Health _playerHealth;
    
    public GameFlowState FlowState => _flowState;
    public StageController CurrentStage => _currentStage;
    // 0부터 시작하는 현재 Stage 순번(0 = Stage 1)
    public int CurrentStageIndex => _currentStageIndex;
    
    public event Action<GameFlowState> ChangedState;
    // 새 Stage가 시작되면 호출된다. HUD가 대상 Stage를 교체하는 데 사용한다.
    public event Action<StageController> StageStarted;

    private void Awake()
    {
        GameManager.Instance.RegisterFlowManager(this);
        _playerHealth = _cogs.GetComponent<Health>();
    }
    
    
    private void OnEnable()
    {
        _inventory.Confirmed += OnInvenConfirm;
        _inventory.Opened += OnInvenOpen;
        _inventory.Closed += OnInvenClose;

        if (_playerHealth != null)
            _playerHealth.OnDeath += GameOver;
    }
    
    private void Start()
    {
        InitGameRun();
        StartStage();
    }

    private void OnDisable()
    {
        _inventory.Confirmed -= OnInvenConfirm;
        _inventory.Opened -= OnInvenOpen;
        _inventory.Closed -= OnInvenClose;

        if (_playerHealth != null)
            _playerHealth.OnDeath -= GameOver;
    }

    private void OnDestroy()
    {
        GameManager.Instance.UnRegisterFlowManager(this);
    }
    
    
    private void InitGameRun()
    {
        // 새 Run에서는 이전 결과 스냅샷을 쓰지 않는다
        RunResult.Clear();

        // 첫 Stage만 활성화되도록 모든 Stage를 끈 상태에서 시작한다(StartStage가 현재 Stage를 켠다)
        foreach (var stage in _stages)
        {
            stage.gameObject.SetActive(false);
        }

        _currentStageIndex = FIRST_STAGE;
        _currentStage = _stages[_currentStageIndex];
    }

    private void StartStage()
    {
        // TO-DO _currentStage.PlayStartAnim();
        
        _currentStage.gameObject.SetActive(true);
        
        if (_prevStage != null)
        {
            _prevStage.gameObject.SetActive(false);
        }
        
        _cogs.position = _currentStage.PlayerSpawnPoint.position;
        _cameraFollow2D.SetCameraBounds(_currentStage.CameraBounds);
        
        _currentStage.Cleared += StageClear;
        _currentStage.StartStage();
        StageStarted?.Invoke(_currentStage);
        
        ChangeFlowState(GameFlowState.PLAYING);
    }

    private void NextStage()
    {
        _prevStage = _currentStage;

        if (_currentStageIndex == _stages.Count - 1)
        {
            GameClear();
            return;
        }
        
        _currentStageIndex++;
        _currentStage = _stages[_currentStageIndex];
        
        StartStage();
    }
    
    private void OnInvenConfirm()
    {
        // if (_flowState != GameFlowState.STAGE_CLEAR) return;

        NextStage();
    }

    private void OnInvenOpen()
    {
        ChangeFlowState(GameFlowState.INVENTORY);
    }

    private void OnInvenClose()
    {
        ChangeFlowState(GameFlowState.PLAYING);
    }
    
    
    private void StageClear()
    {
        _currentStage.Cleared -= StageClear;

        // 마지막 Stage는 인벤토리 정비 없이 바로 Run을 클리어한다
        if (_currentStageIndex == _stages.Count - 1)
        {
            GameClear();
            return;
        }

        ChangeFlowState(GameFlowState.STAGE_CLEAR);
    }

    private void GameClear()
    {
        if (IsRunEnded) return;

        SaveRunResult(true);
        ChangeFlowState(GameFlowState.GAME_CLEAR);
    }

    // 플레이어 Health.OnDeath에서 호출된다. 이미 종료된 Run이면 무시한다(중복 처리 방지).
    private void GameOver()
    {
        if (IsRunEnded) return;

        SaveRunResult(false);
        ChangeFlowState(GameFlowState.GAME_OVER);
    }

    private bool IsRunEnded =>
        _flowState == GameFlowState.GAME_CLEAR || _flowState == GameFlowState.GAME_OVER;

    // 지금까지 진행한 모든 Stage의 처치 수 합계(각 Stage는 Run 동안 한 번만 진행된다)
    public int TotalKilledMonsters
    {
        get
        {
            int total = 0;
            for (int i = 0; i <= _currentStageIndex && i < _stages.Count; i++)
                total += _stages[i].KilledMonsterCount;

            return total;
        }
    }

    // 종료 순간의 결과를 스냅샷으로 보관한다. 결과 화면은 이 값만 읽는다.
    private void SaveRunResult(bool isClear)
    {
        RunResult.Save(new RunResult(
            isClear,
            _currentStageIndex + 1,
            TotalKilledMonsters,
            _inventory.CaptureInventoryLayout()));
    }

    // PLAYING에서만 정지한다(인벤토리 정비·종료·전환 중에는 무시). 시간 정지는 GameManager가 상태 변경으로 처리한다.
    public bool Pause()
    {
        if (_flowState != GameFlowState.PLAYING) return false;

        ChangeFlowState(GameFlowState.GAME_PAUSE);
        return true;
    }

    public void Resume()
    {
        if (_flowState != GameFlowState.GAME_PAUSE) return;

        ChangeFlowState(GameFlowState.PLAYING);
    }
    
    private void ChangeFlowState(GameFlowState flowState)
    {
        if (_flowState == flowState) return;
        _flowState = flowState;
        
        ChangedState?.Invoke(_flowState);
    }
}
