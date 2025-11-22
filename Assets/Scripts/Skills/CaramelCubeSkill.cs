using System.Collections;
using UnityEngine;

public class CaramelCubeSkill : MonoBehaviour
{
    private PlayerShooting shooting;
    private Player player;

    [Header("Caramel Cube Settings")]
    public float duration = 5f;        // 5초 동안 유지
    public float cooldown = 20f;       // 20초 쿨타임
    public float speedMultiplier = 2f; // 공격 속도 2배 (100% 증가)

    private bool isActive = false;
    private float originalFireRate;

    void Start()
    {
        player = GetComponent<Player>();
        shooting = GetComponent<PlayerShooting>();

        if (shooting != null)
            originalFireRate = shooting.fireRate;
    }

    void Update()
    {
        // 카라멜큐브 아이템이 활성화되어 있고, 아직 코루틴이 돌고 있지 않다면 실행
        if (player != null && player.hasCaramelCube && !isActive)
        {
            StartCoroutine(CaramelCubeRoutine());
        }
    }

    IEnumerator CaramelCubeRoutine()
    {
        isActive = true;

        // 공격 속도 증가
        shooting.fireRate = originalFireRate / speedMultiplier;
        Debug.Log("카라멜 큐브 발동! 공격 속도 2배!");

        yield return new WaitForSeconds(duration);

        // 기본 속도로 복귀
        shooting.fireRate = originalFireRate;
        Debug.Log("카라멜 큐브 종료. 기본 공격 속도로 복귀.");

        // 3쿨타임 대기
        yield return new WaitForSeconds(cooldown);

        isActive = false; // 다음 사이클 준비
    }
}
