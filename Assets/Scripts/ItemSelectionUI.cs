using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;
    public Button[] itemButtons;
    public Image[] itemIcons;
    public Text[] itemLabels;

    [Header("Items")]
    public List<ItemData> itemPool = new List<ItemData>(); // 전체 아이템 목록
    private List<ItemData> currentChoices = new List<ItemData>(); // 현재 표시 중인 3개
    private HashSet<ItemData> acquiredItems = new HashSet<ItemData>(); // 이미 선택된 아이템 저장

    private bool isOpen = false;

    void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        // 버튼 이벤트 설정
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int idx = i;
            itemButtons[i].onClick.RemoveAllListeners();
            itemButtons[i].onClick.AddListener(() => OnClickItem(idx));
        }

        Debug.Log("[ItemSelectionUI] 초기화 완료. 버튼 이벤트 등록 완료 ✅");
    }

    public void Open()
    {
        Debug.Log("[ItemSelectionUI] Open() 호출됨");
        if (isOpen)
        {
            Debug.LogWarning("[ItemSelectionUI] 이미 열려 있음 ❌");
            return;
        }

        isOpen = true;
        Time.timeScale = 0f;

        currentChoices.Clear();
        List<int> used = new List<int>();
        System.Random r = new System.Random();

        // 🔸 획득하지 않은 아이템만 필터링
        List<ItemData> availableItems = itemPool.FindAll(item => !acquiredItems.Contains(item));
        Debug.Log($"[ItemSelectionUI] 선택 가능한 아이템 수: {availableItems.Count}");

        int choices = Mathf.Min(itemButtons.Length, availableItems.Count);

        for (int i = 0; i < choices; i++)
        {
            int idx;
            do { idx = r.Next(0, availableItems.Count); } while (used.Contains(idx));
            used.Add(idx);
            currentChoices.Add(availableItems[idx]);
        }

        Debug.Log($"[ItemSelectionUI] currentChoices.Count = {currentChoices.Count}");

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
        Debug.Log("[ItemSelectionUI] 패널 활성화 완료 ✅");
    }

    void OnClickItem(int buttonIndex)
    {
        Debug.Log($"[ItemSelectionUI] OnClickItem 실행됨 / isOpen={isOpen}, index={buttonIndex}");

        if (!isOpen)
        {
            Debug.LogWarning("[ItemSelectionUI] 패널이 닫혀 있어서 클릭 무시됨 ❌");
            return;
        }
        if (buttonIndex < 0 || buttonIndex >= currentChoices.Count)
        {
            Debug.LogWarning("[ItemSelectionUI] currentChoices 범위 벗어남 ❌");
            return;
        }

        ItemData chosen = currentChoices[buttonIndex];
        Debug.Log($"[ItemSelectionUI] 선택된 아이템: {chosen.itemName}");

        // 🔸 선택된 아이템을 획득 목록에 추가
        acquiredItems.Add(chosen);

        if (SkillManager.Instance == null)
        {
            Debug.LogError("[ItemSelectionUI] SkillManager.Instance가 없음 ❌");
        }
        else
        {
            Debug.Log("[ItemSelectionUI] SkillManager Apply 호출 시도");
            SkillManager.Instance.Apply(chosen);
        }

        Close();
    }

    public void Close()
    {
        Debug.Log("[ItemSelectionUI] Close() 호출됨");
        if (!isOpen) return;

        isOpen = false;
        if (panelRoot != null) panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    void CreateSampleItems()
    {
        Debug.Log("[ItemSelectionUI] 샘플 아이템 생성 호출됨");
        // (임시용 아이템 생성 코드 - 필요시 여기에 추가)
    }
}
