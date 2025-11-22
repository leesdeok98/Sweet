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
    
    // ★★★ [최적화] 현재 활성화된 스킬 목록 기억용 ★★★
    // (이 목록과 비교해서 변화가 없으면 리셋을 안 함 -> 깜빡임 방지)
    private HashSet<ItemData.ItemType> currentActiveSkills = new HashSet<ItemData.ItemType>();

    // 변경 사항 감지 플래그
    public bool isDirty = false; 

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
        
        isDirty = true; // 변경됨

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
        
        if (removed) isDirty = true; // 변경됨
        
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
                    if (currentY == 2) return false; 
                    if (tetrisGrid[currentX, currentY] != null && tetrisGrid[currentX, currentY] != itemData)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    // ★★★ [핵심 수정] 스마트 스킬 갱신 (깜빡임 방지) ★★★
    // ─────────────────────────────────────────────────────────────
    public void UpdateActiveSkills()
    {
        if (SkillManager.Instance == null) return;
        
        // 1. 이번 턴에 활성화되어야 할 스킬 목록을 새로 조사합니다.
        // (중복 제거를 위해 HashSet 사용)
        HashSet<ItemData.ItemType> newActiveSkills = new HashSet<ItemData.ItemType>();

        // 활성 구역 스캔
        for (int y = activeZoneStartY; y < gridHeight; y++) 
        {
            for (int x = 0; x < gridWidth; x++) {
                InventoryItemData item = tetrisGrid[x, y];
                
                // 아이템이 있고, 스킬 타입이 None이 아니면 목록에 추가
                if (item != null && item.skillType != ItemData.ItemType.None) {
                    newActiveSkills.Add(item.skillType);
                }
            }
        }

        // 2. [비교] 기존 목록(currentActiveSkills)과 새 목록(newActiveSkills)이 완전히 똑같은지 확인합니다.
        // (아이템 위치만 바뀌고 스킬 종류 구성은 그대로라면 굳이 초기화할 필요 없음!)
        if (newActiveSkills.SetEquals(currentActiveSkills))
        {
            // 변화 없음 -> 갱신 건너뜀 (애니메이션 유지됨)
            isDirty = false;
            // Debug.Log("[InventoryManager] 스킬 구성 변동 없음. 갱신 생략.");
            return;
        }

        // 3. [갱신] 구성이 달라졌을 때만 리셋하고 다시 켭니다.
        SkillManager.Instance.ResetAllSkills();
        
        foreach (var skillType in newActiveSkills)
        {
            SkillManager.Instance.ActivateSkill(skillType);
        }

        // 4. 현재 상태 저장 및 플래그 해제
        currentActiveSkills = newActiveSkills;
        isDirty = false;
        
        Debug.Log("[InventoryManager] 스킬 구성 변경됨 -> 재활성화 완료");
    }

    public void ApplySkillsWithDelay(float delay)
    {
        StartCoroutine(DelayedUpdateRoutine(delay));
    }

    private IEnumerator DelayedUpdateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay); 
        UpdateActiveSkills(); 
        Debug.Log($"[InventoryManager] {delay}초 지연 후 스킬 갱신 체크 완료");
    }

    // (AddItem - 보관함 -> 활성 영역)
    public bool AddItem(InventoryItemData itemData)
    {
        // 1. 보관함
        for (int y = 0; y < 2; y++) 
        {
            for (int x = 0; x < gridWidth; x++) 
            {
                if (CanPlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape)) 
                {
                    PlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape, 0); 
                    return true; 
                }
            }
        }

        // 2. 활성 영역
        for (int y = activeZoneStartY; y < gridHeight; y++) 
        {
            for (int x = 0; x < gridWidth; x++) 
            {
                if (CanPlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape)) 
                {
                    PlaceItem(itemData, x, y, itemData.width, itemData.height, itemData.shape, 0); 
                    return true; 
                }
            }
        }

        Debug.LogWarning($"[InventoryManager] 공간 부족! {itemData.itemName} 생성 실패.");
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