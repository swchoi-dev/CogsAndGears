using System.Collections.Generic;
using UnityEngine;

// 인벤토리에 확정 배치된 무기 1개(결과 표시용 복사본)
public readonly struct PlacedItemRecord
{
    public readonly ItemDataSO Data;
    public readonly Vector2Int Position;
    public readonly ItemRotation Rotation;

    public PlacedItemRecord(ItemDataSO data, Vector2Int position, ItemRotation rotation)
    {
        Data = data;
        Position = position;
        Rotation = rotation;
    }
}

// 종료 시점의 인벤토리 형태(격자 활성 상태·배치 무기). 원본 배열을 복사해 이후 변경의 영향을 받지 않는다.
public class InventoryLayout
{
    public Vector2Int GridSize { get; }
    public IReadOnlyList<PlacedItemRecord> Items { get; }

    private readonly bool[,] _unlocked;

    public InventoryLayout(Vector2Int gridSize, bool[,] unlocked, List<PlacedItemRecord> items)
    {
        GridSize = gridSize;
        _unlocked = (bool[,])unlocked.Clone();
        Items = new List<PlacedItemRecord>(items);
    }

    public bool IsUnlocked(int x, int y) => _unlocked[x, y];
}

// Run 종료 결과 스냅샷. GameFlowManager가 종료 순간에 만들고, 결과 화면은 파괴된 전투 오브젝트 대신 이 값만 읽는다.
// Scene 전환 후에도 남도록 정적 보관하며, 새 Run 시작·다시 하기·타이틀 이동 시 비운다.
public class RunResult
{
    public static RunResult Last { get; private set; }

    public bool IsClear { get; }
    public int ReachedStage { get; }
    public int KilledMonsters { get; }
    public InventoryLayout Inventory { get; }

    public RunResult(bool isClear, int reachedStage, int killedMonsters, InventoryLayout inventory)
    {
        IsClear = isClear;
        ReachedStage = reachedStage;
        KilledMonsters = killedMonsters;
        Inventory = inventory;
    }

    public static void Save(RunResult result) => Last = result;

    public static void Clear() => Last = null;
}
