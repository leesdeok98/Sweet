// KillCounter.cs
using UnityEngine;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }
    public int KillCount { get; private set; } = 0;

    [Header("Options")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        Enemy.OnAnyEnemyDied += HandleEnemyDied;
    }

    void OnDisable()
    {
        Enemy.OnAnyEnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied()
    {
        KillCount++;
        // 🔹 UI가 자동 갱신되도록 알림 (UI가 구독해서 즉시 반영)
        KillCounterUI.NotifyKillCountChanged(KillCount);
    }

    // 외부에서 초기화/리셋이 필요할 때 호출
    public void ResetCount()
    {
        KillCount = 0;
        KillCounterUI.NotifyKillCountChanged(KillCount);
    }
}
