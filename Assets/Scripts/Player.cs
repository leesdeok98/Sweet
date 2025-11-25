using UnityEngine;
using Spine.Unity;  // 🔹 Spine 사용
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed = 5f;

    private Rigidbody2D rigid;
   
    [SerializeField] private GameObject diepanel;
    [SerializeField] private DeathScreenCapture deathScreenCapture; 
    public StrawberryPopCoreSkill popCoreSkill;

    //  Spine 관련 필드
    [Header("Spine")]
    [SerializeField] private SkeletonAnimation skeletonAnim;   // 플레이어 Spine 컴포넌트 (직접 할당 or 자식에서 자동 탐색)
    [SpineAnimation] public string idleAnimationName = "idle";  // 가만히 있을 때
    [SpineAnimation] public string walkAnimationName = "walk";  // 이동 시
    [SpineAnimation] public string deadAnimationName = "dead";  // 사망 시

    private string currentAnimationName = ""; //  현재 재생 중인 애니메이션 이름
    private float spineInitialScaleX = 1f;    //  좌우 반전을 위한 기본 스케일

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
    public bool hasSnowflakeCandy = false;
    public bool hasCaramelCube = false;

    //인스펙터에서 체크된 스킬들을 한 번만 적용하기 위한 플래그
    private bool startingSkillsApplied = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();


        //  Spine SkeletonAnimation 자동/수동 할당
        if (skeletonAnim == null)
            skeletonAnim = GetComponentInChildren<SkeletonAnimation>();

        if (skeletonAnim != null)
        {
            spineInitialScaleX = skeletonAnim.transform.localScale.x;
            // 시작 시 idle 애니메이션 재생
            PlaySpineAnimation(idleAnimationName, true);
        }
        else
        {
            Debug.LogWarning("[Player] SkeletonAnimation이 할당되지 않았습니다. Spine 애니메이션이 재생되지 않습니다.");
        }

        popCoreSkill = GetComponent<StrawberryPopCoreSkill>();

        // 항상 풀피로 시작
        health = maxHealth;
        isLive = true;

        if (diepanel) diepanel.SetActive(false);
    }

    void OnEnable()
    {
        // 씬 초기화 시 이동
        isLive = true;
        if (rigid) rigid.velocity = Vector2.zero;

        // 씬 초기화 시 스킬 초기화
        startingSkillsApplied = false;

        // 🔹 다시 활성화될 때 idle 상태로 초기화
        PlaySpineAnimation(idleAnimationName, true);
    }

    void Update()
    {
        if (!isLive) return;

        // 이동 입력
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // 이동량에 따라 idle / walk 애니메이션 전환
        UpdateSpineAnimationByMove();

        //인스펙터에서 체크된 스킬들을 보고 스킬을 한 번만 적용
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
        {
            // 좌우 이동 방향에 따라 Spine 캐릭터 좌우 반전
            if (skeletonAnim != null)
            {
                Transform t = skeletonAnim.transform;
                float sign = (inputVec.x < 0) ? -1f : 1f;
                t.localScale = new Vector3(Mathf.Abs(spineInitialScaleX) * sign, t.localScale.y, t.localScale.z);
            }


        }
    }

    /// 게임 시작/부활 후, 인스펙터에서 체크된 스킬들을 SkillManager에 한 번만 전달
    void TryApplyStartingSkills()
    {
        // 이미 한 번 처리했으면 다시 안 함
        if (startingSkillsApplied) return;

        // SkillManager가 아직 준비 안 됐으면, 다음 프레임에 다시 시도
        if (SkillManager.Instance == null) return;

        var sm = SkillManager.Instance;

        // ───────── 인스펙터 bool → SkillManager.ActivateSkill 매핑 ─────────
        if (hasIcedJellySkill) sm.ActivateSkill(ItemData.ItemType.IcedJelly);
        if (hasSugarShield) sm.ActivateSkill(ItemData.ItemType.SugarShield);
        if (hasRollingChocolateBar) sm.ActivateSkill(ItemData.ItemType.RollingChocolateBar);
        if (hasPoppingCandy) sm.ActivateSkill(ItemData.ItemType.PoppingCandy);
        if (hasCocoaPowder) sm.ActivateSkill(ItemData.ItemType.CocoaPowder);
        if (hasStrawberryPopCore) sm.ActivateSkill(ItemData.ItemType.StrawberryPopCore);
        if (hasCaramelCube) sm.ActivateSkill(ItemData.ItemType.CaramelCube);

        // 🔥 여기 세 개가 “안 되던 애들” → 이제 시작 시에도 강제로 실행
        if (hasHoneySpin) sm.ActivateSkill(ItemData.ItemType.HoneySpin);
        if (hasSyrupTornado) sm.ActivateSkill(ItemData.ItemType.SyrupTornado);
        if (hasDarkChip) sm.ActivateSkill(ItemData.ItemType.DarkChip);

        // ★ 추가: 눈꽃사탕 자동 적용
        if (hasSnowflakeCandy) sm.ActivateSkill(ItemData.ItemType.SnowflakeCandy);

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

    // 🔹 사망 애니메이션 재생
    PlaySpineAnimation(deadAnimationName, false);

    // 🔹 화면 캡쳐 + die panel 위에 띄우기
    if (deathScreenCapture != null)
    {
        deathScreenCapture.ShowDeathScreen();
    }
    else
    {
        Debug.LogWarning("[Player] DeathScreenCapture 참조가 비었습니다.");
        // 만약 캡쳐 스크립트 연결 안 돼 있으면 최소한 기존처럼 패널만 켜기
        if (diepanel)
            diepanel.SetActive(true);
    }

    // 🔹 기존 GameOver 로직 & 정지
    if (GameManager.instance != null)
        GameManager.instance.GameOver();
    else
        Debug.LogError("[Player] GameManager.instance가 null입니다.");

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

        // 부활 시 idle 애니메이션으로 돌아가기
        PlaySpineAnimation(idleAnimationName, true);
    }

    // 씬이 바뀔 때 새 사망 패널을 다시 연결하기 위한 세터
    public void SetDiePanel(GameObject panel)
    {
        diepanel = panel;
        if (diepanel != null)
            diepanel.SetActive(false);  // 기본은 꺼진 상태
    }


    void UpdateSpineAnimationByMove()
    {
        if (skeletonAnim == null) return;

        // 죽었으면 여기서는 상태를 건드리지 않고 Die()에서 dead를 재생
        if (!isLive) return;

        string nextAnim;

        if (inputVec.sqrMagnitude > 0.01f)
            nextAnim = walkAnimationName;
        else
            nextAnim = idleAnimationName;

        if (currentAnimationName == nextAnim) return; // 애니 중복 방지

        bool loop = nextAnim != deadAnimationName;
        PlaySpineAnimation(nextAnim, loop);
    }


    /// Spine 애니메이션을 재생하는 공통 함수.

    void PlaySpineAnimation(string animName, bool loop)
    {
        if (skeletonAnim == null) return;
        if (string.IsNullOrEmpty(animName)) return;

        currentAnimationName = animName;
        skeletonAnim.AnimationState.SetAnimation(0, animName, loop);
    }
}
