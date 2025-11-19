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
    [SerializeField] private int gridHeight = 8; // 8줄

    [Header("Ability Zone")]
    [SerializeField]
    private int activeZoneStartY = 3; // Y=3부터 활성

    [Header("UI Settings")]
    [SerializeField]
    private RectTransform itemLayerRect; 

    private InventoryItemData[,] tetrisGrid; 
    private HashSet<InventoryItemData> processedItems = new HashSet<InventoryItemData>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        tetrisGrid = new InventoryItemData[gridWidth, gridHeight];
    }

    // --- '공용' (Public) '테트리스' '핵심' '인터페이스' 함수들 ---

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
        
        // (즉시 스킬 갱신 방지 - 확인 버튼 누를 때 갱신됨)
        // UpdateActiveSkills(); 

        SpawnItemUI(itemData, gridX, gridY, 
                    rotatedWidth, rotatedHeight, 
                    rotatedShape, rotationAngle);
        
        return true; 
    }

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
        // (즉시 스킬 갱신 방지)
        // if (removed) { UpdateActiveSkills(); }
        
        return removed;
    }

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

    public void UpdateActiveSkills()
    {
        if (SkillManager.Instance == null) return;
        SkillManager.Instance.ResetAllSkills();
        processedItems.Clear();

        for (int y = activeZoneStartY; y < gridHeight; y++) 
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

    // (지연 실행 함수)
    public void ApplySkillsWithDelay(float delay)
    {
        StartCoroutine(DelayedUpdateRoutine(delay));
    }

    private IEnumerator DelayedUpdateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay); 
        UpdateActiveSkills(); 
        Debug.Log($"[InventoryManager] {delay}초 지연 후 스킬 갱신 완료");
    }


    // ─────────────────────────────────────────────────────────────
    // ★★★ [수정된 함수] 보관함 -> 활성 영역 순서로 빈 공간 탐색 ★★★
    // ─────────────────────────────────────────────────────────────
    public bool AddItem(InventoryItemData itemData)
    {
        // 1단계: '보관함(Storage)' (Y = 0 ~ 1) 우선 탐색
        for (int y = 0; y < 2; y++) 
        {
            for (int x = 0; x < gridWidth; x++) 
            {
                if (CanPlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape)) 
                {
                    PlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape, 0); 
                    return true; // 보관함에 배치 성공
                }
            }
        }

        // 2단계: 보관함이 꽉 찼다면 '활성 영역(Active Zone)' (Y = 3 ~ 끝) 탐색
        // (Y=2는 경계선이라 건너뜁니다)
        for (int y = activeZoneStartY; y < gridHeight; y++) 
        {
            for (int x = 0; x < gridWidth; x++) 
            {
                if (CanPlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape)) 
                {
                    PlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape, 0); 
                    return true; // 활성 영역 빈 곳에 배치 성공
                }
            }
        }

        // 3단계: 전체 맵 어디에도 자리가 없음 -> 생성 포기
        Debug.LogWarning($"[InventoryManager] 공간 부족! {itemData.itemName} 생성 실패. (보관함 및 활성 영역 모두 꽉 참)");
        return false;
    }

    private bool IsWithinGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight; 
    }

    public RectTransform GetItemLayerRect()
    {
        return itemLayerRect;
    }

    private void SpawnItemUI(InventoryItemData itemData, int gridX, int gridY, int w, int h, bool[] shape, int rot)
    {
        if (itemUIPrefab == null) {
            Debug.LogError("Item UI Prefab이 InventoryManager에 연결되지 않았습니다!");
            return;
        }

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
            itemObj = Instantiate(itemUIPrefab, itemLayerRect);
            dragHandler = itemObj.GetComponent<ItemDragHandler>();
        }

        if (dragHandler == null) {
            Debug.LogError("Item UI Prefab에 ItemDragHandler.cs가 없습니다!");
            if (existingHandler == null) Destroy(itemObj);
            return;
        }

        dragHandler.Initialize(itemData, gridX, gridY, w, h, shape, rot);

        RectTransform rt = itemObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one; 
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);
            const float SLOT_SIZE = 100f; 
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(
                (gridX * SLOT_SIZE) + (w * SLOT_SIZE / 2f),
                -(gridY * SLOT_SIZE) - (h * SLOT_SIZE / 2f)
            );
        }
    }
}