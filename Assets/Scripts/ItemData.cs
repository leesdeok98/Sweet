using UnityEngine;

[System.Serializable]
public class ItemData
{
    // --- 1. 유저님의 '원래' 변수들 (100% '유지') ---
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType type; 
    //public float value;

    // --- 2. '테트리스' (5x5) '전용' '추가' 변수들 ---
    public int width;
    public int height;
    public bool[,] shape; 

    // --- 3. ★★★ "None" + "유저님의 12개 목록"이 합쳐진 '최종' Enum ★★★ ---
    public enum ItemType
    {
        None,               // ★ (제가 추가했던 '스킬 없음' - 필수)

        // (↓ 사용자님의 '마스터 목록' 12개)
        CaramelCube,        
        SugarShield,        
        StrawberryPopCore,  
        RollingChocolateBar, 
        CocoaPowder,        
        SnowflakeCandy,     
        PoppingCandy,       // (이전 버전에서 누락됨)
        DarkChip,           
        IcedJelly,          
        SyrupTornado,       // (이전 버전에서 누락됨)
        HoneySpin,          // (이전 버전에서 누락됨)
        SugarPorridge       // (이전 버전에서 누락됨)
    }
    // ---

    // (생성자 - 수정 없음)
    public ItemData(ItemData original, int w, int h, bool[,] s)
    {
        this.itemName = original.itemName;
        this.description = original.description;
        this.icon = original.icon;
        this.type = original.type;
        //this.value = original.value;
        this.width = w;
        this.height = h;
        this.shape = s;
    }

    // (ActivateAbility 함수 - 수정 없음)
    public void ActivateAbility()
    {
        Debug.Log(itemName + "의 '능력'이 '발동'되었습니다!");
        
        switch (type)
        {
            case ItemType.CaramelCube:
                // ...
                break;
            case ItemType.SugarShield:
                // ...
                break;
        }
    }

    // (DeactivateAbility 함수 - 수정 없음)
    public void DeactivateAbility()
    {
        Debug.Log(itemName + "의 '능력'이 '비활성'되었습니다.");
        
        switch (type)
        {
            case ItemType.CaramelCube:
                // ...
                break;
        }
    }
}