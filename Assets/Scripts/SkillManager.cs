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
    public Player player; // 네 프로젝트의 Player 스크립트 타입에 맞게 연결

    public void Apply(ItemData item)
    {
        if (item == null) return;

        switch (item.type)
        {
            case ItemData.ItemType.CaramelCube:
                ApplySpeedBuff(item.value, 5f); // 이동속도 증가
                break;
            case ItemData.ItemType.SugarShield:
                ApplyShield(item.value, 20f); // 자동 쉴드 생성
                break;
            case ItemData.ItemType.StrawberryPopCore:
                ApplyExtraProjectile(item.value); // 추가 투사체
                break;
            case ItemData.ItemType.RollingChocolateBar:
                ApplyRollingBar(item.value, 5f); // 굴러가는 초코바 소환
                break;
            case ItemData.ItemType.CocoaPowder:
                ApplyAreaAttack(item.value); // 공격 범위 확장
                break;
            case ItemData.ItemType.SnowflakeCandy:
                ApplyFreeze(item.value, 1f); // 빙결 공격
                break;
            case ItemData.ItemType.PoppingCandy:
                ApplySplashDamage(item.value); // 폭발형 사탕 조각
                break;
            case ItemData.ItemType.DarkChip:
                ApplyAttackBuff(item.value, 8f); // 공격력 상승
                break;
            case ItemData.ItemType.IcedJelly:
                ApplyColdSpread(item.value, 3f); // 냉기 확산
                break;
            case ItemData.ItemType.SyrupTornado:
                ApplySyrupTornado(item.value); // 시럽 회오리 생성
                break;
            case ItemData.ItemType.HoneySpin:
                ApplyHoneySpin(item.value); // 회전 꿀 공격
                break;
            case ItemData.ItemType.SugarPorridge:
                ApplySugarWave(item.value); // 슬로우 웨이브 생성
                break;
        }
    }

    // ──────────────────────────────
    // 아이템별 효과 함수
    // ──────────────────────────────

    void ApplySpeedBuff(float power, float duration)
    {
        Debug.Log($"🏃‍♂️ 카라멜 큐브: 이동속도 +100% (지속 {duration}초)");
        // 5초간 이동속도 +100%, 쿨타임 15초
    }

    void ApplyShield(float shieldPower, float cooldown)
    {
        Debug.Log($"🛡️ 설탕 방패: 자동 방어막 생성 (쿨타임 {cooldown}초, 최대 1회)");
        // 20초마다 자동 방어막 생성, 최대 1회 중첩
    }

    void ApplyExtraProjectile(float multiplier)
    {
        Debug.Log($"🍓 딸기팝 코어: 추가 투사체 발사 (공격력 {multiplier * 100 - 100}% 증가)");
        // 기본 공격 시 30% 확률로 추가 투사체 발사
    }

    void ApplyRollingBar(float damage, float interval)
    {
        Debug.Log($"🍫 굴러가는 초코바: {interval}초마다 소환 (데미지 {damage})");
        // 5초마다 초코바 생성, 반경 2m 내 적 타격
    }

    void ApplyAreaAttack(float radius)
    {
        Debug.Log($"☕ 코코아 가루: 공격 범위 +{radius * 0.2f}m 증가");
        // 기본 공격이 범위 공격으로 변경, 반경 0.2m 증가
    }

    void ApplyFreeze(float chance, float duration)
    {
        Debug.Log($"❄️ 눈송이 사탕: {chance * 100}% 확률로 적을 {duration}초간 빙결");
        // 기본 공격 시 15% 확률로 적 1초간 빙결
    }

    void ApplySplashDamage(float damage)
    {
        Debug.Log($"🍭 팝핑 캔디: 폭발형 사탕 조각 (데미지 {damage}, 반경 2m)");
        // 20% 확률로 공격 시 8방향 사탕 조각 생성 (각 2데미지)
    }

    void ApplyAttackBuff(float multiplier, float duration)
    {
        Debug.Log($"🌑 다크칩: 공격력 +{(multiplier - 1f) * 100}% (지속 {duration}초)");
        // 공격력 +30%
    }

    void ApplyColdSpread(float damage, float duration)
    {
        Debug.Log($"🧊 아이스젤리: 냉기 확산 (반경 1.5m, 초당 피해 {damage}, {duration}초 지속)");
        // 10% 확률로 냉기 폭발 발생, 냉기 속도 -50%
    }

    void ApplySyrupTornado(float damage)
    {
        Debug.Log($"🌪️ 시럽토네이도: 회전 시럽 생성 (반경 2m, 초당 피해 {damage})");
        // 공격 4회마다 반경 2m 시럽 회오리 생성
    }

    void ApplyHoneySpin(float damage)
    {
        Debug.Log($"🐝 허니스핀: 회전 꿀 생성 (데미지 {damage}, 타격 시 1초간 이동속도 -30%)");
        // 몸 주위에 회전 꿀 2개 생성, 타격 시 적 이동속도 감소
    }

    void ApplySugarWave(float damage)
    {
        Debug.Log($"🎆 설탕 죽: 슬로우 웨이브 생성 (데미지 {damage}, 반경 1m)");
        // 공격 시 25% 확률로 광역 폭발 (반경 1m, 데미지 12)
    }
}
