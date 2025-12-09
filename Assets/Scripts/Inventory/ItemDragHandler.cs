using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static readonly System.Collections.Generic.List<ItemDragHandler> activeDrags
        = new System.Collections.Generic.List<ItemDragHandler>();
    private InventoryItemData itemData;

    // 원본 데이터 (복구 및 시각적 기준)
    private int startGridX, startGridY;
    private int startWidth, startHeight; // 원본 너비/높이 (UI 크기용)
    private int startRotation;
    private bool[] startShape; // 배치 당시의 모양 (복구용)

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

        // 아이템이 null일 경우 width/height로 대체
        this.startWidth = (item != null) ? item.width : width;
        this.startHeight = (item != null) ? item.height : height;

        // shape이 null이면 안전하게 새 배열 생성
        if (shape != null)
        {
            this.startShape = (bool[])shape.Clone();
            this.currentShape = (bool[])shape.Clone();
        }
        else
        {
            this.startShape = new bool[width * height];
            this.currentShape = new bool[width * height];
        }

        this.startRotation = rotation;

        // 현재 상태 (회전된 상태 반영)
        this.currentWidth = width;
        this.currentHeight = height;
        this.rotationAngle = rotation;

        // 혹시 Awake 전에 Initialize가 호출될 경우 대비
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (itemImage == null) itemImage = GetComponent<Image>();

        // RectTransform 관련 처리는 null 체크 후 진행
        if (rectTransform != null)
        {
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(startWidth * SLOT_SIZE, startHeight * SLOT_SIZE);
            rectTransform.localRotation = Quaternion.Euler(0, 0, -rotation);
        }

        // 아이콘 세팅
        // 아이콘 세팅 (그리드 크기에 맞게, NativeSize 사용 안 함)
        if (itemImage != null && item != null)
        {
            itemImage.sprite = item.icon;
            // itemImage.SetNativeSize();  // ← 이 줄 삭제
        }

        // 아이템 레이어 Rect 연결
        if (InventoryManager.instance != null)
        {
            this.itemLayerRect = InventoryManager.instance.GetItemLayerRect();
        }
        else
        {
            this.itemLayerRect = null; // instance가 아직 없으면 나중에 OnBeginDrag에서 다시 받음
        }
    }

    public InventoryItemData GetItemData() => itemData;
    public void SetDropSuccess(bool success) => this.dropSuccessful = success;

    void Update()
    {
        // R로 회전
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
            if (Input.mousePresent)
                SnapToGrid(Input.mousePosition);
        }

        // ESC로 드래그 취소 + 원래 자리로 복귀
        if (isDragging && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDragAndRestore();
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

        // 3. 모양 배열 회전 (currentShape가 null일 가능성 방어)
        if (currentShape == null)
        {
            currentShape = new bool[currentWidth * currentHeight];
        }

        bool[] newShape = new bool[currentWidth * currentHeight];
        int oldW = currentHeight;
        int oldH = currentWidth;

        for (int y = 0; y < oldH; y++)
        {
            for (int x = 0; x < oldW; x++)
            {
                int oldIndex = y * oldW + x;
                if (oldIndex < 0 || oldIndex >= currentShape.Length) continue;

                int newX = (oldH - 1) - y;
                int newY = x;
                int newIndex = newY * currentWidth + newX;
                if (newIndex < 0 || newIndex >= newShape.Length) continue;

                newShape[newIndex] = currentShape[oldIndex];
            }
        }
        currentShape = newShape;

        // 4. 시각적 처리
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

        if (!activeDrags.Contains(this))
            activeDrags.Add(this);

        // 그리드 데이터에서 잠시 제거
        InventoryManager.instance.RemoveDataOnly(itemData);

        // 최상단으로 올리기
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        // itemLayerRect가 비어있으면 여기서 다시 받기
        if (this.itemLayerRect == null)
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
        if (itemLayerRect == null || rectTransform == null) return;

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
        activeDrags.Remove(this);
        isDragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
        if (itemImage != null)
            itemImage.color = Color.white;

        if (InventoryManager.instance == null)
        {
            Destroy(gameObject);
            return;
        }

        // 드랍 성공 안 했으면 → 원래 자리로 복구
        if (!dropSuccessful)
        {
            RestoreToStartCell();
        }

        Destroy(gameObject);
    }

    // ESC로 드래그 취소할 때 호출
    // ESC로 드래그 취소할 때 호출
    private void CancelDragAndRestore()
    {
        // 이미 취소되었거나 드래그 중이 아니면 그냥 무시 (두 번 호출 대비)
        if (!isDragging)
            return;

        isDragging = false;

        // 리스트에서도 제거
        activeDrags.Remove(this);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
        if (itemImage != null)
            itemImage.color = Color.white;

        // 드래그 취소도 "실패한 드랍"과 동일하게 원위치 복구
        RestoreToStartCell();

        // 드래그용 UI 제거
        Destroy(gameObject);
    }

    // ★ ESC 메뉴에서 한 번에 정리할 때 사용하는 static 메서드
    public static void CancelAllDragsAndRestore()
    {
        // Destroy가 들어가 있으니 뒤에서부터 순회
        for (int i = activeDrags.Count - 1; i >= 0; i--)
        {
            if (activeDrags[i] != null)
            {
                activeDrags[i].CancelDragAndRestore();
            }
        }

        activeDrags.Clear();
    }




    // 실제 복구 로직 (OnEndDrag 실패, ESC 취소 둘 다 여기 사용)
    private void RestoreToStartCell()
    {
        if (InventoryManager.instance == null || itemData == null)
            return;

        int restoreW = startWidth;
        int restoreH = startHeight;

        if (startRotation % 180 != 0)
        {
            restoreW = startHeight;
            restoreH = startWidth;
        }

        bool[] shapeToUse = startShape ?? currentShape;
        if (shapeToUse == null)
        {
            shapeToUse = new bool[restoreW * restoreH];
        }

        InventoryManager.instance.PlaceItem(
            itemData,
            startGridX,
            startGridY,
            restoreW,
            restoreH,
            shapeToUse,
            startRotation
        );
    }
}
