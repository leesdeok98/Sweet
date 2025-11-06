using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;      // 전체 패널 (활성/비활성)
    public Button[] itemButtons;      // 버튼 배열 (Inspector에서 버튼들을 넣어둔다)
    public Image[] itemIcons;         // 버튼 위 아이콘들 (옵션)
    public Text[] itemLabels;         // 버튼 위 텍스트들 (옵션)
    //public Text titleText;

    [Header("Items (example)")]
    public List<ItemData> itemPool = new List<ItemData>(); // inspector로 채우거나 코드에서 채움

    private List<ItemData> currentChoices = new List<ItemData>();
    private bool isOpen = false;

    void Start()
    {
        // 처음에 패널을 숨김
        if (panelRoot != null) panelRoot.SetActive(false);

        // 예시 아이템이 비어있으면 기본 샘플 3개 생성 (Inspector에 아이콘 넣지 않았다면 null)
        if (itemPool.Count == 0)
        {
            CreateSampleItems();
        }

        // 버튼에 리스너 초기화
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int idx = i;
            itemButtons[i].onClick.RemoveAllListeners();
            itemButtons[i].onClick.AddListener(() => OnClickItem(idx));
        }
    }

    void CreateSampleItems()
    {
        itemPool = new List<ItemData>();

        itemPool.Add(new ItemData
        {
            itemName = "카라멜 큐브",
            description = "기본 공격 속도 +100%, 5초간 유지 (재사용 대기 15초)",
            type = ItemData.ItemType.CaramelCube,
            value = 1.0f
        });

        itemPool.Add(new ItemData
        {
            itemName = "슈가 실드",
            description = "자동 방어막 생성 (20초마다 1회, 최대 1회 저장)",
            type = ItemData.ItemType.SugarShield,
            value = 50f
        });

        itemPool.Add(new ItemData
        {
            itemName = "딸기 팝코어",
            description = "기본 공격 시 30% 확률로 사방으로 탄 추가 발사",
            type = ItemData.ItemType.StrawberryPopCore,
            value = 1.3f
        });

        itemPool.Add(new ItemData
        {
            itemName = "롤링 초코바",
            description = "5초마다 긴 초코바가 반지름 2m의 원형태를 그리고 사라짐 (데미지 15, 작은 넉백)",
            type = ItemData.ItemType.RollingChocolateBar,
            value = 15f
        });

        itemPool.Add(new ItemData
        {
            itemName = "코코아 파우더",
            description = "기본 공격 색이 갈색으로 바뀌며 작은 넉백과 0.2초 경직 추가",
            type = ItemData.ItemType.CocoaPowder,
            value = 5f
        });

        itemPool.Add(new ItemData
        {
            itemName = "눈꽃 사탕",
            description = "기본 공격 시 15% 확률로 적을 1초간 빙결",
            type = ItemData.ItemType.SnowflakeCandy,
            value = 0.15f
        });

        itemPool.Add(new ItemData
        {
            itemName = "팝핑 캔디",
            description = "20% 확률로 공격 명중 시 8개의 별사탕 조각이 반경 2m로 퍼짐 (개당 데미지 2)",
            type = ItemData.ItemType.PoppingCandy,
            value = 2f
        });

        itemPool.Add(new ItemData
        {
            itemName = "다크 칩",
            description = "공격력 +30%",
            type = ItemData.ItemType.DarkChip,
            value = 1.3f
        });

        itemPool.Add(new ItemData
        {
            itemName = "아이스 젤리",
            description = "10% 확률로 주변 적에게 냉기 확산 (범위 1.5m, 초당 데미지 2, 3초 지속, 속도 -50%)",
            type = ItemData.ItemType.IcedJelly,
            value = 2f
        });

        itemPool.Add(new ItemData
        {
            itemName = "시럽 토네이도",
            description = "매초 4 데미지를 주는 반경 2m의 지속 시럽 생성",
            type = ItemData.ItemType.SyrupTornado,
            value = 4f
        });

        itemPool.Add(new ItemData
        {
            itemName = "허니 스핀",
            description = "회전하는 꿀 2개 생성 (개당 데미지 5), 타격 시 1초간 이동속도 -30%",
            type = ItemData.ItemType.HoneySpin,
            value = 5f
        });

        itemPool.Add(new ItemData
        {
            itemName = "설탕 폭죽",
            description = "25% 확률로 반경 1m 폭발을 일으키는 폭죽 추가 발사 (데미지 12)",
            type = ItemData.ItemType.SugarPorridge,
            value = 12f
        });
    }


    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // 시간 일시정지 (게임 로직 멈춤)
        Time.timeScale = 0f;

        // 랜덤 또는 순서대로 아이템 3개 선택
        currentChoices.Clear();
        int choices = Mathf.Min(itemButtons.Length, itemPool.Count);
        List<int> used = new List<int>();
        System.Random r = new System.Random();
        for (int i = 0; i < choices; i++)
        {
            int idx;
            do { idx = r.Next(0, itemPool.Count); } while (used.Contains(idx));
            used.Add(idx);
            currentChoices.Add(itemPool[idx]);
        }

        // UI에 반영
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                itemButtons[i].gameObject.SetActive(true);
                if (itemIcons != null && i < itemIcons.Length) itemIcons[i].sprite = currentChoices[i].icon;
                if (itemLabels != null && i < itemLabels.Length) itemLabels[i].text = currentChoices[i].itemName;
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }

        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (panelRoot != null) panelRoot.SetActive(false);

        // 게임 재개
        Time.timeScale = 1f;
    }

    void OnClickItem(int buttonIndex)
    {
        if (!isOpen) return;
        if (buttonIndex < 0 || buttonIndex >= currentChoices.Count) return;

        ItemData chosen = currentChoices[buttonIndex];
        Debug.Log($"선택한 아이템: {chosen.itemName}");

        // 효과 적용 (SkillManager 호출)
        SkillManager.Instance.Apply(chosen);

        // 닫기 및 재개
        Close();
    }
}
