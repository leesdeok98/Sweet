using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using Spine.Unity;

public class Enemy : MonoBehaviour
{
    [Header("Spine Setting")]
    [Tooltip("true면 이동 Spine / 사망 Spine을 따로 사용합니다.")]
    public bool useSeparateSpine = false;

    [Tooltip("단일 Spine 또는 이동용 Spine")]
    public SkeletonAnimation skeletonAnimation;

    [Tooltip("이동용 Spine (useSeparateSpine이 true일 때 사용, 비우면 위 SkeletonAnimation 사용)")]
    public SkeletonAnimation runSkeleton;

    [Tooltip("사망용 Spine (useSeparateSpine이 true일 때 사용)")]
    public SkeletonAnimation deathSkeleton;

   [SpineAnimation] public string runAnimName = "move";  // 이동만 SpineAnimation 유지
    public string deadAnimName = "dead";


    //  전역 이벤트: 어떤 적이든 죽으면 한 번만 방송
    public static Action OnAnyEnemyDied;

    [Header("Stats")]
    public float speed;           // 이동 속도 (현재 속도)
    public float health;
    public float maxHealth;
    public float dps;             // 초당 피해량 (Player가 읽어 씀)

    [Header("Target")]
    public Rigidbody2D target;    // 추적 대상 (Player)

    protected bool isLive;
    protected Rigidbody2D rb;

    //이성덕이 적음 
    private float freezeRemain = 0f;
    private float savedSpeed = 0f;
    private Color originalColor = Color.white;
    //private float originalAnimSpeed = 1f;

    [HideInInspector] public bool isSlowed = false;
    protected float originalSpeed;      // 기본 속도(슬로우/해제에 필요)

    [Header("넉백, 경직")]
    public float knockbackDuration = 0.1f;
    public bool isKnockback = false;
    public bool isStunned = false;
    public bool isFrozen = false;

    private Coroutine removeSlowRoutine;

    //  처치수 중복 집계 방지용
    private bool hasCountedKill = false;

    public Vector2 vec2;
    private float spineInitialScaleX = 1f;

    private float runInitialScaleX = 1f;
    private float deathInitialScaleX = 1f;

    private Collider2D[] colliders;

    //  공통 이동 스파인 반환용
    private SkeletonAnimation RunSpine
    {
        get
        {
            if (useSeparateSpine)
            {
                // 별도 설정이 없으면 기존 skeletonAnimation을 이동 Spine으로 사용
                return runSkeleton != null ? runSkeleton : skeletonAnimation;
            }
            else
            {
                return skeletonAnimation;
            }
        }
    }

    void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    originalSpeed = speed; // 인스펙터의 초기 speed 저장 

    colliders = GetComponentsInChildren<Collider2D>();

    // 이동 스파인 기본 스케일 저장
    SkeletonAnimation runSa = RunSpine;
    if (runSa != null)
    {
        runInitialScaleX = runSa.transform.localScale.x;
    }
    else
    {
        runInitialScaleX = transform.localScale.x;
    }

    // 사망 스파인 기본 스케일 저장
    if (deathSkeleton != null)
    {
        deathInitialScaleX = deathSkeleton.transform.localScale.x;
    }
}


    void Start()
    {
        // GameManager에서 Player 준비되면 타겟 연결
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        else
            Debug.LogError("GameManager or Player not ready! Enemy target not set.");

        isLive = true;
        health = maxHealth;
        speed = originalSpeed;
        isSlowed = false;

        // 스파인 초기 상태 + 이동 애니 실행
        ResetSpineState();     // 
        PlayRunAnimation();    // 
    }

    void Update()
    {
        // 빙결 지속시간 관리
        //이성덕이 작성
        if (isFrozen)
        {
            freezeRemain -= Time.deltaTime;
            if (freezeRemain <= 0f)
                Unfreeze();
        }
    }

    public virtual void FixedUpdate()
    {
        if (isFrozen || isStunned || isKnockback)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (!isLive || target == null) return;

        // 플레이어를 향해 이동
        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;

        vec2 = dir.normalized; //이동 방향 기록 x 값으로 좌우 판별
        rb.MovePosition(rb.position + nextVec);

        // 물리 잔여속도 제거
        rb.velocity = Vector2.zero;
    }

    void LateUpdate()
{
    if (!isLive) return;
    if (target == null) return;

    // 이동 방향(vec2.x)에 따라 "이동 스파인"만 좌우 반전
    SkeletonAnimation runSa = RunSpine;
    if (runSa != null && Mathf.Abs(vec2.x) > 0.001f)
    {
        float sign = (vec2.x < 0) ? -1f : 1f;

        Transform t = runSa.transform;
        float baseRunScaleX = (runInitialScaleX != 0f) ? runInitialScaleX : t.localScale.x;

        t.localScale = new Vector3(
            Mathf.Abs(baseRunScaleX) * sign,
            t.localScale.y,
            t.localScale.z
        );
    }

    // 🔹 deathSkeleton 쪽은 여기서 건드리지 않는다
    //    → 프리팹에 세팅해둔 스케일/방향 그대로 사망 애니 재생
}



    protected virtual void OnEnable()
    {
        // 씬/풀 재활성화 시 기본 상태로 초기화
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();

        isLive = true;
        health = maxHealth;

        if (removeSlowRoutine != null)
        {
            StopCoroutine(removeSlowRoutine);
            removeSlowRoutine = null;
        }

        speed = originalSpeed;
        isSlowed = false;

        //  처치 집계 플래그 초기화 (오브젝트 풀 대비)
        hasCountedKill = false;

        isFrozen = false;          // 빙결 상태 해제
        freezeRemain = 0f;         // 남은 빙결 시간 초기화
        isStunned = false;         // 스턴 상태 해제
        isKnockback = false;       // 넉백 상태 해제

        //  다시 살아날 때 물리 복구
    if (rb != null)
    {
        rb.simulated = true;
        rb.velocity = Vector2.zero;
    }

    if (colliders != null)
    {
        foreach (var col in colliders)
            col.enabled = true;
    }

        // 스파인 상태 초기화 + 이동 애니 재생
        ResetSpineState();     // 
        PlayRunAnimation();    // 
    }

    // 스파인 상태 초기화
    // 스파인 상태 초기화
