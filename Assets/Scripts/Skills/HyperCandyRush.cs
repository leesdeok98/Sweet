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

    // 세트 효과가 이미 한 번 발동되었는지 여부 (중복 발동/이펙트 방지)
    private bool hasPlayedOnce = false;

    //  타임스케일 0일 때 FX/사운드를 나중에 재생하기 위한 플래그
    private bool fxPending = false;

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
        // ★ 이미 세트 효과가 발동된 상태라면 다시 실행하지 않음 (FX 1회만 표시)
        if (hasPlayedOnce)
        {
            Debug.Log("[HyperCandyRush] 이미 세트 효과가 발동된 상태입니다. 다시 실행하지 않습니다.");
            return;
        }
        hasPlayedOnce = true;

        // 비주얼 생성
        if (comboVisualPrefab != null)
        {
            activeComboVisual = Instantiate(comboVisualPrefab, transform.position, Quaternion.identity, transform);
            activeComboVisual.transform.localPosition = Vector3.zero;

            skeleton = activeComboVisual.GetComponentInChildren<SkeletonAnimation>(true);
            if (skeleton != null)
            {
                skeleton.timeScale = spineTimeScale;
            }
        }
        else
        {
            Debug.LogError("HyperCandyRush: 비주얼 프리팹이 할당되지 않아 애니메이션이 실행되지 않습니다.");
        }

        // HyperCandyRush의 핵심 로직 시작 (이동 기반 공격속도 체크)
        StartMovementCheck();

        //  타임스케일 0이면 FX/사운드는 나중에 재생
        if (Time.timeScale == 0f)
        {
            fxPending = true;
        }
        else
        {
            PlayRushFx();
        }

        Debug.Log("[HyperCandyRush] Set Effect Activated!");
    }

    // ★ 실제 Spine 애니 + 사운드 재생 함수
    private void PlayRushFx()
    {
        if (skeleton != null)
        {
            skeleton.timeScale = spineTimeScale;
            skeleton.AnimationState.SetAnimation(0, RushAnimation, false);
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.HyperCandyRush_SFX);
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

    void Update()
    {
        // 인벤토리에서 세트가 완성된 뒤, 게임이 다시 진행되면 FX/사운드를 딱 한 번 재생
        if (fxPending && Time.timeScale > 0f)
        {
            fxPending = false;
            PlayRushFx();
        }
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
