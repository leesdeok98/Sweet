using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventoryItemData itemData;
    private int startX, startY, startWidth, startHeight, startRotation;
    private bool[] startShape;

    public int currentWidth, currentHeight, rotationAngle = 0;
    public bool[] currentShape;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Image itemImage;
    
    private bool isDragging = false;
    private bool dropSuccessful = false; // ★★★ [추가] 드롭 성공 여부 플래그 ★★★
    private const float SLOT_SIZE = 100f;
    private RectTransform itemLayerRect;
    private int lastGridX = -1, lastGridY = -1;
    private Color color_CanPlace = new Color(1f, 1f, 1f, 0.8f);
    private Color color_CannotPlace = new Color(1f, 0.5f, 0.5f, 0.8f);

    // (Initialize - 수정 없음)
    public void Initialize(InventoryItemData item, int x, int y, int width, int height, bool[] shape, int rotation)
    {
        this.itemData = item;
        this.startX = x;
        this.startY = y;
        this.startWidth = width;
        this.startHeight = height;
        this.startShape = (bool[])shape.Clone(); 
        this.startRotation = rotation;
        this.currentWidth = width;
        this.currentHeight = height;
        this.currentShape = (bool[])shape.Clone(); 
        this.rotationAngle = rotation;

        this.rectTransform = GetComponent<RectTransform>();
        this.canvasGroup = GetComponent<CanvasGroup>();
        this.itemImage = GetComponent<Image>();

        if (this.rectTransform != null)
        {
            this.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        if (itemImage != null && item != null)
        {
            itemImage.sprite = item.icon;
            itemImage.SetNativeSize(); 
        }
        rectTransform.sizeDelta = new Vector2(width * SLOT_SIZE, height * SLOT_SIZE);
        rectTransform.rotation = Quaternion.Euler(0, 0, -rotation);
        
        if (InventoryManager.instance != null)
        {
            this.itemLayerRect = InventoryManager.instance.GetItemLayerRect(); 
        }
    }
    
    public InventoryItemData GetItemData() => itemData;

    /// <summary>
    /// InventoryDropHandler.cs 에서 호출됩니다.
    /// </summary>
    public void SetDropSuccess(bool success)
    {
        this.dropSuccessful = success;
    }

    // (Update - R키 감지 / 수정 없음)
    void Update()
    {
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
            SnapToGrid(Input.mousePosition); 
        }
    }

    /// <summary>
    /// ★★★ [수정] 회전 함수 (sizeDelta 갱신) ★★★
    /// </summary>
    private void Rotate()
    {
        rotationAngle = (rotationAngle + 90) % 360;
        // (회전은 맨 마지막에)

        // 1. (데이터) 너비/높이/모양을 먼저 계산
        int newWidth = currentHeight;
        int newHeight = currentWidth;
        bool[] newShape = new bool[newWidth * newHeight];
        
        for (int y = 0; y < currentHeight; y++) {
            for (int x = 0; x < currentWidth; x++) {
                int newX = (currentHeight - 1) - y;
                int newY = x;
                int oldIndex = y * currentWidth + x;
                int newIndex = newY * newWidth + newX;
                newShape[newIndex] = currentShape[oldIndex];
            }
        }
        
        // 2. (데이터) 현재 값 갱신
        currentWidth = newWidth;
        currentHeight = newHeight;
        currentShape = newShape;

        // 3. ★★★ (수정) 'RectTransform'의 시각적 크기(sizeDelta)도 갱신 ★★★
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(currentWidth * SLOT_SIZE, currentHeight * SLOT_SIZE);
        }

        // 4. (시각) 'RectTransform' 회전
        rectTransform.rotation = Quaternion.Euler(0, 0, -rotationAngle); 
    }


    // (OnBeginDrag - 수정: dropSuccessful 초기화)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null || InventoryManager.instance == null) return;
        
        isDragging = true;
        dropSuccessful = false; // ★★★ [수정] 드래그 시작 시 초기화 ★★★
        originalParent = transform.parent; 

        InventoryManager.instance.RemoveDataOnly(itemData);
        transform.SetParent(transform.root); 
        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.8f;

        if(this.itemLayerRect == null)
        {
            this.itemLayerRect = InventoryManager.instance.GetItemLayerRect();
        }

        lastGridX = -1; 
        lastGridY = -1;
        SnapToGrid(eventData.position); 
    }

    // (OnDrag - 수정 없음)
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        SnapToGrid(eventData.position);
    }

    // (SnapToGrid - 중심 스냅 / 수정 없음)
    private void SnapToGrid(Vector2 screenPosition)
    {
        if (itemLayerRect == null) return; 

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            itemLayerRect, screenPosition, null, out localPoint
        );
        int targetX = Mathf.FloorToInt(localPoint.x / SLOT_SIZE);
        int targetY = Mathf.FloorToInt(-localPoint.y / SLOT_SIZE);

        Vector2 snapPosLocal = new Vector2(
            (targetX * SLOT_SIZE) + (currentWidth * SLOT_SIZE / 2f),
            -(targetY * SLOT_SIZE) - (currentHeight * SLOT_SIZE / 2f)
        );
        
        rectTransform.position = itemLayerRect.TransformPoint(snapPosLocal);
        
        if (targetX == lastGridX && targetY == lastGridY) return;
        lastGridX = targetX;
        lastGridY = targetY;

        bool canPlace = InventoryManager.instance.CanPlaceItem( 
            itemData, targetX, targetY,
            currentWidth, currentHeight, currentShape
        );
        if (itemImage != null)
        {
            itemImage.color = canPlace ? color_CanPlace : color_CannotPlace;
        }
    }


    /// <summary>
    /// ★★★ [수정] 드래그가 끝날 때, 성공 여부에 따라 아이템 복구 또는 파괴만 수행 ★★★
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        if (itemImage != null) itemImage.color = Color.white;
        canvasGroup.alpha = 1f;

        if (InventoryManager.instance == null) {
             Destroy(gameObject); 
             return;
        }

        // 드롭에 실패했을 경우 (InventoryDropHandler에서 SetDropSuccess가 호출되지 않았을 경우)
        if (!dropSuccessful) 
        {
            // 원래 위치에 아이템 데이터 복원 및 UI 재생성
            InventoryManager.instance.PlaceItem( 
                itemData, startX, startY, 
                startWidth, startHeight, startShape, startRotation 
            );
        }
        // 드롭에 성공했든 실패했든, 드래그 중인 임시 UI는 파괴해야 합니다.
        Destroy(gameObject);
    }
}