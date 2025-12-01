using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용

/// <summary>
/// (ItemData.ItemType)과 (InventoryItemData)를 연결하는 '번역기' 클래스
/// </summary>
[System.Serializable]
public class TetrisItemMapping
{
    public ItemData.ItemType frameworkItemType; // (예: ItemData.ItemType.IcedJelly)
    public InventoryItemData tetrisItemData;    // (예: IcedJelly_TetrisItem.asset)
}

public class ItemSelectionUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;       // 아이템 3개 선택창 루트 (전체 패널)
    public Button[] itemButtons;       // 각 선택지 버튼 (3개)
    public Image[] itemIcons;          // 각 선택지 아이콘 이미지
    public TMP_Text[] itemLabels;      // 각 선택지 이름+설명 TMP 라벨

    [Header("Linked Panels")]
    public GameObject tetrisInventoryPanel; // 테트리스 인벤토리 패널

    [Header("Items")]
    public List<ItemData> itemPool = new List<ItemData>(); // 전체 아이템 풀
    private List<ItemData> currentChoices = new List<ItemData>(); // 이번에 뽑힌 아이템들
    private HashSet<ItemData> acquiredItems = new HashSet<ItemData>(); // 이미 획득한 아이템

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
    
    // ★★★ [추가] 선택 횟수 및 해금 횟수 카운트 ★★★
    private int selectionCount = 0;
    private int unlockTriggerCount = 0;

    void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false);

        // 버튼 클릭 이벤트 등록
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i; 
            itemButtons[i].onClick.AddListener(() => OnItemSelected(index));
        }
    }

    /// <summary>
    /// TimeGauge.cs에서 호출 (선택창 열기)
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        Time.timeScale = 0f; // 게임 시간 정지

        ChooseItems(); // 아이템 3개 무작위 선택
        UpdateUI();    // UI 표시

        if (panelRoot != null) panelRoot.SetActive(true);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(true);
        
        // 버튼 다시 활성화 (투명도 등은 UpdateUI에서 재설정됨)
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
            // 투명도 조절을 위한 로컬 함수
            void SetAlpha(Graphic graphic, float alpha)
            {
                if (graphic != null)
                {
                    Color c = graphic.color;
                    c.a = alpha;
                    graphic.color = c;
                }
            }

            if (i < currentChoices.Count)
            {
                // [아이템 있음] 버튼 활성화 및 불투명하게(1f) 설정
                itemButtons[i].gameObject.SetActive(true);
                itemButtons[i].interactable = true;
                SetAlpha(itemButtons[i].GetComponent<Image>(), 1f);

                if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
                {
                    itemIcons[i].sprite = currentChoices[i].icon;
                    SetAlpha(itemIcons[i], 1f);
                }

                if (itemLabels != null && i < itemLabels.Length && itemLabels[i] != null)
                {
                    TMP_Text label = itemLabels[i];
                    SetAlpha(label, 1f); // 텍스트 보이게

                    // 텍스트 스타일 적용
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
                // [아이템 없음] 버튼은 켜두되(레이아웃 유지), 투명하게(0f) 만들고 클릭 방지
                itemButtons[i].gameObject.SetActive(true); // 켜둠 (자리 차지)
                itemButtons[i].interactable = false;       // 클릭 불가

                // 배경 이미지 투명하게
                SetAlpha(itemButtons[i].GetComponent<Image>(), 0f);

                // 아이콘 투명하게
                if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
                {
                    SetAlpha(itemIcons[i], 0f);
                }

                // 라벨 투명하게
                if (itemLabels != null && i < itemLabels.Length && itemLabels[i] != null)
                {
                    SetAlpha(itemLabels[i], 0f);
                }
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

        // ─────────────────────────────────────────────────────────────
        // ★★★ [해금 로직] 2회 선택 시마다 테두리 해금 ★★★
        // ─────────────────────────────────────────────────────────────
        selectionCount++;
        
        // 짝수 번째(2, 4, 6...) 선택일 때 해금 시도
        if (selectionCount % 2 == 0)
        {
            if (InventoryManager.instance != null)
            {
                unlockTriggerCount++;

                // ★★★ [수정됨] 3번째 해금(6회 선택)까지는 2칸씩 엽니다! ★★★
                // 1, 2, 3번째 해금 -> 2칸
                // 4번째 이후 해금 -> 1칸
                int amountToUnlock = (unlockTriggerCount <= 3) ? 2 : 1;

                // 개수만큼 반복해서 해금 함수 호출
                InventoryManager.instance.UnlockRandomOuterCells(amountToUnlock);
            }
        }

        // 선택창 끄기
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // 인벤토리 켜기 (정리 화면)
        if (tetrisInventoryPanel != null)
        {
            tetrisInventoryPanel.SetActive(true);
        }

        Debug.Log($"[ItemSelectionUI] 아이템 선택 완료 (누적 {selectionCount}회). 확인 버튼 대기 중.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 연결 함수들 (InventoryInput.cs에서 호출)
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