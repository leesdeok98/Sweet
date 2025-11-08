using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public Player player; // Inspector에서 Player 연결

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Player가 연결 안 되어 있으면 자동 할당
        if (player == null)
        {
            player = GameObject.FindObjectOfType<Player>();
        }
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
                // 다른 아이템은 필요 시 구현
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
            //player.hasSugarShield = true;
        }
    }
}