private void ResetSpineState()
{
    if (useSeparateSpine)
    {
        if (RunSpine != null)
        {
            // ★ 컴포넌트/렌더러 다시 켜기
            RunSpine.enabled = true;
            var mr = RunSpine.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = true;

            RunSpine.gameObject.SetActive(true);
            RunSpine.timeScale = 1f;
        }
        if (deathSkeleton != null)
        {
            deathSkeleton.enabled = true;
            var mr2 = deathSkeleton.GetComponent<MeshRenderer>();
            if (mr2 != null) mr2.enabled = true;

            deathSkeleton.gameObject.SetActive(false);
            deathSkeleton.timeScale = 1f;
        }
    }
    else
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.enabled = true;
            var mr = skeletonAnimation.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = true;

            skeletonAnimation.gameObject.SetActive(true);
            skeletonAnimation.timeScale = 1f;
        }
    }
}

    // 이동 애니메이션 실행
    private void PlayRunAnimation()
    {
        SkeletonAnimation runSa = RunSpine;
        if (runSa != null)
        {
            runSa.gameObject.SetActive(true);
            runSa.timeScale = 1f;
            runSa.AnimationState.SetAnimation(0, runAnimName, true);
        }

        if (useSeparateSpine && deathSkeleton != null)
        {
            deathSkeleton.gameObject.SetActive(false);
        }
    }

    // 사망 애니메이션 실행
    // 사망 애니메이션 실행
