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
        if (!collision.CompareTag("Enemy")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            // 🔹 최종 데미지 계산 (모든 탄환이 multiplier 반영)
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            hitEnemy.TakeDamage(finalDamage);

            // 아이스젤리 스킬 체크
            if (SkillManager.Instance != null && SkillManager.Instance.player != null)
            {
                if (SkillManager.Instance.player.hasIcedJellySkill && icedJellyPrefab != null)
                {
                    float roll = Random.value;
                    if (roll <= icedJellyChance)
                        Instantiate(icedJellyPrefab, hitEnemy.transform.position, Quaternion.identity);
                }
            }
        }

        Destroy(gameObject);
    }
}
