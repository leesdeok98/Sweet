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

    //  추가: 넉백 제외할 태그 (기본값 Golem)
    [SerializeField] private string golemTag = "Golem";

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

        
        //  골렘 태그인 경우에는 넉백만 제외
        if (!collision.CompareTag(golemTag))
        {
            Vector2 knockDir = (hitEnemy.transform.position - transform.position).normalized;
            hitEnemy.ApplyKnockback(knockDir, knockbackPower);
        }

        // 골렘이든 아니든 스턴은 동일하게 적용
        hitEnemy.ApplyStun(stunDuration);

        
        Deactivate();
    }
}
