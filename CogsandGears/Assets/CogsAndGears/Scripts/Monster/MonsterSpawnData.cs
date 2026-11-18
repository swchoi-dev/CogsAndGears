using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MonsterSpawnData
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _spawnTime;
    
    public GameObject Prefab => _prefab;
    public Transform SpawnPoint => _spawnPoint;
    public float SpawnTime => _spawnTime;
    public bool IsSpawned { get; set; }
}