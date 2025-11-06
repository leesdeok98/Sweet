using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;       // 아이템 이름
    public string description;    // 아이템 설명
    public Sprite icon;           // 아이콘 이미지 (Inspector에서 직접 넣기)
    public ItemType type;         // 아이템의 종류 (버프 / 회복 / 폭탄 등)
    public float value;           // 수치 (예: 회복량, 배율, 지속시간 등)

    public enum ItemType
    {
        CaramelCube,        // 카라멜 큐브 (1)
        SugarShield,        // 슈가 실드 (2)
        StrawberryPopCore,  // 딸기 팝코어 (3)
        RollingChocolateBar, // 롤링 초코바 (4)
        CocoaPowder,        // 코코아 파우더 (5)
        SnowflakeCandy,     // 눈꽃 사탕 (6)
        PoppingCandy,       // 팝핑 캔디 (7)
        DarkChip,           // 다크 칩 (8)
        IcedJelly,          // 아이스 젤리 (9)
        SyrupTornado,       // 시럽 토네이도 (10)
        HoneySpin,          // 허니 스핀 (11)
        SugarPorridge       // 설탕 폭죽 (12)
    }

}
