using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject rootPanel;   // 전체 패널 (BossHpPanel)
    [SerializeField] private Slider hpSlider;        // 슬라이더

    private EnemyBoss currentBoss;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (rootPanel == null)
            rootPanel = gameObject;   // 필요하면 자기 자신을 패널로 사용

        // 시작할 때는 숨겨두기
        rootPanel.SetActive(false);
    }

    /// <summary>
    /// 보스가 스폰될 때 EnemyBoss가 자신을 등록
    /// </summary>
    public void RegisterBoss(EnemyBoss boss)
    {
        currentBoss = boss;

        if (currentBoss == null)
        {
            Hide();
            return;
        }

        rootPanel.SetActive(true);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = currentBoss.maxHealth;
            hpSlider.value = currentBoss.health;
        }
    }

    void Update()
    {
        if (currentBoss == null)
        {
            Hide();
            return;
        }

        // 죽었거나 풀에 의해 비활성화되면 숨김
        if (!currentBoss.gameObject.activeInHierarchy || currentBoss.health <= 0f)
        {
            Hide();
            return;
        }

        if (hpSlider != null)
        {
            hpSlider.value = currentBoss.health;
        }
    }

    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        currentBoss = null;
    }
}
