using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;    
    bool isLive;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (!isLive) return;

        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive) return;
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
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    // ✅ Bullet.cs에서 호출되는 피격 처리 함수
    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;
        Debug.Log($"Enemy hit! HP: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isLive = false;
        rb.velocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Dead");

        Destroy(gameObject, 1f);
    }
}
