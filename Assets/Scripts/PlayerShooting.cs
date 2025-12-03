using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject cocoaBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 4f;
    public float fireRate = 1f; // 초당 1발 (60발/분)

    // HyperCandyRush 관련
    private float initialFireRate;
    private float hcrBaseAttackSpeedIncrease = 0f;       // 기본 50% 공격증가 (쿨타임 감소)
    private float hcrMovingAttackSpeedIncrease = 0f;     // 이동 시 누적 공격증가 (최대 30%)

    [Header("Sugar Porridge Skill")]
    public GameObject porridgeBulletPrefab;
    public float porridgeDamage = 12f;      // 폭죽의 기본 데미지
    public float porridgeRadius = 4.96f;     // 폭죽의 폭발 반경
    private float initialPorridgeRadius;
    public float porridgeFlightTime = 1.0f; // 폭죽의 최대 비행 시간 (사거리)

    [Header("SugarBombParty")]
    bool sugarBombPartyEnabled = false;
    public GameObject miniFireworkPrefab; // 미니 폭죽 프리팹
    private const float MINI_FIREWORK_DAMAGE = 6f;
    private const float MINI_FIREWORK_RADIUS = 0.3f;
    private const float MINI_FIREWORK_PROBABILITY = 0.30f; // 미니 폭죽 발사 확률

    private float nextFireTime = 0f;

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError("Player component not found on this GameObject!");
        }

        initialFireRate = fireRate;
        initialPorridgeRadius = porridgeRadius;
    }

    // PlayerShooting.cs (Update 함수 수정)

    void Update()
    {
        // 1. 쿨타임 체크: 발사 시간이 되지 않았으면 함수 종료
        if (Time.time < nextFireTime)
        {
            return;
        }

        // 2. 다음 쿨타임 설정: 발사 시간이 되었으므로, 다음 쿨타임을 먼저 계산하여 설정합니다.
        if (player != null && player.hasHyperCandyRushActive)
        {
            nextFireTime = Time.time + GetCurrentFireCoolDown();
        }
        else
        {
            nextFireTime = Time.time + fireRate;
        }

        // 3. 설탕 폭죽 발사: 아이템을 갖고 있을 때만, 25% 확률로
        if (player != null && player.hasSugarPorridge && porridgeBulletPrefab != null)
        {
            if (Random.Range(0f, 1f) < 0.25f)
            {
                ShootPorridgeBullet();
                return; // 설탕 폭죽이 발사되었으므로, 일반 총알 발사를 막기 위해 함수 종료
            }
        }

        // 4. 슈가 밤 파티의 미니 폭죽
        if (sugarBombPartyEnabled && player.hasSugarBombParty && miniFireworkPrefab != null)
        {
            if (Random.Range(0f, 1f) < MINI_FIREWORK_PROBABILITY)
            {
                float miniRadius = MINI_FIREWORK_RADIUS;

                ShootMiniPorridgeBullet(miniFireworkPrefab, miniRadius, (int)MINI_FIREWORK_DAMAGE);

                // 미니 폭죽이 발사되었으므로, 일반 총알 발사를 막기 위해 함수 종료
                return;
            }
        }

        // 5. 기본 총알 발사 (폭죽들이 발사되지 않았을 때만 실행)
        Shoot();


    }


    void Shoot()
    {
        // 마우스 방향 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = firePoint.position.z; // 🔹 Z값을 같게 설정

        Vector2 direction = ((Vector2)(mousePos - firePoint.position)).normalized;

        // 어떤 탄을 쏠지 결정
        GameObject prefabToUse = bulletPrefab;
        if (player.hasCocoaPowder && cocoaBulletPrefab != null)
        {
            prefabToUse = cocoaBulletPrefab;
        }

        // 총알 생성
        GameObject bullet = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);

        // 회전 방향 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 속도 부여 (항상 bulletSpeed 고정)
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;

        // 딸기팝코어 아이템 관련
        if (player != null && player.hasStrawberryPopCore && player.popCoreSkill != null)
        {
            player.popCoreSkill.UseSkill();
        }

    }

    public void ShootPorridgeBullet()    // 설탕폭죽 발사체 생성과 초기화
    {
        if (porridgeBulletPrefab == null)
        {
            Debug.LogError("PorridgeBulletPrefab is not assigned!");
            return;
        }

        // 마우스 방향 계산 (일반 Shoot 함수와 동일)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = firePoint.position.z;
        Vector2 direction = ((Vector2)(mousePos - firePoint.position)).normalized;

        // 설탕 폭죽 발사체 생성
        GameObject bulletGO = Instantiate(porridgeBulletPrefab, firePoint.position, Quaternion.identity);

        // PorridgeBulletSkill 컴포넌트 가져오기
        PorridgeBulletSkill porridgeSkill = bulletGO.GetComponent<PorridgeBulletSkill>();

        if (porridgeSkill != null)
        {
            // 통합된 Initialize 함수 호출
            porridgeSkill.Initialize(porridgeRadius, (int)porridgeDamage, bulletSpeed, porridgeFlightTime, direction);
        }
        else
        {
            Debug.LogError("PorridgeBulletSkill component not found on the Porridge Bullet Prefab!");
            Destroy(bulletGO);
        }
    }

    private float GetCurrentFireCoolDown()
    {
        float totalIncreaseRate = hcrBaseAttackSpeedIncrease + hcrMovingAttackSpeedIncrease;

        // 공격 속도 증가율: 1 + totalIncreaseRate
        if (totalIncreaseRate > 0)
        {
            return initialFireRate / (1f + totalIncreaseRate);
        }
        return initialFireRate;
    }

    //기본 공격 속도 증가(50%)를 적용/해제 (하이퍼캔디 러쉬)
    public void ApplyHyperCandyRushBaseAttackSpeed(bool activate)
    {
        // 기본 50% 공격 속도 증가
        hcrBaseAttackSpeedIncrease = activate ? 0.50f : 0f;

        if (!activate)
        {
            // 세트 효과 해제 시 이동 누적 효과도 초기화
            hcrMovingAttackSpeedIncrease = 0f;
        }

        Debug.Log($"[HyperCandyRush] 기본 공격 속도: {(hcrBaseAttackSpeedIncrease * 100):0}% 적용");
    }

    public void SetMovingAttackSpeedIncrease(float increase)
    {
        // 0% ~ 30% 범위로 제한
        hcrMovingAttackSpeedIncrease = Mathf.Clamp(increase, 0f, 0.30f);

        // Debug.Log($"[HyperCandyRush] 이동 누적 공속: {(hcrMovingAttackSpeedIncrease * 100):0.##}%");
    }

    public void ShootMiniPorridgeBullet(GameObject prefab, float radius, int damage)
    {
        if (prefab == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = firePoint.position.z;
        Vector2 direction = ((Vector2)(mousePos - firePoint.position)).normalized;

        GameObject bulletGO = Instantiate(prefab, firePoint.position, Quaternion.identity);

        PorridgeBulletSkill porridgeSkill = bulletGO.GetComponent<PorridgeBulletSkill>();

        if (porridgeSkill != null)
        {
            // 미니 폭죽 초기화
            porridgeSkill.Initialize(radius, damage, bulletSpeed, porridgeFlightTime, direction);
        }
        else
        {
            Debug.LogError("PorridgeBulletSkill component not found on the Porridge Bullet Prefab!");
            Destroy(bulletGO);
        }
    }

    public void ActivateSugarBombParty(bool activate)
    {
        if (sugarBombPartyEnabled == activate) return;

        sugarBombPartyEnabled = activate;

        if (activate)
        {
            // 폭죽 반경 30% 증가 적용
            porridgeRadius = initialPorridgeRadius * 1.30f;
            Debug.Log($"[SugarBombParty] 설탕 폭죽 반경 30% 증가 적용: {initialPorridgeRadius:0.##} -> {porridgeRadius:0.##}");
        }
        else
        {
            // 초기값으로 복원
            porridgeRadius = initialPorridgeRadius;
            Debug.Log($"[SugarBombParty] 설탕 폭죽 반경 복원: {porridgeRadius:0.##}");
        }
    }
}
