using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed = 5f;

    private Rigidbody2D rigid;
    private SpriteRenderer spr;
    [SerializeField] private GameObject diepanel;
    public StrawberryPopCoreSkill popCoreSkill;

    [Header("HP")]
    public float maxHealth = 100f;
    public float health = 100f;
    private bool isLive = true;

    // 스킬 보유 상태 (인스펙터에서 체크)
    [Header("has skill")]
    public bool hasIcedJellySkill = false;
    public bool hasSugarShield = false;
    public bool hasDarkChip = false;
    public bool hasRollingChocolateBar = false;
    public bool hasPoppingCandy = false;
    public bool hasSyrupTornado = false;
    public bool hasCocoaPowder = false;
    public bool hasStrawberryPopCore = false;
    public bool hasHoneySpin = false;   

    // ✅ 인스펙터에서 체크된 스킬들을 한 번만 적용하기 위한 플래그
    private bool startingSkillsApplied = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();

        popCoreSkill = GetComponent<StrawberryPopCoreSkill>();

        // 항상 풀피로 시작 + 생존 상태 보장
        health = maxHealth;
        isLive = true;

        if (diepanel) diepanel.SetActive(false);
    }

    void OnEnable()
    {
        // 씬 재시작/부활 시 이동 가능 상태 보장
        isLive = true;
        if (rigid) rigid.velocity = Vector2.zero;

        // 재시작 시에도 처음부터 스킬 다시 적용할 수 있게 초기화
        startingSkillsApplied = false;
    }

    void Update()
    {
        if (!isLive) return;

        // 이동 입력
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // ✅ 인스펙터에서 체크된 hasXXX들을 보고 스킬을 한 번만 적용
        TryApplyStartingSkills();
    }

    void FixedUpdate()
    {
        if (!isLive) return;

        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (!isLive) return;

        if (inputVec.x != 0)
            spr.flipX = (inputVec.x < 0);
    }

    /// <summary>
    /// ✅ 게임 시작/부활 후, 인스펙터에서 체크된 스킬들을 SkillManager에 한 번만 전달
    /// </summary>
    void TryApplyStartingSkills()
    {
        // 이미 한 번 처리했으면 다시 안 함
        if (startingSkillsApplied) return;

        // SkillManager가 아직 준비 안 됐으면, 다음 프레임에 다시 시도
        if (SkillManager.Instance == null) return;

        var sm = SkillManager.Instance;

        // ───────── 인스펙터 bool → SkillManager.ActivateSkill 매핑 ─────────
        if (hasIcedJellySkill)
            sm.ActivateSkill(ItemData.ItemType.IcedJelly);

        if (hasSugarShield)
            sm.ActivateSkill(ItemData.ItemType.SugarShield);

        if (hasRollingChocolateBar)
            sm.ActivateSkill(ItemData.ItemType.RollingChocolateBar);

        if (hasPoppingCandy)
            sm.ActivateSkill(ItemData.ItemType.PoppingCandy);

        if (hasCocoaPowder)
            sm.ActivateSkill(ItemData.ItemType.CocoaPowder);

        if (hasStrawberryPopCore)
            sm.ActivateSkill(ItemData.ItemType.StrawberryPopCore);

        // 🔥 여기 세 개가 “안 되던 애들” → 이제 시작 시에도 강제로 실행
        if (hasHoneySpin)
            sm.ActivateSkill(ItemData.ItemType.HoneySpin);

        if (hasSyrupTornado)
            sm.ActivateSkill(ItemData.ItemType.SyrupTornado);

        if (hasDarkChip)
            sm.ActivateSkill(ItemData.ItemType.DarkChip);

        // 한 번 적용 완료
        startingSkillsApplied = true;
    }

    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;
        Debug.Log($"[Player] 피해: {damage:0.##}, HP: {Mathf.Max(health, 0):0.##}/{maxHealth}");

        if (health <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!isLive) return;
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        Debug.Log($"[Player] 회복: {amount:0.##}, HP: {health:0.##}");
    }

    void Die()
    {
        if (!isLive) return;

        isLive = false;
        if (rigid != null) rigid.velocity = Vector2.zero;

        if (GameManager.instance != null)
            GameManager.instance.GameOver();
        else
            Debug.LogError("[Player] GameManager.instance가 null입니다.");

        if (diepanel)
            diepanel.SetActive(true);   // 🔹 여기서 버튼 포함한 사망 패널 활성화

        Time.timeScale = 0f;            // 게임 일시정지
    }

    // 물리 충돌로 지속 피해를 받는 경우(Non-Trigger)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isLive) return;
        if (!collision.collider.CompareTag("Enemy")) return;

        Enemy enemy = collision.collider.GetComponent<Enemy>();
        if (enemy == null) return;

        float dmg = enemy.dps * Time.fixedDeltaTime;
        if (dmg > 0f) TakeDamage(dmg);
    }

    // 재시작/부활 시 호출하면 체력/상태 초기화(씬 리로드 없이도 사용 가능)
    public void ResetForRetry()
    {
        health = maxHealth;
        isLive = true;
        if (rigid) rigid.velocity = Vector2.zero;
        if (diepanel) diepanel.SetActive(false);
        Time.timeScale = 1f;

        startingSkillsApplied = false;
    }

    // 🔹 씬이 바뀔 때 새 사망 패널을 다시 연결하기 위한 세터
    public void SetDiePanel(GameObject panel)
    {
        diepanel = panel;
        if (diepanel != null)
            diepanel.SetActive(false);  // 기본은 꺼진 상태
    }
}
