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
    }

    void Update()
    {
        // 자동 사격
        if (Time.time >= nextFireTime)
        {
            Shoot();

            if (player != null && player.hasHyperCandyRushActive)
            {
                // HyperCandyRush 활성화 시: 공격속도 증가가 반영된 쿨타임 사용
                nextFireTime = Time.time + GetCurrentFireCoolDown();
            }
            else
            {
                // HyperCandyRush 비활성화 시: 원래의 fireRate 변수(기본 쿨타임) 사용
                nextFireTime = Time.time + fireRate;
            }
        }
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
}
