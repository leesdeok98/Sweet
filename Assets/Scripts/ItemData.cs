using UnityEngine;

[System.Serializable]
public class ItemData
{
    
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType type; 
    //public float value;

    // 테트리스 (5x5) 전용 추가 변수들
    public int width;
    public int height;
    public bool[,] shape; 

    // 아이템 목록
    public enum ItemType
    {
        None,               // 스킬 없음(지우면안됨)

        
        CaramelCube,        //카라멜 큐브
        SugarShield,        // 슈가 실드
        StrawberryPopCore,  //딸기 팝 코어
        RollingChocolateBar, // 롤링 초코 바
        CocoaPowder,        // 코코아 파우더
        SnowflakeCandy,     // 눈꽃 사탕
        PoppingCandy,       // 팝핑 캔디
        DarkChip,           // 다크 칩
        IcedJelly,          // 아이스 젤리
        SyrupTornado,       // 시럽 토네이도
        HoneySpin,          // 허니 스핀
        SugarPorridge       // 설탕 폭죽
    }
    

    
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