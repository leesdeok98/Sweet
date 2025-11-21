using UnityEngine;

/// <summary>
/// 비터멜트 카오스(Bittermelt Chaos) 세트 효과로 인해
/// 적에게 5초간 초당 2의 지속 피해를 주는 컴포넌트입니다.
/// Enemy 오브젝트에 동적으로 붙어서 스스로 lifetime을 관리합니다.
/// </summary>
public class BittermeltChaosDot : MonoBehaviour
{
    private Enemy targetEnemy;

    // 설정값 (Bullet에서 세트 효과 값으로 채워줌)
    private float duration = 5f; // 총 지속 시간
    private float dps = 2f;      // 초당 피해량

    private float elapsed = 0f;

    /// <summary>
    /// DOT를 처음 적용할 때 호출.
    /// </summary>
    public void Initialize(Enemy enemy, float durationSeconds, float damagePerSecond)
    {
        targetEnemy = enemy;
        duration = durationSeconds;
        dps = damagePerSecond;
        elapsed = 0f;
    }

    /// <summary>
    /// 이미 DOT가 걸려있는 적에게 다시 총알이 맞았을 때,
    /// 남은 시간을 5초로 갱신하고 dps도 새 값으로 덮어씌웁니다.
    /// </summary>
    public void Refresh(float durationSeconds, float damagePerSecond)
    {
        duration = durationSeconds;
        dps = damagePerSecond;
        elapsed = 0f;
    }

    void Update()
    {
        // 적이 이미 죽었거나 파괴되었으면 자기 자신도 제거
        if (targetEnemy == null)
        {
            Destroy(this);
            return;
        }

        float dt = Time.deltaTime;
        elapsed += dt;

        if (elapsed <= duration)
        {
            // 매 프레임, 초당 dps 비율로 피해 누적
            float damageThisFrame = dps * dt;
            if (damageThisFrame > 0f)
            {
                targetEnemy.TakeDamage(damageThisFrame);
            }
        }
        else
        {
            // DOT 종료
            Destroy(this);
        }
    }
}
