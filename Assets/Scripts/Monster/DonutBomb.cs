using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Unity.VisualScripting;

public class DonutBomb : Enemy
{
    [Header("Explosion Settings")]
    [Tooltip("자폭 범위")]
    public float explosionRadius = 1.0f;
    [Tooltip("자폭 데미지")]
    public float explosionDamage = 10f;
    [Tooltip("데미지 타이밍")]
    public float explosionDelay = 0.1f;

    [Header("Spine")]
    [SpineAnimation] public string explosionAnimName = "Explosion";

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

        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

