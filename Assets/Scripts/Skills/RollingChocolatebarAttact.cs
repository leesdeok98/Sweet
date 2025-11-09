using UnityEngine;

// 플레이어에 부착
public class RollingChocolateBarSkill : MonoBehaviour
{
    // 롤링 초코바 공격 프리팹 (Inspector에서 연결)
    [Header("Skill Settings")]
    public GameObject rollingChocolateBarPrefab;
    public float cooldown = 5f; // 쿨타임 5초
    public float skillDuration = 20f; // 롤링 초코바 애니메이션이 재생되는 시간 (필요에 따라 조절) // 아니 이거 원래 1f 였거든 근데 중간에 재생 되다가 끊기길래 20f까지 늘렸는데도 안됨

    private float timer; // 쿨타임 타이머
    private Player player; // Player 컴포넌트 참조

    void Awake()
    {
        player = GetComponent<Player>();
        timer = cooldown; // 게임 시작 시 바로 실행되도록 초기화
    }

    void Update()
    {
        // Player 스크립트의 bool 변수가 true일 때만 스킬 실행 로직 작동
        if (player != null && player.hasRollingChocolateBar)
        {
            timer += Time.deltaTime;

            if (timer >= cooldown)
            {
                ActivateSkill();
                timer = 0f; // 타이머 리셋
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

        // 1. 플레이어 위치에 프리팹 생성
        // 생성된 프리팹의 애니메이션이 원형태를 그리고 사라지는 역할을 담당합니다.
        GameObject skillInstance = Instantiate(rollingChocolateBarPrefab, transform.position, Quaternion.identity);


        // 2. 공격 스크립트에 정보 전달 및 초기화
        RollingChocolateBarAttack attackScript = skillInstance.GetComponent<RollingChocolateBarAttack>();
        if (attackScript != null)
        {
            // 스케일 1,1,1로 설정 (크기는 프리팹 자체에서 조절)
            //skillInstance.transform.localScale = Vector3.one;

            // Player를 부모로 설정하여 Player를 따라다니도록 설정 (선택 사항, 애니메이션에 따라 필요 없을 수 있음)
            skillInstance.transform.SetParent(transform);

            // 데미지 및 넉백 설정 초기화
            attackScript.InitializeAttack(15, 5f); // 15 데미지, 넉백 힘 5f (조절 가능)
        }
        else
        {
            Debug.LogError("RollingChocolateBarPrefab is missing the RollingChocolateBarAttack script!");
        }

        // 3. 스킬 지속 시간 후 프리팹 파괴 로직 추가
        Destroy(skillInstance, skillDuration); // skillDuration 시간 후에 생성된 오브젝트를 파괴합니다.

        Debug.Log("Rolling Chocolate Bar Activated!");
    }
}

