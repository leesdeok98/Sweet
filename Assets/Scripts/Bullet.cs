
using UnityEngine;
using Spine.Unity;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int baseDamage = 8;
    public static float damageMultiplier = 1f;

    [Header("Iced Jelly (Spine) - 선택")]
    public GameObject icedJellySpinePrefab;
    [Range(0f, 1f)] public float icedJellyChance = 0.8f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy/Boss 외 무시
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            // 1) 기본 데미지
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            hitEnemy.TakeDamage(finalDamage);

            // (옵션) 2) 아이스젤리 스파인 확률 발동
            if (SkillManager.Instance != null && SkillManager.Instance.player != null)
            {
                if (SkillManager.Instance.player.hasIcedJellySkill && icedJellySpinePrefab != null)
                {
                    if (Random.value <= icedJellyChance)
                    {
                        Instantiate(icedJellySpinePrefab, hitEnemy.transform.position, Quaternion.identity);
                    }
                }
            }

            // 충돌지점 근사
            Vector2 hitPoint = collision.ClosestPoint(transform.position);

            // 3) 팝핑캔디 8방향 버스트
            if (SkillManager.Instance != null && SkillManager.Instance.player != null
                && SkillManager.Instance.player.hasPoppingCandy)
            {
                int burstDamage = Mathf.RoundToInt(finalDamage * SkillManager.Instance.poppingDamageFactor);

                GameObject burst = new GameObject("PoppingCandyBurst");
                burst.transform.position = hitPoint;

                var comp = burst.AddComponent<PoppingCandyBurst>();
                comp.Initialize(
                    colliderRadius: SkillManager.Instance.poppingColliderRadius,
                    range: SkillManager.Instance.poppingRange,
                    speed: SkillManager.Instance.poppingShardSpeed,
                    damage: burstDamage,
                    defaultShardPrefab: SkillManager.Instance.poppingDefaultShardPrefab,
                    shardPrefabs: SkillManager.Instance.poppingShardPrefabs
                );
            }
        }

        // 총알 소멸
        Destroy(gameObject);
    }
}
