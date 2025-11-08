using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int damage = 8;

    [Header("Iced Jelly Skill Settings")]
    public GameObject icedJellyPrefab;          // 아이스젤리 프리팹
    [Range(0f, 1f)] public float icedJellyChance = 0.8f; // 80% 확률

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
            // 1️⃣ 기본 데미지 적용
            hitEnemy.TakeDamage(damage);

            // 2️⃣ 아이스젤리 스킬 체크
            if (SkillManager.Instance != null && SkillManager.Instance.player != null)
            {
                if (SkillManager.Instance.player.hasIcedJellySkill)
                {
                    if (icedJellyPrefab != null)
                    {
                        float roll = Random.value; // 0~1 랜덤값
                        Debug.Log($"Iced Jelly Roll: {roll} / Chance: {icedJellyChance}");

                        if (roll <= icedJellyChance)
                        {
                            Instantiate(icedJellyPrefab, hitEnemy.transform.position, Quaternion.identity);
                            Debug.Log("✅ Iced Jelly spawned!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("IcedJellyPrefab is not assigned in the inspector!");
                    }
                }
                else
                {
                    Debug.Log("Player does not have IcedJellySkill.");
                }
            }
            else
            {
                Debug.LogWarning("SkillManager or Player is null!");
            }
        }

        Destroy(gameObject);
    }
}
