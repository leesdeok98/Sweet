using UnityEngine;
using UnityEngine.UI; // Image 색상 제어용
using System.Collections;
using System.Collections.Generic; // HashSet, List 사용

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI Prefab")]
    public GameObject itemUIPrefab; 

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 8; // 전체 8줄

    [Header("Ability Zone")]
    [SerializeField]
    private int activeZoneStartY = 3; // Y=3부터 활성 구역 (5x5 영역의 시작점)

    [Header("UI Settings")]
    [SerializeField]
    private RectTransform itemLayerRect; 

    // ★★★ [시각 효과] 슬롯 배경 이미지 리스트 (인스펙터 연결 필수) ★★★
    [Header("Slot Visuals")]
    public List<Image> slotImages; // 슬롯 40개를 순서대로(좌상단->우하단) 넣어주세요.
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 잠금(회색)
    public Color unlockedColor = Color.white; // 해제(흰색)

    [Header("Audio")]
    public AudioClip placeItemSound; 

    private InventoryItemData[,] tetrisGrid; 
    
    //  [기능] 칸 잠금 상태 배열 (true=잠김, false=해제) 
    private bool[,] cellLocked; 

    //  [최적화] 스킬 깜빡임 방지용 목록 
    private HashSet<ItemData.ItemType> currentActiveSkills = new HashSet<ItemData.ItemType>();

    // 변경 사항 감지 플래그
    public bool isDirty = false; 

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        tetrisGrid = new InventoryItemData[gridWidth, gridHeight];
        cellLocked = new bool[gridWidth, gridHeight]; // 잠금 배열 초기화
    }

    void Start()
    {
        InitializeLocks(); // 게임 시작 시 테두리 잠금 실행
    }

    
    //   자동 회전 배치 기능
    
    public bool AddItem(InventoryItemData itemData)
    {
        // 0도, 90도, 180도, 270도 순서로 회전하며 시도
        for (int r = 0; r < 4; r++)
        {
            int rotAngle = r * 90;
            
            // 현재 각도에 맞는 모양과 크기 계산
            bool[] currentShape;
            int currentW, currentH;
            GetRotatedShape(itemData.shape, itemData.width, itemData.height, rotAngle, out currentShape, out currentW, out currentH);

            // 1. 보관함(Y=0, 1) 탐색 (잠금 영향 없음)
            for (int y = 0; y < activeZoneStartY; y++) 
            {
                for (int x = 0; x < gridWidth; x++) 
                {
                    if (CanPlaceItem(itemData, x, y, currentW, currentH, currentShape)) 
                    {
                        PlaceItem(itemData, x, y, currentW, currentH, currentShape, rotAngle); 
                        return true; 
                    }
                }
            }

            // 2. 활성 영역(Y=3~) 탐색 (잠금 영향 받음)
            for (int y = activeZoneStartY; y < gridHeight; y++) 
            {
                for (int x = 0; x < gridWidth; x++) 
                {
                    if (CanPlaceItem(itemData, x, y, currentW, currentH, currentShape)) 
                    {
                        PlaceItem(itemData, x, y, currentW, currentH, currentShape, rotAngle); 
                        return true; 
                    }
                }
            }
        }

        Debug.LogWarning($"[InventoryManager] 공간 부족! {itemData.itemName} 생성 실패 (모든 회전 시도함).");
        return false;
    }

    // [내부 함수] 모양 배열을 회전시켜 반환
    private void GetRotatedShape(bool[] originalShape, int originalW, int originalH, int angle, out bool[] rotatedShape, out int rotatedW, out int rotatedH)
    {
        int turns = (angle % 360) / 90;
        
        rotatedShape = (bool[])originalShape.Clone();
        rotatedW = originalW;
        rotatedH = originalH;

        for (int i = 0; i < turns; i++)
        {
            bool[] newShape = new bool[rotatedW * rotatedH];
            int newW = rotatedH; 
            int newH = rotatedW;

            for (int y = 0; y < rotatedH; y++)
            {
                for (int x = 0; x < rotatedW; x++)
                {
                    if (rotatedShape[y * rotatedW + x])
                    {
                        // 90도 시계방향: (x, y) -> (H - 1 - y, x)
                        int newX = rotatedH - 1 - y;
                        int newY = x;
                        newShape[newY * newW + newX] = true;
                    }
                }
            }
            rotatedShape = newShape;
            rotatedW = newW;
            rotatedH = newH;
        }
    }

    
    //  잠금 시스템 (초기화 & 해금) 
    
    private void InitializeLocks()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                bool shouldLock = false;
                // 활성 구역(Y=3~7)의 테두리만 잠금
                if (y >= activeZoneStartY && y < gridHeight)
                {
                    if (x == 0 || x == gridWidth - 1 || y == activeZoneStartY || y == gridHeight - 1)
                    {
                        shouldLock = true;
                    }
                }
                SetCellLock(x, y, shouldLock);
            }
        }
    }

    private void SetCellLock(int x, int y, bool isLocked)
    {
        cellLocked[x, y] = isLocked;
        int index = y * gridWidth + x;

        if (slotImages != null && index >= 0 && index < slotImages.Count)
        {
            if (slotImages[index] != null)
            {
                slotImages[index].color = isLocked ? lockedColor : unlockedColor;
            }
        }
    }

    // 여러 개 해금 (ItemSelectionUI에서 호출)
    public void UnlockRandomOuterCells(int count)
    {
        for (int i = 0; i < count; i++)
        {
            List<Vector2Int> lockedCells = new List<Vector2Int>();
            for (int y = activeZoneStartY; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (cellLocked[x, y]) lockedCells.Add(new Vector2Int(x, y));
                }
            }

            if (lockedCells.Count > 0)
            {
                int randIndex = Random.Range(0, lockedCells.Count);
                Vector2Int target = lockedCells[randIndex];
                SetCellLock(target.x, target.y, false); // 해금
                Debug.Log($"[InventoryManager] 슬롯 해금 ({i + 1}/{count}): ({target.x}, {target.y})");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 배치 검사 (잠금 확인 포함)
    // ─────────────────────────────────────────────────────────────
    public bool CanPlaceItem(InventoryItemData itemData, int gridX, int gridY, int rotatedWidth, int rotatedHeight, bool[] rotatedShape)
    {
        for (int y = 0; y < rotatedHeight; y++) {
            for (int x = 0; x < rotatedWidth; x++) {
                if (rotatedShape[y * rotatedWidth + x]) {
                    int currentX = gridX + x;
                    int currentY = gridY + y;

                    if (!IsWithinGrid(currentX, currentY)) return false; 
                    if (currentY == 2) return false; // 버퍼존

                    // ★ 잠긴 칸(회색)이면 배치 불가
                    if (cellLocked[currentX, currentY]) return false; 

                    if (tetrisGrid[currentX, currentY] != null && tetrisGrid[currentX, currentY] != itemData)
                        return false;
                }
            }
        }
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    // 기본 기능들 (배치, 제거, 스킬, 소리, UI생성)
    // ─────────────────────────────────────────────────────────────

    public void PlayDropSound()
    {
        if (placeItemSound != null)
            AudioSource.PlayClipAtPoint(placeItemSound, Camera.main.transform.position);
    }

    public bool PlaceItem(InventoryItemData itemData, int gridX, int gridY, int rotatedWidth, int rotatedHeight, bool[] rotatedShape, int rotationAngle)
    {
        if (!CanPlaceItem(itemData, gridX, gridY, rotatedWidth, rotatedHeight, rotatedShape)) return false;
        
        for (int y = 0; y < rotatedHeight; y++) {
            for (int x = 0; x < rotatedWidth; x++) {
                if (rotatedShape[y * rotatedWidth + x]) {
                    tetrisGrid[gridX + x, gridY + y] = itemData;
                }
            }
        }
        
        isDirty = true; // 변경됨
        SpawnItemUI(itemData, gridX, gridY, rotatedWidth, rotatedHeight, rotatedShape, rotationAngle);
        // UpdateActiveSkills();
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
        if (removed) isDirty = true;

        // UpdateActiveSkills();
        return removed;
    }

    public void UpdateActiveSkills()
    {
        if (SkillManager.Instance == null) return;

        // 활성 구역에 들어있는 스킬 타입 수집
        HashSet<ItemData.ItemType> newActiveSkills = new HashSet<ItemData.ItemType>();

        for (int y = activeZoneStartY; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                InventoryItemData item = tetrisGrid[x, y];
                if (item != null && item.skillType != ItemData.ItemType.None)
                {
                    newActiveSkills.Add(item.skillType);
                }
            }
        }

        // 이전과 완전히 같고, 인벤토리 변경 플래그도 없으면 그냥 패스
        if (newActiveSkills.SetEquals(currentActiveSkills) && !isDirty)
        {
            return;
        }

        // 1) 기존 스킬들/세트 효과 전부 초기화
        SkillManager.Instance.ResetAllSkills();

        // 2) 이번에 활성 구역에서 발견된 스킬 타입들만 다시 켜기
        foreach (var skillType in newActiveSkills)
        {
            SkillManager.Instance.ActivateSkill(skillType);
        }

        // 3) 세트 효과 갱신
        SkillManager.Instance.CheckAllSetEffects();

        currentActiveSkills = newActiveSkills;
        isDirty = false;

        Debug.Log("[InventoryManager] 스킬 갱신 완료. 활성 스킬 개수: " + currentActiveSkills.Count);
    }




    public void ApplySkillsWithDelay(float delay)
    {
        StartCoroutine(DelayedUpdateRoutine(delay));
    }

    private IEnumerator DelayedUpdateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay); 
        UpdateActiveSkills(); 
    }

    private bool IsWithinGrid(int x, int y) { return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight; }
    public RectTransform GetItemLayerRect() { return itemLayerRect; }

    private void SpawnItemUI(InventoryItemData itemData, int gridX, int gridY, int w, int h, bool[] shape, int rot)
    {
        if (itemUIPrefab == null) return;

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

        GameObject itemObj = (existingHandler != null) ? existingHandler.gameObject : Instantiate(itemUIPrefab, itemLayerRect);
        ItemDragHandler dragHandler = itemObj.GetComponent<ItemDragHandler>();

        if (dragHandler == null) {
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