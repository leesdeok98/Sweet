using UnityEngine;

// 충돌 감지
// 플레이어에게 실드 소모를 요청하는 역할
public class SugarShieldManager : MonoBehaviour
{
    private Player player;
    private SugarShieldSkill skillManager;

    //  Enemy 레이어 번호
    private int enemyLayer;

    void Start()
    {
        player = GetComponentInParent<Player>();

        if (player == null)
        {
            Debug.LogError("SugarShieldVisual: 부모에서 Player 컴포넌트를 찾을 수 없습니다 실드 기능 오류.");
            Destroy(gameObject);
        }

        skillManager = player.GetComponent<SugarShieldSkill>();
        if (skillManager == null)
        {
            Debug.LogError("SugarShieldManager: Player 오브젝트에서 SugarShieldSkill 컴포넌트를 찾을 수 없습니다");
            Destroy(gameObject);
        }

        // Enemy 레이어
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // 태그 확인 → 레이어 확인으로 변경
        if (other.gameObject.layer == enemyLayer)
        {
            if (skillManager != null)
            {
                bool shieldConsumed = skillManager.ConsumeShieldByVisual(gameObject);

                /* if (shieldConsumed && other.CompareTag("EnemyBullet"))
                {
                    Destroy(other.gameObject);
                } */
            }
        }
    }
}
