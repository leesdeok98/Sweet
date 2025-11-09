using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public Player player; // Inspector에서 Player 연결
    private GameObject darkChipEffectInstance; // 다크칩 이펙트 오브젝트 저장용
    private bool darkChipApplied = false;      // 중복 방지용

    [Header("Skill Prefabs")]
    public GameObject darkChipEffectPrefab; // 다크칩 이펙트 프리팹 (Inspector에서 연결)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (player == null)
            player = FindObjectOfType<Player>();
    }

    public void Apply(ItemData item)
    {
        if (item == null) return;

        switch (item.type)
        {
            case ItemData.ItemType.IcedJelly:
                ApplyIcedJelly(item.value, 3f);
                break;

            case ItemData.ItemType.SugarShield:
                ApplySugarShield(item.value, 3f);
                break;

            case ItemData.ItemType.DarkChip:
                ApplyDarkChip(0.3f); // 30% 증가
                break;
        }
    }

    void ApplyIcedJelly(float damage, float duration)
    {
        Debug.Log("아이스젤리 획득!");
        if (player != null)
        {
            player.hasIcedJellySkill = true;
        }
    }

    void ApplySugarShield(float damage, float duration)
    {
        Debug.Log("슈가쉴드 획득!");
        if (player != null)
        {
            player.hasSugarShield = true;
        }
    }

    void ApplyDarkChip(float percent)
    {
        if (darkChipApplied) return;
        darkChipApplied = true;

        Debug.Log($"다크칩 획득! 총알 공격력 +{percent * 100}%");

        // 🔹 플레이어에 다크칩 보유 상태 표시
        if (player != null)
            player.hasDarkChip = true;

        // 🔹 전역 데미지 배율 증가
        Bullet.damageMultiplier = 1f + percent;

        // 🔹 프리팹 효과 생성
        if (darkChipEffectPrefab != null && player != null)
        {
            darkChipEffectInstance = Instantiate(
                darkChipEffectPrefab,
                player.transform.position,
                Quaternion.identity,
                player.transform
            );
        }
    }

    void ApplyRollingChocolateBar()
    {
        Debug.Log("롤링 초코바 획득!");
        if (player != null)
        {
            player.hasRollingChocolateBar = true;
        }
    }
}
