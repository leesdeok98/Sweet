using UnityEngine;
using Spine.Unity;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int baseDamage = 8;
    public static float damageMultiplier = 1f;

    [Header("Iced Jelly (Spine) - 선택")]
    public GameObject icedJellySpinePrefab;
    [Range(0f, 1f)] public float icedJellyChance = 0.8f;

    // ★ 눈꽃사탕 오라 내부용 참조
    private SpriteRenderer auraSR;


    // ─────────────────────────────────────────────────────────────
    // ★ 비터멜트 카오스 세트 효과용 설정값 (인스펙터에서 조절) ★
    [Header("Bittermelt Chaos Set Settings")]
    [Tooltip("세트 효과 지속 피해 (초당)")]
    public float bittermeltChaosDps = 2f;           // 5초간 초당 2의 지속 피해
    [Tooltip("세트 효과 지속 시간 (초)")]
    public float bittermeltChaosDuration = 5f;      // 기본 5초
    [Tooltip("플레이어 HP가 50% 이상일 때 적용될 공격력 배율 (1.3 = +30%)")]
    public float bittermeltChaosHpBuffMultiplier = 1.3f;
    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // 눈꽃사탕 보유 + 오라 스프라이트가 세팅된 경우, 총알 밑 레이어에 오라 생성
        var sm = SkillManager.Instance;
        if (sm != null && sm.player != null && sm.player.hasSnowflakeCandy && sm.snowflakeAuraSprite != null)
        {
            AttachSnowflakeAura(sm);
        }
    }

    void AttachSnowflakeAura(SkillManager sm)
    {
        // 총알 SR 기준으로 정렬/레이어 맞춤
        var mySR = GetComponent<SpriteRenderer>();
        var auraGO = new GameObject("SnowflakeAura");
        auraGO.transform.SetParent(transform);
        auraGO.transform.localPosition = Vector3.zero;
        auraGO.transform.localScale = Vector3.one * Mathf.Max(0.01f, sm.snowflakeAuraScale);

        auraSR = auraGO.AddComponent<SpriteRenderer>();
        auraSR.sprite = sm.snowflakeAuraSprite;
        auraSR.color = new Color(1f, 1f, 1f, Mathf.Clamp01(sm.snowflakeAuraAlpha));

        if (mySR != null)
        {
            auraSR.sortingLayerID = mySR.sortingLayerID;
            auraSR.sortingOrder = mySR.sortingOrder + sm.snowflakeAuraSortingOrderOffset; // 보통 -1
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy/Boss 외 무시
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            // 1) 기본 데미지
            float finalDamageFloat = baseDamage * damageMultiplier;

            // ─────────────────────────────────────────────────────
            // 비터멜트 카오스 세트 효과: HP 50% 이상이면 공격력 +30%
            //  (DarkChip + CocoaPowder + RollingChocolateBar 세트가 활성화된 상태에서만 동작)
            // ─────────────────────────────────────────────────────
            var smSet = SkillManager.Instance;
            if (smSet != null && smSet.IsBittermeltChaosActive)
            {
                var player = smSet.player;
                if (player != null && player.maxHealth > 0f)
                {
                    float hpRatio = player.health / player.maxHealth;
                    if (hpRatio >= 0.5f)
                    {
                        // HP가 50% 이상일 때만 추가 배율 적용
                        finalDamageFloat *= bittermeltChaosHpBuffMultiplier;
                    }
                }

                // ─────────────────────────────────────────────
                // 비터멜트 카오스 세트 효과: 5초간 초당 2의 지속 피해
                //  Enemy 오브젝트에 BittermeltChaosDot 컴포넌트를 붙여서 Tick
                //  이미 걸려있으면 남은 시간을 5초로 갱신
                // ─────────────────────────────────────────────
                if (bittermeltChaosDps > 0f && bittermeltChaosDuration > 0f)
                {
                    var dot = hitEnemy.GetComponent<BittermeltChaosDot>();
                    if (dot == null)
                    {
                        dot = hitEnemy.gameObject.AddComponent<BittermeltChaosDot>();
                        dot.Initialize(hitEnemy, bittermeltChaosDuration, bittermeltChaosDps);
                    }
                    else
                    {
                        dot.Refresh(bittermeltChaosDuration, bittermeltChaosDps);
                    }
                }
            }

            // ─────────────────────────────────────────────
            // ★ 아이스브레이커 세트 효과:
            //   SnowflakeCandy + IcedJelly + PoppingCandy 세트가 활성 상태라면
            //   얼음 계열 공격에 고정 데미지 +5 추가 (finalDamageFloat에 더함)
            // ─────────────────────────────────────────────
            var smIce = SkillManager.Instance;
            if (smIce != null && smIce.IsIcebreakerActive)
            {
                finalDamageFloat += smIce.IcebreakerBonusDamage;
            }

            int finalDamage = Mathf.RoundToInt(finalDamageFloat);
            hitEnemy.TakeDamage(finalDamage);

            // (옵션) 2) 아이스젤리 스파인 확률 발동
            if (SkillManager.Instance != null && SkillManager.Instance.player != null)
            {
                if (SkillManager.Instance.player.hasIcedJellySkill && icedJellySpinePrefab != null)
                {
                    if (Random.value <= icedJellyChance)
                    {
                        Instantiate(icedJellySpinePrefab, hitEnemy.transform.position, Quaternion.identity);
                    }
                }
            }

            // ★ 3) 눈꽃사탕: 15% 확률로 빙결
            var sm = SkillManager.Instance;
            if (sm != null && sm.player != null && sm.player.hasSnowflakeCandy)
            {
                if (Random.value <= Mathf.Clamp01(sm.snowflakeFreezeChance))
                {
                    hitEnemy.ApplyFreeze(sm.snowflakeFreezeDuration);
                }
            }

            // ★ 4) 팝핑캔디: 적 맞은 지점에서 8방향 버스트
            if (sm != null && sm.player != null && sm.player.hasPoppingCandy)
            {
                // 발동 확률 체크 (확률 쓰기 싫으면 이 if는 지워도 됨)
                if (Random.value <= (sm.poppingChance <= 0f ? 1f : sm.poppingChance))
                {
                    // 충돌 지점 (already have collision)
                    Vector2 hitPoint = collision.ClosestPoint(transform.position);

                    // 샤드 데미지 = 최종 데미지 * 배율
                    int burstDamage = Mathf.RoundToInt(finalDamageFloat * sm.poppingDamageFactor);

                    // 버스트 컨테이너 생성
                    GameObject burst = new GameObject("PoppingCandyBurst");
                    burst.transform.position = hitPoint;

                    var comp = burst.AddComponent<PoppingCandyBurst>();
                    comp.Initialize(
                        colliderRadius: sm.poppingColliderRadius,
                        range: sm.poppingRange,
                        speed: sm.poppingShardSpeed,
                        damage: burstDamage,
                        defaultShardPrefab: sm.poppingDefaultShardPrefab,
                        shardPrefabs: sm.poppingShardPrefabs
                    );
                }
            }
        }

        // 총알 소멸
        Destroy(gameObject);
    }
}
