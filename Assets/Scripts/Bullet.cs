// Bullet.cs
using UnityEngine;
using Spine.Unity; // SkeletonAnimation 사용

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public int baseDamage = 8;
    public static float damageMultiplier = 1f;

    [Header("Iced Jelly (Spine)")]
    public GameObject icedJellySpinePrefab;
    [Range(0f, 1f)] public float icedJellyChance = 0.8f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy/Boss 이외는 무시
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            // 1) 기본 데미지
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            hitEnemy.TakeDamage(finalDamage);

            // 2) 아이스젤리 스파인 확률 발동
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

            // 충돌 지점(콜라이더 표면 근사)
            Vector2 hitPoint = collision.ClosestPoint(transform.position);

            // 3) 팝핑캔디 8방향 버스트 생성
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
                    damage: burstDamage
                );
            }

            // 4) 팝핑캔디 스파인 FX(원샷) 생성 — 옵션
            if (SkillManager.Instance != null && SkillManager.Instance.poppingSpinePrefab != null)
            {
                var fx = Instantiate(
                    SkillManager.Instance.poppingSpinePrefab,
                    (Vector3)hitPoint + SkillManager.Instance.poppingSpineOffset,
                    Quaternion.identity
                );

                // ★ 정렬은 MeshRenderer(=Renderer)에서 처리
                var sa = fx.GetComponent<SkeletonAnimation>();
                if (sa != null)
                {
                    var r = sa.GetComponent<Renderer>(); // MeshRenderer
                    if (r != null)
                    {
                        // 필요 시 정렬 레이어 이름도 지정 가능
                        // r.sortingLayerName = "Effects";
                        r.sortingOrder = SkillManager.Instance.poppingSpineSortingOrder;
                    }

                    // 프리팹 기본 애니 대신 SkillManager 설정으로 재생하고 싶을 때
                    if (!string.IsNullOrEmpty(SkillManager.Instance.poppingSpineAnim))
                        sa.AnimationState.SetAnimation(0, SkillManager.Instance.poppingSpineAnim, SkillManager.Instance.poppingSpineLoop);
                }
            }
        }

        // 총알 소멸
        Destroy(gameObject);
    }
}
