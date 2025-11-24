using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("플레이어 참조 (비워두면 런타임 자동 탐색)")]
    public Player player;

    // ─────────────────────────────────────────────────────────────
    // ▼▼▼ 기존 스킬 설정값 ▼▼▼
    // ─────────────────────────────────────────────────────────────

    [Header("Syrup Tornado Settings")]
    [SerializeField] private GameObject syrupTornadoPrefab;
    private SyrupTornadoSkill syrupTornadoInstance;

    [Header("Popping Candy Settings")]
    public float poppingColliderRadius = 0.6f;
    public float poppingRange = 6f;
    public float poppingShardSpeed = 11f;
    [Range(0f, 3f)] public float poppingDamageFactor = 0.8f;
    public GameObject poppingDefaultShardPrefab;
    public GameObject[] poppingShardPrefabs = new GameObject[8];
    [Range(0f, 1f)] public float poppingChance = 1f;   // 1 = 100% 발동

    [Header("Dark Chip Settings")]
    [Range(0f, 1f)] public float darkChipPercent = 0.3f; // +30%
    [SerializeField] private GameObject darkChipSpinePrefab;
    [SerializeField] private string darkChipAnimName = "activate";
    [SerializeField] private bool darkChipAnimLoop = false;
    [SerializeField] private Vector3 darkChipLocalOffset = Vector3.zero;
    private SkeletonAnimation darkChipSpineInstance;
    private int darkChipLevel = 0;

    // 허니스핀 설정값 
    [Header("Honey Spin Settings")]
    [Tooltip("허니스핀 ‘구체(오브젝트)’ 프리팹 (SkeletonAnimation + HoneySpin.cs 포함)")]
    public GameObject honeySpinOrbPrefab;

    [Tooltip("플레이어를 기준으로 도는 반지름")]
    public float honeySpinRadius = 1.5f;

    [Tooltip("초당 회전 각도 (deg/sec)")]
    public float honeySpinRotateSpeed = 180f;

    [Tooltip("허니스핀 1회 충돌 시 데미지")]
    public float honeySpinDamage = 5f;

    [Range(0f, 1f), Tooltip("이동속도 감소 퍼센트 (0.3 = -30%)")]
    public float honeySpinSlowPercent = 0.3f;

    [Tooltip("디버프 지속 시간 (초)")]
    public float honeySpinSlowDuration = 1f;

    [Tooltip("허니스핀 구체 개수 (기본 2개)")]
    public int honeySpinOrbCount = 2;

    // 현재 씬에서 살아있는 허니스핀 오브젝트들
    private List<HoneySpin> honeySpinInstances = new List<HoneySpin>();


    // ★ 눈꽃사탕(SnowflakeCandy) 설정
    [Header("Snowflake Candy Settings")]
    [Range(0f, 1f)] public float snowflakeFreezeChance = 0.15f; // 15%
    [Min(0f)] public float snowflakeFreezeDuration = 1f;         // 인스펙터에서 조절
    [Tooltip("총알 아래쪽 레이어에 표시할 하늘색 오라 이미지 (PNG Sprite)")]
    public Sprite snowflakeAuraSprite;
    [Tooltip("오라 크기 배율")]
    public float snowflakeAuraScale = 1.2f;
    [Range(0f, 1f)] public float snowflakeAuraAlpha = 0.7f;
    [Tooltip("오라 정렬순서: 총알보다 아래로 내려가게 음수 권장(-1)")]
    public int snowflakeAuraSortingOrderOffset = -1;

    // ★ 비터멜트 카오스 세트 효과 설정 (DarkChip + CocoaPowder + RollingChocolateBar)
    [Header("Bittermelt Chaos Set Settings")]
    [Tooltip("세트 효과 발동 시 재생할 Spine 프리팹 (1회 재생 후 파괴)")]
    [SerializeField] private GameObject bittermeltChaosSpinePrefab;
    [Tooltip("Spine 애니메이션 이름")]
    [SerializeField] private string bittermeltChaosAnimName = "activate";
    [Tooltip("세트 FX는 1번만 재생하고 끝나야 하므로 기본값은 false 입니다.")]
    [SerializeField] private bool bittermeltChaosAnimLoop = false; // ★ 요구사항: loop = false
    [Tooltip("플레이어 기준 위치 오프셋")]
    [SerializeField]
    private Vector3 bittermeltChaosLocalOffset = Vector3.zero;

    // Icebreaker 세트 발동 시 보여줄 Spine FX 설정
    [Header("Icebreaker Set FX (Spine)")]
    [Tooltip("아이스브레이커 세트 발동 시 1회 재생할 Spine 프리팹")]
    [SerializeField] private GameObject icebreakerSpinePrefab;
    [Tooltip("Spine 애니메이션 이름")]
    [SerializeField] private string icebreakerAnimName = "activate";
    [Tooltip("세트 FX는 1번만 재생하고 끝나야 하므로 기본값은 false 입니다.")]
    [SerializeField] private bool icebreakerAnimLoop = false;   // 요구사항: loop = false
    [Tooltip("플레이어 기준 위치 오프셋")]
    [SerializeField] private Vector3 icebreakerLocalOffset = Vector3.zero;


    //아이스 브레이커 세트 효과
    //눈꽃사탕+아이스 젤리+팝핑 캔디
    //모든 적 이동속도 10%감소, 눈꽃사탕,아이스 젤리 데미지 5 증가
    [Header("Icebreaker Set Settings")]
    [Tooltip("세트 효과 발동 시, 모든 적 이동속도 감소 비율 (0.1 = 10% 느려짐)")]
    [Range(0f, 1f)]
    public float icebreakerSlowPercent = 0.1f;

    [Tooltip("세트 효과 발동 시, 눈꽃사탕/아이스젤리 관련 공격에 추가되는 고정 데미지")]
    public float icebreakerBonusDamage = 5f;

    //세트효과 발동 여부
    private bool bittermeltChaosActive = false; //비터멜트 카오스
    private bool icebreakerActive = false;     //아이스 브레이커



    public bool IsBittermeltChaosActive => bittermeltChaosActive;
    public bool IsIcebreakerActive => icebreakerActive;

    public float IcebreakerBonusDamage => icebreakerBonusDamage;

    //아이스브레이커로 느려지기 전 에너미 속도 저장
    private readonly Dictionary<Enemy, float> icebreakerOriginalSpeed = new Dictionary<Enemy, float>();

    private Coroutine icebreakerSlowRoutine; //아이스 브레이커 지속 슬로우 적용 코루틴 핸들
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (player == null)
            player = FindObjectOfType<Player>();
    }

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        // ▶ 시작할 때 이미 hasHoneySpin이 true면 자동으로 적용
        if (player != null && player.hasHoneySpin)
        {
            ApplyHoneySpin();
        }
    }

    void Update()
    {
        // ▶ 런타임 중에 hasHoneySpin true로 바뀐 경우 자동 생성
        if (player != null && player.hasHoneySpin)
        {
            if (honeySpinInstances == null || honeySpinInstances.Count == 0)
            {
                ApplyHoneySpin();
            }
        }
    }

    /// <summary>
    /// 모든 스킬 플래그와 중첩 스택을 0으로 초기화합니다.
    /// </summary>
    public void ResetAllSkills()
    {
        if (player == null) return;

        // 플래그 리셋
        player.hasIcedJellySkill = false;
        player.hasSugarShield = false;
        player.hasDarkChip = false;
        player.hasRollingChocolateBar = false;
        player.hasPoppingCandy = false;
        player.hasSyrupTornado = false;
        player.hasCocoaPowder = false;
        player.hasStrawberryPopCore = false;
        player.hasHoneySpin = false;
        player.hasSnowflakeCandy = false;
        player.hasCaramelCube = false;

        // 스탯 및 중첩 횟수 초기화
        darkChipLevel = 0;
        Bullet.damageMultiplier = 1f;

        // 세트 효과 리셋
        bittermeltChaosActive = false;
        DeactivateIcebreakerSet(); // 아이스브레이커 세트 효과도 해제

        // 지속형 오브젝트 파괴
        if (syrupTornadoInstance != null)
        {
            Destroy(syrupTornadoInstance.gameObject);
            syrupTornadoInstance = null;
        }

        if (darkChipSpineInstance != null)
        {
            Destroy(darkChipSpineInstance.gameObject);
            darkChipSpineInstance = null;
        }

        ClearHoneySpinInstances();
    }

    /// <summary>
    /// 특정 스킬 '하나'를 활성화합니다. (InventoryManager가 호출)
    /// </summary>
    public void ActivateSkill(ItemData.ItemType type)
    {
        if (player == null) return;

        switch (type)
        {
            // --- 단순 플래그만 켜는 스킬들 ---
            case ItemData.ItemType.IcedJelly:
                player.hasIcedJellySkill = true;
                break;
            case ItemData.ItemType.SugarShield:
                player.hasSugarShield = true;
                break;
            case ItemData.ItemType.RollingChocolateBar:
                player.hasRollingChocolateBar = true;
                break;
            case ItemData.ItemType.PoppingCandy:
                player.hasPoppingCandy = true;
                break;
            case ItemData.ItemType.CaramelCube:
                player.hasCaramelCube = true;
                break;

            // --- 코코아/딸기/허니스핀 ---
            case ItemData.ItemType.CocoaPowder:
                ApplyCocoaPowder();
                break;
            case ItemData.ItemType.StrawberryPopCore:
                ApplyStrawberryPopCore();
                break;
            case ItemData.ItemType.HoneySpin:
                ApplyHoneySpin();
                break;

            // --- 별도 로직이 있는 스킬들 ---
            case ItemData.ItemType.DarkChip:
                ApplyDarkChip(darkChipPercent);
                break;
            case ItemData.ItemType.SyrupTornado:
                ApplySyrupTornado();
                break;

            // ★ 추가: 눈꽃사탕
            case ItemData.ItemType.SnowflakeCandy:
                player.hasSnowflakeCandy = true;
                Debug.Log("[SkillManager] SnowflakeCandy 활성화");
                break;

            case ItemData.ItemType.None:
            default:
                Debug.Log($"[SkillManager] '{type}' 처리 미구현 아이템입니다.");
                break;
        }
        //세트 효과 체크
        //DarkChip + CocoaPowder + RollingChocolateBar
        CheckBittermeltChaosSet();
        CheckIcebreakerSet(); // SnowflakeCandy + IcedJelly + PoppingCandy 세트 체크
    }

    // ─────────────────────────────────────────────────────────────
    // ▼▼▼ 개별 스킬 함수들 ▼▼▼
    // ─────────────────────────────────────────────────────────────

    private void ApplySyrupTornado()
    {
        if (player == null) return;
        player.hasSyrupTornado = true;

        if (syrupTornadoInstance == null)
        {
            if (syrupTornadoPrefab == null)
            {
                Debug.LogError("[SkillManager] syrupTornadoPrefab 비어 있음 (프리팹 연결 필요)");
                return;
            }
            var go = Instantiate(syrupTornadoPrefab, player.transform);
            go.name = "SyrupTornado(AoE)";
            go.transform.localPosition = Vector3.zero;
            syrupTornadoInstance = go.GetComponent<SyrupTornadoSkill>();
        }
    }

    void ApplyDarkChip(float percent)
    {
        darkChipLevel++;

        if (player != null)
            player.hasDarkChip = true;

        Bullet.damageMultiplier = 1f + (darkChipLevel * percent);
        Debug.Log($"다크칩 획득! (Lvl {darkChipLevel}) 총알 공격력 +{percent * 100f}%");

        if (player == null) return;
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
        }

        var state = darkChipSpineInstance.AnimationState;
        TrackEntry entry = state.SetAnimation(0, darkChipAnimName, darkChipAnimLoop);

        if (!darkChipAnimLoop)
        {
            state.AddEmptyAnimation(0, 0.15f, 0f);
            StartCoroutine(DestroyAfter(entry));
        }
    }

    void ApplyCocoaPowder()
    {
        if (player == null) return;
        player.hasCocoaPowder = true;
        Debug.Log("[SkillManager] 코코아 파우더 활성화");
    }

    void ApplyStrawberryPopCore()
    {
        if (player == null) return;
        player.hasStrawberryPopCore = true;
        Debug.Log("[SkillManager] 딸기 팝코어 활성화");
    }

    // ★★★ 허니스핀 구체 생성 함수 ★★★
    void ApplyHoneySpin()
    {
        if (player == null)
        {
            Debug.LogError("[SkillManager] ApplyHoneySpin 호출 시 player가 null 입니다.");
            return;
        }

        // 플레이어 플래그 ON
        player.hasHoneySpin = true;

        // 이미 구체가 떠 있으면 중복 생성 방지
        if (honeySpinInstances != null && honeySpinInstances.Count > 0)
        {
            Debug.Log("[SkillManager] 허니스핀 이미 활성화됨");
            return;
        }

        if (honeySpinOrbPrefab == null)
        {
            Debug.LogError("[SkillManager] honeySpinOrbPrefab 비어 있음 (프리팹 연결 필요)");
            return;
        }

        if (honeySpinOrbCount <= 0) honeySpinOrbCount = 2;

        float angleStep = 360f / honeySpinOrbCount;

        honeySpinInstances = new List<HoneySpin>();

        for (int i = 0; i < honeySpinOrbCount; i++)
        {
            GameObject orb = Instantiate(honeySpinOrbPrefab, player.transform);
            orb.name = $"HoneySpinOrb_{i + 1}";

            HoneySpin spin = orb.GetComponent<HoneySpin>();
            if (spin == null)
            {
                spin = orb.AddComponent<HoneySpin>();
            }

            float startAngle = i * angleStep;

            spin.Initialize(
                player.transform,
                honeySpinRadius,
                honeySpinRotateSpeed,
                startAngle,
                honeySpinDamage,
                honeySpinSlowPercent,
                honeySpinSlowDuration
            );

            honeySpinInstances.Add(spin);
        }

        Debug.Log("[SkillManager] 허니스핀 활성화: 구체 " + honeySpinOrbCount + "개 생성");
    }

    // 허니스핀 오브젝트 전부 제거
    void ClearHoneySpinInstances()
    {
        if (honeySpinInstances == null) return;

        for (int i = 0; i < honeySpinInstances.Count; i++)
        {
            if (honeySpinInstances[i] != null)
            {
                Destroy(honeySpinInstances[i].gameObject);
            }
        }
        honeySpinInstances.Clear();
    }



    /// DarkChip + CocoaPowder + RollingChocolateBar 를 모두 보유했는지 검사하고,

    void CheckBittermeltChaosSet()
    {
        if (bittermeltChaosActive) return; // 이미 발동된 상태면 무시
        if (player == null) return;

        if (player.hasDarkChip && player.hasCocoaPowder && player.hasRollingChocolateBar)
        {
            ApplyBittermeltChaosSet();
        }
    }


    void ApplyBittermeltChaosSet() //비터멜트 세트효과 로직
    {
        bittermeltChaosActive = true;
        Debug.Log("[SkillManager] 비터멜트 카오스 세트 효과 발동!");

        if (player == null) return;
        if (bittermeltChaosSpinePrefab == null)
        {
            // Spine FX를 아직 안 넣었어도 게임 진행에는 지장 없도록 로그만 남김
            Debug.LogWarning("[SkillManager] bittermeltChaosSpinePrefab 비어 있음 (세트 FX는 나중에 연결 가능)");
            return;
        }

        GameObject fx = Instantiate(bittermeltChaosSpinePrefab, player.transform);
        fx.name = "BittermeltChaos_SetFX";
        fx.transform.localPosition = bittermeltChaosLocalOffset;

        SkeletonAnimation sa = fx.GetComponent<SkeletonAnimation>();
        if (sa != null)
        {
            var state = sa.AnimationState;
            TrackEntry entry = state.SetAnimation(0, bittermeltChaosAnimName, bittermeltChaosAnimLoop);

            // 세트 FX는 1번만 재생하고 끝나야 하므로 loop = false 기준
            if (!bittermeltChaosAnimLoop)
            {
                StartCoroutine(DestroySpineEffectAfter(sa, entry));
            }
        }
    }

    // 아이스브레이커 세트 효과 체크 (SnowflakeCandy + IcedJelly + PoppingCandy)
    void CheckIcebreakerSet()
    {
        if (icebreakerActive) return;
        if (player == null) return;

        if (player.hasSnowflakeCandy && player.hasIcedJellySkill && player.hasPoppingCandy)
        {
            ApplyIcebreakerSet();
        }
    }

    // 아이스브레이커 세트 효과 실제 적용: 플래그 on + 슬로우 코루틴 시작
    void ApplyIcebreakerSet()
    {
        icebreakerActive = true;
        Debug.Log("[SkillManager] 아이스브레이커 세트 효과 발동!");

        if (icebreakerSlowRoutine != null)
        {
            StopCoroutine(icebreakerSlowRoutine);
        }
        icebreakerSlowRoutine = StartCoroutine(ApplyIcebreakerSlowLoop());

        // ─────────────────────────────────────────────
        // 아이스브레이커 세트 발동 시 Spine FX 1회 재생
        // ─────────────────────────────────────────────
        if (player == null) return;
        if (icebreakerSpinePrefab == null)
        {
            Debug.LogWarning("[SkillManager] icebreakerSpinePrefab 비어 있음 (세트 FX는 나중에 연결 가능)");
            return;
        }

        GameObject fx = Instantiate(icebreakerSpinePrefab, player.transform);
        fx.name = "Icebreaker_SetFX";
        fx.transform.localPosition = icebreakerLocalOffset;

        SkeletonAnimation sa = fx.GetComponent<SkeletonAnimation>();
        if (sa != null)
        {
            var state = sa.AnimationState;
            TrackEntry entry = state.SetAnimation(0, icebreakerAnimName, icebreakerAnimLoop);

            // 세트 FX는 1번만 재생하고 끝나야 하므로 loop = false 기준
            if (!icebreakerAnimLoop)
            {
                StartCoroutine(DestroySpineEffectAfter(sa, entry));
            }
        }
    }


    // 아이스브레이커 세트 효과 해제: 적 속도 원복 + 코루틴 정리
    void DeactivateIcebreakerSet()
    {
        icebreakerActive = false;

        if (icebreakerSlowRoutine != null)
        {
            StopCoroutine(icebreakerSlowRoutine);
            icebreakerSlowRoutine = null;
        }

        if (icebreakerOriginalSpeed.Count > 0)
        {
            foreach (var pair in icebreakerOriginalSpeed)
            {
                if (pair.Key != null)
                {
                    pair.Key.speed = pair.Value;
                }
            }
            icebreakerOriginalSpeed.Clear();
        }
    }

    // 세트가 유지되는 동안 주기적으로 모든 Enemy 이동속도 10% 감소 적용
    IEnumerator ApplyIcebreakerSlowLoop()
    {
        var wait = new WaitForSeconds(0.5f);

        while (icebreakerActive)
        {
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            float multiplier = 1f - Mathf.Clamp01(icebreakerSlowPercent);

            foreach (var e in enemies)
            {
                if (e == null) continue;

                if (!icebreakerOriginalSpeed.ContainsKey(e))
                {
                    icebreakerOriginalSpeed[e] = e.speed;
                }

                float baseSpeed = icebreakerOriginalSpeed[e];
                e.speed = baseSpeed * multiplier;
            }

            yield return wait;
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

    /// 비터멜트 카오스 전용 Spine FX 제거용 코루틴
    IEnumerator DestroySpineEffectAfter(SkeletonAnimation sa, TrackEntry entry)
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

        if (sa != null)
        {
            Destroy(sa.gameObject);
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
