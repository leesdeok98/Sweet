using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int baseDamage = 8; // 기본 공격력
    public static float damageMultiplier = 1f; // 전체 배율 (다크칩 등 효과 반영용)

    [Header("Iced Jelly Skill Settings")]
    public GameObject icedJellyPrefab;
    [Range(0f, 1f)] public float icedJellyChance = 0.8f;

    void Start()
    {
        Destroy(gameObject, lifeTime);


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy 또는 Boss 태그만 통과
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            hitEnemy.TakeDamage(finalDamage);

            // 아이스젤리 스킬
            if (SkillManager.Instance != null && SkillManager.Instance.player != null)
            {
                if (SkillManager.Instance.player.hasIcedJellySkill && icedJellyPrefab != null)
                {
                    if (Random.value <= icedJellyChance)
                        Instantiate(icedJellyPrefab, hitEnemy.transform.position, Quaternion.identity);
                }
            }
        }

        Destroy(gameObject);
    }

}