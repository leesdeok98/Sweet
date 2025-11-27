// IcedJellySkillSpine.cs (Sorting Controls 제거 버전)
using System.Collections;
using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class IcedJellySkillSpine : MonoBehaviour
{
    [Header("Effect Logic")]
    public float duration = 3f;
    public float tickDamage = 2f;
    [Range(0f, 1f)] public float slowAmount = 0.5f;
    public float tickInterval = 1f;
    public float radius = 1.5f;
    public LayerMask enemyMask;

    [Header("Spine")]
    public string loopAnimation = "loop";
    public float timeScale = 1f;

    private SkeletonAnimation skeleton;

    void Awake()
    {
        skeleton = GetComponent<SkeletonAnimation>();
        skeleton.Initialize(true);
        skeleton.timeScale = timeScale;
        skeleton.AnimationState.SetAnimation(0, loopAnimation, true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.IceJelly_SFX);
    }

    void OnEnable()
    {
        StartCoroutine(RunRoutine());
    }

    IEnumerator RunRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
            for (int i = 0; i < hits.Length; i++)
            {
                var enemy = hits[i].GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(tickDamage);
                    enemy.ApplySlow(slowAmount, duration - elapsed);
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
