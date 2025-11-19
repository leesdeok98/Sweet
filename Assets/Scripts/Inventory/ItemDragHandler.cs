using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventoryItemData itemData;
    
    // 원본 데이터 (복구 및 시각적 기준)
    private int startGridX, startGridY;
    private int startWidth, startHeight; // ★ 원본 너비/높이 (UI 크기용)
    private int startRotation;
    private bool[] startShape; // ★ 배치 당시의 모양 (복구용)

    // 현재 상태 (드래그 및 회전 계산용)
    public int currentWidth, currentHeight, rotationAngle = 0;
    public bool[] currentShape;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Image itemImage;
    
    private bool isDragging = false;
    private bool dropSuccessful = false;
    
    private const float SLOT_SIZE = 100f;
    private RectTransform itemLayerRect;
    private int lastGridX = -1, lastGridY = -1;
    private Color color_CanPlace = new Color(1f, 1f, 1f, 0.8f);
    private Color color_CannotPlace = new Color(1f, 0.5f, 0.5f, 0.8f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();
    }

    public void Initialize(InventoryItemData item, int x, int y, int width, int height, bool[] shape, int rotation)
    {
        this.itemData = item;
        this.startGridX = x;
        this.startGridY = y;
        
        // [핵심 1] UI가 찌그러지지 않게 하기 위해 '원본 크기'를 따로 저장합니다.
        this.startWidth = item.width;   
        this.startHeight = item.height; 
        
        // [핵심 2 수정] '원본 모양'이 아니라, '현재 배치된 모양(회전된 상태)'을 저장해야 합니다.
        // 그래야 OnEndDrag에서 복구할 때 회전된 너비/높이와 짝이 맞습니다.
        this.startShape = (bool[])shape.Clone(); 
        
        this.startRotation = rotation;

        // 현재 상태 (회전된 상태 반영)
        this.currentWidth = width;      
        this.currentHeight = height;    
        this.currentShape = (bool[])shape.Clone();
        this.rotationAngle = rotation;
        
        if (rectTransform != null) rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        if (itemImage != null && item != null)
        {
            itemImage.sprite = item.icon;
            itemImage.SetNativeSize(); 
        }

        // [핵심 3] UI 크기는 무조건 '원본 크기'로 고정 (찌그러짐 방지)
        rectTransform.sizeDelta = new Vector2(startWidth * SLOT_SIZE, startHeight * SLOT_SIZE);
        
        // [핵심 4] 회전 적용
        rectTransform.localRotation = Quaternion.Euler(0, 0, -rotation);
        
        if (InventoryManager.instance != null)
        {
            this.itemLayerRect = InventoryManager.instance.GetItemLayerRect(); 
        }
    }
    
    public InventoryItemData GetItemData() => itemData;
    public void SetDropSuccess(bool success) => this.dropSuccessful = success;

    void Update()
    {
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
            if (Input.mousePresent) SnapToGrid(Input.mousePosition); 
        }
    }

    private void Rotate()
    {
        // 1. 각도 증가
        rotationAngle = (rotationAngle + 90) % 360;

        // 2. 논리적 데이터 교체 (그리드 계산용)
        int tempW = currentWidth;
        currentWidth = currentHeight;
        currentHeight = tempW;

        // 3. 모양 배열 회전
        bool[] newShape = new bool[currentWidth * currentHeight];
        int oldW = currentHeight; 
        int oldH = currentWidth;

        for (int y = 0; y < oldH; y++) {
            for (int x = 0; x < oldW; x++) {
                int newX = (oldH - 1) - y;
                int newY = x;
                newShape[newY * currentWidth + newX] = currentShape[y * oldW + x];
            }
        }
        currentShape = newShape;

        // 4. [시각적 처리] sizeDelta는 건드리지 않고 회전만 시킴 (원본 비율 유지)
        if (rectTransform != null)
        {
            rectTransform.localRotation = Quaternion.Euler(0, 0, -rotationAngle);
            rectTransform.sizeDelta = new Vector2(startWidth * SLOT_SIZE, startHeight * SLOT_SIZE);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null || InventoryManager.instance == null) return;
        
        isDragging = true;
        dropSuccessful = false;
        originalParent = transform.parent; 

        InventoryManager.instance.RemoveDataOnly(itemData);
        transform.SetParent(transform.root); 
        transform.SetAsLastSibling();        

        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.8f;

        if(this.itemLayerRect == null)
            this.itemLayerRect = InventoryManager.instance.GetItemLayerRect();

        lastGridX = -1; 
        lastGridY = -1;
        SnapToGrid(eventData.position); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        SnapToGrid(eventData.position);
    }

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
            itemImage.color = canPlace ? color_CanPlace : color_CannotPlace;
    }

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

        if (!dropSuccessful) 
        {
            // ★ 실패 시 원래 상태로 복구
            
            // startRotation이 90도, 270도(홀수 번 회전)라면 
            // '원본 너비'가 '현재 높이'가 되어야 하므로 스왑해서 넘겨줍니다.
            int restoreW = startWidth;
            int restoreH = startHeight;

            if (startRotation % 180 != 0)
            {
                restoreW = startHeight;
                restoreH = startWidth;
            }

            InventoryManager.instance.PlaceItem( 
                itemData, 
                startGridX, 
                startGridY, 
                restoreW,  // 회전 고려된 너비
                restoreH,  // 회전 고려된 높이
                startShape, // 배치 당시의 모양
                startRotation 
            );
        }
        
        Destroy(gameObject);
    }
}