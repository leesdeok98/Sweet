using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// (ItemData.ItemType)과 (InventoryItemData)를 연결하는 '번역기' 클래스
/// </summary>
[System.Serializable]
public class TetrisItemMapping
{
    public ItemData.ItemType frameworkItemType; // (예: ItemData.ItemType.IcedJelly)
    public InventoryItemData tetrisItemData;    // (예: IcedJelly_TetrisItem.asset)
}


public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot; // (아이템 3개 선택창)
    public Button[] itemButtons;
    public Image[] itemIcons;
    public Text[] itemLabels;

    [Header("Linked Panels")]
    public GameObject tetrisInventoryPanel; // (Tetris_Inventory_Panel 연결)

    [Header("Items")]
    public List<ItemData> itemPool = new List<ItemData>();
    private List<ItemData> currentChoices = new List<ItemData>(); 
    private HashSet<ItemData> acquiredItems = new HashSet<ItemData>(); 

    private bool isOpen = false;

    // ★ [추가됨] ESC 설정 창의 상태를 외부에 알리는 플래그
    // (ESC 설정 창을 켜는 스크립트에서 이 값을 true/false로 설정해야 합니다.)
    [Header("State Control")]
    public bool IsEscMenuOpen = false; 

    [Header("Tetris Inventory Mapping")]
    public List<TetrisItemMapping> tetrisMappings = new List<TetrisItemMapping>();
    
    void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false); 

        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i; 
            itemButtons[i].onClick.AddListener(() => OnItemSelected(index));
        }
    }

    /// <summary>
    /// (TimeGauge.cs가 호출)
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        Time.timeScale = 0f; // ★ 시간 정지

        ChooseItems(); // 1. 아이템 3개 고르기
        UpdateUI();    // 2. UI에 표시하기 (★ 안전 코드가 적용된 함수)

        if (panelRoot != null) panelRoot.SetActive(true);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(true); 
    }

    /// <summary>
    /// (내부 함수) 아이템 3개 무작위 선택
    /// </summary>
    void ChooseItems()
    {
        currentChoices.Clear();
        List<ItemData> availableItems = itemPool.FindAll(item => !acquiredItems.Contains(item));
        
        // (선택 가능한 풀이 부족하면, 이미 획득한 아이템을 포함시킴 - 수정 없음)
        if (availableItems.Count < 3)
        {
            availableItems = new List<ItemData>(itemPool);
        }

        int choices = Mathf.Min(itemButtons.Length, availableItems.Count);
        for (int i = 0; i < choices; i++)
        {
             int randIdx = Random.Range(0, availableItems.Count);
             ItemData chosen = availableItems[randIdx];
             currentChoices.Add(chosen);
             availableItems.RemoveAt(randIdx);
        }
    }

    /// <summary>
    /// ★★★ 여기가 수정된 'UpdateUI' 함수입니다 ★★★
    /// (내부 함수) UI 버튼 3개 업데이트 (Null '안전 코드' 포함)
    /// </summary>
    void UpdateUI()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                // 1. 버튼은 무조건 활성화
                itemButtons[i].gameObject.SetActive(true);

                // 2. ★ (안전) 아이콘 목록/슬롯이 '있을' 때만 스프라이트 설정
                if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null) 
                {
                    itemIcons[i].sprite = currentChoices[i].icon;
                }
                    
                // 3. ★ (안전) 라벨 목록/슬롯이 '있을' 때만 텍스트 설정
                if (itemLabels != null && i < itemLabels.Length && itemLabels[i] != null) 
                {
                    // (사용자님의 원래 코드처럼 설명까지 포함)
                    itemLabels[i].text = $"{currentChoices[i].itemName}\n{currentChoices[i].description}"; 
                }
            }
            else
            {
                // 4. (안전) 남는 버튼은 비활성화
                itemButtons[i].gameObject.SetActive(false);
            }
        }
    }


    /// <summary>
    /// ★★★ 여기가 "5x5 활성 영역" 로직이 적용된 함수입니다 ★★★
    /// (버튼 클릭 시 호출)
    /// </summary>
    public void OnItemSelected(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;
        
        ItemData chosen = currentChoices[index];
        
        if (!acquiredItems.Contains(chosen))
        {
            acquiredItems.Add(chosen);
        }

        // ★★★ (수정됨) '즉시' 스킬 적용 '삭제' ★★★
        // (InventoryManager가 5x5 영역을 보고 스킬을 켜므로, 여기서는 아무것도 안 함)
        // ★★★

        // (테트리스 인벤토리 추가 로직 - 수정 없음)
        InventoryItemData tetrisItemToAdd = null;
        foreach (var mapping in tetrisMappings)
        {
            if (mapping.frameworkItemType == chosen.type) 
            {
                tetrisItemToAdd = mapping.tetrisItemData;
                break;
            }
        }
        
        if (tetrisItemToAdd != null)
        {
            if (InventoryManager.instance != null)
            {
                // (이 함수는 이제 5x2 '대기 영역'에 아이템을 추가합니다)
                InventoryManager.instance.AddItem(tetrisItemToAdd); 
            }
        }

        // (Time.timeScale = 0f 상태 유지! - 'P'키 대기)
        if (panelRoot != null)
        {
            panelRoot.SetActive(false); // '선택창'만 끔 (인벤토리는 켜둠)
        }
        Debug.Log("[ItemSelectionUI] 아이템 선택 완료. 'P' 키를 눌러 게임을 재개하세요.");
    }

    /// <summary>
    /// (InventoryInput.cs가 'P'키를 누르면 호출)
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        
        if (panelRoot != null) panelRoot.SetActive(false);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false); 
        
        Time.timeScale = 1f; // ★ 시간 재개
    }

    /// <summary>
    /// (InventoryInput.cs가 'P'키를 감시하기 위해 호출)
    /// </summary>
    public bool IsWaitingForClose()
    {
        // (isOpen == true) 이고 (panelRoot == false) 이면
        // -> (아이템은 선택했고 P키를 기다리는 중)
        return isOpen && (panelRoot != null && !panelRoot.activeSelf);
    }
}