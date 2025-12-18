using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;   //  Spine 애니메이션 컨트롤용

public class CaramelCubeSkill : MonoBehaviour
{
    private PlayerShooting shooting;
    private Player player;

    [Header("Caramel Cube Settings")]
    [Tooltip("스파인 SkeletonAnimation 이 달려 있는 카라멜 큐브 프리팹")]
    public GameObject caramelCubePrefab;

    [Tooltip("버프 지속 시간 (초)")]
    public float duration = 5f;        // 5초 동안 유지

    [Tooltip("버프 종료 후 다음 발동까지 쿨타임 (초)")]
    public float cooldown = 20f;       // 20초 쿨타임

    [Tooltip("공격 속도 배율 (2면 2배 빠르게)")]
    public float speedMultiplier = 2f; // 공격 속도 2배 (100% 증가)

    [Header("Visual Position")]
    [Tooltip("플레이어 기준 로컬 위치 오프셋")]
    public Vector3 visualOffset = Vector3.zero;

    [Header("Spine Visual")]
    [Tooltip("카라멜 큐브에서 재생할 Spine 애니메이션 이름")]
    public string spineAnimationName = "animation";   // 스파인에서 설정한 이름으로 변경

    [Tooltip("애니메이션 루프 여부")]
    public bool spineLoop = true;

    [Tooltip("스파인 애니메이션 재생 배속")]
    public float spineTimeScale = 1f;

    private bool isActive = false;
    private float originalFireRate;
    private GameObject activeVisual;          // 현재 살아 있는 카라멜 큐브 비주얼

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

        // 비주얼 생성 (Spine 프리팹)
        if (caramelCubePrefab != null)
        {
            activeVisual = Instantiate(
                caramelCubePrefab,
                transform.position,
                Quaternion.identity,
                transform   // 플레이어 밑으로 붙이기
            );

            // 플레이어 기준 위치 오프셋
            activeVisual.transform.localPosition = visualOffset;

            //  스파인 애니메이션 세팅
            var skeleton = activeVisual.GetComponent<SkeletonAnimation>();
            if (skeleton != null)
            {
                skeleton.Initialize(true);
                skeleton.timeScale = spineTimeScale;

                if (!string.IsNullOrEmpty(spineAnimationName))
                {
                    skeleton.AnimationState.SetAnimation(0, spineAnimationName, spineLoop);
                }
            }

            // 이펙트 등장 사운드
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.CaramelCube_SFX);
            }
        }

        // 공격 속도 증가
        if (shooting != null && speedMultiplier > 0f)
        {
            shooting.fireRate = originalFireRate / speedMultiplier;
        }
        Debug.Log("카라멜 큐브 발동! 공격 속도 " + speedMultiplier + "배!");

        // 버프 지속 시간
        yield return new WaitForSeconds(duration);

        // 기본 속도로 복귀
        if (shooting != null)
        {
            shooting.fireRate = originalFireRate;
        }
        Debug.Log("카라멜 큐브 종료. 기본 공격 속도로 복귀.");

        // 비주얼 삭제
        if (activeVisual != null)
        {
            Destroy(activeVisual);
            activeVisual = null;
        }

        // 쿨타임 대기
        yield return new WaitForSeconds(cooldown);

        isActive = false; // 다음 사이클 준비
    }
}
