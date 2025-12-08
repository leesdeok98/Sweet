using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CocoaPowderBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int damage = 5;
    public float knockbackPower = 5f;
    public float stunDuration = 0.4f;

    private SpriteRenderer sr;
    private Player player;
    private int enemyLayer;

    private readonly Color cocoaColor = new Color(0.55f, 0.3f, 0.1f);

    // 🔸 넉백 / 스턴 처리에 사용할 태그들
    [Header("Knockback / Stun Exclude Tags")]
    [SerializeField] private string golemTag = "Golem";
    [SerializeField] private string bossTag = "Boss";

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (sr != null)
            sr.color = cocoaColor;

        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    void OnEnable()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        CancelInvoke();
        Invoke(nameof(Deactivate), lifeTime);
    }

    void Deactivate()
    {
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != enemyLayer) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy == null) return;

        hitEnemy.TakeDamage(damage);

        // 🔸 골렘도 아니고, 보스도 아닐 때만 넉백 적용
        if (!collision.CompareTag(golemTag) && !collision.CompareTag(bossTag))
        {
            Vector2 knockDir = (hitEnemy.transform.position - transform.position).normalized;
            hitEnemy.ApplyKnockback(knockDir, knockbackPower);
        }

        // 🔸 보스만 스턴 제외 (골렘은 스턴 O)
        if (!collision.CompareTag(bossTag))
        {
            hitEnemy.ApplyStun(stunDuration);
        }

        Deactivate();
    }
}
