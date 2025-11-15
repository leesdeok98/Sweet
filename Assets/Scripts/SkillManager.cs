// SkillManager.cs
using System.Collections;
using UnityEngine;
// ★ Spine 네임스페이스는 기존 그대로…
using Spine;
using Spine.Unity;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("References")]
    public Player player;

    [Header("Popping Candy Settings")]
    [Tooltip("원형(투명) 콜라이더 반지름")]
    public float poppingColliderRadius = 0.6f;

    [Tooltip("한 조각이 이동할 최대 거리(사거리)")]
    public float poppingRange = 6f;

    [Tooltip("바깥으로 퍼져나가는 속도")]
    public float poppingShardSpeed = 11f;

    [Tooltip("총알 최종 데미지 대비 배율 (예: 0.8 = 80%)")]
    [Range(0f, 3f)] public float poppingDamageFactor = 0.8f;
    // SkillManager.cs (일부 추가)
    [Header("Popping Candy Spine FX")]
    public GameObject poppingSpinePrefab;     // ★ SkeletonAnimation + SpineOneShot 붙은 프리팹
    public string poppingSpineAnim = "burst"; // 프리팹 기본값 덮어쓰기 원하면 사용
    public bool poppingSpineLoop = false;
    public Vector3 poppingSpineOffset = Vector3.zero;
    public int poppingSpineSortingOrder = 220; // 화면 위에 보이도록

    // (나머지 기존 필드/Spine 관련 설정은 생략 없이 유지)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (player == null) player = FindObjectOfType<Player>();
    }

    public void Apply(ItemData item)
    {
        if (item == null) return;

        switch (item.type)
        {
            case ItemData.ItemType.IcedJelly:
                ApplyIcedJelly(item.value, 3f); break;
            case ItemData.ItemType.SugarShield:
                ApplySugarShield(item.value, 3f); break;
            case ItemData.ItemType.DarkChip:
                ApplyDarkChip(0.3f); break;
            case ItemData.ItemType.RollingChocolateBar:
                ApplyRollingChocolateBar(); break;
            case ItemData.ItemType.PoppingCandy:
                ApplyPoppingCandy(); break;
        }
    }

    void ApplyIcedJelly(float damage, float duration)
    {
        Debug.Log("아이스젤리 획득!");
        if (player != null) player.hasIcedJellySkill = true;
    }

    void ApplySugarShield(float value, float duration)
    {
        Debug.Log("슈가쉴드 획득!");
        if (player != null) player.hasSugarShield = true;
    }

    void ApplyRollingChocolateBar()
    {
        Debug.Log("롤링 초코바 획득!");
        if (player != null) player.hasRollingChocolateBar = true;
    }

    void ApplyPoppingCandy()
    {
        Debug.Log("팝핑 캔디 획득!");
        if (player != null) player.hasPoppingCandy = true; // ✅ 버그 수정
    }

    // ─ 다크칩 등 기존 구현은 그대로 유지 ─
    void ApplyDarkChip(float percent)
    {
        // (네 기존 코드 유지)
    }
}
