using UnityEngine;
using System.Collections;
using Spine.Unity;

public class HyperCandyRush : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject comboVisualPrefab;
    private GameObject activeComboVisual = null;

    [Header("Spine Settings")]
    private SkeletonAnimation skeleton;
    public string RushAnimation = "animation";
    public float spineTimeScale = 1.0f;


    private Player player;
    private PlayerShooting shooting;
    private Coroutine movingAttackSpeedCoroutine;

    private const float MAX_MOVING_INCREASE = 0.30f;
    private const float INCREASE_PER_SECOND = 0.03f;


    void Start()
    {
        // 컴포넌트 참조 초기화
        player = GetComponent<Player>();
        shooting = GetComponent<PlayerShooting>();

        if (player == null || shooting == null)
        {
            Debug.LogError("HyperCandyRush: Player 또는 PlayerShooting 컴포넌트를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }
    }


    public void ActivateSetEffect()
    {
        // 비주얼 생성
        if (comboVisualPrefab != null)
        {
            activeComboVisual = Instantiate(comboVisualPrefab, transform.position, Quaternion.identity, transform);
            activeComboVisual.transform.localPosition = Vector3.zero;

            skeleton = activeComboVisual.GetComponentInChildren<SkeletonAnimation>(true);
        }
        else
        {
            Debug.LogError("HyperCandyRush: 비주얼 프리팹이 할당되지 않아 애니메이션이 실행되지 않습니다.");
        }

        // Spine 애니메이션 재생
        if (skeleton != null)   // 여나야 여기다!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            skeleton.timeScale = spineTimeScale;
            skeleton.AnimationState.SetAnimation(0, RushAnimation, false);   // 이게 스파인 한 번만 실행되게 해주는 코드야!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11
        }

        // HyperCandyRush의 핵심 로직 시작 (이동 기반 공격속도 체크)
        StartMovementCheck();

        Debug.Log("[HyperCandyRush] Set Effect Activated!");
    }

    public void StartMovementCheck()    // 루프를 시작하고 공격 속도 증가 효과를 초기화하는 메서드
    {
        StopMovementCheck();
        shooting?.SetMovingAttackSpeedIncrease(0f);

        movingAttackSpeedCoroutine = StartCoroutine(CheckPlayerMovementLoop());
        Debug.Log("[HyperCandyRush] Movement Check Loop 시작.");
    }

    public void StopMovementCheck()    // CheckPlayerMovementLoop 코루틴을 관리하고 공격 속도 증가 효과를 중단하거나 초기화하는 메서드
    {
        if (movingAttackSpeedCoroutine != null)
        {
            StopCoroutine(movingAttackSpeedCoroutine);
            movingAttackSpeedCoroutine = null;
        }
        shooting?.SetMovingAttackSpeedIncrease(0f);
        Debug.Log("[HyperCandyRush] Movement Check Loop 정지.");
    }

    private IEnumerator CheckPlayerMovementLoop()    // 플레이어의 이동 상태에 따라 공격속도를 다르게 해주는 코루틴
    {
        var wait = new WaitForSeconds(1.0f);
        float currentIncrease = 0f;

        while (player != null && shooting != null)
        {
            bool isMoving = player.inputVec.sqrMagnitude > 0.01f;

            if (isMoving)
            {
                if (currentIncrease < MAX_MOVING_INCREASE)
                {
                    currentIncrease = Mathf.Min(currentIncrease + INCREASE_PER_SECOND, MAX_MOVING_INCREASE);
                    shooting.SetMovingAttackSpeedIncrease(currentIncrease);
                }
            }
            else
            {
                if (currentIncrease > 0f)
                {
                    currentIncrease = 0f;
                    shooting.SetMovingAttackSpeedIncrease(currentIncrease);
                    Debug.Log("[HyperCandyRush] 플레이어 멈춤: 이동 공속 증가 효과 초기화 (0% 복원)");
                }
            }
            yield return wait;
        }
        StopMovementCheck();
    }

    
    void OnDestroy()
    {
        StopMovementCheck();
        if (activeComboVisual != null)
        {
            Destroy(activeComboVisual);
        }
    }
}