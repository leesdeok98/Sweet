// SyrupTornadoSkill.cs
using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SkeletonAnimation))]
public class SyrupTornadoSkill : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("초당 가하는 피해량")]
    public float damagePerSecond = 2f;          // ← 요구 3

    [Header("Area")]
    [Tooltip("원형 트리거 반경(미터)")]
    public float radius = 1.5f;                 // ← 요구 2
    [Tooltip("Trigger 안의 대상 레이어")]
    public LayerMask enemyMask;

    [Header("Spine")]
    [Tooltip("루프시킬 Spine 애니메이션 이름")]
    public string loopAnimation = "loop";
    [Tooltip("스파인 재생 배속")]
    public float timeScale = 1f;

    [Header("Optional")]
    [Tooltip("고정 틱으로 가할지 (false면 매 프레임 deltaTime 누적)")]
    public bool useFixedTick = false;
    [Tooltip("고정 틱 간격(초)")]
    public float tickInterval = 0.2f;

    CircleCollider2D circle;
    Rigidbody2D rb2d;
    SkeletonAnimation skeleton;

    float tickTimer;

    private bool twistBuffApplied = false; // 트오트 세트효과 적용 확인

    void Awake()
    {
        circle = GetComponent<CircleCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        skeleton = GetComponent<SkeletonAnimation>();

        // 콜라이더/리짓바디 기본값 세팅
        circle.isTrigger = true;
        circle.radius = radius;

        rb2d.isKinematic = true;   // 트리거 이벤트용
        rb2d.gravityScale = 0f;

        // Spine 루프 재생
        skeleton.Initialize(true);
        skeleton.timeScale = timeScale;
        if (!string.IsNullOrEmpty(loopAnimation))
            skeleton.AnimationState.SetAnimation(0, loopAnimation, true);
    }

    void OnValidate()
    {
        // 에디터에서 값 바뀌면 콜라이더 즉시 반영
        if (circle == null) circle = GetComponent<CircleCollider2D>();
        if (circle != null) circle.radius = Mathf.Max(0.01f, radius);
    }

    void Update()
    {
        if (useFixedTick)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                DealDamageTick(tickTimer);
                tickTimer = 0f;
            }
        }
        else
        {
            // 매 프레임 deltaTime 만큼 DPS 누적
            DealDamageTick(Time.deltaTime);
        }
    }

    void DealDamageTick(float dt)
    {
        if (dt <= 0f || damagePerSecond <= 0f) return;

        // 원 안의 적 검색 (원형 트리거 반경과 동일하게 사용)
        var hits = Physics2D.OverlapCircleAll(transform.position, circle != null ? circle.radius : radius, enemyMask);
        if (hits == null || hits.Length == 0) return;

        float damage = damagePerSecond * dt;

        for (int i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    /// <summary>
    /// SkillManager에서 '트위스트 오어 트릿' 세트 효과가 발동될 때
    /// 한 번만 호출해 주면 되는 버프 함수입니다.
    /// rangeMultiplier : 범위 배율 (예: 1.25f = 25% 증가)
    /// damageMultiplier: 데미지 배율 (예: 2f    = 2배 데미지)
    /// </summary>
    public void ApplyTwistOrTreatBuff(float rangeMultiplier, float damageMultiplier)
    {
        // 이미 버프가 적용되었다면 다시 적용하지 않음
        if (twistBuffApplied) return;
        twistBuffApplied = true;

        // 1) 데미지 배율 적용 (시럽 토네이도 DPS 2배 등)
        damagePerSecond *= damageMultiplier;

        // 2) 범위 배율 적용 (논리값 + 실제 콜라이더 둘 다)
        radius *= rangeMultiplier;

        if (circle == null)
            circle = GetComponent<CircleCollider2D>();

        if (circle != null)
        {
            // 실제 충돌 범위도 함께 키워줌
            circle.radius = radius;
        }

        Debug.Log("[SyrupTornadoSkill] Twist or Treat 버프 적용 완료");
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
