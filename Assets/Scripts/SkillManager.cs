using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 예시: 플레이어 참조 (Inspector에 연결)
    public Player player; // 네 프로젝트의 플레이어 스크립트 타입에 맞게 변경

    public void Apply(ItemData item)
    {
        if (item == null) return;

        switch (item.type)
        {
            case ItemData.ItemType.CaramelCube:
                ApplyHeal(item.value);
                break;
            case ItemData.ItemType.SugarShield:
                ApplyShield(item.value, 10f);
                break;
            case ItemData.ItemType.StrawberryPopCore:
                ApplyAttackBuff(item.value, 10f);
                break;
            case ItemData.ItemType.RollingChocolateBar:
                ApplySpeedBuff(item.value, 10f);
                break;
            case ItemData.ItemType.CocoaPowder:
                ApplyBomb(item.value);
                break;
            case ItemData.ItemType.SnowflakeCandy:
                ApplyFreeze(item.value, 5f);
                break;
            case ItemData.ItemType.PoppingCandy:
                ApplySplashDamage(item.value);
                break;
            case ItemData.ItemType.DarkChip:
                ApplyDarkMode(item.value, 8f);
                break;
            case ItemData.ItemType.IcedJelly:
                ApplySlowEnemies(item.value, 5f);
                break;
            case ItemData.ItemType.SyrupTornado:
                ApplySpinAttack(item.value, 6f);
                break;
            case ItemData.ItemType.HoneySpin:
                ApplyStickyEffect(item.value, 7f);
                break;
            case ItemData.ItemType.SugarPorridge:
                ApplyExplosion(item.value);
                break;
        }
    }

    // ──────────────────────────────
    // 아래는 각각의 아이템 효과 예시 함수들
    // (원하는 효과에 맞게 구현 변경 가능)
    // ──────────────────────────────

    void ApplyHeal(float amount)
    {
        Debug.Log($"🍬 카라멜큐브: 체력 {amount} 회복!");
        // 기본 공격 속도 +100%, 5초간 유지, 재사용 대기 15초
    }

    void ApplyShield(float shieldValue, float duration)
    {
        Debug.Log($"🛡️ 슈가실드: {duration}초간 보호막 생성 (강도 {shieldValue})");
        // 자동 방어막 생성 (20초마다 1회, 최대 1회 저장)
    }

    void ApplyAttackBuff(float multiplier, float duration)
    {
        Debug.Log($"🍓 딸기팝코어: 공격력 {multiplier}배 (지속 {duration}초)");
        // 기본 공격 시 30% 확률로 사방으로 탄 추가 발사
    }

    void ApplySpeedBuff(float multiplier, float duration)
    {
        Debug.Log($"🍫 롤링초코바: 이동속도 {multiplier}배 (지속 {duration}초)");
        // 5초마다 긴 초코바가 플레이어를 중심으로 반지름 2m의 원형태를 한번 그린 후 사라짐 (데미지 15, 작은 넉백 있음)
    }

    void ApplyBomb(float damage)
    {
        Debug.Log($"💥 코코아파우더: 폭발 데미지 {damage}");
        // 기본 공격 색깔이 갈색으로 바뀌고 넉백 추가 (작은 넉백 + 경직 0.2초)
    }

    void ApplyFreeze(float power, float duration)
    {
        Debug.Log($"❄️ 눈꽃사탕: 적 동결 ({duration}초)");
        //기본공격 시 탄 테두리에 하늘빛이 나면서15 % 확률로 적을 1초간 빙결하는 탄 발사
    }

    void ApplySplashDamage(float damage)
    {
        Debug.Log($"🍭 팝핑캔디: 주변 범위 피해 {damage}");
        //20% 확률로 기본 공격이 맞은 후 8개의 별사탕조각이 반지름 2m의 원 형태로 퍼짐(개당데미지 2)

    }

    void ApplyDarkMode(float power, float duration)
    {
        Debug.Log($"🌑 다크칩: 어둠 강화모드 (지속 {duration}초)");
        //공격력 +30%
    }

    void ApplySlowEnemies(float slowPower, float duration)
    {
        Debug.Log($"🧊 아이스젤리: 적 속도 {slowPower} 감소 ({duration}초)");
        //10% 확률로 주변 적에게 냉기를 확산 시키는 탄 발사 (범위 반지름 1.5m 원, 초당데미지 2,3초 유지 후 사라짐, 냉기 속에 있을때 속도 - 50 %)
    }

    void ApplySpinAttack(float damage, float duration)
    {
        Debug.Log($"🍯 시럽토네이도: 회전 공격 ({duration}초, 데미지 {damage})");
        //주변 적에게 매 초마다 4 데미지를 주는 반지름 2m의 원형태 상시유지되는 시럽이 생김.

    }

    void ApplyStickyEffect(float slowPower, float duration)
    {
        Debug.Log($"🐝 허니스핀: 적 느려짐 ({duration}초, 효과 {slowPower})");
        //몸을 중심으로 상시유지되는 회전하는 꿀이 2개 생김. (개당데미지 5) 적에게 타격 시 1초간 이동속도 -30 %

    }

    void ApplyExplosion(float damage)
    {
        Debug.Log($"🎆 설탕폭죽: 대폭발 데미지 {damage}");
        //일반 공격 시 25% 확률로 광역공격을 하는 폭죽을 추가 발사 
        //(타격 부분에서 반지름 1m 크기의 원형 범위폭발, 데미지 12)
    }
}
