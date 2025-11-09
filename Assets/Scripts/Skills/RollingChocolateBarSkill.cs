using UnityEngine;

// 플레이어에 부착
public class RollingChocolateBarSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject rollingChocolateBarPrefab;
    public float cooldown = 5f;
    public float orbitDuration = 2f; // ★ 실제 한 바퀴 도는 시간(2초)
    public float orbitRadius = 1.6f; // ★ 플레이어 기준 반지름(이미지 길이에 맞춰 조절)


    private float timer;
    private Player player;

    void Awake()
    {
        player = GetComponent<Player>();
        timer = cooldown; // 시작하자마자 가능하도록
    }

    void Update()
    {
        if (player != null && player.hasRollingChocolateBar)
        {
            timer += Time.deltaTime;
            if (timer >= cooldown)
            {
                ActivateSkill();
                timer = 0f;
            }
        }
    }

    void ActivateSkill()
    {
        if (rollingChocolateBarPrefab == null)
        {
            Debug.LogError("RollingChocolateBar Prefab is not assigned in the inspector!");
            return;
        }

        // 1) 플레이어 위치에 생성 + 플레이어의 자식으로 붙여서 추적 보장
        GameObject skillInstance = Instantiate(rollingChocolateBarPrefab, transform.position, Quaternion.identity);
        skillInstance.transform.SetParent(transform); // 플레이어 이동 따라감

        // 2) 공격 스크립트 초기화 및 궤도 회전 시작
        RollingChocolateBarAttack attackScript = skillInstance.GetComponent<RollingChocolateBarAttack>();
        if (attackScript != null)
        {
            attackScript.InitializeAttack(15, 1.5f);
            // 시작 각도(0도)에서 1바퀴(2초) 회전
            attackScript.BeginOrbit(transform, orbitDuration, 0f, 1);

        }
        else
        {
            Debug.LogError("RollingChocolateBarPrefab is missing the RollingChocolateBarAttack script!");
        }

        Debug.Log("Rolling Chocolate Bar Activated!");
    }
}
