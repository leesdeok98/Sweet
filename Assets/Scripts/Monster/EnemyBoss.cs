using System.Collections;
using UnityEngine;
using Spine.Unity;

public class EnemyBoss : Enemy
{
    [Header("Boss Settings")]
    [Tooltip("플레이어를 추적하는 시간(초)")]
    public float followDuration = 4.0f;

    [Header("Pattern 1: Charge (돌진)")]
    public float chargeSpeed = 5.0f;
    public float chargeDuration = 3.0f;
    public float chargeDamage = 10.0f;
    [SpineAnimation] public string chargeAnimName = "Run";

    [Header("Pattern 2: Shoot (발사)")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 4.0f;
    public float bulletDamage = 15.0f;
    public float bulletDuration = 5.0f;
    [SpineAnimation] public string shootAnimName = "Attack";
    [SpineAnimation] public string idleAnimName = "Idle";

    [Header("스킬 딜레이")]
    public float chargeWindupTime = 0.5f;
    public float shootDelay = 2f;

    private bool isActing = false;   // 패턴 동작 중인지(추적 off)

    // ★ 추가: 돌진 관통/데미지용
    private bool isCharging = false;
    private Collider2D[] bossColliders;

    void Start()
    {
        // BGM 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(AudioManager.Bgm.Boss_BGM);
        }

        // 기본 컴포넌트 확보
        rb = GetComponent<Rigidbody2D>();
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        // GameManager에서 플레이어 가져오기
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
        else
        {
            // 혹시 몰라서 Player 태그로 한 번 더 찾기
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.GetComponent<Rigidbody2D>();
        }

        originalSpeed = speed;
        isLive = true;
        health = maxHealth;

        // 기본 추적 애니메이션
        if (skeletonAnimation != null && !string.IsNullOrEmpty(runAnimName))
            skeletonAnimation.AnimationState.SetAnimation(0, runAnimName, true);

        // ★ 추가: 보스 자신의 콜라이더들 캐싱
        bossColliders = GetComponentsInChildren<Collider2D>();

        StartCoroutine(BossPatternRoutine());
    }

    //  기본적으로는 Enemy의 움직임을 비슷하게 사용하되,
    //    패턴(isActing=true)일 때만 추적 이동을 막는다.
    public override void FixedUpdate()
    {
        // 빙결/스턴이면 무조건 멈춤
        if (isFrozen || isStunned)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        // 넉백 중이면 Enemy 쪽 코루틴이 밀고 있을 것이므로 여기선 건드리지 않음
        if (isKnockback)
            return;

        if (!isLive || target == null)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        //  패턴 동작 중이면 기본 추적은 잠시 꺼둔다
        if (isActing)
            return;

        //  여기서 일반 Enemy처럼 플레이어 방향으로 추적
        Vector2 dir = target.position - rb.position;
        Vector2 nextVec = dir.normalized * speed * Time.fixedDeltaTime;

        vec2 = dir.normalized;  // Enemy.LateUpdate에서 좌우 반전에 사용
        rb.MovePosition(rb.position + nextVec);
        rb.velocity = Vector2.zero;
    }

