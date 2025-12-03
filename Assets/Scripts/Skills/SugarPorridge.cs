using UnityEngine;
using Spine.Unity;
using System.Collections;

/// <summary>
/// 설탕 폭죽의 발사체 및 폭발 이펙트 역할을 모두 담당하는 단일 스크립트입니다.
/// 발사체 충돌 감지용 Radius와 폭발 범위용 Radius를 분리하여 관리합니다.
/// </summary>
public class PorridgeBulletSkill : MonoBehaviour
{
    private Rigidbody2D rb;
    private SkeletonAnimation skeletonAnimation;
    private CircleCollider2D porridgeCollider;

    [Header("Projectile Collision")]
    [Tooltip("적과 닿는 것을 감지하기 위한 충돌체 반경 (로켓 본체 크기)")]
    public float collisionRadius = 0.2f;

    [Header("Spine Setting")]
    [SpineAnimation] public string flyingAnimName = "flying";  // 발사체 애니메이션 이름
    [SpineAnimation] public string explosionAnimName = "explosion"; // 폭발 애니메이션 이름

    [Tooltip("Spine 애니메이션이 끝난 후 오브젝트를 파괴할 때까지의 추가 대기 시간")]
    public float destroyDelay = 0.2f; // 폭발 후 파괴 딜레이

    // PlayerShooting.cs로부터 받을 설정값
    private float moveSpeed;
    private float flightTime;
    private float porridgeRadius; // 폭발 범위용 Radius (Initialize에서 받음)
    private int porridgeDamage;

    private bool isExploded = false;
    private int enemyLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        porridgeCollider = GetComponent<CircleCollider2D>();
        enemyLayer = LayerMask.NameToLayer("Enemy");

        // 콜라이더 생성 로직
        if (porridgeCollider == null)
        {
            porridgeCollider = gameObject.AddComponent<CircleCollider2D>();
            porridgeCollider.isTrigger = true;
        }



        if (skeletonAnimation == null)
        {
            Debug.LogError("PorridgeBulletSkill requires a SkeletonAnimation component!");
        }

        // 발사체 충돌 감지용 Radius 설정
        if (porridgeCollider != null)
        {
            porridgeCollider.radius = collisionRadius;
        }

        porridgeCollider.enabled = false;

        // 초기에는 Rigidbody의 시뮬레이션을 꺼둡니다.
        if (rb != null) rb.simulated = false;
    }

    /// <summary>
    /// 폭죽의 능력치, 속도, 수명, 방향을 초기화하고 발사합니다.
    /// PlayerShooting.cs에서 호출되며, 폭발 범위(explosionRadius)를 별도로 받습니다.
    /// </summary>
    public void Initialize(float explosionRadius, int damage, float speed, float time, Vector3 direction)
    {
        porridgeRadius = explosionRadius; // 폭발 범위 Radius 저장
        porridgeDamage = damage;
        moveSpeed = speed;
        flightTime = time;
        isExploded = false;

        if (porridgeCollider != null)
        {
            porridgeCollider.enabled = true;
        }

        // 애니메이션 시작 및 방향 설정
        if (skeletonAnimation != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            skeletonAnimation.enabled = true;
            skeletonAnimation.AnimationState.SetAnimation(0, flyingAnimName, true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarCraker_Fly_SFX);
        }

        // Rigidbody 활성화 및 이동 시작
        if (rb != null)
        {
            rb.simulated = true;
            rb.velocity = direction.normalized * moveSpeed;
        }

        StartCoroutine(ExplodeAfterFlightTime());
    }

    // 적과 충돌 시 즉시 폭발 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploded) return;
        if (collision.gameObject.layer != enemyLayer) return;
        {
            TryExplode();
        }
    }

    // 수명 만료 시 폭발 처리
    private IEnumerator ExplodeAfterFlightTime()
    {
        yield return new WaitForSeconds(flightTime);
        TryExplode();
    }

    /// <summary>
    /// 발사체의 이동을 멈추고 폭발 애니메이션을 재생하며 데미지를 적용합니다.
    /// </summary>
    private void TryExplode()
    {
        if (isExploded) return;
        isExploded = true;

        // 투사체 역할 중지
        StopAllCoroutines();

        // Rigidbody 비활성화 및 움직임 멈춤
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        if (porridgeCollider != null) porridgeCollider.enabled = false;

        // 폭발 로직 시작
        ExplodeAndApplyDamage(); // 데미지 적용

        // 애니메이션 전환 및 파괴 예약
        if (skeletonAnimation != null)
        {
            // 폭발 애니메이션 재생 (단발)
            var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, explosionAnimName, false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarCracker_Explode_SFX);
            trackEntry.Complete += OnExplosionComplete;
        }
        else
        {
            // 애니메이션이 없으면 바로 파괴
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 현재 위치를 중심으로 설정된 폭발 범위(porridgeRadius) 내의 모든 적을 감지하고 피해를 적용합니다.
    /// </summary>
    private void ExplodeAndApplyDamage()
    {
        int finalDamage = porridgeDamage;

        // 폭발 범위 Radius 사용
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, porridgeRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("Boss"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(finalDamage);
                }
            }
        }
    }

    // Spine 애니메이션 재생 완료 시 호출되는 콜백
    private void OnExplosionComplete(Spine.TrackEntry trackEntry)
    {
        // 애니메이션이 끝나면 잠시 후 파괴합니다.
        Destroy(gameObject, destroyDelay);
    }
}