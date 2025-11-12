// KillCounterUI.cs
using UnityEngine;
using TMPro; // TextMeshProUGUI 사용

public class KillCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private string format = "Kills: {0}"; // 한글 원하면 "처치: {0}"

    // 🔹 정적 이벤트 없이도 간단히 브로드캐스트 받을 수 있도록 static 메서드 제공
    private static KillCounterUI _instance;

    void Awake()
    {
        _instance = this;
        if (killText == null)
        {
            killText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        // 시작 시 현재 값 표시 (씬 재시작/로드 대비)
        int current = KillCounter.Instance != null ? KillCounter.Instance.KillCount : 0;
        Apply(current);
    }

    private void Apply(int value)
    {
        if (killText != null)
            killText.text = string.Format(format, value);
    }

    // 🔹 KillCounter에서 호출
    public static void NotifyKillCountChanged(int value)
    {
        if (_instance != null)
            _instance.Apply(value);
    }
}
