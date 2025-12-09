using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine;
using UnityEngine;
using UnityEngine.Rendering;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("플레이어 참조 (비워두면 런타임 자동 탐색)")]
    public Player player;


    //시럽 토네이도 설정
    [Header("Syrup Tornado Settings")]
    [SerializeField] private GameObject syrupTornadoPrefab;
    private SyrupTornadoSkill syrupTornadoInstance;

    //팝핑 캔디 설정
    [Header("Popping Candy Settings")]
    public float poppingColliderRadius = 0.6f;
    public float poppingRange = 6f;
    public float poppingShardSpeed = 11f;
    [Range(0f, 3f)] public float poppingDamageFactor = 0.8f;
    public GameObject poppingDefaultShardPrefab;
    public GameObject[] poppingShardPrefabs = new GameObject[8];
    [Range(0f, 1f)] public float poppingChance = 1f;   // 1 = 100% 발동

    //다크칩 설정
    [Header("Dark Chip Settings")]
    [Range(0f, 1f)] public float darkChipPercent = 0.3f; // +30%
    [SerializeField] private GameObject darkChipSpinePrefab;
    [SerializeField] private string darkChipAnimName = "activate";
    [SerializeField] private bool darkChipAnimLoop = false;
    [SerializeField] private Vector3 darkChipLocalOffset = Vector3.zero;
    private SkeletonAnimation darkChipSpineInstance;
    private int darkChipLevel = 0;

    // 허니스핀 설정
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


    // 눈꽃사탕 설정
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



    //  비터멜트 카오스 세트 효과 설정 (DarkChip + CocoaPowder + RollingChocolateBar)
    [Header("Bittermelt Chaos Set Settings")]
    [Tooltip("세트 효과 발동 시 재생할 Spine 프리팹 (1회 재생 후 파괴)")]
    [SerializeField] private GameObject bittermeltChaosSpinePrefab;
    [Tooltip("Spine 애니메이션 이름")]
    [SerializeField] private string bittermeltChaosAnimName = "activate";
    [Tooltip("세트 FX는 1번만 재생하고 끝나야 하므로 기본값은 false 입니다.")]
    [SerializeField] private bool bittermeltChaosAnimLoop = false; // 요구사항: loop = false
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
    [SerializeField] private bool icebreakerAnimLoop = false;
    [Tooltip("플레이어 기준 위치 오프셋")]
    [SerializeField] private Vector3 icebreakerLocalOffset = Vector3.zero;

    //  트위스트 오어 트릿 세트 효과 설정 (SyrupTornado + HoneySpin + RollingChocolateBar)
    [Header("Twist Or Treat Set Settings")]
    [Tooltip("세트 발동 시 3개 스킬 범위 배율 (1.25 = 25% 증가)")]
    [Range(1f, 3f)]
    public float twistRangeMultiplier = 1.25f;

    [Tooltip("세트 발동 시 시럽 토네이도 데미지 배율 (2 = 2배)")]
    public float twistSyrupDamageMultiplier = 2f;

    [Tooltip("세트 발동 시 허니스핀/롤링초코바 생성 개수 배율 (2 = 2배)")]
    public int twistCountMultiplier = 2;

    //  트위스트 오어 트릿 세트 발동 여부
    private bool twistOrTreatActive = false;

    //트오트 FX 설정
    [Header("Twist Or Treat Set FX")]
    [Tooltip("트위스트 오어 트릿 발동 시 재생할 Spine 프리팹")]
    [SerializeField] private GameObject twistOrTreatSpinePrefab;

    [Tooltip("Spine 애니메이션 이름 (예: activate)")]
    [SerializeField] private string twistOrTreatAnimName = "animation";

    [Tooltip("세트 FX는 1번만 재생하고 끝나야 하므로 false로 두는걸 권장")]
    [SerializeField] private bool twistOrTreatAnimLoop = true;

    [Tooltip("플레이어 기준 위치 오프셋")]
    [SerializeField] private Vector3 twistOrTreatLocalOffset = Vector3.zero;

    // 하이퍼캔디 러쉬 세트 효과
    [Header("하이퍼캔디 러쉬 설정")]
    [SerializeField] private GameObject hyperCandyRushSpinePrefab;
    [SerializeField] private string hyperCandyRushAnimName = "animation";
    [SerializeField] private bool hyperCandyRushAnimLoop = false;
    [Tooltip("플레이어 기준 위치 오프셋")]
    [SerializeField] private Vector3 hyperCandyRushLocalOffset = Vector3.zero;
    private HyperCandyRush hyperCandyRushComponent;

    [Header("스위트아머 콤보 설정")]
    // 스위트아머 콤보 프리팹 
    public GameObject sweetarmorComboVisualPrefab;
    private SweetarmorCombo sweetarmorComboComponent;

    [Header("슈가 밤 파티 설정")]
    public GameObject sugarBombPartySpinePrefab;
    public Vector3 sugarBombPartyLocalOffset = Vector3.zero;
    private SugarBombParty sugarBombPartyComponent;


    // 다른 스크립트에서 세트 발동 여부 확인용 (읽기 전용)
    public bool IsTwistOrTreatActive => twistOrTreatActive;

    // 슈가실드
    private SugarShieldSkill sugarShieldSkill;


    //아이스 브레이커 세트 효과
    //눈꽃사탕+아이스 젤리+팝핑 캔디
    //모든 적 이동속도 10%감소, 눈꽃사탕,아이스 젤리 데미지 5 증가
    [Header("Icebreaker Set Settings")]
    [Tooltip("세트 효과 발동 시, 모든 적 이동속도 감소 비율 (0.1 = 10% 느려짐)")]
    [Range(0f, 1f)]
    public float icebreakerSlowPercent = 0.1f;

    [Tooltip("세트 효과 발동 시, 눈꽃사탕/아이스젤리 관련 공격에 추가되는 고정 데미지")]
    public float icebreakerBonusDamage = 5f;

    private bool bittermeltChaosActive = false;
    private bool icebreakerActive = false;
    private bool hasHyperCandyRushActive = false;
    private bool hasSweetarmorComboActive = false;
    private bool isSugarBombPartyActive = false;

    //세트효과 애니메이션 중복 방지용
    private bool icebreakerFxPlayed = false;
    private bool bittermeltChaosFxPlayed = false;
    private bool twistOrTreatFxPlayed = false;

    public bool IsBittermeltChaosActive => bittermeltChaosActive;
    public bool IsIcebreakerActive => icebreakerActive;
    public float IcebreakerBonusDamage => icebreakerBonusDamage;

    private readonly Dictionary<Enemy, float> icebreakerOriginalSpeed = new Dictionary<Enemy, float>();
    private Coroutine icebreakerSlowRoutine;

    // ─────────────────────────────────────────────
    // 세트 조건 변화 감지용 (이전 상태 저장)
    // ─────────────────────────────────────────────
    private bool prevHasBittermeltSet = false;
    private bool prevHasIcebreakerSet = false;
    private bool prevHasTwistOrTreatSet = false;
    private bool prevHasHyperCandyRushSet = false;
    private bool prevHasSweetarmorComboSet = false;
    private bool prevHasSugarBombPartySet = false;

    // ResetAllSkills 후 한 번만 세트 버프 재구성할지 여부 (트위스트 오어 트릿용)
    private bool needRebuildSetEffects = false;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (player == null)
            player = FindObjectOfType<Player>();

        // 실제 슈가쉴드 컴포넌트는 플레이어에서 가져오도록 수정
        if (player != null)
        {
            sugarShieldSkill = player.GetComponent<SugarShieldSkill>();
        }
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

        // 인벤토리 재계산 후 세트 버프(특히 트위스트 오어 트릿)를 다시 깔 수 있게 플래그
        needRebuildSetEffects = true;

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
        player.hasSugarPorridge = false;

        var shieldManager = player.GetComponent<SugarShieldSkill>();
        if (shieldManager != null)
            shieldManager.StopSugarShieldGeneration(true);

        darkChipLevel = 0;
        Bullet.damageMultiplier = 1f;

        // 개별 세트/콤보 컴포넌트는 "세트가 실제로 깨질 때" DeactivateXXXSet 에서 정리
        // 여기선 순수 스킬 오브젝트만 정리

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
            case ItemData.ItemType.RollingChocolateBar:
                player.hasRollingChocolateBar = true;
                break;
            case ItemData.ItemType.PoppingCandy:
                player.hasPoppingCandy = true;
                break;
            case ItemData.ItemType.CaramelCube:
                player.hasCaramelCube = true;
                break;
            case ItemData.ItemType.SugarPorridge:
                player.hasSugarPorridge = true;
                break;

            // --- 코코아/딸기/허니스핀/슈가실드 ---
            case ItemData.ItemType.CocoaPowder:
                ApplyCocoaPowder();
                break;
            case ItemData.ItemType.StrawberryPopCore:
                ApplyStrawberryPopCore();
                break;
            case ItemData.ItemType.HoneySpin:
                ApplyHoneySpin();
                break;
            case ItemData.ItemType.SugarShield:
                ApplySugarShield();
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
    }

    // ─────────────────────────────────────────────────────────────
    // ▼▼▼ 개별 스킬 함수들 ▼▼▼
    // ─────────────────────────────────────────────────────────────

    void ApplySugarShield()
    {
        Debug.Log("슈가쉴드 획득!");
        if (player != null)
        {
            player.hasSugarShield = true;

            SugarShieldSkill shieldManager = player.GetComponent<SugarShieldSkill>();

            if (shieldManager == null)
            {
                shieldManager = player.gameObject.AddComponent<SugarShieldSkill>();
            }

            // 실드 생성 + 주기 관리 시작
            shieldManager.StartSugarShieldGeneration();
        }
    }

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

        AudioManager.instance.PlaySfx(AudioManager.Sfx.DarkChip_SFX);

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

    // ─────────────────────────────────────────────────────────────
    // 스위트아머 콤보
    // ─────────────────────────────────────────────────────────────

    private void CheckSweetarmorCombo()
    {
        if (player == null) return;

        bool hasSet = player.hasSugarShield && player.hasCaramelCube && player.hasCocoaPowder;

        if (hasSet && !prevHasSweetarmorComboSet)
        {
            ApplySweetarmorCombo();
        }
        else if (!hasSet && prevHasSweetarmorComboSet)
        {
            DeactivateSweetarmorCombo();
        }

        prevHasSweetarmorComboSet = hasSet;
    }

    private void ApplySweetarmorCombo()
    {
        hasSweetarmorComboActive = true;
        Debug.Log("SweetarmorCombo 발동!");

        sweetarmorComboComponent = player.gameObject.GetComponent<SweetarmorCombo>();
        if (sweetarmorComboComponent == null)
        {
            sweetarmorComboComponent = player.gameObject.AddComponent<SweetarmorCombo>();
        }

        // 비주얼 프리팹 전달
        if (sweetarmorComboVisualPrefab != null)
        {
            sweetarmorComboComponent.comboVisualPrefab = sweetarmorComboVisualPrefab;
        }
        else
        {
            Debug.LogError("SweetarmorCombo Visual Prefab이 SkillManager에 할당되지 않았습니다!");
        }

        // 컴포넌트의 활성화 함수를 명시적으로 호출하여 로직 시작
        sweetarmorComboComponent.ActivateComboEffect();
    }

    void DeactivateSweetarmorCombo()
    {
        if (!hasSweetarmorComboActive) return;

        Debug.Log("SweetarmorCombo 해제!");
        hasSweetarmorComboActive = false;

        if (sweetarmorComboComponent != null)
        {
            Destroy(sweetarmorComboComponent);
            sweetarmorComboComponent = null;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 슈가 밤 파티 세트
    // ─────────────────────────────────────────────────────────────

    public void CheckSugarBombParty()
    {
        if (player == null) return;

        bool hasSet =
            player.hasPoppingCandy &&
            player.hasStrawberryPopCore &&
            player.hasSugarPorridge;

        if (hasSet && !prevHasSugarBombPartySet)
        {
            ApplySugarBombPartySet();
        }
        else if (!hasSet && prevHasSugarBombPartySet)
        {
            DeactivateSugarBombPartySet();
        }

        prevHasSugarBombPartySet = hasSet;
    }

    /// <summary>
    /// 슈가밤 파티 세트 효과를 플레이어에게 적용하고 코루틴을 시작합니다.
    /// </summary>
    private void ApplySugarBombPartySet()
    {
        if (player.hasSugarBombParty) return;

        isSugarBombPartyActive = true;
        player.hasSugarBombParty = true;

        // 플레이어 오브젝트에 SugarBombParty 스크립트 추가
        sugarBombPartyComponent = player.gameObject.AddComponent<SugarBombParty>();

        if (sugarBombPartySpinePrefab != null)
        {
            sugarBombPartyComponent.comboVisualPrefab = sugarBombPartySpinePrefab;
            // 필요하다면 오프셋도 전달
            // sugarBombPartyComponent.visualLocalOffset = sugarBombPartyLocalOffset;
        }
        else
        {
            // 프리팹이 할당되지 않은 경우 경고
            Debug.LogWarning("[SkillManager] sugarBombPartySpinePrefab이 SkillManager에 할당되지 않았습니다. 비주얼이 나오지 않습니다.");
        }

        if (player != null)
        {
            player.StartSugarBombPartyActivation(sugarBombPartyComponent);
        }
        else
        {
            Debug.LogError("Player 참조가 null입니다. SugarBombParty 코루틴을 시작할 수 없습니다.");
        }

        Debug.Log("[SugarBombParty] 컴포넌트 추가 및 활성화 예약됨.");
    }

    void DeactivateSugarBombPartySet()
    {
        if (!isSugarBombPartyActive) return;

        Debug.Log("[SkillManager] 슈가밤 파티 세트 효과 해제!");
        isSugarBombPartyActive = false;

        if (player != null)
        {
            player.hasSugarBombParty = false;
        }

        if (sugarBombPartyComponent != null)
        {
            Destroy(sugarBombPartyComponent);
            sugarBombPartyComponent = null;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 하이퍼 캔디 러쉬 세트
    // ─────────────────────────────────────────────────────────────

    void CheckHyperCandyRushSet()
    {
        if (player == null) return;

        bool hasSet =
            player.hasHoneySpin &&
            player.hasStrawberryPopCore &&
            player.hasSugarPorridge;

        if (hasSet && !prevHasHyperCandyRushSet)
        {
            ApplyHyperCandyRushSet();
        }
        else if (!hasSet && prevHasHyperCandyRushSet)
        {
            DeactivateHyperCandyRushSet();
        }

        prevHasHyperCandyRushSet = hasSet;
    }

    void ApplyHyperCandyRushSet()
    {
        // 이미 활성화된 상태라면 중복 실행 방지
        if (hasHyperCandyRushActive) return;

        hasHyperCandyRushActive = true;
        Debug.Log("[SkillManager] 하이퍼 캔디 러쉬 세트 효과 발동!");

        // Player에게 효과 활성화 요청
        player?.ActivateHyperCandyRush(true);

        // HyperCandyRush 컴포넌트 추가 및 비주얼 / 기능 활성화 요청
        if (hyperCandyRushComponent == null)
        {
            hyperCandyRushComponent = player.gameObject.AddComponent<HyperCandyRush>();

            // SkillManager의 프리팹 정보를 HyperCandyRush 컴포넌트에 전달
            if (hyperCandyRushSpinePrefab != null)
            {
                hyperCandyRushComponent.comboVisualPrefab = hyperCandyRushSpinePrefab;
            }
            else
            {
                Debug.LogWarning("[SkillManager] hyperCandyRushSpinePrefab 비어 있음. 비주얼이 나오지 않습니다.");
            }
            // 컴포넌트의 비주얼 생성 및 핵심 로직
            hyperCandyRushComponent.ActivateSetEffect();
        }

        // Spine FX 1회 재생
        if (player != null && hyperCandyRushSpinePrefab != null)
        {
            GameObject fx = Instantiate(hyperCandyRushSpinePrefab, player.transform);
            fx.name = "HyperCandyRush_SetFX_OneShot";
            fx.transform.localPosition = hyperCandyRushLocalOffset;

            SkeletonAnimation sa = fx.GetComponent<SkeletonAnimation>();
            if (sa != null)
            {
                var state = sa.AnimationState;
                TrackEntry entry = state.SetAnimation(0, hyperCandyRushAnimName, hyperCandyRushAnimLoop);
                if (!hyperCandyRushAnimLoop)
                {
                    StartCoroutine(DestroySpineEffectAfter(sa, entry));
                }
            }
        }
    }

    void DeactivateHyperCandyRushSet()
    {
        if (!hasHyperCandyRushActive) return;

        Debug.Log("[SkillManager] 하이퍼 캔디 러쉬 세트 효과 해제!");
        hasHyperCandyRushActive = false;

        if (player != null)
        {
            player.ActivateHyperCandyRush(false);
        }

        if (hyperCandyRushComponent != null)
        {
            Destroy(hyperCandyRushComponent);
            hyperCandyRushComponent = null;
        }
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

    // ─────────────────────────────────────────────────────────────
    // 비터멜트 카오스 세트
    // ─────────────────────────────────────────────────────────────

    void CheckBittermeltChaosSet()
    {
        if (player == null) return;

        bool hasSet =
            player.hasDarkChip &&
            player.hasCocoaPowder &&
            player.hasRollingChocolateBar;

        // 세트가 막 완성된 순간
        if (hasSet && !prevHasBittermeltSet)
        {
            ApplyBittermeltChaosSet();
        }
        // 세트가 깨진 순간
        else if (!hasSet && prevHasBittermeltSet)
        {
            DeactivateBittermeltChaosSet();
        }

        prevHasBittermeltSet = hasSet;
    }

    void ApplyBittermeltChaosSet() //비터멜트 세트효과 로직
    {
        bittermeltChaosActive = true;
        Debug.Log("[SkillManager] 비터멜트 카오스 세트 효과 발동!");

        // ★ FX는 처음 발동할 때만 재생 (이미 재생했다면 다시 생성하지 않음)
        if (bittermeltChaosFxPlayed)
        {
            return;
        }
        bittermeltChaosFxPlayed = true;

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
            AudioManager.instance.PlaySfx(AudioManager.Sfx.BitterMeltChaos_SFX);

            // 세트 FX는 1번만 재생하고 끝나야 하므로 loop = false 기준
            if (!bittermeltChaosAnimLoop)
            {
                StartCoroutine(DestroySpineEffectAfter(sa, entry));
            }
        }
    }

    void DeactivateBittermeltChaosSet()
    {
        if (!bittermeltChaosActive) return;

        Debug.Log("[SkillManager] 비터멜트 카오스 세트 효과 해제!");
        bittermeltChaosActive = false;
        bittermeltChaosFxPlayed = false;   // 다음에 다시 세트 맞추면 FX 재생
        // 필요 시 비터멜트 관련 추가 버프 롤백 로직을 여기서 처리
    }

    // ─────────────────────────────────────────────────────────────
    // 아이스브레이커 세트
    // ─────────────────────────────────────────────────────────────

    // 아이스브레이커 세트 효과 체크 (SnowflakeCandy + IcedJelly + PoppingCandy)
    void CheckIcebreakerSet()
    {
        if (player == null) return;

        bool hasSet =
            player.hasSnowflakeCandy &&
            player.hasIcedJellySkill &&
            player.hasPoppingCandy;

        // 세트가 막 완성된 순간
        if (hasSet && !prevHasIcebreakerSet)
        {
            ApplyIcebreakerSet();
        }
        // 세트가 깨진 순간
        else if (!hasSet && prevHasIcebreakerSet)
        {
            DeactivateIcebreakerSet();
        }

        prevHasIcebreakerSet = hasSet;
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
        //  (이미 한 번 재생된 상태라면 FX는 다시 나오지 않음)
        // ─────────────────────────────────────────────
        if (icebreakerFxPlayed) return;  // ★ FX는 한 번만
        icebreakerFxPlayed = true;

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

            if (!icebreakerAnimLoop)
            {
                StartCoroutine(DestroySpineEffectAfter(sa, entry));
            }
        }
    }

    // 아이스브레이커 세트 효과 해제: 적 속도 원복 + 코루틴 정리
    void DeactivateIcebreakerSet()
    {
        if (!icebreakerActive) return;

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

        icebreakerFxPlayed = false;   // 세트 다시 맞추면 FX 재생 가능
    }

    // ─────────────────────────────────────────────────────────────
    // 트위스트 오어 트릿 세트
    // ─────────────────────────────────────────────────────────────

    void CheckTwistOrTreatSet()
    {
        if (player == null) return; // 플레이어가 null이면 리턴

        bool hasSet =
            player.hasSyrupTornado &&
            player.hasHoneySpin &&
            player.hasRollingChocolateBar;

        // 1) 세트가 "막 완성"된 순간 → 버프 + FX
        if (hasSet && !prevHasTwistOrTreatSet)
        {
            ApplyTwistOrTreatSet();
        }
        // 2) 세트는 유지 중인데 ResetAllSkills 덕분에 스킬 오브젝트만 날아간 경우
        else if (hasSet && prevHasTwistOrTreatSet && needRebuildSetEffects)
        {
            // 버프만 다시 깔고, FX는 재생하지 않음
            ApplyTwistOrTreatBuffOnly();
        }
        // 3) 세트가 깨진 순간 → 해제
        else if (!hasSet && prevHasTwistOrTreatSet)
        {
            DeactivateTwistOrTreatSet();
        }

        prevHasTwistOrTreatSet = hasSet;
    }

    // 트위스트 오어 트릿 버프만 적용 (FX 재생 X)
    void ApplyTwistOrTreatBuffOnly()
    {
        UpgradeHoneySpinForTwistOrTreat();
        UpgradeSyrupTornadoForTwistOrTreat();
        UpgradeRollingChocolateBarForTwistOrTreat();
    }

    //트위스트 오어 트릿 세트 효과 적용
    //세트 플래그를 키면 각 3개의 스킬 효과 버프 적용
    void ApplyTwistOrTreatSet() // 트위스트 오어 트릿 세트 효과 로직
    {
        // 세트 발동 플래그 ON
        twistOrTreatActive = true;
        Debug.Log("[SkillManager] 트위스트 오어 트릿 세트 효과 발동!");

        // 버프 적용
        ApplyTwistOrTreatBuffOnly();

        //  Spine 세트 이펙트 한 번만 재생 후 Destroy
        // ★ FX는 처음 발동할 때만 재생 (이미 재생했다면 아래 FX 로직 스킵)
        if (twistOrTreatFxPlayed)
        {
            return;
        }
        twistOrTreatFxPlayed = true;

        if (player == null) return;

        if (twistOrTreatSpinePrefab == null)
        {
            // FX 프리팹이 비어 있어도 게임이 터지진 않게 경고만 출력
            Debug.LogWarning("[SkillManager] twistOrTreatSpinePrefab 비어 있음 (세트 FX는 나중에 연결 가능)");
            return;
        }

        // 플레이어를 부모로 해서 FX 생성 (플레이어를 따라다니게)
        GameObject fx = Instantiate(twistOrTreatSpinePrefab, player.transform);
        fx.name = "TwistOrTreat_SetFX";
        fx.transform.localPosition = twistOrTreatLocalOffset; // 인스펙터에서 준 오프셋만큼 위치 조정

        // SkeletonAnimation 가져와서 애니메이션 1회 재생
        SkeletonAnimation sa = fx.GetComponent<SkeletonAnimation>();
        if (sa != null)
        {
            var state = sa.AnimationState;

            const string animName = "animation"; // Spine 안에 있는 애니메이션 이름
            const bool loop = false;             // 세트 FX는 항상 한 번만 재생

            TrackEntry entry = state.SetAnimation(0, animName, loop);
            Invoke("TwistorTreatSound", 1f); // 인보크 = 매서드 지연 시키는 함수 (매서드이름,지연시킬시간) 

            if (!loop)
            {
                StartCoroutine(DestroySpineEffectAfter(sa, entry));
            }
        }
    }

    void DeactivateTwistOrTreatSet()
    {
        if (!twistOrTreatActive && !prevHasTwistOrTreatSet) return;

        Debug.Log("[SkillManager] 트위스트 오어 트릿 세트 효과 해제!");
        twistOrTreatActive = false;
        twistOrTreatFxPlayed = false;   // 세트 다시 맞추면 FX 재생될 수 있도록

        // 허니스핀 세트 버프 해제: 세트용 허니스핀 제거 후 기본 허니스핀 다시 생성
        ClearHoneySpinInstances();
        if (player != null && player.hasHoneySpin)
        {
            ApplyHoneySpin();
        }

        // SyrupTornado / RollingChocolateBar 는 IsTwistOrTreatActive 플래그를 보고
        // 새로 생성되는 것부터 일반 모드로 동작하도록 설계되어 있다면
        // 여기서는 플래그만 내려줘도 충분함.
    }

    void UpgradeHoneySpinForTwistOrTreat()
    {
        if (player == null) return;
        if (!player.hasHoneySpin) return;
        if (honeySpinOrbPrefab == null) return;

        // 기존 스킬 제거
        ClearHoneySpinInstances();

        int targetCount = Mathf.Max(1, honeySpinOrbCount * twistCountMultiplier);
        float radius = honeySpinRadius * twistRangeMultiplier;

        honeySpinInstances = new List<HoneySpin>();

        for (int i = 0; i < targetCount; i++)
        {
            GameObject orb = Instantiate(honeySpinOrbPrefab, player.transform);
            orb.name = $"HoneySpinOrb_Twist_{i + 1}";

            HoneySpin spin = orb.GetComponent<HoneySpin>();
            if (spin == null)
                spin = orb.AddComponent<HoneySpin>();

            float startAngle;

            //   동(0) / 북(90) / 서(180) / 남(270)
            if (targetCount == 4)
            {
                startAngle = i * 90f;
            }
            else
            {
                float angleStep = 360f / targetCount;
                startAngle = i * angleStep;
            }

            spin.Initialize(
                player.transform,
                radius,
                honeySpinRotateSpeed,
                startAngle,
                honeySpinDamage,
                honeySpinSlowPercent,
                honeySpinSlowDuration
            );

            honeySpinInstances.Add(spin);
        }

        Debug.Log("[SkillManager] 트위스트 오어 트릿: 허니스핀 오브젝트 재구성 완료");
    }

    //롤링 초코바 강화
    //플래그만 키고 롤링초코바 스크립트에서 실행
    void UpgradeRollingChocolateBarForTwistOrTreat()
    {
        if (player == null) return;
        if (!player.hasRollingChocolateBar) return;

        Debug.Log("[SkillManager] 트위스트 오어 트릿: 롤링초코바 세트 버프 활성화 (발사 로직은 RollingChocolateBar 스크립트에서 처리)");
    }

    void UpgradeSyrupTornadoForTwistOrTreat()
    {
        if (syrupTornadoInstance == null) return;

        // SyrupTornadoSkill 스크립트에 이 메서드를 추가해서 사용합니다.
        syrupTornadoInstance.ApplyTwistOrTreatBuff(
            twistRangeMultiplier,
            twistSyrupDamageMultiplier
        );

        Debug.Log("[SkillManager] 트위스트 오어 트릿: 시럽 토네이도 강화 적용");
    }

    // ─────────────────────────────────────────────────────────────
    // 아이스브레이커 슬로우 루프
    // ─────────────────────────────────────────────────────────────

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

    // Spine 트랙 종료 후 이펙트 제거 (다크칩용)
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

    /// 세트 FX 제거용 코루틴
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

    void TwistorTreatSound()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.TwistOatNut_SFX);
    }

    // ─────────────────────────────────────────────────────────────
    //  현재 플레이어 스킬 상태를 기준으로 세트 효과를 한 번에 검사하는 함수
    //  (InventoryManager.UpdateActiveSkills / Player.TryApplyStartingSkills 에서 호출)
    // ─────────────────────────────────────────────────────────────
    public void CheckAllSetEffects()
    {
        CheckBittermeltChaosSet();   // DarkChip + CocoaPowder + RollingChocolateBar
        CheckIcebreakerSet();        // SnowflakeCandy + IcedJelly + PoppingCandy
        CheckTwistOrTreatSet();      // SyrupTornado + HoneySpin + RollingChocolateBar
        CheckHyperCandyRushSet();    // HoneySpin + StrawberryPopCore + SugarPorridge
        CheckSweetarmorCombo();      // SugarShield + CaramelCube + CocoaPowder
        CheckSugarBombParty();       // PoppingCandy + StrawberryPopCore + SugarPorridge

        // ResetAllSkills 이후 한 번만 재빌드하게 플래그 끔
        needRebuildSetEffects = false;
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
