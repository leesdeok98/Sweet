using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    public float speed;           // 이동 속도
    public float health;
    public float maxHealth;
    public float dps;

    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;    // 추적할 대상 (Player)

    bool isLive;                  // 생존 여부

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriter;

    [HideInInspector]
    public bool isSlowed = false;
    float originalSpeed;

    [Header("Knockback")] // 넉백 효과
    public float knockbackDuration = 0.1f; // 넉백 유지 시간(필요시 사용)

    // 슬로우 해제 코루틴 핸들
    private Coroutine removeSlowRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();

        originalSpeed = speed;
    }

    private void Start()
    {
        // GameManager와 Player가 준비될 때까지 기다리고 target 설정
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("GameManager or Player not ready! Enemy target not set.");
        }

        isLive = true;
        health = maxHealth;
        speed = originalSpeed;
        isSlowed = false;
    }

    void FixedUpdate()
    {
        if (!isLive || target == null) return;

        // 기본 추적 이동
        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + nextVec);

        // 프로젝트 구조상 유지
        rb.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive) return;
        // 좌우 반전 (플레이어 기준)
        spriter.flipX = target.position.x < rb.position.x;
    }

    void OnEnable()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();

        isLive = true;
        health = maxHealth;

        // 슬로우 상태 초기화 안전장치
        if (removeSlowRoutine != null)
        {
            StopCoroutine(removeSlowRoutine);
            removeSlowRoutine = null;
        }
        speed = originalSpeed;
        isSlowed = false;
    }

    public void Init(SpawnData data)
    {
        speed = data.speed;
        originalSpeed = data.speed;
        maxHealth = data.health;
        health = data.health;
        dps = data.dps;

        // 필요 시 애니메이션 타입 적용
        // anim.runtimeAnimatorController = animCon[data.spriteType];
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isLive && collision.gameObject.CompareTag("Player"))
        {
            GameManager.instance.player.GetComponent<Player>().TakeDamage(dps * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;

        if (health <= 0f)
            Die();
    }

    void Die()
    {
        isLive = false;
        rb.velocity = Vector2.zero;
        if (anim != null)
            anim.SetTrigger("Dead");
        StartCoroutine(DeactivateAfterDelay(1f));
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    // 🔧 슬로우: 갱신 가능(중복 호출 시 지속시간 리셋)
    public void ApplySlow(float slowAmount, float duration)
    {
        // 안전 클램프
        slowAmount = Mathf.Clamp01(slowAmount);
        duration = Mathf.Max(0.01f, duration);

        // ✅ 디버그에서 쓰는 이전 속도 기록 (오류 원인 해결)
        float prevSpeed = speed;

        // 즉시 속도 반영
        speed = originalSpeed * slowAmount;
        isSlowed = true;

        // 기존 해제 타이머가 있으면 끊고 새로 시작 (지속시간 갱신)
        if (removeSlowRoutine != null)
            StopCoroutine(removeSlowRoutine);

        removeSlowRoutine = StartCoroutine(RemoveSlowAfterDelay(duration));

        // 디버그 로그 (원하면 주석 처리해도 됨)
        Debug.Log($"[Enemy] ApplySlow: amount={slowAmount:0.00}, dur={duration:0.00}, speed {prevSpeed:0.00} -> {speed:0.00}");
    }

    IEnumerator RemoveSlowAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        speed = originalSpeed;
        isSlowed = false;
        removeSlowRoutine = null;

        // 디버그 로그 (원하면 주석 처리)
        Debug.Log("[Enemy] Slow cleared -> speed restored to original");
    }

    // 넉백
    public void ApplyKnockback(Vector2 direction, float force)
    {
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }
}
