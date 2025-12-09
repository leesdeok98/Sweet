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

    // 코코아 색
    private readonly Color cocoaColor = new Color(0.55f, 0.3f, 0.1f);

    // 🔹 눈꽃사탕 오라용
    private SpriteRenderer auraSR;

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

        // 눈꽃사탕을 나중에 먹은 경우도 대비해서,
        // 재활성화될 때마다 한 번 오라 붙이기 시도
        TryAttachSnowflakeAura();

        CancelInvoke();
        Invoke(nameof(Deactivate), lifeTime);
    }

    void Deactivate()
    {
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }

    // 🔸 여기 오타 있었음: OnTriggerEnter2D(Collider2D collision) 이게 정답
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != enemyLayer) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy == null) return;

        // 1) 기본 데미지
        hitEnemy.TakeDamage(damage);

        // 2) 골렘도 아니고, 보스도 아닐 때만 넉백 적용
        if (!collision.CompareTag(golemTag) && !collision.CompareTag(bossTag))
        {
            Vector2 knockDir = (hitEnemy.transform.position - transform.position).normalized;
            hitEnemy.ApplyKnockback(knockDir, knockbackPower);
        }

        // 3) 보스만 스턴 제외 (골렘은 스턴 O)
        if (!collision.CompareTag(bossTag))
        {
            hitEnemy.ApplyStun(stunDuration);
        }

        // 4) 눈꽃사탕: 일정 확률로 빙결
        var sm = SkillManager.Instance;
        if (sm != null && sm.player != null && sm.player.hasSnowflakeCandy)
        {
            if (Random.value <= Mathf.Clamp01(sm.snowflakeFreezeChance))
            {
                hitEnemy.ApplyFreeze(sm.snowflakeFreezeDuration);
            }
        }

        Deactivate();
    }

    /// <summary>
    /// 눈꽃사탕을 보유했을 때, 총알 아래에 오라 스프라이트를 붙여준다.
    /// 이미 붙어 있으면 다시 만들지 않음.
    /// </summary>
    void TryAttachSnowflakeAura()
    {
        // 이미 오라가 한 번 만들어졌으면 다시 안 만듦
        if (auraSR != null) return;

        var sm = SkillManager.Instance;
        if (sm == null || sm.player == null) return;
        if (!sm.player.hasSnowflakeCandy) return;
        if (sm.snowflakeAuraSprite == null) return;

        GameObject auraGO = new GameObject("SnowflakeAura");
        auraGO.transform.SetParent(transform);
        auraGO.transform.localPosition = Vector3.zero;
        auraGO.transform.localScale = Vector3.one * Mathf.Max(0.01f, sm.snowflakeAuraScale);

        auraSR = auraGO.AddComponent<SpriteRenderer>();
        auraSR.sprite = sm.snowflakeAuraSprite;
        auraSR.color = new Color(1f, 1f, 1f, Mathf.Clamp01(sm.snowflakeAuraAlpha));

        if (sr != null)
        {
            // 총알과 같은 소팅 레이어 + 오프셋
            auraSR.sortingLayerID = sr.sortingLayerID;
            auraSR.sortingOrder = sr.sortingOrder + sm.snowflakeAuraSortingOrderOffset;
        }
    }
}
