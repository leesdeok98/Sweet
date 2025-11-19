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
    // =======================================================

    void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false);

        // 버튼 클릭 이벤트 등록
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지용 로컬 변수
            itemButtons[i].onClick.AddListener(() => OnItemSelected(index));
        }
    }

    /// <summary>
    /// TimeGauge.cs에서 30초마다 호출하는 함수
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        Time.timeScale = 0f; // 게임 시간 정지

        ChooseItems(); // 1. 아이템 3개 무작위 선택
        UpdateUI();    // 2. UI에 아이콘/텍스트 반영

        if (panelRoot != null) panelRoot.SetActive(true);
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(true);
    }

    /// <summary>
    /// (내부용) 아이템 3개 무작위 선택
    /// </summary>
    void ChooseItems()
    {
        currentChoices.Clear();

        // 아직 얻지 않은 아이템만 우선 후보로 사용
        List<ItemData> availableItems = itemPool.FindAll(item => !acquiredItems.Contains(item));

        // 남은 풀이 3개 미만이면, 이미 얻은 아이템도 다시 포함
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
    /// 각 버튼의 아이콘, 이름, 설명을 UI에 반영 (TextMeshPro 스타일 적용)
    /// </summary>
    void UpdateUI()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                // 1. 버튼 활성화
                itemButtons[i].gameObject.SetActive(true);

                // 2. 아이콘 설정
                if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
                {
                    itemIcons[i].sprite = currentChoices[i].icon;
                }

                // 3. 라벨(이름+설명) 설정
                if (itemLabels != null && i < itemLabels.Length && itemLabels[i] != null)
                {
                    TMP_Text label = itemLabels[i];

                    // 3-1. 기본 스타일 적용 (설명 기준)
                    if (applyLabelStyle)
                    {
                        // 설명은 왼쪽 정렬 기준(위쪽 정렬)
                        label.alignment = TextAlignmentOptions.TopLeft;
                        label.enableAutoSizing = labelAutoSize;

                        if (!labelAutoSize)
                        {
                            label.fontSize = labelFontSize; // 설명 기본 크기
                        }
                    }

                    string nameText = currentChoices[i].itemName;
                    string descText = currentChoices[i].description;

                    // 3-2. 이름: 가운데 정렬 + 크게 + 굵게
                    string formattedName =
                        $"<align=\"center\"><size={nameSizeMultiplier * 100f}%><b>{nameText}</size></align>";

                    // 3-3. 설명: 왼쪽 정렬 + 기본 크기
                    string formattedDesc =
                        $"<align=\"left\">{descText}</align>";

                    // 3-4. 최종 텍스트 (위: 이름 / 아래: 설명)
                    label.text = $"{formattedName}\n{formattedDesc}";
                    // ※ TextMeshPro 컴포넌트에서 Rich Text 옵션이 켜져 있어야 합니다.
                }
            }
            else
            {
                // 뽑힌 아이템 수보다 인덱스가 크면 버튼 숨김
                itemButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (index는 0~2)
    /// </summary>
    public void OnItemSelected(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;

        ItemData chosen = currentChoices[index];

        // 획득한 아이템 목록에 기록
        if (!acquiredItems.Contains(chosen))
        {
            acquiredItems.Add(chosen);
        }

        // 테트리스 인벤토리(대기 영역)에 추가
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

        // 선택창만 끄고, 인벤토리는 계속 보여줌 (확인 버튼 대기)
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        Debug.Log("[ItemSelectionUI] 아이템 선택 완료. 확인 버튼을 눌러 게임을 재개하세요.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ▼▼▼ [수정됨] InventoryInput.cs와의 연결을 위한 핵심 함수들 ▼▼▼
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// (InventoryInput.cs에서 버튼 클릭 시 호출)
    /// 인벤토리를 닫고 게임을 재개합니다.
    /// </summary>
    public void ClosePanelAndResume()
    {
        // 1. 테트리스 인벤토리 닫기
        if (tetrisInventoryPanel != null) tetrisInventoryPanel.SetActive(false);

        // 2. 아이템 선택창 닫기
        if (panelRoot != null) panelRoot.SetActive(false);

        // 3. 플래그 초기화
        isOpen = false;
        IsEscMenuOpen = false;

        // 4. 게임 시간 재개
        Time.timeScale = 1f; 

        Debug.Log("[ItemSelectionUI] Closed & Resumed.");
    }

    /// <summary>
    /// (InventoryInput.cs에서 호출) 
    /// 확인 버튼을 보여줘야 하는 상태인지(인벤토리 정리 중인지) 체크합니다.
    /// </summary>
    public bool IsWaitingForClose()
    {
        // 테트리스 패널이 켜져 있다면 -> 유저가 정리를 마치고 나가길 기다리는 중
        return tetrisInventoryPanel != null && tetrisInventoryPanel.activeSelf;
    }
}