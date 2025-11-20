// Enemy.cs
using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 🔸 전역 이벤트: 어떤 적이든 죽으면 한 번만 방송
    public static Action OnAnyEnemyDied;

    [Header("Stats")]
    public float speed;           // 이동 속도 (현재 속도)
    public float health;
    public float maxHealth;
    public float dps;             // 초당 피해량 (Player가 읽어 씀)

    [Header("Animation / Target")]
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;    // 추적 대상 (Player)

    protected bool isLive;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer spriter;
    //이성덕이 적음 
    private float freezeRemain = 0f;
    private float savedSpeed = 0f;
    private Color originalColor = Color.white;
    private float originalAnimSpeed = 1f;

    [HideInInspector] public bool isSlowed = false;
    private float originalSpeed;          // 기본 속도(슬로우/해제에 필요)

    [Header("넉백, 경직")]
    public float knockbackDuration = 0.1f;
    public bool isKnockback = false;
    public bool isStunned = false;
    private bool isFrozen = false;

    private Coroutine removeSlowRoutine;

    // 🔸 처치수 중복 집계 방지용
    private bool hasCountedKill = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        originalSpeed = speed; // 인스펙터의 초기 speed 저장ㅌ
        //이성덕이 추가했음 
        anim = GetComponent<Animator>();
        if (spriter != null) originalColor = spriter.color;
        if (anim != null) originalAnimSpeed = anim.speed;
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
        rb.MovePosition(rb.position + nextVec);

        // 물리 잔여속도 제거
        rb.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive) return;
        if (!isLive || target == null) return;

        // 좌우 플립
        spriter.flipX = target.position.x < rb.position.x;
    }

    void OnEnable()
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

        // 🔸 처치 집계 플래그 초기화 (오브젝트 풀 대비)
        hasCountedKill = false;

        isFrozen = false;          // 빙결 상태 해제
        freezeRemain = 0f;         // 남은 빙결 시간 초기화
        isStunned = false;         // 스턴 상태 해제
        isKnockback = false;       // 넉백 상태 해제

        if (anim != null) anim.speed = originalAnimSpeed;   // 애니메이션 재생속도 원복
        if (spriter != null) spriter.color = originalColor; // 파란 틴트 등 색상 원복
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

        if (anim != null) anim.speed = originalAnimSpeed;   // 애니메이션 재생속도 원복
        if (spriter != null) spriter.color = originalColor; // 파란 틴트 등 색상 원복

    }

    // ★★ 중요: 플레이어에게 데미지는 Player.cs에서만 처리하도록 유지
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
        rb.velocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Dead");

        // 🔸 처치수는 정확히 한 번만 증가
        if (!hasCountedKill)
        {
            hasCountedKill = true;
            OnAnyEnemyDied?.Invoke();
        }

        // 비주얼 연출 후 비활성화 (오브젝트 풀 전제)
        StartCoroutine(DeactivateAfterDelay(1f));
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
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

            if (anim != null)
            {
                originalAnimSpeed = anim.speed;
                anim.speed = 0f;       // 애니메이션도 정지
            }

            if (spriter != null)
            {
                // 살짝 푸른빛(원래 색에乘해 약간 파랗게). 필요하면 여기서 고정 색도 가능
                spriter.color = originalColor * new Color(0.7f, 0.85f, 1.15f, 1f);
            }
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

        if (anim != null)
            anim.speed = originalAnimSpeed;

        if (spriter != null)
            spriter.color = originalColor;
    }
}


