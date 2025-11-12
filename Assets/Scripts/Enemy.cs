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

    [HideInInspector] public bool isSlowed = false;
    private float originalSpeed;          // 기본 속도(슬로우/해제에 필요)

    [Header("Knockback")]
    public float knockbackDuration = 0.1f;

    private Coroutine removeSlowRoutine;

    // 🔸 처치수 중복 집계 방지용
    private bool hasCountedKill = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        originalSpeed = speed; // 인스펙터의 초기 speed 저장
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

    public virtual void FixedUpdate()
    {
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

    /// <summary>
    /// 넉백 적용 (간단한 임펄스)
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        // 필요하다면 knockbackDuration 동안 이동 로직 잠깐 끄는 처리도 가능
    }
}
