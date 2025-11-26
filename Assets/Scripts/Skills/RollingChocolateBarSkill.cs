using UnityEngine;

// 플레이어에 부착
public class RollingChocolateBarSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject rollingChocolateBarPrefab;
    public float cooldown = 5f;
    public float orbitDuration = 1f; // ★ 실제 한 바퀴 도는 시간(2초)
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

        // ─────────────────────────────────────────────────────────────
        // ★ Twist or Treat 세트 효과 반영
        //   - 기본: 1개 생성, 기본 orbitRadius 사용
        //   - 세트 발동 시:
        //       · 2개 생성 (양쪽)
        //       · SkillManager의 twistRangeMultiplier만큼 반지름(사거리) 증가
        //       · 시작 각도 0도 / 180도 → 양쪽에서 일자처럼 보이도록 배치
        // ─────────────────────────────────────────────────────────────
        int spawnCount = 1;
        float[] startAngles = { 0f };  // 기본 한 개는 0도에서 시작
        float useRadius = orbitRadius; // 기본 반지름

        var sm = SkillManager.Instance;
        if (sm != null && sm.IsTwistOrTreatActive)
        {
            // 현재 1개 생성 → 세트 발동 시 2배 = 2개
            spawnCount = 2;
            startAngles = new float[] { 0f, 180f };  // 오른쪽 / 왼쪽

            // 범위(반지름) 25% 증가 (예: 1.6 * 1.25)
            useRadius *= sm.twistRangeMultiplier;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // 1) 플레이어 위치에 생성 + 플레이어의 자식으로 붙여서 추적 보장
            GameObject skillInstance = Instantiate(rollingChocolateBarPrefab, transform.position, Quaternion.identity);
            skillInstance.transform.SetParent(transform); // 플레이어 이동 따라감

            // 2) 공격 스크립트 초기화 및 궤도 회전 시작
            RollingChocolateBarAttack attackScript = skillInstance.GetComponent<RollingChocolateBarAttack>();
            if (attackScript != null)
            {
                // 두 번째 인자는 기존 1.5f를 orbitRadius 대신 사용하도록 변경
                attackScript.InitializeAttack(15, useRadius);

                // 시작 각도 결정
                float startAngle =
                    (i < startAngles.Length)
                    ? startAngles[i]
                    : i * (360f / spawnCount); // 예외적으로 배열 길이보다 많으면 균등 분배

                // direction은 기존과 동일하게 1 사용 (시계/반시계는 RollingChocolateBarAttack 쪽 로직을 따름)
                attackScript.BeginOrbit(transform, orbitDuration, startAngle, 1);
            }
            else
            {
                Debug.LogError("RollingChocolateBarPrefab is missing the RollingChocolateBarAttack script!");
            }
        }

        Debug.Log("Rolling Chocolate Bar Activated!");
    }
}
