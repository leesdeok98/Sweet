using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;           // 이동 속도
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;    // 추적할 대상 (Player)
    bool isLive;    // 생존 여부

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
//        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

    }
}
