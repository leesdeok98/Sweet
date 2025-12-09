using UnityEngine;
using System.Collections;
using Spine.Unity;
using System.Xml.Serialization;

public class SweetarmorCombo : MonoBehaviour
{
    private Player player;
    private SugarShieldSkill shieldSkill;


    [Header("Visuals")]
    public GameObject comboVisualPrefab;
    private GameObject activeComboVisual = null;

    // 실드 자동 발동 관련 변수
    private bool isShieldCooldown = false;
    private const float SHIELD_AUTOCAST_COOLDOWN = 20f;
    private const float HP_THRESHOLD = 30f;

    // 회복 관련 변수
    private const float HEAL_AMOUNT = 3f;
    private const float HEAL_INTERVAL = 5f;

    // 스파인 관련 변수
    public SkeletonAnimation skeleton;
    public string ShieldAnimation = "animation";
    public float spineTimeScale = 1.0f;

    //  세트 효과가 이미 한 번 발동되었는지 여부 (중복 발동/이펙트 방지)
    private bool comboActivatedOnce = false;

    //  타임스케일 0일 때 FX/사운드를 나중에 재생하기 위한 플래그
    private bool fxPending = false;

    void Start()
    {
        // 컴포넌트 할당만 수행
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("SweetarmorCombo: Player 컴포넌트를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        shieldSkill = player.GetComponent<SugarShieldSkill>();
        if (shieldSkill == null)
        {
            Debug.LogWarning("SweetarmorCombo: SugarShieldSkill 컴포넌트가 없어 실드 자동 발동 기능이 작동하지 않습니다.");
        }
    }

    public void ActivateComboEffect()
    {
        Debug.Log("[1] ActivateComboEffect 시작");

        // ★ 이미 한 번 발동했다면 다시 실행하지 않음
        if (comboActivatedOnce)
        {
            Debug.Log("[SweetarmorCombo] 이미 세트 효과가 발동된 상태입니다. 다시 실행하지 않습니다.");
            return;
        }
        comboActivatedOnce = true;

        // 비주얼 생성
        if (comboVisualPrefab != null)
        {
            activeComboVisual = Instantiate(comboVisualPrefab, transform.position, Quaternion.identity, transform);
            activeComboVisual.transform.localPosition = Vector3.zero;

            skeleton = activeComboVisual.GetComponentInChildren<SkeletonAnimation>();

            Debug.Log($"[2] Skeleton 참조 결과: {(skeleton != null ? "성공" : "실패")}");
        }
        else
        {
            Debug.LogError("SweetarmorCombo: 비주얼 프리팹이 할당되지 않았습니다. 애니메이션이 실행되지 않습니다.");
        }

        // Spine 레퍼런스만 세팅 (실제 애니/사운드는 타임스케일에 맞춰 따로 처리)
        if (skeleton != null)
        {
            skeleton.timeScale = spineTimeScale;
        }

        // ★ 타임스케일이 0이면 FX/사운드는 나중에 재생
        if (Time.timeScale == 0f)
        {
            fxPending = true;
        }
        else
        {
            PlayComboFx();
        }

        // 5초마다 체력 회복 코루틴 시작 (기존 기능 유지)
        StartCoroutine(HealRoutine());
    }

    //  실제 Spine 애니 + 사운드 재생 함수
    private void PlayComboFx()
    {
        if (skeleton != null)
        {
            Debug.Log("[3] 스파인 애니메이션 재생 명령");
            skeleton.timeScale = spineTimeScale;
            skeleton.AnimationState.SetAnimation(0, ShieldAnimation, false);
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.SweetArmorCombo_SFX);
    }

    void Update()
    {
        // 체력 조건 및 쿨타임 확인 후 실드 자동 발동
        if (!isShieldCooldown && player.health <= HP_THRESHOLD)
        {
            TryAutoCastShield();
        }

        //  인벤토리(타임스케일 0)에서 세트가 완성된 뒤,
        //    게임이 다시 진행되면 FX/사운드를 딱 한 번만 재생
        if (fxPending && Time.timeScale > 0f)
        {
            fxPending = false;
            PlayComboFx();
        }
    }

    IEnumerator HealRoutine()
    {
        // 5초마다 플레이어 회복
        while (true)
        {
            yield return new WaitForSeconds(HEAL_INTERVAL);
            player.Heal(HEAL_AMOUNT);
            Debug.Log($"SweetarmorCombo: 5초마다 HP {HEAL_AMOUNT} 회복");
        }
    }

    private void TryAutoCastShield()
    {
        //  실드 자동 발동 로직
        if (shieldSkill != null && shieldSkill.CurrentShieldCount < 2 && player.hasCaramelCube && player.hasCocoaPowder && player.hasSugarShield)
        {
            Debug.Log("SweetarmorCombo: 체력 30 이하, 업그레이드 실드 자동 발동!");

            shieldSkill.AutoCastUpgradeShield();

            StartCoroutine(ShieldCooldownRoutine());
        }
    }

    IEnumerator ShieldCooldownRoutine()
    {
        // 쿨타임 관리 로직
        isShieldCooldown = true;
        yield return new WaitForSeconds(SHIELD_AUTOCAST_COOLDOWN);
        isShieldCooldown = false;
        Debug.Log("SweetarmorCombo: 실드 자동 발동 쿨타임 종료.");
    }

    void OnDestroy()
    {
        if (activeComboVisual != null)
        {
            Destroy(activeComboVisual); // 비주얼 삭제해주기
        }
    }
}
