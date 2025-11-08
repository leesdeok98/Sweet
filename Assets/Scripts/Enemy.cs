using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 5f;
    public float health;
    public float maxHealth;

    [Header("Animation")]
    public RuntimeAnimatorController[] animCon;

    [Header("Target")]
    public Rigidbody2D target;

    bool isLive;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriter;

    [HideInInspector] public bool isSlowed = false;
    float originalSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();

        // Inspector에서 설정한 speed를 originalSpeed로 저장
        originalSpeed = speed;
    }

    void Start()
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

        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;
        spriter.flipX = target.position.x < rb.position.x;
    }

    public void Init(SpawnData data)
    {
        if (data != null)
        {
            if (data.speed != 0f)
            {
                speed = data.speed;
                originalSpeed = data.speed;
            }

            maxHealth = data.health != 0f ? data.health : maxHealth;
            health = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;
        Debug.Log($"Enemy hit! HP: {health}");

        if (health <= 0)
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
}
