using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// GameClear / GameOver 결과 화면. RunResult 스냅샷만 읽어 결과·도달 Stage·처치 수·최종 가방(읽기 전용)을 그린다.
// 가방 아이콘은 인벤토리와 같은 InvenItemUI 프리팹을 드래그 설정 없이 재사용한다(배치·회전·버리기 불가).
public class RunResultUI : MonoBehaviour
{
    [Header("Background")]
    [Tooltip("전체 화면 배경. AspectRatioFitter(Envelope Parent)로 비율을 유지한 채 화면을 덮는다")]
    [SerializeField] private Image _background;
    [SerializeField] private Sprite _clearBackground;
    [SerializeField] private Sprite _overBackground;

    [Header("Texts")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _killText;

    [Header("Final Inventory")]
    [SerializeField] private GridLayoutGroup _grid;
    [SerializeField] private RectTransform _itemLayer;
    [SerializeField] private InvenItemUI _itemPrefab;
    [SerializeField] private Vector2 _cellSize = new Vector2(56f, 56f);
    [SerializeField] private Vector2 _spacing = new Vector2(6f, 6f);
    [SerializeField] private Color _unlockedColor = Color.white;
    [SerializeField] private Color _lockedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Scenes")]
    [SerializeField] private SceneReference _retryScene;
    [SerializeField] private SceneReference _titleScene;

    private readonly List<RectTransform> _cells = new();
    private bool _isLeaving;

    private void Start()
    {
        Time.timeScale = 1f;
        Build(RunResult.Last);
    }

    private void Build(RunResult result)
    {
        if (result == null)
        {
            _titleText.text = "NO RESULT";
            _stageText.text = string.Empty;
            _killText.text = string.Empty;
            _grid.gameObject.SetActive(false);
            return;
        }

        _titleText.text = result.IsClear ? "GAME CLEAR" : "GAME OVER";
        SetBackground(result.IsClear ? _clearBackground : _overBackground);
        _stageText.text = $"Reached Stage {result.ReachedStage}";
        _killText.text = $"Monsters Killed: {result.KilledMonsters}";

        BuildInventory(result.Inventory);
    }

    // 결과 상태에 맞는 배경으로 교체하고 원본 비율을 AspectRatioFitter에 반영한다(늘어남 방지)
    private void SetBackground(Sprite sprite)
    {
        if (_background == null || sprite == null) return;

        _background.sprite = sprite;
        _background.color = Color.white;

        if (_background.TryGetComponent(out AspectRatioFitter fitter) && sprite.rect.height > 0f)
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    private void BuildInventory(InventoryLayout layout)
    {
        if (layout == null)
        {
            _grid.gameObject.SetActive(false);
            return;
        }

        _grid.cellSize = _cellSize;
        _grid.spacing = _spacing;
        _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _grid.constraintCount = layout.GridSize.x;

        for (int y = 0; y < layout.GridSize.y; y++)
        {
            for (int x = 0; x < layout.GridSize.x; x++)
            {
                var cell = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(_grid.transform, false);

                Image image = cell.GetComponent<Image>();
                image.color = layout.IsUnlocked(x, y) ? _unlockedColor : _lockedColor;
                image.raycastTarget = false;

                _cells.Add((RectTransform)cell.transform);
            }
        }

        RectTransform gridRect = (RectTransform)_grid.transform;
        gridRect.sizeDelta = new Vector2(
            layout.GridSize.x * _cellSize.x + (layout.GridSize.x - 1) * _spacing.x,
            layout.GridSize.y * _cellSize.y + (layout.GridSize.y - 1) * _spacing.y);
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

        foreach (PlacedItemRecord record in layout.Items)
            CreateItem(record, gridRect);
    }

    // InvenGrid.GetPlacementWorldPosition과 같은 기준(첫 셀 중심 + 회전 후 크기의 중앙)으로 배치한다
    private void CreateItem(PlacedItemRecord record, RectTransform gridRect)
    {
        var item = new InvenItem(record.Data);
        InvenItemUI itemUI = Instantiate(_itemPrefab, _itemLayer);

        // 회전 전 크기로 RectTransform 크기를 정한 뒤 회전을 적용한다(인벤토리 드래그 회전과 같은 방식)
        itemUI.Initialize(item, _cellSize, _spacing);

        RectTransform rect = (RectTransform)itemUI.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        for (int i = 0; i < (int)record.Rotation; i++)
        {
            item.Rotate();
            rect.Rotate(0f, 0f, -90f, Space.Self);
        }

        Vector2 first = gridRect.InverseTransformPoint(_cells[0].TransformPoint(_cells[0].rect.center));
        Vector2Int size = item.RotatedSize();
        float stepX = _cellSize.x + _spacing.x;
        float stepY = _cellSize.y + _spacing.y;

        var local = new Vector3(
            first.x + (record.Position.x + (size.x - 1) * 0.5f) * stepX,
            first.y - (record.Position.y + (size.y - 1) * 0.5f) * stepY,
            0f);

        rect.position = gridRect.TransformPoint(local);
    }

    public void Retry() => LoadScene(_retryScene);

    public void GoToTitle() => LoadScene(_titleScene);

    // 연타해도 한 번만 이동한다
    private void LoadScene(SceneReference scene)
    {
        if (_isLeaving || scene == null || !scene.IsValid) return;
        _isLeaving = true;

        Time.timeScale = 1f;
        RunResult.Clear();
        SceneManager.LoadScene(scene.SceneName);
    }
}
