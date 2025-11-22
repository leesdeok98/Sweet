using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용

[System.Serializable]
public class TetrisItemMapping
{
    public ItemData.ItemType frameworkItemType; 
    public InventoryItemData tetrisItemData;    
}

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;       // 아이템 3개 선택창 루트
    public Button[] itemButtons;       // 각 선택지 버튼
    public Image[] itemIcons;          // 각 선택지 아이콘
    public TMP_Text[] itemLabels;      // 각 선택지 라벨

    [Header("Linked Panels")]
    public GameObject tetrisInventoryPanel; // 테트리스 인벤토리 패널

    [Header("Items")]
    public List<ItemData> itemPool = new List<ItemData>(); 
    private List<ItemData> currentChoices = new List<ItemData>(); 
    private HashSet<ItemData> acquiredItems = new HashSet<ItemData>(); 

    private bool isOpen = false;

    [Header("State Control")]
    [Tooltip("ESC 설정 창이 열려있는지 여부 (Stage1Setting.cs에서 제어용)")]
    public bool IsEscMenuOpen = false;

    [Header("Tetris Inventory Mapping")]
    public List<TetrisItemMapping> tetrisMappings = new List<TetrisItemMapping>();

    // ========= 텍스트 스타일 설정(인스펙터에서 조절) =========
    [Header("Label Style")]
    [Tooltip("true면 아래 설정값으로 라벨 스타일을 덮어씁니다.")]
    public bool applyLabelStyle = true;

    [Tooltip("라벨에 Auto Size를 사용할지 여부")]
    public bool labelAutoSize = false;

    [Tooltip("Auto Size를 쓰지 않을 때 설명 텍스트의 기본 폰트 크기")]
    public float labelFontSize = 28f;

    [Tooltip("아이템 이름의 크기 배율 (기본 크기 대비 배수, 1.3 = 130%)")]
    [Range(0.5f, 2f)]
    public float nameSizeMultiplier = 1.3f;

    [Tooltip("아이템 이름과 설명 사이에 넣을 빈 줄 수")]
    [Range(0, 10)]
    public float nameDescEmptyLines = 0;
    // =======================================================

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

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        Time.timeScale = 0f; // 게임 시간 정지

        ChooseItems(); 
        UpdateUI();    

        if (panelRoot != null) panelRoot.SetActive(true);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(true);
        
        // 버튼 활성화
        foreach(var btn in itemButtons) btn.interactable = true;
    }

    void ChooseItems()
    {
        currentChoices.Clear();
        List<ItemData> availableItems = itemPool.FindAll(item => !acquiredItems.Contains(item));

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

    void UpdateUI()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                itemButtons[i].gameObject.SetActive(true);

                if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
                {
                    itemIcons[i].sprite = currentChoices[i].icon;
                }

                if (itemLabels != null && i < itemLabels.Length && itemLabels[i] != null)
                {
                    TMP_Text label = itemLabels[i];

                    if (applyLabelStyle)
                    {
                        label.alignment = TextAlignmentOptions.TopLeft;
                        label.enableAutoSizing = labelAutoSize;
                        if (!labelAutoSize) label.fontSize = labelFontSize; 
                    }

                    string nameText = currentChoices[i].itemName;
                    string descText = currentChoices[i].description;

                    string formattedName = $"<align=\"center\"><size={nameSizeMultiplier * 100f}%><b>{nameText}</size></align>";
                    string formattedDesc = $"<align=\"left\">{descText}</align>";
                    
                    // 빈 줄 추가 로직
                    string gap = "\n"; 
                    for (int n = 0; n < nameDescEmptyLines; n++)
                    {
                        gap += "\n";
                    }  
                    
                    label.text = $"{formattedName}{gap}{formattedDesc}";
                }
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnItemSelected(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;

        ItemData chosen = currentChoices[index];

        if (!acquiredItems.Contains(chosen))
        {
            acquiredItems.Add(chosen);
        }

        InventoryItemData tetrisItemToAdd = null;
        foreach (var mapping in tetrisMappings)
        {
            if (mapping.frameworkItemType == chosen.type)
            {
                tetrisItemToAdd = mapping.tetrisItemData;
                break;
            }
        }

        if (tetrisItemToAdd != null && InventoryManager.instance != null)
        {
            InventoryManager.instance.AddItem(tetrisItemToAdd);
        }

        // 선택창 끄기 (인벤토리는 유지)
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // 인벤토리 강제 활성화
        if (tetrisInventoryPanel != null)
        {
            tetrisInventoryPanel.SetActive(true);
        }

        Debug.Log("[ItemSelectionUI] 아이템 선택 완료.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 연결 함수들
    // ─────────────────────────────────────────────────────────────────────

    public void ClosePanelAndResume()
    {
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false);
        if (panelRoot != null) panelRoot.SetActive(false);

        isOpen = false;
        IsEscMenuOpen = false;
        Time.timeScale = 1f; 

        Debug.Log("[ItemSelectionUI] Closed & Resumed.");
    }

    // ★★★ [수정된 부분] 선택창은 꺼지고 인벤토리는 켜져야 버튼 노출
    public bool IsWaitingForClose()
    {
        // 1. 인벤토리 패널 켜짐 (정리 중)
        bool isInventoryOpen = tetrisInventoryPanel != null && tetrisInventoryPanel.activeSelf;

        // 2. 선택창 패널 꺼짐 (선택 완료)
        bool isSelectionDone = panelRoot != null && !panelRoot.activeSelf;

        // 두 조건 만족 시 '확인 버튼' 노출
        return isInventoryOpen && isSelectionDone;
    }
}