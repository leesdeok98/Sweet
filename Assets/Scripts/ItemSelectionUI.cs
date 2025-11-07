using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;      // 아이템 선택창 전체 패널 (활성/비활성 제어)
    public Button[] itemButtons;      // 아이템 선택 버튼 배열
    public Image[] itemIcons;         // 버튼 위의 아이템 아이콘 이미지
    public Text[] itemLabels;         // 버튼 위의 아이템 이름 텍스트
    //public Text titleText;          // 제목 텍스트 (현재 사용 안 함)

    [Header("Items (example)")]
    public List<ItemData> itemPool = new List<ItemData>(); // 선택 가능한 아이템 풀

    private List<ItemData> currentChoices = new List<ItemData>(); // 현재 선택창에 표시 중인 아이템 3개
    private bool isOpen = false; // 선택창 열림 여부

    void Start()
    {
        // 시작 시 패널 비활성화
        if (panelRoot != null) panelRoot.SetActive(false);

        // 아이템 풀 비어 있으면 샘플 아이템 자동 생성
        if (itemPool.Count == 0)
        {
            CreateSampleItems();
        }

        // 버튼 클릭 이벤트 초기화 및 등록
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int idx = i;
            itemButtons[i].onClick.RemoveAllListeners();
            itemButtons[i].onClick.AddListener(() => OnClickItem(idx));
        }
    }

    /// <summary>
    /// 샘플 아이템 데이터를 생성 (테스트용)
    /// </summary>
    void CreateSampleItems()
    {
        itemPool = new List<ItemData>();

        itemPool.Add(new ItemData
        {
            itemName = "카라멜 큐브",
            description = "기본 이동 속도 +100%, 5초간 지속 (쿨타임 15초)",
            type = ItemData.ItemType.CaramelCube,
            value = 1.0f
        });

        itemPool.Add(new ItemData
        {
            itemName = "설탕 방패",
            description = "자동 방어막 생성 (20초마다 1회, 최대 1회 중첩)",
            type = ItemData.ItemType.SugarShield,
            value = 50f
        });

        itemPool.Add(new ItemData
        {
            itemName = "딸기팝 코어",
            description = "기본 공격 시 30% 확률로 추가 투사체 발사",
            type = ItemData.ItemType.StrawberryPopCore,
            value = 1.3f
        });

        itemPool.Add(new ItemData
        {
            itemName = "굴러가는 초코바",
            description = "5초마다 초코바 소환, 2m 범위의 적 타격 (공격력 15)",
            type = ItemData.ItemType.RollingChocolateBar,
            value = 15f
        });

        itemPool.Add(new ItemData
        {
            itemName = "코코아 가루",
            description = "기본 공격이 범위 공격으로 변경, 반경 0.2 증가",
            type = ItemData.ItemType.CocoaPowder,
            value = 5f
        });

        itemPool.Add(new ItemData
        {
            itemName = "눈송이 사탕",
            description = "기본 공격 시 15% 확률로 적을 1초간 얼림",
            type = ItemData.ItemType.SnowflakeCandy,
            value = 0.15f
        });

        itemPool.Add(new ItemData
        {
            itemName = "팝핑 캔디",
            description = "20% 확률로 공격 시 8방향 폭발 생성 (피해 2)",
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
            description = "10% 확률로 적 주위 얼음 폭발 발생 (범위 1.5m, 피해 2)",
            type = ItemData.ItemType.IcedJelly,
            value = 2f
        });

        itemPool.Add(new ItemData
        {
            itemName = "시럽 토네이도",
            description = "공격 4회마다 시럽 회오리 소환 (범위 2m)",
            type = ItemData.ItemType.SyrupTornado,
            value = 4f
        });

        itemPool.Add(new ItemData
        {
            itemName = "허니 스핀",
            description = "회전 공격 2회 추가 (피해 5), 타격 후 1초간 이동속도 -30%",
            type = ItemData.ItemType.HoneySpin,
            value = 5f
        });

        itemPool.Add(new ItemData
        {
            itemName = "설탕 죽",
            description = "25% 확률로 공격 시 1m 범위 슬로우 웨이브 생성 (지속 12초)",
            type = ItemData.ItemType.SugarPorridge,
            value = 12f
        });
    }

    /// <summary>
    /// 아이템 선택창 열기
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // 게임 일시정지
        Time.timeScale = 0f;

        // 랜덤으로 3개 아이템 선택
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

        // UI 갱신
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                itemButtons[i].gameObject.SetActive(true);
                if (itemIcons != null && i < itemIcons.Length)
                    itemIcons[i].sprite = currentChoices[i].icon;
                if (itemLabels != null && i < itemLabels.Length)
                    itemLabels[i].text = currentChoices[i].itemName;
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }

        if (panelRoot != null) panelRoot.SetActive(true);
    }

    /// <summary>
    /// 아이템 선택창 닫기
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (panelRoot != null) panelRoot.SetActive(false);

        // 게임 재개
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 아이템 선택 버튼 클릭 시 호출
    /// </summary>
    void OnClickItem(int buttonIndex)
    {
        if (!isOpen) return;
        if (buttonIndex < 0 || buttonIndex >= currentChoices.Count) return;

        ItemData chosen = currentChoices[buttonIndex];
        Debug.Log($"선택된 아이템: {chosen.itemName}");

        // 선택된 아이템 효과 적용
        SkillManager.Instance.Apply(chosen);

        // 창 닫기 및 시간 재개
        Close();
    }
}
