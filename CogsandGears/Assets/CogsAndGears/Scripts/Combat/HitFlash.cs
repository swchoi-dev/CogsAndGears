using System.Collections.Generic;
using UnityEngine;

// 실제 피해(Health.OnDamaged)를 받으면 모델 Renderer 전체를 잠깐 붉게 칠한다.
// MaterialPropertyBlock으로 색상만 덮어써 원본 Material·텍스처는 건드리지 않고, 끝나면 원래 PropertyBlock으로 되돌린다.
// Hit Stop·Slow 중에도 같은 길이로 보이도록 실제 시간(Unscaled) 기준으로 끝낸다.
public class HitFlash : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [Tooltip("비워두면 부모의 Health를 사용한다")]
    [SerializeField] private Health _health;
    [Tooltip("비워두면 이 오브젝트 아래 모델 Renderer(Mesh/SkinnedMesh)를 자동으로 찾는다")]
    [SerializeField] private Renderer[] _renderers;
    [Tooltip("_BaseColor(Albedo)에 곱해지는 색. 1보다 크면 더 밝게 보인다")]
    [ColorUsage(false, true)]
    [SerializeField] private Color _flashColor = new Color(1.6f, 0.15f, 0.15f, 1f);
    [Tooltip("실제 시간(초)")]
    [SerializeField] private float _flashDuration = 0.12f;

    private readonly List<MaterialPropertyBlock> _originalBlocks = new List<MaterialPropertyBlock>();
    private MaterialPropertyBlock _flashBlock;
    private float _flashEndRealtime = -1f;
    private bool _isFlashing;

    private void Awake()
    {
        if (_health == null)
            _health = GetComponentInParent<Health>();

        if (_renderers == null || _renderers.Length == 0)
            _renderers = FindModelRenderers();

        _flashBlock = new MaterialPropertyBlock();

        // 기존 PropertyBlock을 보관했다가 그대로 복원한다
        foreach (Renderer target in _renderers)
        {
            var block = new MaterialPropertyBlock();
            if (target != null)
                target.GetPropertyBlock(block);

            _originalBlocks.Add(block);
        }
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDamaged -= OnDamaged;

        Restore();
    }

    private void Update()
    {
        if (_isFlashing && Time.unscaledTime >= _flashEndRealtime)
            Restore();
    }

    private void OnDamaged(float appliedDamage)
    {
        // 연속 피격은 종료 시각만 갱신한다
        _flashEndRealtime = Time.unscaledTime + _flashDuration;

        if (_isFlashing) return;

        _isFlashing = true;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer target = _renderers[i];
            if (target == null) continue;

            _flashBlock.Clear();
            target.GetPropertyBlock(_flashBlock);
            _flashBlock.SetColor(BaseColorId, _flashColor);
            _flashBlock.SetColor(ColorId, _flashColor);
            target.SetPropertyBlock(_flashBlock);
        }
    }

    private void Restore()
    {
        if (!_isFlashing) return;

        _isFlashing = false;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer target = _renderers[i];
            if (target == null) continue;

            // 원래 비어 있었다면 null로 되돌려 SRP Batcher 호환 상태를 유지한다
            target.SetPropertyBlock(_originalBlocks[i].isEmpty ? null : _originalBlocks[i]);
        }
    }

    private Renderer[] FindModelRenderers()
    {
        var found = new List<Renderer>();

        foreach (Renderer candidate in GetComponentsInChildren<Renderer>(true))
        {
            bool isModel = candidate is SkinnedMeshRenderer || candidate is MeshRenderer;
            if (isModel && candidate.sharedMaterial != null)
                found.Add(candidate);
        }

        return found.ToArray();
    }
}
