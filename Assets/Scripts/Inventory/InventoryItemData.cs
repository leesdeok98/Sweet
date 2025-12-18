// InventoryItemData.cs

using UnityEngine;

[CreateAssetMenu(fileName = "New InventoryItem", menuName = "Inventory/Inventory Item Data")]
public class InventoryItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public Sprite icon;

    //여기에 '스킬 타입' 변수를 추가합니다
    [Header("Skill Link")]
    // (주의: ItemData.cs 파일의 ItemType enum을 참조합니다)
    public ItemData.ItemType skillType = ItemData.ItemType.None; // 'None'을 기본값으로
    

    [Header("Item Shape (Tetris)")]
    public int width = 1;
    public int height = 1;
    public bool[] shape; // (1D 배열)

    // (x, y) 좌표가 채워져 있는지 확인하는 헬퍼 함수
    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return false; // 범위 밖
        }
        // (1D -> 2D 인덱스 변환: y * width + x)
        return shape[y * width + x];
    }
}