// SkillManager.cs (CocoaPowder 및 StrawberryPopCore 기능 추가 완료)
using System.Collections;
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
    // ▼▼▼ 1번(A)의 모든 스킬 '설정값'과 '변수'들 ▼▼▼
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

    [Header("Dark Chip Settings")]
    [Range(0f, 1f)] public float darkChipPercent = 0.3f; // +30%
    [SerializeField] private GameObject darkChipSpinePrefab;
    [SerializeField] private string darkChipAnimName = "activate";
    [SerializeField] private bool darkChipAnimLoop = false;
    [SerializeField] private Vector3 darkChipLocalOffset = Vector3.zero;
    private SkeletonAnimation darkChipSpineInstance;
    private int darkChipLevel = 0; 

    // ─────────────────────────────────────────────────────────────
    // ▲▲▲ 1번(A)의 변수들 끝 ▲▲▲
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (player == null) player = FindObjectOfType<Player>();
    }

    /// <summary>
    /// ★ 2번(B)의 구조 ★
    /// 모든 스킬 플래그와 중첩 스택을 0으로 초기화합니다.
    /// </summary>
    public void ResetAllSkills()
    {
        if (player == null) return;

        // 1번(A)의 모든 스킬 플래그를 false로 리셋
        player.hasIcedJellySkill = false;
        player.hasSugarShield = false;
        player.hasDarkChip = false;
        player.hasRollingChocolateBar = false;
        player.hasPoppingCandy = false;
        player.hasSyrupTornado = false;
        
        // ▼▼▼ [추가됨] 코코아/딸기 플래그 리셋 ▼▼▼
        player.hasCocoaPowder = false;
        player.hasStrawberryPopCore = false;
        // ▲▲▲ 추가 완료 ▲▲▲

        // 스탯 및 중첩 횟수 초기화
        darkChipLevel = 0;
        Bullet.damageMultiplier = 1f; // 총알 데미지 배율 원상복구
        
        // ▼▼▼ [버그 수정] 지속형 애니메이션/오브젝트 파괴 ▼▼▼
        
        // 1. 시럽 토네이도 인스턴스 파괴
        if (syrupTornadoInstance != null)
        {
            Destroy(syrupTornadoInstance.gameObject);
            syrupTornadoInstance = null;
        }

        // 2. 다크 칩 스파인 인스턴스 파괴
        if (darkChipSpineInstance != null)
        {
            Destroy(darkChipSpineInstance.gameObject);
            darkChipSpineInstance = null;
        }
        // ▲▲▲ [수정 완료] ▲▲▲
    }

    /// <summary>
    /// ★ 2번(B)의 구조 ★
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
                
            // ▼▼▼ [추가됨] 코코아/딸기 활성화 로직 ▼▼▼
            case ItemData.ItemType.CocoaPowder:
                ApplyCocoaPowder();
                break;
            case ItemData.ItemType.StrawberryPopCore:
                ApplyStrawberryPopCore();
                break;
            // ▲▲▲ 추가 완료 ▲▲▲


            // --- 1번(A)의 로직이 필요한 스킬들 ---
            case ItemData.ItemType.DarkChip:
                ApplyDarkChip(darkChipPercent);
                break;
            case ItemData.ItemType.SyrupTornado:
                ApplySyrupTornado();
                break;

            case ItemData.ItemType.None:
            default:
                 Debug.Log($"[SkillManager] '{type}' 처리 미구현 아이템입니다."); // None 케이스 처리
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ▼▼▼ [추가됨] 스킬 실행 함수들 ▼▼▼
    // ─────────────────────────────────────────────────────────────

    private void ApplySyrupTornado()
    {
        if (player == null) return;
        player.hasSyrupTornado = true; 

        if (syrupTornadoInstance == null) // 중복 생성 방지
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
        // 2번(B)의 중첩 로직 적용
        darkChipLevel++; 

        if (player != null)
            player.hasDarkChip = true;

        // 전역 데미지 배율 (중첩 적용)
        Bullet.damageMultiplier = 1f + (darkChipLevel * percent);
        Debug.Log($"다크칩 획득! (Lvl {darkChipLevel}) 총알 공격력 +{percent * 100f}%");

        // Spine 프리팹 인스턴스 생성/재사용
        if (player == null) return;
        if (darkChipSpinePrefab == null)
        {
            Debug.LogError("[DarkChip] darkChipSpinePrefab 비어 있음 (Spine 프리팹 연결 필요)");
            return;
        }

        if (darkChipSpineInstance == null) // ★ 중복 생성 방지
        {
            var go = Instantiate(darkChipSpinePrefab, player.transform);
            go.transform.localPosition = darkChipLocalOffset;
            darkChipSpineInstance = go.GetComponent<SkeletonAnimation>();
        }

        // 애니메이션 재생 (이미 켜져 있어도 새로 재생)
        var state = darkChipSpineInstance.AnimationState;
        TrackEntry entry = state.SetAnimation(0, darkChipAnimName, darkChipAnimLoop);

        // 루프가 아니면 자연스럽게 제거
        if (!darkChipAnimLoop)
        {
            state.AddEmptyAnimation(0, 0.15f, 0f);
            StartCoroutine(DestroyAfter(entry)); // 자동 파괴 코루틴
        }
    }
    
    // ▼▼▼ [추가됨] 코코아 파우더 구현 함수 ▼▼▼
    void ApplyCocoaPowder()
    {
        if (player == null) return;
        player.hasCocoaPowder = true;
        Debug.Log("[SkillManager] 코코아 파우더 활성화");
    }

    // ▼▼▼ [추가됨] 딸기 팝코어 구현 함수 ▼▼▼
    void ApplyStrawberryPopCore()
    {
        if (player == null) return;
        player.hasStrawberryPopCore = true;
        Debug.Log("[SkillManager] 딸기 팝코어 활성화");
    }
    // ▲▲▲ 추가 완료 ▲▲▲


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