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

    bool isLive;    // 생존 여부

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriter;

    [HideInInspector]
    public bool isSlowed = false;
    float originalSpeed;

    [Header("Knockback")] // 넉백 효과 
    public float knockbackDuration = 0.1f; // 넉백 지속 시간
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

        // 플레이어 방향 벡터 구하기
        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;
        // 이동
        rb.MovePosition(rb.position + nextVec);
        // 물리 회전 방지
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
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {

        //if (data == null)
        //{
        //    if (data.speed != 0f)
        //    {
        //        speed = data.speed;
        //        originalSpeed = data.speed;
        //    }

        //    maxHealth = data.health != 0f ? data.health : maxHealth;
        //    health = maxHealth;
        //}

        //        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed; 
        originalSpeed = data.speed;
        maxHealth = data.health;
        health = data.health;
        dps = data.dps;

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
        Debug.Log($"Enemy hit! HP: {health}");

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

    public void ApplySlow(float slowAmount, float duration)
    {
        if (isSlowed) return;

        isSlowed = true;
        speed = originalSpeed * slowAmount;
        StartCoroutine(RemoveSlowAfterDelay(duration));
    }

    IEnumerator RemoveSlowAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
        isSlowed = false;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }
}
