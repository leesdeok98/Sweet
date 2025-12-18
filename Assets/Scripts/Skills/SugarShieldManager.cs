using UnityEngine;

// 충돌 감지
// 플레이어에게 실드 소모를 요청하는 역할
public class SugarShieldManager : MonoBehaviour
{
    private Player player;
    private SugarShieldSkill skillManager;

    
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

      
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.layer == enemyLayer)
        {
            if (skillManager != null)
            {
                bool shieldConsumed = skillManager.ConsumeShieldByVisual(gameObject);

            
            }
        }
    }
}
