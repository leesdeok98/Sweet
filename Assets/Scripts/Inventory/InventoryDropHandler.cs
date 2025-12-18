using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropHandler : MonoBehaviour, IDropHandler
{
    private const float SLOT_SIZE = 100f;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        ItemDragHandler dragHandler = draggedObject.GetComponent<ItemDragHandler>();
        if (dragHandler == null) return;

        
        InventoryItemData itemData = dragHandler.GetItemData();
        int rotatedWidth = dragHandler.currentWidth;
        int rotatedHeight = dragHandler.currentHeight;
        bool[] rotatedShape = dragHandler.currentShape;
        int rotationAngle = dragHandler.rotationAngle;

        RectTransform dropAreaRect = GetComponent<RectTransform>();
        Vector2 localPoint; 
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dropAreaRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint
        );

        int targetX = Mathf.FloorToInt(localPoint.x / SLOT_SIZE);
        int targetY = Mathf.FloorToInt(-localPoint.y / SLOT_SIZE);
        
        //  InventoryManager.instance.PlaceItem 호출
        bool success = InventoryManager.instance.PlaceItem(
            itemData, 
            targetX, 
            targetY, 
            rotatedWidth, 
            rotatedHeight, 
            rotatedShape, 
            rotationAngle
        );

        if (success)
        {
            //배치 성공 시 플래그만 설정합니다.
            dragHandler.SetDropSuccess(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.ItemDrop_SFX);
            
        }
    }
}