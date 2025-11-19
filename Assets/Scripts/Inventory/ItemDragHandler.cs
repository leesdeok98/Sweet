using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventoryItemData itemData;
    
    // 원본 데이터 (절대 변하지 않는 기준값)
    private int startGridX, startGridY;
    private int startWidth, startHeight; // ★ 원본 너비/높이
    private int startRotation;
    private bool[] startShape;

    // 현재 상태 (회전된 상태 반영)
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

    // ★★★ [여기가 핵심 수정됨] ★★★
    public void Initialize(InventoryItemData item, int x, int y, int width, int height, bool[] shape, int rotation)
    {
        this.itemData = item;
        this.startGridX = x;
        this.startGridY = y;
        
        // [중요] UI 크기는 매개변수(width/height)가 아니라 'itemData'의 원본 크기를 따라야 합니다!
        // 매개변수로 들어온 width/height는 이미 회전된 상태일 수 있기 때문입니다.
        this.startWidth = item.width;   // 원본 너비 (예: 2)
        this.startHeight = item.height; // 원본 높이 (예: 1)
        
        this.startShape = (bool[])item.shape.Clone(); // 원본 모양
        this.startRotation = rotation;

        // 현재 상태 (회전된 상태)
        this.currentWidth = width;      // 현재 너비 (예: 1)
        this.currentHeight = height;    // 현재 높이 (예: 2)
        this.currentShape = (bool[])shape.Clone();
        this.rotationAngle = rotation;
        
        if (rectTransform != null) rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        if (itemImage != null && item != null)
        {
            itemImage.sprite = item.icon;
            itemImage.SetNativeSize(); 
        }

        // [핵심] sizeDelta는 무조건 '원본 크기'로 설정
        rectTransform.sizeDelta = new Vector2(startWidth * SLOT_SIZE, startHeight * SLOT_SIZE);
        
        // [핵심] 회전 적용
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

        // 4. [시각적 처리] sizeDelta는 건드리지 않고 회전만 시킴
        if (rectTransform != null)
        {
            rectTransform.localRotation = Quaternion.Euler(0, 0, -rotationAngle);
            // 안전 장치: 크기는 항상 원본 유지
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
            // 실패 시 원래 상태로 복구
            // startWidth/Height는 '원본(2x1)'이므로, 여기서도 'startRotation'을 넘겨주면
            // Initialize 함수가 알아서 90도 돌리고 크기를 맞춥니다.
            InventoryManager.instance.PlaceItem( 
                itemData, startGridX, startGridY, 
                // 복구할 때는 원본 데이터가 아니라 '배치되어 있던 상태(회전된)'를 넘겨야 안전할 수 있으나
                // PlaceItem 내부 로직상 원본 width/height를 기반으로 다시 계산하므로
                // 여기서는 '회전하기 전' 상태인 startWidth(원본)를 넘기는 게 아니라
                // *** '배치 당시에 차지하고 있던 공간' ***을 넘겨야 합니다.
                // 하지만 Initialize에서 startWidth를 itemData에서 가져오도록 고쳤으므로
                // 아래 값들은 '그리드 점유'용으로만 쓰입니다.
                
                // [수정] 복구 시에는 'Initialize' 당시의 width/height를 넘겨야 함
                // (하지만 startWidth는 이제 원본이므로, 로직이 꼬이지 않게 원본 로직대로 호출)
                // -> 간단하게: 복구는 '배치되어 있던 모양' 그대로 다시 놓는 것.
                // Initialize 때 저장해둔 currentWidth/Height가 아닌, 
                // '드래그 시작 전'의 상태가 필요함. 
                // 편의상 startWidth/Height가 원본이 되었으므로, 
                // InventoryManager가 알아서 회전시킬 수 있도록 처리.
                
                // 여기서는 안전하게 '드래그 시작 전' 모양을 유지하기 위해 아래와 같이 넘깁니다.
                // (PlaceItem은 rotatedWidth/Height를 받음)
                (startRotation % 180 != 0) ? startHeight : startWidth,  // Grid W
                (startRotation % 180 != 0) ? startWidth : startHeight,  // Grid H
                startShape, // 배치되어 있던 모양
                startRotation 
            );
        }
        
        Destroy(gameObject);
    }
}