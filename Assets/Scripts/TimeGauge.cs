using UnityEngine;
using UnityEngine.UI;

public class TimeGauge : MonoBehaviour
{
    public Slider timeSlider;           // UI 슬라이더 연결
    public float totalTime = 300f;      // 전체 시간(초)
    public float itemInterval = 30f;    // 아이템 선택 창 뜨는 간격(초)
    private float currentTime = 0f;
    private float lastItemTime = 0f;
    public ItemSelectionUI itemSelectionUI; // Inspector에서 연결

    void Start()
    {
        timeSlider.minValue = 0f;
        timeSlider.maxValue = totalTime;
        timeSlider.value = 0f;
        currentTime = 0f;
        lastItemTime = 0f;
    }

    void Update()
    {
        // Time.timeScale == 0 이면 Time.deltaTime == 0 이므로 자연스럽게 멈춤
        currentTime += Time.deltaTime;
        timeSlider.value = currentTime;

        // 전체 시간 도달 시 처리
        if (currentTime >= totalTime)
        {
            currentTime = totalTime;
            GameClear();
            return;
        }

        // 아이템 창 띄울 조건: itemInterval 간격 지났고, 아직 아이템창 열려있지 않을 때
        if (currentTime - lastItemTime >= itemInterval)
        {
            // 추가: 마지막 선택 이후 충분한 시간 지났는지 체크
            lastItemTime = currentTime;
            if (itemSelectionUI != null)
            {
                itemSelectionUI.Open(); // 일시정지 및 UI 오픈
            }
            else
            {
                Debug.LogWarning("ItemSelectionUI가 연결되지 않았습니다 (Inspector 확인).");
            }
        }
    }

    void GameClear()
    {
        Debug.Log("🎉 5분 버티기 성공!");
        // 게임 클리어 로직 추가
    }
}
