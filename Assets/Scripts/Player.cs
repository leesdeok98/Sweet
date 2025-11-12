using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed = 5f;

    private Rigidbody2D rigid;
    private SpriteRenderer spr;

    [Header("HP")]
    public float maxHealth = 100f;
    public float health = 100f;
    private bool isLive = true;

    // 스킬 보유 상태 (필요 시 사용)
    public bool hasIcedJellySkill = false;
    public bool hasSugarShield = false;
    public bool hasDarkChip = false;
    public bool hasRollingChocolateBar = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        health = Mathf.Clamp(health, 0f, maxHealth);
        isLive = (health > 0f);
    }

    void Update()
    {
        if (!isLive) return;
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if (!isLive) return;
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (!isLive) return;
        if (inputVec.x != 0)
            spr.flipX = (inputVec.x < 0);
    }

    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        // (옵션) 방어막이나 무적 처리 넣으려면 여기서 가드
        // if (hasSugarShield) return;

        health -= damage;
        Debug.Log($"[Player] 피해: {damage:0.##}, HP: {Mathf.Max(health, 0):0.##}/{maxHealth}");

        if (health <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!isLive) return;
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        Debug.Log($"[Player] 회복: {amount:0.##}, HP: {health:0.##}/{maxHealth}");
    }

    void Die()
    {
        if (!isLive) return;
        isLive = false;
        if (rigid != null) rigid.velocity = Vector2.zero;

        if (GameManager.instance != null)
            GameManager.instance.GameOver();
        else
            Debug.LogError("[Player] GameManager.instance가 null입니다.");

        gameObject.SetActive(false);
    }

    // ===== 충돌 데미지: 물리 충돌(Non-Trigger)만 쓸 경우 이 메서드만 유지 =====
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isLive) return;
        if (!collision.collider.CompareTag("Enemy")) return;

        Enemy enemy = collision.collider.GetComponent<Enemy>();
        if (enemy == null) return;

        float dmg = enemy.dps * Time.deltaTime;  // 초당 dps를 물리 스텝만큼 나눠서 적용
        if (dmg > 0f) TakeDamage(dmg);
    }

    // ===== 트리거 충돌을 쓸 경우, 위 OnCollisionStay2D 대신 이걸 사용 =====
    /*
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isLive) return;
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        float dmg = enemy.dps * Time.deltaTime;
        if (dmg > 0f) TakeDamage(dmg);
    }
    */
}
