using UnityEngine;
using UnityEngine.UI;

public class TimeGauge : MonoBehaviour
{
    public Slider timeSlider;      // UI 슬라이더 연결
    public float totalTime = 300f; // 5분 (초 단위)
    private float currentTime = 0f;

    void Start()
    {
        timeSlider.minValue = 0f;
        timeSlider.maxValue = totalTime;
        timeSlider.value = 0f; // 시작 시 게이지 0으로
    }

    void Update()
    {
        currentTime += Time.deltaTime;      // 시간 누적
        timeSlider.value = currentTime;     // 게이지가 차오름

        if (currentTime >= totalTime)
        {
            currentTime = totalTime;
            GameClear();
        }
    }

    void GameClear()
    {
        Debug.Log("🎉 5분 버티기 성공!");
        // 여기에 게임 클리어 로직 (예: UI 띄우기, 멈추기 등)
    }
}
