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

    // ★ 눈꽃사탕 오라 내부용 참조
    private SpriteRenderer auraSR;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // 눈꽃사탕 보유 + 오라 스프라이트가 세팅된 경우, 총알 밑 레이어에 오라 생성
        var sm = SkillManager.Instance;
        if (sm != null && sm.player != null && sm.player.hasSnowflakeCandy && sm.snowflakeAuraSprite != null)
        {
            AttachSnowflakeAura(sm);
        }
    }

    void AttachSnowflakeAura(SkillManager sm)
    {
        // 총알 SR 기준으로 정렬/레이어 맞춤
        var mySR = GetComponent<SpriteRenderer>();
        var auraGO = new GameObject("SnowflakeAura");
        auraGO.transform.SetParent(transform);
        auraGO.transform.localPosition = Vector3.zero;
        auraGO.transform.localScale = Vector3.one * Mathf.Max(0.01f, sm.snowflakeAuraScale);

        auraSR = auraGO.AddComponent<SpriteRenderer>();
        auraSR.sprite = sm.snowflakeAuraSprite;
        auraSR.color = new Color(1f, 1f, 1f, Mathf.Clamp01(sm.snowflakeAuraAlpha));

        if (mySR != null)
        {
            auraSR.sortingLayerID = mySR.sortingLayerID;
            auraSR.sortingOrder = mySR.sortingOrder + sm.snowflakeAuraSortingOrderOffset; // 보통 -1
        }
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

            // ★ 3) 눈꽃사탕: 15% 확률로 빙결
            var sm = SkillManager.Instance;
            if (sm != null && sm.player != null && sm.player.hasSnowflakeCandy)
            {
                if (Random.value <= Mathf.Clamp01(sm.snowflakeFreezeChance))
                {
                    hitEnemy.ApplyFreeze(sm.snowflakeFreezeDuration);
                }
            }

            // (기존) 팝핑캔디 등 다른 처리들 있으면 아래에 그대로…
            // 충돌지점 근사 등 기존 코드 유지
        }

        // 총알 소멸
        Destroy(gameObject);
    }
}