// 사망 애니메이션 실행
private void PlayDieAnimation()
{
    const float extraTime = 0.05f;  // 여유로 0.05초 정도 더 보기

    if (useSeparateSpine)
    {
        // 이동 Spine 숨기기
        SkeletonAnimation runSa = RunSpine;
        if (runSa != null)
        {
            // Enemy 루트를 끄지 않고, 이동 스파인만 안 보이게
            if (runSa.gameObject == gameObject)
            {
                runSa.enabled = false;
                var mr = runSa.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }
            else
            {
                runSa.gameObject.SetActive(false);
            }
        }

        // 사망 Spine 켜고 dead 재생
        if (deathSkeleton != null)
        {
            deathSkeleton.gameObject.SetActive(true);
            deathSkeleton.enabled = true;

            var mr2 = deathSkeleton.GetComponent<MeshRenderer>();
            if (mr2 != null) mr2.enabled = true;

            deathSkeleton.timeScale = 1f;

            // ★ dead 애니 한 번 재생
            var entry = deathSkeleton.AnimationState.SetAnimation(0, deadAnimName, false);

            // ★ 애니 길이(Duration) 가져오기
            float duration = 2.0f;
            if (entry != null && entry.Animation != null)
                duration = entry.Animation.Duration;

            // ★ dead 애니 끝날 때쯤 Enemy 비활성화
            StartCoroutine(DeactivateAfterDelay(duration + extraTime));
        }
    }
    else
    {
        // 단일 Spine 사용하는 몬스터 (몬스터1 같은 애들)
        if (skeletonAnimation != null)
        {
            var entry = skeletonAnimation.AnimationState.SetAnimation(0, deadAnimName, false);

            float duration = 0.5f;
            if (entry != null && entry.Animation != null)
                duration = entry.Animation.Duration;

            StartCoroutine(DeactivateAfterDelay(duration + extraTime));
        }
    }
}


    /// <summary>
    /// 스폰 시 외부에서 스탯 일괄 설정 (스폰러가 호출)
    /// </summary>
    public void Init(SpawnData data)
    {
        speed = data.speed;
        originalSpeed = data.speed;
        maxHealth = data.health;
        health = data.health;
        dps = data.dps;

        // 슬로우/플래그 리셋
        if (removeSlowRoutine != null)
        {
            StopCoroutine(removeSlowRoutine);
            removeSlowRoutine = null;
        }
        isSlowed = false;
        hasCountedKill = false;
        isLive = true;
        isFrozen = false;          // 빙결 상태 해제
        freezeRemain = 0f;         // 남은 빙결 시간 초기화
        isStunned = false;         // 스턴 상태 해제
        isKnockback = false;       // 넉백 상태 해제

        if (rb != null)
    {
        rb.simulated = true;
        rb.velocity = Vector2.zero;
    }

    if (colliders != null)
    {
        foreach (var col in colliders)
            col.enabled = true;
    }

        // Spine 초기화 + 이동 애니메이션 실행 (단일 / 분리 공통 처리)
        ResetSpineState();     // ★
        PlayRunAnimation();    // ★
    }

    //  중요: 플레이어에게 데미지는 Player.cs에서만 처리하도록 유지
    // (OnCollisionStay2D/OnTriggerStay2D는 Player.cs에서 처리 중복 방지)

    /// <summary>
    /// 외부(총알/스킬 등)에서 호출하는 데미지 처리
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;
        if (health <= 0f)
            Die();
    }

    /// <summary>
    /// 사망 처리 (애니메이션 트리거 + 처치 이벤트 + 비활성)
    /// </summary>
    void Die()
{
    if (!isLive) return;

    isLive = false;

    // ★ 물리 완전 차단
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.simulated = false;   // 더 이상 물리 계산 X
    }

    if (colliders != null)
    {
        foreach (var col in colliders)
            col.enabled = false; // 플레이어/총알과 충돌 X
    }

    // ★ 사망 애니 재생 (이동 스파인 숨기고, 죽는 스파인 켜기)
    PlayDieAnimation();

    // 처치 이벤트, 코루틴 그대로 유지
    if (!hasCountedKill)
    {
        hasCountedKill = true;
        OnAnyEnemyDied?.Invoke();
    }

    StartCoroutine(DeactivateAfterDelay(2.0f)); // 사망 애니 길이에 맞게 조절
}

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isLive && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(dps * Time.deltaTime);
        }
    }

    /// <summary>
    /// 슬로우 적용 (slowAmount: 0~1, duration초 후 원복)
    /// </summary>
    public void ApplySlow(float slowAmount, float duration)
    {
        slowAmount = Mathf.Clamp01(slowAmount);
        duration = Mathf.Max(0.01f, duration);

        if (removeSlowRoutine != null)
            StopCoroutine(removeSlowRoutine);

        speed = originalSpeed * slowAmount;
        isSlowed = true;
        removeSlowRoutine = StartCoroutine(RemoveSlowAfterDelay(duration));
    }

    IEnumerator RemoveSlowAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
        isSlowed = false;
        removeSlowRoutine = null;
    }

    public void ApplyKnockback(Vector2 direction, float force) // 넉백 효과
    {
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    public void ApplyStun(float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(WaitKnockbackThenStun(duration));
    }

    IEnumerator WaitKnockbackThenStun(float duration)
    {
        // 넉백 중이면 넉백 효과가 끝날 때까지 경직 효과 실행 X
        while (isKnockback)
            yield return null;

        // 넉백이 끝난 후 이제 Stun 시작
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        speed = 0f;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        if (RunSpine != null) RunSpine.timeScale = 1f;   // ★ 이동 Spine 기준

        speed = isSlowed ? originalSpeed * 0.5f : originalSpeed;

        isStunned = false;
    }

    public void ApplyFreeze(float duration) //아성덕이 작성
    {
        if (duration <= 0f) return;

        if (!isFrozen)
        {
            isFrozen = true;
            freezeRemain = duration;
            savedSpeed = speed;        // 기존 이동속도 저장
            speed = 0f;                // 속도 0으로

            if (RunSpine != null)
                RunSpine.timeScale = 0f; // 시간 0 = 멈춤

            if (rb != null) rb.velocity = Vector2.zero;
        }
        else
        {
            // 중첩 시 남은 시간 연장(선호 로직에 맞게 변경 가능)
            freezeRemain = Mathf.Max(freezeRemain, duration);
        }
    }

    private void Unfreeze() // 이성덕이 작성
    {
        isFrozen = false;
        speed = savedSpeed;

        if (RunSpine != null)
            RunSpine.timeScale = 1f;
    }
}
