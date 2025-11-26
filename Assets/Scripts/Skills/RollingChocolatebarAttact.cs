using System.Collections;
using UnityEngine;

public class RollingChocolateBarAttack : MonoBehaviour
{
    private int damage = 15;
    private float knockbackForce = 2f;

    Transform center;   // 플레이어
    float duration;     // 한 바퀴 시간(초)
    float startAngle;   // 시작 각도(옵션)
    int laps;           // 몇 바퀴 도는지(기본 1)

    // ★ 트위스트 오어 트릿 세트에서 한 번만 길이 증가를 적용했는지 확인용
    private bool twistRangeApplied = false;

    public void InitializeAttack(int dmg, float kbForce)
    {
        damage = dmg;
        knockbackForce = kbForce;
    }

    // ★ 시계침 모드: 막대 피벗이 "한쪽 끝"에 있어야 함 (Sprite Editor에서 설정)
    public void BeginOrbit(Transform center, float duration, float startAngleDeg = 0f, int laps = 1)
    {
        this.center = center;
        this.duration = Mathf.Max(0.01f, duration);
        this.startAngle = startAngleDeg;
        this.laps = Mathf.Max(1, laps);

        // 플레이어에 붙여서 정확히 중심 고정
        transform.SetParent(center);
        transform.localPosition = Vector3.zero;               // 피벗(막대 한쪽 끝)이 플레이어 위치
        transform.localRotation = Quaternion.Euler(0, 0, startAngle);

        // ★ 트위스트 오어 트릿 세트 효과가 발동 중이면, 막대 길이를 범위 배율만큼 키움
        //   - SkillManager.IsTwistOrTreatActive : 세트 발동 여부
        //   - SkillManager.twistRangeMultiplier : 예) 1.25f = 25% 증가
        var sm = SkillManager.Instance;
        if (sm != null && sm.IsTwistOrTreatActive && !twistRangeApplied)
        {
            twistRangeApplied = true;

            float mul = sm.twistRangeMultiplier;

            // ★ 트위스트 오어 트릿 세트일 때 전체 크기를 배율만큼 키움 (X/Y/Z 모두)
            transform.localScale = new Vector3(
                transform.localScale.x * mul,
                transform.localScale.y * mul,
                transform.localScale.z * mul
            );
        }


        StartCoroutine(ClockHandRoutine());
    }

    IEnumerator ClockHandRoutine()
    {
        float elapsed = 0f;
        float totalAngle = 360f * laps;

        while (elapsed < duration && center != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = startAngle - Mathf.Lerp(0f, totalAngle, t);

            // ★ 시계침: 반지름 방향으로 그대로 두기 (접선 +90도 회전 금지)
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;
        var hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            hitEnemy.TakeDamage(damage);
            Vector2 dir = (hitEnemy.transform.position - transform.position).normalized;
            hitEnemy.ApplyKnockback(dir, knockbackForce);
        }
    }
}
