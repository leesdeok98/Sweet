using UnityEngine;
using System.Collections;
using System.Collections.Generic; // HashSet 사용

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI Prefab")]
    public GameObject itemUIPrefab; 

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 8; // ★ (수정됨) 8줄

    [Header("Ability Zone")]
    [SerializeField]
    private int activeZoneStartY = 3; // ★ (수정됨) Y=3부터 활성

    [Header("UI Settings")]
    [SerializeField]
    private RectTransform itemLayerRect; 

    private InventoryItemData[,] tetrisGrid; 
    private HashSet<InventoryItemData> processedItems = new HashSet<InventoryItemData>();

    // (Awake - 수정 없음)
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        tetrisGrid = new InventoryItemData[gridWidth, gridHeight];
    }

    // --- '공용' (Public) '테트리스' '핵심' '인터페이스' 함수들 ---

    // (PlaceItem - 수정 없음)
    public bool PlaceItem(InventoryItemData itemData, int gridX, int gridY, int rotatedWidth, int rotatedHeight, bool[] rotatedShape, int rotationAngle)
    {
        if (!CanPlaceItem(itemData, gridX, gridY, rotatedWidth, rotatedHeight, rotatedShape))
        {
            return false;
        }
        
        for (int y = 0; y < rotatedHeight; y++) {
            for (int x = 0; x < rotatedWidth; x++) {
                if (rotatedShape[y * rotatedWidth + x]) {
                    int currentX = gridX + x;
                    int currentY = gridY + y;
                    if (IsWithinGrid(currentX, currentY)) {
                        tetrisGrid[currentX, currentY] = itemData;
                    }
                }
            }
        }
        
        UpdateActiveSkills(); 

        SpawnItemUI(itemData, gridX, gridY, 
                    rotatedWidth, rotatedHeight, 
                    rotatedShape, rotationAngle);
        
        return true; 
    }

    // (RemoveDataOnly - 수정 없음)
    public bool RemoveDataOnly(InventoryItemData itemData)
    {
        bool removed = false;
        for (int y = 0; y < gridHeight; y++) {
            for (int x = 0; x < gridWidth; x++) {
                if (tetrisGrid[x, y] == itemData) {
                    tetrisGrid[x, y] = null;
                    removed = true;
                }
            }
        }
        if (removed) { UpdateActiveSkills(); }
        return removed;
    }


    // (CanPlaceItem - Y=2 버퍼 영역 체크)
    public bool CanPlaceItem(InventoryItemData itemData, int gridX, int gridY, int rotatedWidth, int rotatedHeight, bool[] rotatedShape)
    {
        for (int y = 0; y < rotatedHeight; y++) {
            for (int x = 0; x < rotatedWidth; x++) {
                if (rotatedShape[y * rotatedWidth + x]) {
                    int currentX = gridX + x;
                    int currentY = gridY + y;

                    if (!IsWithinGrid(currentX, currentY)) return false; 
                    if (currentY == 2) return false; // (Y=2 배치 불가)
                    if (tetrisGrid[currentX, currentY] != null && tetrisGrid[currentX, currentY] != itemData)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }


    // (UpdateActiveSkills - Y=3~7 활성 영역 스캔)
    public void UpdateActiveSkills()
    {
        if (SkillManager.Instance == null) return;
        SkillManager.Instance.ResetAllSkills();
        processedItems.Clear();

        for (int y = activeZoneStartY; y < gridHeight; y++) // (Y=3 부터 Y=7 까지)
        {
            for (int x = 0; x < gridWidth; x++) {
                InventoryItemData item = tetrisGrid[x, y];
                if (item != null && item.skillType != ItemData.ItemType.None && !processedItems.Contains(item)) {
                    processedItems.Add(item);
                    SkillManager.Instance.ActivateSkill(item.skillType);
                }
            }
        }
    }


    // (AddItem - Y=0, 1 보관함에 배치)
    public bool AddItem(InventoryItemData itemData)
    {
        for (int y = 0; y < 2; y++) // (Y=0 부터 Y=1 까지)
        {
            for (int x = 0; x < gridWidth; x++) {
                if (CanPlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape)) {
                    PlaceItem(itemData, x, y, 
                              itemData.width, itemData.height, 
                              itemData.shape, 0); 
                    return true;
                }
            }
        }
        Debug.LogWarning($"{itemData.itemName}을(를) 보관함에 배치할 공간이 없습니다!");
        return false;
    }

    // (IsWithinGrid - Y=0~7 허용)
    private bool IsWithinGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight; // (y < 8)
    }

    // (GetItemLayerRect - 수정 없음)
    public RectTransform GetItemLayerRect()
    {
        return itemLayerRect;
    }

    // ---------------------------------------------------------------------
    // ★ [수정] '중심' 피벗 기준 '스폰' 함수
    // ---------------------------------------------------------------------
    private void SpawnItemUI(InventoryItemData itemData, int gridX, int gridY, int w, int h, bool[] shape, int rot)
    {
        if (itemUIPrefab == null) {
            Debug.LogError("Item UI Prefab이 InventoryManager에 연결되지 않았습니다!");
            return;
        }

        // (중복 UI 검사 - 수정 없음)
        ItemDragHandler existingHandler = null;
        if (itemLayerRect != null) {
            foreach (Transform child in itemLayerRect) {
                ItemDragHandler handler = child.GetComponent<ItemDragHandler>();
                if (handler != null && handler.GetItemData() == itemData) {
                    existingHandler = handler;
                    break;
                }
            }
        }

        GameObject itemObj;
        ItemDragHandler dragHandler;

        if (existingHandler != null) {
            itemObj = existingHandler.gameObject;
            dragHandler = existingHandler;
        } else {
            // 부모를 지정하며 생성
            itemObj = Instantiate(itemUIPrefab, itemLayerRect);
            dragHandler = itemObj.GetComponent<ItemDragHandler>();
        }

        if (dragHandler == null) {
            Debug.LogError("Item UI Prefab에 ItemDragHandler.cs가 없습니다!");
            if (existingHandler == null) Destroy(itemObj);
            return;
        }

        // 1. (중요) DragHandler 초기화 (Initialize가 피벗을 0.5로 설정함)
        dragHandler.Initialize(itemData, gridX, gridY, w, h, shape, rot);

        // 2. (UI) 그리드 '중심' 좌표에 맞게 RectTransform 위치 설정
        RectTransform rt = itemObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            // ★★★ [여기가 수정되었습니다] ★★★
            // 생성 직후 스케일이 엉망이거나 Z축이 뒤로 밀려 안 보이는 문제를 방지합니다.
            rt.localScale = Vector3.one; 
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);

            const float SLOT_SIZE = 100f; 
            
            // ★ (수정) (0,1) 좌상단 앵커 사용 (계산 편의)
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            
            // ★ (수정) 좌상단 앵커 기준 (0.5, 0.5) 피벗의 '중심' 위치 계산
            rt.anchoredPosition = new Vector2(
                (gridX * SLOT_SIZE) + (w * SLOT_SIZE / 2f),
                -(gridY * SLOT_SIZE) - (h * SLOT_SIZE / 2f)
            );
        }
    }
}