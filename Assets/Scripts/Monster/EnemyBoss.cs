using System.Collections;
using UnityEngine;
using Spine.Unity;

public class EnemyBoss : Enemy
{
    [Header("Boss Settings")]
    public float patternInterval = 7.0f;

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

    //private bool isActing = false;

    void Start()
    {

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(AudioManager.Bgm.Boss_BGM);
        }

        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();

        rb = GetComponent<Rigidbody2D>();
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponent<SkeletonAnimation>();

        originalSpeed = speed;
        isLive = true;
        health = maxHealth;

        StartCoroutine(BossPatternRoutine());
    }

    public override void FixedUpdate()
    {
        // 보스는 패턴 코루틴으로 움직이므로 부모의 추적 기능 끔
    }

    IEnumerator BossPatternRoutine()
    {
        while (isLive)
        {
            // 7초 동안 대기 (Idle)
            yield return StartCoroutine(IdleState());

            // 랜덤 뽑기 (0 또는 1)
            int randomAction = Random.Range(0, 2);

            if (randomAction == 0)
            {
                // 50% 확률로 돌진
                yield return StartCoroutine(ChargePattern());
            }
            else
            {
                // 50% 확률로 발사
                yield return StartCoroutine(ShootPattern());
            }
        }
    }

    IEnumerator IdleState()
    {
        rb.velocity = Vector2.zero;
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimName, true);

        // 여기서 7초 동안 쉼
        yield return new WaitForSeconds(patternInterval);
    }

    IEnumerator ChargePattern()
    {
        //isActing = true;
        float originalDps = dps;
        dps = chargeDamage; // 데미지 10으로 변경

        Vector2 dir = Vector2.zero;
        if (target != null)
            dir = (target.position - rb.position).normalized;

        skeletonAnimation.AnimationState.SetAnimation(0, chargeAnimName, true);

        float timer = 0;
        while (timer < chargeDuration) // 3초 돌진
        {
            rb.velocity = dir * chargeSpeed; // 속도 5
            timer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        dps = originalDps; // 데미지 복구
        //isActing = false;
    }

    IEnumerator ShootPattern()
    {
        //isActing = true;
        rb.velocity = Vector2.zero;

        var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, shootAnimName, false);

        // 발사 애니메이션 타이밍에 맞추려면 여기에 딜레이 추가 가능 (예: 0.5초)
        // yield return new WaitForSeconds(0.5f);

        if (target != null)
        {
            Vector2 dir = (target.position - rb.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            // 탄속 4, 데미지 15
            bullet.GetComponent<EnemyBullet>().Initialize(dir, bulletDamage, bulletSpeed, bulletDuration);
        }

        yield return new WaitForSeconds(trackEntry.Animation.Duration);
        //isActing = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }
}