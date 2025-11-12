// IcedJellySkillSpine.cs
using System.Collections;
using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class IcedJellySkillSpine : MonoBehaviour
{
    [Header("Effect Logic")]
    public float duration = 3f;          // 전체 유지시간
    public float tickDamage = 2f;        // 틱 데미지
    [Range(0f, 1f)] public float slowAmount = 0.5f; // 0.5 = 50% 감속
    public float tickInterval = 1f;      // 몇 초마다 한 번 적용
    public float radius = 1.5f;          // 적용 반경
    public LayerMask enemyMask;          // Enemy 레이어 지정(Inspector)

    [Header("Spine")]
    public string loopAnimation = "loop"; // 루프 애니 이름
    public float timeScale = 1f;

    [Header("Follow (optional)")]
    public bool followTarget = false;
    public Transform target;
    public Vector3 localOffset = Vector3.zero;

    SkeletonAnimation skeleton;
    float elapsed;

    public void SetTarget(Transform t)
    {
        target = t;
        followTarget = (t != null);
    }

    void Awake()
    {
        skeleton = GetComponent<SkeletonAnimation>();
        skeleton.Initialize(true);
        skeleton.timeScale = timeScale;
        skeleton.AnimationState.SetAnimation(0, loopAnimation, false);
    }

    void OnEnable()
    {
        StartCoroutine(RunRoutine());
    }

    IEnumerator RunRoutine()
    {
        elapsed = 0f;

        while (elapsed < duration)
        {
            if (followTarget && target != null)
            {
                if (transform.parent != target) transform.SetParent(target);
                transform.localPosition = localOffset;
            }

            // 범위 내 적에게 틱 적용
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
