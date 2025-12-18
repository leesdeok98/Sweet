using UnityEngine;


public class BittermeltChaosDot : MonoBehaviour
{
    private Enemy targetEnemy;

    // 설정값 (Bullet에서 세트 효과 값으로 채워줌)
    private float duration = 5f; // 총 지속 시간
    private float dps = 2f;      // 초당 피해량

    private float elapsed = 0f;

 
    public void Initialize(Enemy enemy, float durationSeconds, float damagePerSecond)
    {
        targetEnemy = enemy;
        duration = durationSeconds;
        dps = damagePerSecond;
        elapsed = 0f;
    }

  
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
