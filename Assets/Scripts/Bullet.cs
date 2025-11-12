// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int baseDamage = 8;
    public static float damageMultiplier = 1f;

    [Header("Iced Jelly (Spine)")]
    public GameObject icedJellySpinePrefab;   // ★ 스파인 프리팹으로 교체
    [Range(0f, 1f)] public float icedJellyChance = 0.8f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            hitEnemy.TakeDamage(finalDamage);

            // 아이스젤리 스킬(스파인)
            if (SkillManager.Instance != null && SkillManager.Instance.player != null)
            {
                if (SkillManager.Instance.player.hasIcedJellySkill && icedJellySpinePrefab != null)
                {
                    if (Random.value <= icedJellyChance)
                    {
                        var go = Instantiate(icedJellySpinePrefab, hitEnemy.transform.position, Quaternion.identity);

                        // 옵션: 지속시간/데미지/슬로우를 런타임에 주입하고 싶다면
                        var skill = go.GetComponent<IcedJellySkillSpine>();
                        if (skill != null)
                        {
                            // 필요하면 값 덮어쓰기 (Inspector 기본값을 쓰면 생략)
                            // skill.duration = 3f;
                            // skill.tickDamage = 2f;
                            // skill.slowAmount = 0.5f;
                            // skill.radius = 1.5f;
                        }
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}
