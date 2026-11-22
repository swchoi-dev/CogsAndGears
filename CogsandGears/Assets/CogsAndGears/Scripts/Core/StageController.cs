using System;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField] private List<MonsterSpawnData> _monsters;
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private BoxCollider2D _cameraBounds;
    
    private float _elapsedTime;
    private int _spawnedMonsterCount;
    private int _aliveMonsterCount;
    private bool _isRunning;
    private bool _isCleared;
    private readonly List<MonsterController> _aliveMonsters = new List<MonsterController>();

    public event Action Cleared;
    // 남은 몬스터 수(RemainingMonsterCount)가 바뀌면 호출된다. HUD 표시용.
    public event Action RemainingMonsterCountChanged;
    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    public BoxCollider2D CameraBounds => _cameraBounds;
    // 스폰되어 아직 살아 있는 몬스터. HUD 위치 표시용 읽기 전용 목록.
    public IReadOnlyList<MonsterController> AliveMonsters => _aliveMonsters;
    // 살아 있는 몬스터 + 아직 스폰되지 않은 몬스터 = 전체 - 처치 수
    public int RemainingMonsterCount => _monsters.Count - (_spawnedMonsterCount - _aliveMonsterCount);
    // 이 Stage에서 처치한 몬스터 수(스폰 수 - 생존 수)
    public int KilledMonsterCount => _spawnedMonsterCount - _aliveMonsterCount;

    private void Update()
    {
        if (!_isRunning) return;
        
        _elapsedTime += Time.deltaTime;
        
        TryMonsterSpawn();
    }

    private void OnMonsterDead(MonsterController monster)
    {
        _aliveMonsters.Remove(monster);
        _aliveMonsterCount--;
        RemainingMonsterCountChanged?.Invoke();
        
        CheckStageClear();
    }

    public void StartStage()
    {
        // 같은 Stage를 다시 시작해도 처음부터 스폰되도록, 남아 있던 몬스터를 정리하고 스폰 상태를 초기화한다.
        // (Destroy는 사망이 아니므로 처치 집계·Cleared에 영향을 주지 않는다)
        foreach (var alive in _aliveMonsters)
        {
            if (alive != null)
                Destroy(alive.gameObject);
        }

        foreach (var monster in _monsters)
        {
            monster.IsSpawned = false;
        }

        _elapsedTime = 0f;
        _spawnedMonsterCount = 0;
        _aliveMonsterCount = 0;
        _aliveMonsters.Clear();
        
        _isRunning = true;
        _isCleared = false;

        RemainingMonsterCountChanged?.Invoke();
    }

    private void TryMonsterSpawn()
    {
        foreach (var monster in _monsters)
        {
            if (monster.IsSpawned || _elapsedTime < monster.SpawnTime)
            {
                continue;
            }
            
            SpawnMonster(monster);
        }
    }

    private void SpawnMonster(MonsterSpawnData monster)
    {
        var instance = Instantiate(
            monster.Prefab,
            monster.SpawnPoint.position,
            monster.SpawnPoint.rotation,
            transform
        );
        
        var controller = instance.GetComponent<MonsterController>();
        // Health.OnDeath는 몬스터당 한 번만 호출되므로 중복 집계되지 않는다
        instance.GetComponent<Health>().OnDeath += () => OnMonsterDead(controller);
        controller.Initialize(_playerTarget);
        _aliveMonsters.Add(controller);
        
        monster.IsSpawned = true;
        _aliveMonsterCount++;
        _spawnedMonsterCount++;
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
