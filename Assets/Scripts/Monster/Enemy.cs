// Enemy.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using Spine.Unity;
using UnityEditor.U2D.Sprites;

public class Enemy : MonoBehaviour
{

    [Header("Spine Setting")]
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation] public string runAnimName = "Run";
    [SpineAnimation] public string deadAnimName = "Die";

    // 🔸 전역 이벤트: 어떤 적이든 죽으면 한 번만 방송
    public static Action OnAnyEnemyDied;

    [Header("Stats")]
    public float speed;           // 이동 속도 (현재 속도)
    public float health;
    public float maxHealth;
    public float dps;             // 초당 피해량 (Player가 읽어 씀)

    [Header("Target")]
    public Rigidbody2D target;    // 추적 대상 (Player)

    protected bool isLive;
    protected Rigidbody2D rb;

    //이성덕이 적음 
    private float freezeRemain = 0f;
    private float savedSpeed = 0f;
    private Color originalColor = Color.white;
    //private float originalAnimSpeed = 1f;

    [HideInInspector] public bool isSlowed = false;
    protected float originalSpeed;      // 기본 속도(슬로우/해제에 필요)

    [Header("넉백, 경직")]
    public float knockbackDuration = 0.1f;
    public bool isKnockback = false;
    public bool isStunned = false;
    public bool isFrozen = false;

    private Coroutine removeSlowRoutine;

    // 🔸 처치수 중복 집계 방지용
    private bool hasCountedKill = false;

    public Vector2 vec2;
    private float spineInitialScaleX = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = speed; // 인스펙터의 초기 speed 저장 

        //스파인 초기 스케일 저장(좌우 반전용)
        if(skeletonAnimation != null) 
            spineInitialScaleX = skeletonAnimation.transform.localScale.x;
        else
            spineInitialScaleX = transform.localScale.x;
          
    }

    void Start()
    {
        // GameManager에서 Player 준비되면 타겟 연결
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        else
            Debug.LogError("GameManager or Player not ready! Enemy target not set.");

        isLive = true;
        health = maxHealth;
        speed = originalSpeed;
        isSlowed = false;

        //이성덕 작성 애니메이션이 없으면 애니메이션 실행시키는 코드
        if (skeletonAnimation != null)
            skeletonAnimation.AnimationState.SetAnimation(0, runAnimName, true);
    }

    void Update()
    {
        // 빙결 지속시간 관리
        //이성덕이 작성
        if (isFrozen)
        {
            freezeRemain -= Time.deltaTime;
            if (freezeRemain <= 0f)
                Unfreeze();
        }
    }

    public virtual void FixedUpdate()
    {
        if (isFrozen || isStunned || isKnockback)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (!isLive || target == null) return;

        // 플레이어를 향해 이동
        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;
        
        vec2 = dir.normalized; //이동 방향 기록 x 값으로 좌우 판별
        rb.MovePosition(rb.position + nextVec);



        // 물리 잔여속도 제거
        rb.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive) return;
        if (!isLive || target == null) return;


        //이성덕 작성 : 이동 방향(vec2.x)에 따라 Spine 좌우 반전
        if (skeletonAnimation != null && Mathf.Abs(vec2.x) > 0.001f)
        {
            Transform t = skeletonAnimation.transform;

            float sign = (vec2.x < 0) ? 1f : -1f;   // 왼쪽이면 -1, 오른쪽이면 1
            float baseScaleX = (spineInitialScaleX != 0f) ? spineInitialScaleX : t.localScale.x;

            t.localScale = new Vector3(
                Mathf.Abs(baseScaleX) * sign,
                t.localScale.y,
                t.localScale.z
            );
        }

        // 좌우 플립
        //spriter.flipX = target.position.x < rb.position.x;
    }

    protected virtual void OnEnable()
    {
        // 씬/풀 재활성화 시 기본 상태로 초기화
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();

        isLive = true;
        health = maxHealth;

        if (removeSlowRoutine != null)
        {
            StopCoroutine(removeSlowRoutine);
            removeSlowRoutine = null;
        }

        speed = originalSpeed;
        isSlowed = false;

        // 🔸 처치 집계 플래그 초기화 (오브젝트 풀 대비)
        hasCountedKill = false;

        isFrozen = false;          // 빙결 상태 해제
        freezeRemain = 0f;         // 남은 빙결 시간 초기화
        isStunned = false;         // 스턴 상태 해제
        isKnockback = false;       // 넉백 상태 해제

        if (skeletonAnimation != null)
            skeletonAnimation.timeScale = 1f;
        //if (anim != null) anim.speed = originalAnimSpeed;   // 애니메이션 재생속도 원복
        //if (spriter != null) spriter.color = originalColor; // 파란 틴트 등 색상 원복
    }

    /// <summary>
    /// 스폰 시 외부에서 스탯 일괄 설정 (스폰러가 호출)
    /// </summary>
    public void Init(SpawnData data)
    {
        speed = data.speed;
        originalSpeed = data.speed;
        maxHealth = data.health;
        health = data.health;
        dps = data.dps;

        // 슬로우/플래그 리셋
        if (removeSlowRoutine != null)
        {
            StopCoroutine(removeSlowRoutine);
            removeSlowRoutine = null;
        }
        isSlowed = false;
        hasCountedKill = false;
        isLive = true;
        isFrozen = false;          // 빙결 상태 해제
        freezeRemain = 0f;         // 남은 빙결 시간 초기화
        isStunned = false;         // 스턴 상태 해제
        isKnockback = false;       // 넉백 상태 해제

        if (skeletonAnimation != null)
        {
            skeletonAnimation.timeScale = 1f; // 속도 정상화
            skeletonAnimation.AnimationState.SetAnimation(0, runAnimName, true);
        }

        //if (anim != null) anim.speed = originalAnimSpeed;   // 애니메이션 재생속도 원복
        //if (spriter != null) spriter.color = originalColor; // 파란 틴트 등 색상 원복

    }

    // ★★ 중요: 플레이어에게 데미지는 Player.cs에서만 처리하도록 유지
    // (OnCollisionStay2D/OnTriggerStay2D는 Player.cs에서 처리 중복 방지)

    /// <summary>
    /// 외부(총알/스킬 등)에서 호출하는 데미지 처리
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!isLive) return;

        health -= damage;
        if (health <= 0f)
            Die();
    }

    /// <summary>
    /// 사망 처리 (애니메이션 트리거 + 처치 이벤트 + 비활성)
    /// </summary>
    void Die()
    {
        if (!isLive) return;

        isLive = false;
        rb.velocity = Vector2.zero;

        if (skeletonAnimation != null)
            skeletonAnimation.AnimationState.SetAnimation(0, deadAnimName, false);

        //if (anim != null)
        //    anim.SetTrigger("Dead");

        // 🔸 처치수는 정확히 한 번만 증가
        if (!hasCountedKill)
        {
            hasCountedKill = true;
            OnAnyEnemyDied?.Invoke();
        }

        // 비주얼 연출 후 비활성화 (오브젝트 풀 전제)
        StartCoroutine(DeactivateAfterDelay(1f));
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
    public virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isLive && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(dps * Time.deltaTime);
        }
    }

    /// <summary>
    /// 슬로우 적용 (slowAmount: 0~1, duration초 후 원복)
    /// </summary>
    public void ApplySlow(float slowAmount, float duration)
    {
        slowAmount = Mathf.Clamp01(slowAmount);
        duration = Mathf.Max(0.01f, duration);

        if (removeSlowRoutine != null)
            StopCoroutine(removeSlowRoutine);

        speed = originalSpeed * slowAmount;
        isSlowed = true;
        removeSlowRoutine = StartCoroutine(RemoveSlowAfterDelay(duration));
    }

    IEnumerator RemoveSlowAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
        isSlowed = false;
        removeSlowRoutine = null;
    }

    public void ApplyKnockback(Vector2 direction, float force) // 넉백 효과
    {
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    public void ApplyStun(float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(WaitKnockbackThenStun(duration));
    }

    IEnumerator WaitKnockbackThenStun(float duration)
    {
        // 넉백 중이면 넉백 효과가 끝날 때까지 경직 효과 실행 X
        while (isKnockback)
            yield return null;

        // 넉백이 끝난 후 이제 Stun 시작
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        speed = 0f;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        if (skeletonAnimation != null) skeletonAnimation.timeScale = 1f;

        speed = isSlowed ? originalSpeed * 0.5f : originalSpeed;

        isStunned = false;
    }
    public void ApplyFreeze(float duration) //아성덕이 작성
    {
        if (duration <= 0f) return;

        if (!isFrozen)
        {
            isFrozen = true;
            freezeRemain = duration;
            savedSpeed = speed;        // 기존 이동속도 저장
            speed = 0f;                // 속도 0으로

            if (skeletonAnimation != null)
                skeletonAnimation.timeScale = 0f; // 시간 0 = 멈춤

            // (선택사항) 색상 변경 코드 필요 시: skeletonAnimation.skeleton.SetColor(...) 사용

            //if (anim != null)
            //{
            //    originalAnimSpeed = anim.speed;
            //    anim.speed = 0f;       // 애니메이션도 정지
            //}

            //if (spriter != null)
            //{
            //    // 살짝 푸른빛(원래 색에乘해 약간 파랗게). 필요하면 여기서 고정 색도 가능
            //    spriter.color = originalColor * new Color(0.7f, 0.85f, 1.15f, 1f);
            //}
            if (rb != null) rb.velocity = Vector2.zero;
        }
        else
        {
            // 중첩 시 남은 시간 연장(선호 로직에 맞게 변경 가능)
            freezeRemain = Mathf.Max(freezeRemain, duration);
        }
    }
    private void Unfreeze() // 이성덕이 작성
    {
        isFrozen = false;
        speed = savedSpeed;

        if (skeletonAnimation != null)
            skeletonAnimation.timeScale = 1f;
        //if (anim != null)
        //    anim.speed = originalAnimSpeed;

        //if (spriter != null)
        //    spriter.color = originalColor;
    }
}


