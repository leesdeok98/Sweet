using UnityEngine;
using System.Collections;

public class IcedJellySkill : MonoBehaviour
{
    public float duration = 3f;
    public float tickDamage = 2f;
    public float slowAmount = 0.5f;
    public float tickInterval = 1f;

    void Start()
    {
        StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f, LayerMask.GetMask("Enemy"));
            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(tickDamage);
                    enemy.ApplySlow(slowAmount, duration - elapsed);
                    Debug.Log($"[IcedJelly] slow call: remain={(duration - elapsed):0.00}");
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
    }
}