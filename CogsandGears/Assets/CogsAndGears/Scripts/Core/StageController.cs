using System;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField] private List<MonsterSpawnData> _monsters;
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private BoxCollider2D _cameraBounds;

    private float _elapsedTime;
    private int _spawnedMonsterCount;
    private int _aliveMonsterCount;
    private bool _isRunning;
    private bool _isCleared;

    public event Action Cleared;
    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    public BoxCollider2D CameraBounds => _cameraBounds;

    private void Update()
    {
        if (!_isRunning) return;
        
        _elapsedTime += Time.deltaTime;
        
        TryMonsterSpawn();
    }

    private void OnMonsterDead()
    {
        _aliveMonsterCount--;
        
        CheckStageClear();
    }

    public void StartStage()
    {
        _elapsedTime = 0f;
        _spawnedMonsterCount = 0;
        _aliveMonsterCount = 0;
        
        _isRunning = true;
        _isCleared = false;
    }

    private void TryMonsterSpawn()
    {
        foreach (var monster in _monsters)
        {
            if (!monster.IsSpawned && _elapsedTime >= monster.SpawnTime)
            {
                monster.IsSpawned = true;
                _aliveMonsterCount++;
                _spawnedMonsterCount++;
                
                Instantiate(
                    monster.Prefab,
                    monster.SpawnPoint.position,
                    monster.SpawnPoint.rotation,
                    transform
                    );
            }
        }
    }
    
    private void CheckStageClear()
    {
        if (!_isRunning || _isCleared ) return;
        if (!IsStageCleared()) return;
        
        _isRunning = false;
        _isCleared = true;
        
        Cleared?.Invoke();
    }

    private bool IsStageCleared()
    {
        return (_spawnedMonsterCount >= _monsters.Count && _aliveMonsterCount == 0);
    }
}
