using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Unity.VisualScripting;

public class DonutBomb : Enemy
{
    [Header("Explosion Settings")]
    [Tooltip("자폭 범위")]
    public float explosionRadius = 1.5f;
    [Tooltip("자폭 데미지")]
    public float explosionDamage = 10f;
    [Tooltip("데미지 타이밍")]
    public float explosionDelay = 0.1f;

    [Header("Spine")]
    [SpineAnimation] public string explosionAnimName = "Explosion";

    //  추가: Explosion 애니메이션 크기 조절용 배율
    [Tooltip("Explosion 애니메이션 스케일 배율")]
    public float explosionScaleMultiplier = 1.5f;

    //  추가: 원래 스케일 저장용
    private Vector3 originalSpineScale;

    [Header("Target Layer")]
    public LayerMask Player;

    protected override void Die()
    {
        if (!isLive) return;

        isLive = false;
        rb.velocity = Vector2.zero;

        if (OnAnyEnemyDied != null) OnAnyEnemyDied.Invoke();

        StartCoroutine(ExplosionRoutine());
    }

    IEnumerator ExplosionRoutine()
    {
        GetComponent<Collider2D>().enabled = false;

        if (skeletonAnimation != null)
        {
            skeletonAnimation.timeScale = 1f;

            //추가: 폭발할 때만 Spine 스케일을 키움
            // 현재 스케일을 저장해 두고
            // explosionScaleMultiplier 만큼 키운 뒤 애니메이션 재생
            originalSpineScale = skeletonAnimation.transform.localScale;
            skeletonAnimation.transform.localScale = originalSpineScale * explosionScaleMultiplier;

            skeletonAnimation.AnimationState.SetAnimation(0, explosionAnimName, false);
        }

        yield return new WaitForSeconds(explosionDelay);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius);

        if (hit != null)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(explosionDamage);
            }
        }

        float animaDuration = 1f;
        if (skeletonAnimation != null && skeletonAnimation.skeleton.Data.FindAnimation(explosionAnimName) != null)
        {
            animaDuration = skeletonAnimation.skeleton.Data.FindAnimation(explosionAnimName).Duration;
        }

        yield return new WaitForSeconds(animaDuration - explosionDelay);

        // ★ 추가: 풀링 재사용을 위해 스케일 원래 값으로 복구
        if (skeletonAnimation != null)
        {
            skeletonAnimation.transform.localScale = originalSpineScale;
        }

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
