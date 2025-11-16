// SkillManager.cs
using System.Collections;          // ★ DestroyAfter 코루틴에 필요
using Spine.Unity;
using Spine;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("플레이어 참조 (비워두면 런타임 자동 탐색)")]
    public Player player;

    // SkillManager.cs : 필드 영역에 추가
    [Header("Syrup Tornado Settings")]
    [SerializeField] private GameObject syrupTornadoPrefab;   // SkeletonAnimation + CircleCollider2D(isTrigger) 포함 프리팹
    private SyrupTornadoSkill syrupTornadoInstance;           // 중복 생성 방지


    // ─────────────────────────────────────────────────────────────
    // Popping Candy (Prefab-based 8-way burst)
    // ─────────────────────────────────────────────────────────────
    [Header("Popping Candy Settings")]
    [Tooltip("원형(투명) 콜라이더 반지름(미터)")]
    public float poppingColliderRadius = 0.6f;

    [Tooltip("샤드 1개가 이동할 최대 거리(미터)")]
    public float poppingRange = 6f;

    [Tooltip("샤드 이동 속도(미터/초)")]
    public float poppingShardSpeed = 11f;

    [Tooltip("총알 최종 데미지 대비 배율 (예: 0.8 = 80%)")]
    [Range(0f, 3f)] public float poppingDamageFactor = 0.8f;

    [Header("Popping Candy Shard Prefabs")]
    [Tooltip("빈 슬롯 대체용 기본 샤드 프리팹(선택)")]
    public GameObject poppingDefaultShardPrefab;

    [Tooltip("방향별 샤드 프리팹(0~7: 0도부터 시계방향 45도 간격)")]
    public GameObject[] poppingShardPrefabs = new GameObject[8];

    // ─────────────────────────────────────────────────────────────
    // DarkChip (Spine 이펙트 + 데미지 배율)
    // ─────────────────────────────────────────────────────────────
    [Header("Dark Chip Settings")]
    [Tooltip("DarkChip 적용 시 총알 데미지 배율 (1 + percent)")]
    [Range(0f, 1f)] public float darkChipPercent = 0.3f; // +30%

    // ★ 누락되었던 필드들 추가
    [SerializeField] private GameObject darkChipSpinePrefab;   // Spine 프리팹
    [SerializeField] private string darkChipAnimName = "activate";
    [SerializeField] private bool darkChipAnimLoop = false;
    [SerializeField] private Vector3 darkChipLocalOffset = Vector3.zero;
    private SkeletonAnimation darkChipSpineInstance;
    private bool darkChipApplied = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (player == null) player = FindObjectOfType<Player>();
    }

    public void Apply(ItemData item)
    {
        if (item == null) return;

        switch (item.type)
        {
            case ItemData.ItemType.IcedJelly:
                ApplyIcedJelly(item.value, 3f);
                break;

            case ItemData.ItemType.SugarShield:
                ApplySugarShield(item.value, 3f);
                break;

            case ItemData.ItemType.RollingChocolateBar:
                ApplyRollingChocolateBar();
                break;

            case ItemData.ItemType.PoppingCandy:
                ApplyPoppingCandy();
                break;

            case ItemData.ItemType.DarkChip:
                ApplyDarkChip(darkChipPercent);
                break;
            case ItemData.ItemType.SyrupTornado:
                ApplySyrupTornado();
                break;

            default:
                Debug.Log($"[SkillManager] '{item.type}' 처리 미구현 아이템입니다.");
                break;

        }
    }

    void ApplyIcedJelly(float value, float duration)
    {
        if (player == null) { Debug.LogWarning("[SkillManager] Player 없음 (IcedJelly)"); return; }
        player.hasIcedJellySkill = true;
        Debug.Log("[SkillManager] 아이스젤리 활성화");
    }

    void ApplySugarShield(float value, float duration)
    {
        if (player == null) { Debug.LogWarning("[SkillManager] Player 없음 (SugarShield)"); return; }
        player.hasSugarShield = true;
        Debug.Log("[SkillManager] 슈가쉴드 활성화");
    }

    void ApplyRollingChocolateBar()
    {
        if (player == null) { Debug.LogWarning("[SkillManager] Player 없음 (RollingChocolateBar)"); return; }
        player.hasRollingChocolateBar = true;
        Debug.Log("[SkillManager] 롤링 초코바 활성화");
    }

    void ApplyPoppingCandy()
    {
        if (player == null) { Debug.LogWarning("[SkillManager] Player 없음 (PoppingCandy)"); return; }
        player.hasPoppingCandy = true;
        Debug.Log("[SkillManager] 팝핑캔디 활성화 (Prefab 8-way 버스트)");
    }
    // SkillManager.cs : 메서드 추가
    private void ApplySyrupTornado()
    {
        if (player == null)
        {
            Debug.LogWarning("[SkillManager] Player 없음 (SyrupTornado)");
            return;
        }
        player.hasSyrupTornado = true;

        if (syrupTornadoInstance == null)
        {
            if (syrupTornadoPrefab == null)
            {
                Debug.LogError("[SkillManager] syrupTornadoPrefab 비어 있음 (프리팹 연결 필요)");
                return;
            }

            // 플레이어의 발밑에 생성해서 계속 유지
            var go = Instantiate(syrupTornadoPrefab, player.transform);
            go.name = "SyrupTornado(AoE)";
            go.transform.localPosition = Vector3.zero;

            syrupTornadoInstance = go.GetComponent<SyrupTornadoSkill>();
            if (syrupTornadoInstance == null)
            {
                Debug.LogError("[SkillManager] 프리팹에 SyrupTornadoSkill 컴포넌트가 없습니다.");
            }
        }

        Debug.Log("[SkillManager] 시럽 토네이도 활성화");
    }


    void ApplyDarkChip(float percent)
    {
        if (darkChipApplied) return;
        darkChipApplied = true;

        Debug.Log($"다크칩 획득! 총알 공격력 +{percent * 100f}%");

        // 플레이어 상태 플래그
        if (player != null)
            player.hasDarkChip = true;

        // 전역 데미지 배율 (Bullet에 static 멤버 존재해야 함)
        Bullet.damageMultiplier = 1f + percent;

        // Spine 프리팹 인스턴스 생성/재사용
        if (player == null)
        {
            Debug.LogError("[DarkChip] Player 없음");
            return;
        }
        if (darkChipSpinePrefab == null)
        {
            Debug.LogError("[DarkChip] darkChipSpinePrefab 비어 있음 (Spine 프리팹 연결 필요)");
            return;
        }

        if (darkChipSpineInstance == null)
        {
            var go = Instantiate(darkChipSpinePrefab, player.transform);
            go.transform.localPosition = darkChipLocalOffset;

            darkChipSpineInstance = go.GetComponent<SkeletonAnimation>();
            if (darkChipSpineInstance == null)
            {
                Debug.LogError("[DarkChip] 프리팹에 SkeletonAnimation 컴포넌트가 없습니다.");
                return;
            }
        }

        // 애니메이션 재생
        var state = darkChipSpineInstance.AnimationState;
        TrackEntry entry = state.SetAnimation(0, darkChipAnimName, darkChipAnimLoop);

        // 루프가 아니면 자연스럽게 제거
        if (!darkChipAnimLoop)
        {
            // 빈 애니로 짧게 블렌딩해서 꺼짐 연출(선택)
            state.AddEmptyAnimation(0, 0.15f, 0f);

            // 파괴 코루틴 (Time.timeScale=0이라면 UnscaledDeltaTime 사용)
            StartCoroutine(DestroyAfter(entry));
        }
    }

    // Spine 트랙 종료 후 이펙트 제거
    IEnumerator DestroyAfter(TrackEntry entry)
    {
        float duration = (entry != null && entry.Animation != null)
            ? entry.Animation.Duration
            : 0.5f;

        float t = 0f;
        while (t < duration + 0.2f)
        {
            t += Time.unscaledDeltaTime; // 일시정지 중에도 진행
            yield return null;
        }

        if (darkChipSpineInstance != null)
        {
            Destroy(darkChipSpineInstance.gameObject);
            darkChipSpineInstance = null;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        poppingColliderRadius = Mathf.Max(0.01f, poppingColliderRadius);
        poppingRange = Mathf.Max(0.1f, poppingRange);
        poppingShardSpeed = Mathf.Max(0.1f, poppingShardSpeed);

        if (poppingShardPrefabs != null && poppingShardPrefabs.Length != 8)
        {
            var old = poppingShardPrefabs;
            poppingShardPrefabs = new GameObject[8];
            if (old != null)
            {
                for (int i = 0; i < Mathf.Min(8, old.Length); i++)
                    poppingShardPrefabs[i] = old[i];
            }
        }
    }
#endif
}
