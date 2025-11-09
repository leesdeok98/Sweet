using UnityEngine;

// 프리팹에 부착

public class RollingChocolateBarAttack : MonoBehaviour
{
    private int damage = 15;
    private float knockbackForce = 2f;

    // 스킬 프리팹 생성 시 초기 설정
    public void InitializeAttack(int dmg, float kbForce)
    {
        damage = dmg;
        knockbackForce = kbForce;
    }

    // 프리팹의 Collider2D (Is Trigger 체크된 원형 콜라이더 예상)가 적과 충돌했을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 적(Enemy) 태그 확인
        if (!collision.CompareTag("Enemy")) return;

        Enemy hitEnemy = collision.GetComponent<Enemy>();
        if (hitEnemy != null)
        {
            // 2. 데미지 적용
            hitEnemy.TakeDamage(damage);

            // 3. 넉백 적용
            // 충돌 지점에서 적을 밀어낼 방향을 계산 (넉백 방향: 스킬 중심 -> 적 위치)
            Vector2 knockbackDirection = (hitEnemy.transform.position - transform.position).normalized;
            hitEnemy.ApplyKnockback(knockbackDirection, knockbackForce);

            Debug.Log($"Enemy hit by Rolling Chocolate Bar! Damage: {damage}, Knockback: {knockbackForce}");
        }

        // 이 스킬은 한 번의 애니메이션 동안 여러 적을 공격할 수 있도록 
        // 충돌 시 Destroy(gameObject)를 호출하지 않습니다.
    }
}