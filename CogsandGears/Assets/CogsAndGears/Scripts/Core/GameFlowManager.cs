using System;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Serialization;

public class GameFlowManager : MonoBehaviour
{
    private const int FIRST_STAGE = 0;
    [SerializeField] private List<StageController> _stages;
    [SerializeField] private InvenController _inventory;
    [SerializeField] private Transform _cogs;
    
    private GameFlowState _flowState;
    private StageController _currentStage;
    private StageController _prevStage;
    private int _currentStageIndex;

    public event Action<GameFlowState> ChangedFlowState;
    public GameFlowState FlowState => _flowState;
    

    private void OnEnable()
    {
        _inventory.Confirmed += OnInvenConfirm;
    }
    
    private void Start()
    {
        InitGameRun();
        StartStage();
    }

    private void OnDisable()
    {
        _inventory.Confirmed -= OnInvenConfirm;
    }
    
    private void InitGameRun()
    {
        _currentStageIndex = FIRST_STAGE;
        _currentStage = _stages[_currentStageIndex];
    }

    private void StartStage()
    {
        // TO-DO _currentStage.PlayStartAnim();
        
        _currentStage.gameObject.SetActive(true);
        _cogs.position = _currentStage.PlayerSpawnPoint.position;

        if (_prevStage != null)
        {
            _prevStage.gameObject.SetActive(false);
        }
        
        _currentStage.Cleared += StageClear;
        _currentStage.StartStage();
        
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
    
    private void StageClear()
    {
        _currentStage.Cleared -= StageClear;
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
        
        ChangedFlowState?.Invoke(_flowState);
    }
}
