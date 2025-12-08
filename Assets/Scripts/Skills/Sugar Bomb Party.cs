using UnityEngine;
using System.Collections;
using Spine.Unity;

public class SugarBombParty : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject comboVisualPrefab; // SugarBombParty 비주얼 프리팹
    private GameObject activeComboVisual = null;

    [Header("Spine Settings")]
    private SkeletonAnimation skeleton;
    public string PartyAnimation = "animation"; // 세트 효과 발동 시 단발 애니메이션
    public float spineTimeScale = 1.0f;

    private Player player;
    private PlayerShooting shooting;

    //  세트 효과를 이미 한 번 발동했는지 여부 (중복 이펙트 방지용)
    private bool hasPlayedOnce = false;

    void Start()
    {
        // 컴포넌트 참조 초기화
        player = GetComponent<Player>();
        shooting = GetComponent<PlayerShooting>();

        if (player == null || shooting == null)
        {
            Debug.LogError("SugarBombParty: Player 또는 PlayerShooting 컴포넌트를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }
    }

    public void ActivateSetEffect()
    {
        //  이미 한 번 발동했다면 다시 실행하지 않음 (세트 이펙트 1회만 표시)
        if (hasPlayedOnce)
        {
            Debug.Log("[SugarBombParty] 이미 세트 효과가 발동된 상태입니다. 다시 실행하지 않습니다.");
            return;
        }
        hasPlayedOnce = true;

        // 비주얼 생성 및 Spine 애니메이션 재생 (다른 세트 효과와 동일)
        if (comboVisualPrefab != null)
        {
            activeComboVisual = Instantiate(comboVisualPrefab, transform.position, Quaternion.identity, transform);
            activeComboVisual.transform.localPosition = Vector3.zero;

            skeleton = activeComboVisual.GetComponentInChildren<SkeletonAnimation>(true);
        }
        else
        {
            Debug.LogWarning("SugarBombParty: 비주얼 프리팹이 할당되지 않아 애니메이션이 실행되지 않습니다.");
        }

        if (skeleton != null)
        {
            skeleton.timeScale = spineTimeScale;
            skeleton.AnimationState.SetAnimation(0, PartyAnimation, false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarBombParty_SFX);
        }

        // PlayerShooting에 폭발 반경 증가 및 미니 폭죽 발사 활성화 요청
        if (shooting != null)
        {
            shooting.ActivateSugarBombParty(true);
        }

        if (player != null)
        {
            player.hasSugarBombParty = true;
        }

        Debug.Log("[SugarBombParty] Set Effect Activated! (Porridge Radius +30%)");
    }

    public void DeactivateSetEffect()
    {
        // 세트 효과 해제 시 초기화
        if (shooting != null)
        {
            shooting.ActivateSugarBombParty(false);
        }

        if (player != null)
        {
            player.hasSugarBombParty = false;
        }

        if (activeComboVisual != null)
        {
            Destroy(activeComboVisual);
            activeComboVisual = null;
        }

        Destroy(this); // 스크립트 자체 파괴
        Debug.Log("[SugarBombParty] Set Effect Deactivated.");
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시에도 해제 로직 호출
        DeactivateSetEffect();
    }
}
