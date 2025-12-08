using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SugarShieldSkill : MonoBehaviour
{
    [Header("실드 프리팹")]
    public GameObject shieldPrefab1;
    public GameObject shieldPrefab2;

    [Tooltip("실드 생성 주기")]
    public float generationInterval = 20f;

    // 내부 관리용
    [Header("Current State")]
    [SerializeField] private GameObject activeShield1 = null;
    [SerializeField] private GameObject activeShield2 = null;

    private Coroutine generationCoroutine;
    private bool isGenerating = false;


    public int CurrentShieldCount
    {
        get
        {
            int count = 0;
            if (activeShield1 != null) count++;
            if (activeShield2 != null) count++;
            return count;
        }
    }



    public void StartSugarShieldGeneration()
    {
        if (isGenerating) return;
        isGenerating = true;

        // 아이템 획득 즉시 실드1 생성 시도
        GenerateShield1Visual();

        // 쿨타임 20초 코루틴 실행
        generationCoroutine = StartCoroutine(ShieldGenerationRoutine());
        Debug.Log("슈가 실드 관리 시작. 실드 1 즉시 생성 및 20초 고정 타이머 시작.");
    }


    IEnumerator ShieldGenerationRoutine()  // 실드 생성 주기 관리하는 코루틴
    {
        while (isGenerating)
        {
            // 쿨타임 20초 (생성된 시간 기준으로)
            yield return new WaitForSeconds(generationInterval);

            // 실드1이 없으면 실드1 생성
            if (activeShield1 == null)
            {
                GenerateShield1Visual();
                Debug.Log("타이머 만료: [Shield 1] 재생성 시도.");
            }
            // 실드1 존재, 실드2 존재X  ->  실드2 생성 시도
            else if (activeShield2 == null)
            {
                GenerateShield2Visual();
                Debug.Log("타이머 만료: [Shield 2] 재생성 시도 (S1 존재 확인).");
            }
            else
            {
                Debug.Log("실드 최대 개수 도달. 재생성 스킵.");
            }
        }
    }

    private void GenerateShield1Visual()     // 실드1 생성 메서드
    {
        if (activeShield1 == null && shieldPrefab1 != null)
        {
            // Player의 자식으로 생성
            activeShield1 = Instantiate(shieldPrefab1, transform.position, Quaternion.identity, transform);
            activeShield1.transform.localPosition = Vector3.zero;

           AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarShield_Generate_SFX);
        }
    }

    private void GenerateShield2Visual()    // 실드2 생성 메서드
    {
        if (activeShield2 == null && shieldPrefab2 != null)
        {
            // Player의 자식으로 생성
            activeShield2 = Instantiate(shieldPrefab2, transform.position, Quaternion.identity, transform);
            activeShield2.transform.localPosition = Vector3.zero;

            AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarShield_Generate_SFX);
        }
    }

    public bool ConsumeShield()   // 실드2가 먼저 사라지고 그 뒤에 실드1이 사라짐
    {
        if (activeShield2 != null)
        {
            Destroy(activeShield2);
            activeShield2 = null;
            Debug.Log($"[Shield 2] 소모! (Player 본체 충돌 흡수)");
            return true;
        }

        if (activeShield1 != null)
        {
            Destroy(activeShield1);
            activeShield1 = null;
            Debug.Log($"[Shield 1] 소모! (Player 본체 충돌 흡수)");
            return true;
        }
        return false;
    }

    public bool ConsumeShieldByVisual(GameObject visualObject)
    {
        // 실드2에 충돌했다면
        if (activeShield2 == visualObject)
        {
            Destroy(activeShield2);
            activeShield2 = null;
            Debug.Log($"[Shield 2] 소모! (Visual 직접 충돌)");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarShield_Deflect_SFX);
            return true;
        }

        // 실드1에 충돌했다면
        if (activeShield1 == visualObject)
        {
            Destroy(activeShield1);
            activeShield1 = null;
            Debug.Log($"[Shield 1] 소모! (Visual 직접 충돌)");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.SugarShield_Deflect_SFX);
            return true;
        }

        return false;
    }

    public void AutoCastUpgradeShield()     // 스위트 아머 콤보 효과용 (실드1, 실드2 한 번에 생기게 하는 메서드)
    {
        // 실드 1과 실드 2를 모두 생성 시도
        GenerateShield1Visual();
        GenerateShield2Visual();

        Debug.Log("업그레이드 실드: S1과 S2 동시 생성 완료!");

    }

    // ★ 슈가 쉴드 생성 중지 + 기존 쉴드 정리
    public void StopSugarShieldGeneration(bool clearExistingShields)
    {
        // 더 이상 자동 생성 안 함
        isGenerating = false;

        // 코루틴 돌고 있으면 중지
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
            generationCoroutine = null;
        }

        // 필요하면 현재 떠 있는 쉴드도 같이 제거
        if (clearExistingShields)
        {
            if (activeShield1 != null)
            {
                Destroy(activeShield1);
                activeShield1 = null;
            }

            if (activeShield2 != null)
            {
                Destroy(activeShield2);
                activeShield2 = null;
            }
        }
    }

}