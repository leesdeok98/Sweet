using System.Collections;
using UnityEngine;

// ★ Spine 네임스페이스 추가
using Spine;
using Spine.Unity;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("References")]
    public Player player; // Inspector에서 Player 연결 (비어있으면 런타임에서 자동 탐색)

    // ─────────────────────────────────────────────────────────────
    // 기존 Sprite 이펙트 프리팹 (더 이상 사용하지 않지만 남겨둠 - 필요시 끄거나 삭제)
    [Header("Legacy (Optional)")]
    public GameObject darkChipEffectPrefab; // 예전 스프라이트 이펙트 (미사용 권장)
    private GameObject darkChipEffectInstance;
    // ─────────────────────────────────────────────────────────────

    private bool darkChipApplied = false; // 다크칩 중복 적용 방지

    [Header("Spine (DarkChip)")]
    [SerializeField] private GameObject darkChipSpinePrefab;
    // ☝️ SkeletonAnimation 컴포넌트가 붙어 있고, SkeletonData/Atlas/PNG 연결된 프리팹
    [SerializeField] private string darkChipAnimName = "activate"; // 재생할 Spine 애니 이름
    [SerializeField] private bool darkChipAnimLoop = false;        // 루프 여부
    [SerializeField] private Vector3 darkChipLocalOffset = Vector3.zero; // 플레이어 기준 위치 오프셋
    private SkeletonAnimation darkChipSpineInstance; // 인스턴스 참조

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (player == null)
            player = FindObjectOfType<Player>();
    }

    // 아이템 적용 진입점
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

            case ItemData.ItemType.DarkChip:
                ApplyDarkChip(0.3f); // +30%
                break;

            case ItemData.ItemType.RollingChocolateBar:
                ApplyRollingChocolateBar();
                break;
        }
    }

    void ApplyIcedJelly(float damage, float duration)
    {
        Debug.Log("아이스젤리 획득!");
        if (player != null)
            player.hasIcedJellySkill = true;
        // 필요 시 지속/틱 데미지 로직은 별도 스킬 스크립트로
    }

    void ApplySugarShield(float value, float duration)
    {
        Debug.Log("슈가쉴드 획득!");
        if (player != null)
            player.hasSugarShield = true;
        // 쉴드 생성/파괴는 별도 프리팹/컴포넌트로 관리 권장
    }

    void ApplyRollingChocolateBar()
    {
        Debug.Log("롤링 초코바 획득!");
        if (player != null)
            player.hasRollingChocolateBar = true;
    }

    // ─────────────────────────────────────────────────────────────
    // ★ 핵심: 다크칩 적용 + Spine 이펙트 재생
    // ─────────────────────────────────────────────────────────────
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

        // (구버전) 스프라이트 이펙트는 더 이상 사용 안 함
        // if (darkChipEffectPrefab != null && player != null) { ... }

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
}
