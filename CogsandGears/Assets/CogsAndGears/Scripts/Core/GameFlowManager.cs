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
    
    public GameFlowState FlowState => _flowState;
    public StageController CurrentStage => _currentStage;
    
    public event Action<GameFlowState> ChangedState;
    // 새 Stage가 시작되면 호출된다. HUD가 대상 Stage를 교체하는 데 사용한다.
    public event Action<StageController> StageStarted;

    private void Awake()
    {
        GameManager.Instance.RegisterFlowManager(this);
    }
    
    
    private void OnEnable()
    {
        _inventory.Confirmed += OnInvenConfirm;
        _inventory.Opened += OnInvenOpen;
        _inventory.Closed += OnInvenClose;
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
    }

    private void OnDestroy()
    {
        GameManager.Instance.UnRegisterFlowManager(this);
    }
    
    
    private void InitGameRun()
    {
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
        ChangeFlowState(GameFlowState.GAME_CLEAR);
    }

    private void GameOver()
    {
        ChangeFlowState(GameFlowState.GAME_OVER);
    }
    
    private void ChangeFlowState(GameFlowState flowState)
    {
        if (_flowState == flowState) return;
        _flowState = flowState;
        
        ChangedState?.Invoke(_flowState);
    }
}
