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

    // 갈색 계열 색상 (코코아 느낌)
    //private readonly Color cocoaColor = new Color(0.55f, 0.3f, 0.1f);

    void Awake()
    {
        // 본체에 SpriteRenderer 있으므로 이걸로 충분
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        // 프리팹 색상 보장
        if (sr != null)
            sr.color = cocoaColor;
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
        if (!collision.CompareTag("Enemy")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy == null) return;

        hitEnemy.TakeDamage(damage);

        // 넉백, 경직 적용
        Vector2 knockDir = (hitEnemy.transform.position - transform.position).normalized;
        hitEnemy.ApplyKnockback(knockDir, knockbackPower);
        hitEnemy.ApplyStun(stunDuration);

        // 비활성화로
        Deactivate();
    }
}
