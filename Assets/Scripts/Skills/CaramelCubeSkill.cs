using System.Collections;
using UnityEngine;

public class CaramelCubeSkill : MonoBehaviour
{
    private PlayerShooting shooting;
    private Player player;

    [Header("Caramel Cube Settings")]
    public GameObject caramelCubePrefab; 
    public float duration = 5f;        // 5초 동안 유지
    public float cooldown = 20f;       // 20초 쿨타임
    public float speedMultiplier = 2f; // 공격 속도 2배 (100% 증가)

    [Header("Visual Position")]
    [Tooltip("플레이어 기준 로컬 위치 오프셋")]
    public Vector3 visualOffset = Vector3.zero;

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

        // 비주얼 생성
        if (caramelCubePrefab != null)
        {
            GameObject visual = Instantiate(caramelCubePrefab, transform.position, Quaternion.identity, transform);
            visual.transform.localPosition = visualOffset;

            // 효과가 발생하기 시작할 때 사운드가 난다고 해서 프리팹 생기고 소리 나는게 가장 자연스러울 거 같아서 여기!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

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