    //  “추적 4초 → 랜덤 패턴 → 다시 추적 4초 …” 루프
    IEnumerator BossPatternRoutine()
    {
        while (isLive)
        {
            // 1) 추적 구간 (followDuration 초 동안 플레이어 추적)
            isActing = false;

            if (skeletonAnimation != null && !string.IsNullOrEmpty(runAnimName))
                skeletonAnimation.AnimationState.SetAnimation(0, runAnimName, true);

            float timer = 0f;
            while (timer < followDuration && isLive)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!isLive) yield break;

            // 2) 패턴 실행 구간
            isActing = true;

            // 패턴 선택: 50% 돌진 / 50% 발사
            float r = Random.value;
            if (r < 0.5f)
            {
                yield return StartCoroutine(ChargePattern());
            }
            else
            {
                yield return StartCoroutine(ShootPattern());
            }

            // 패턴이 끝나면 while 루프 반복 → 다시 추적 단계로 돌아감
        }
    }

    //  패턴 1: 돌진
    IEnumerator ChargePattern()
    {
        float originalDps = dps;
        dps = chargeDamage; // 돌진 중 데미지 강화

        // ★ 관통 시작: 모든 콜라이더를 Trigger로 전환
        isCharging = true;
        bool[] prevIsTrigger = null;
        if (bossColliders != null && bossColliders.Length > 0)
        {
            prevIsTrigger = new bool[bossColliders.Length];
            for (int i = 0; i < bossColliders.Length; i++)
            {
                prevIsTrigger[i] = bossColliders[i].isTrigger;
                bossColliders[i].isTrigger = true;
            }
        }

        // 1) 돌진 방향(플레이어 기준)
        Vector2 dir;
        if (target != null)
            dir = (target.position - rb.position);
        else
            dir = Vector2.left;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.left;

        dir = dir.normalized;
        vec2 = dir;  // 방향 정보(좌우 반전용)

        // 2) 돌진 애니메이션 재생 (준비동작 포함)
        if (skeletonAnimation != null && !string.IsNullOrEmpty(chargeAnimName))
            skeletonAnimation.AnimationState.SetAnimation(0, chargeAnimName, true);

        // 3)  준비동작 동안은 "애니만 재생, 이동은 없음"
        if (chargeWindupTime > 0f)
        {
            float windup = 0f;
            while (windup < chargeWindupTime && isLive)
            {
                // 스턴/빙결 중이면 그냥 기다리기만 함
                if (isFrozen || isStunned)
                {
                    if (rb != null) rb.velocity = Vector2.zero;
                }

                windup += Time.deltaTime;
                yield return null;
            }
        }

        // 준비동작 동안 플레이어가 많이 움직였을 수 있으니,
        //  실제 돌진 직전에 한 번 더 방향 재계산(원하면 유지해도 됨)
        if (target != null)
        {
            Vector2 newDir = (target.position - rb.position);
            if (newDir.sqrMagnitude >= 0.0001f)
                dir = newDir.normalized;
        }

        float timer = 0f;

        // 4) 여기부터 진짜 이동 시작 (chargeDuration 동안만)
        while (timer < chargeDuration && isLive)
        {
            timer += Time.fixedDeltaTime;

            if (!isFrozen && !isStunned && rb != null)
            {
                Vector2 nextPos = rb.position + dir * chargeSpeed * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);
                vec2 = dir;
            }
            else if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            yield return new WaitForFixedUpdate();
        }

        if (rb != null)
            rb.velocity = Vector2.zero;

        // ★ 관통 종료: 콜라이더 Trigger 상태 복구
        if (bossColliders != null && prevIsTrigger != null)
        {
            for (int i = 0; i < bossColliders.Length && i < prevIsTrigger.Length; i++)
            {
                if (bossColliders[i] != null)
                    bossColliders[i].isTrigger = prevIsTrigger[i];
            }
        }

        isCharging = false;
        dps = originalDps; // dps 원래대로 복구
    }

    //  패턴 2: 탄 발사
    IEnumerator ShootPattern()
    {
        // ★ 발사 패턴 동안에는 제자리에서 캐스팅 + 넉백 무시
        if (rb != null)
            rb.velocity = Vector2.zero;

        ignoreKnockback = true;   // ★ Enemy 쪽에서 넉백 무시

        // 공격 애니메이션 재생
        float waitTime = 0.5f;

        if (skeletonAnimation != null && !string.IsNullOrEmpty(shootAnimName))
        {
            var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, shootAnimName, false);

            if (trackEntry != null && trackEntry.Animation != null)
                waitTime = trackEntry.Animation.Duration;
        }

        if (shootDelay > 0f)
            yield return new WaitForSeconds(shootDelay);

        // 🔹 총알 발사 부분을 JellyPunk 스타일로 통일
        if (target != null && bulletPrefab != null)
        {
            Vector2 fireDirection = (target.position - (Vector2)transform.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            var enemyBullet = bullet.GetComponent<EnemyBullet>();
            if (enemyBullet != null)
            {
                // JellyPunk: Initialize(fireDir, dps, bulletSpeed, bulletDuration)
                // 보스는 bulletDamage를 쓰고 싶으면 dps 대신 bulletDamage 넣으면 됨
                enemyBullet.Initialize(fireDirection, bulletDamage, bulletSpeed, bulletDuration);
            }
        }

        // 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(waitTime);

        // ★ 발사 패턴 종료: 넉백 다시 허용
        if (rb != null)
            rb.velocity = Vector2.zero;

        ignoreKnockback = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (rb != null)
            rb.velocity = Vector2.zero;

        // ★ 비활성화 시에도 안전하게 상태 원복
        isCharging = false;
        if (bossColliders != null)
        {
            foreach (var col in bossColliders)
            {
                if (col != null)
                    col.isTrigger = false;
            }
        }

        // ★ 넉백 무시 플래그도 초기화
        ignoreKnockback = false;
    }

    // ★ 추가: 돌진 중 플레이어 관통 순간에 chargeDamage 한 번 데미지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLive) return;
        if (!isCharging) return;
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player == null) return;

        if (chargeDamage > 0f)
        {
            player.TakeDamage(chargeDamage);
        }
    }
}
