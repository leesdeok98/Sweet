using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeGauge : MonoBehaviour
{
    [Header("시간 설정")]
    public float totalTime = 300f;      // 전체 시간(초) = 최대 시간
    public float itemInterval = 30f;    // 아이템 선택 창 뜨는 간격(초)
    private float currentTime = 0f;     // 누적된 현재 시간
    private float lastItemTime = 0f;    // 마지막으로 아이템 창 띄운 시간

    [Header("UI 연결")]
    public ItemSelectionUI itemSelectionUI; // Inspector에서 연결
    public TextMeshProUGUI timeText;
    public Slider timeSlider;               // UI 슬라이더 연결



    void Start()
    {
        // 슬라이더 초기 설정
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = totalTime;
            timeSlider.value = 0f;
        }

        currentTime = 0f;
        lastItemTime = 0f;

        UpdateUIText(currentTime);
    }

    void Update()
    {
        // 시간 측정 및 슬라이더 업데이트
        TimeCalculation();

        // UI 텍스트 업데이트
        UpdateUIText(currentTime);

        // 이벤트 체크 (아이템 선택 창)
        CheckForIntervalEvents();

        // 게임 종료 조건 체크 (클리어)
        CheckGameClear();
    }

    /* void Update()
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
    } */

    void TimeCalculation()
    {
        // Time.timeScale이 0일 때는 Time.deltaTime도 0이므로, 게임이 자동으로 일시 정지됩니다.
        currentTime += Time.deltaTime;

        if (timeSlider != null)
        {
            // 현재 시간을 최대 시간을 넘지 않도록 제한합니다.
            timeSlider.value = Mathf.Min(currentTime, totalTime);
        }
    }

    void UpdateUIText(float time)
    {
        if (timeText == null) return;

        // 소수점 버리고 정수로 변환
        int totalSeconds = (int)time;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        // "D2" 서식을 사용해 항상 2자리로 표시 (예: "05:15")
        timeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    void CheckForIntervalEvents()
    {
        // itemInterval 간격이 지났고, 아직 totalTime에 도달하지 않았을 때
        if (currentTime >= totalTime) return;

        if (currentTime - lastItemTime >= itemInterval)
        {
            lastItemTime = currentTime;

            if (itemSelectionUI != null)
            {
                itemSelectionUI.Open(); // Time.timeScale = 0; 및 UI 오픈 로직 실행
            }
            else
            {
                Debug.LogWarning("ItemSelectionUI가 연결되지 않았습니다. 아이템 선택 창을 띄울 수 없습니다.");
            }
        }
    }

    void CheckGameClear()
    {
        if (currentTime >= totalTime)
        {
            // 최대 시간에 도달한 후 한 번만 GameClear()를 호출하도록 추가적인 체크를 할 수 있지만,
            // 여기서는 Update()에서 매번 호출되더라도 GameClear() 내부에서 중복 처리하면 됩니다.
            // 일단은 간단하게 로직만 호출합니다.
            GameClearLogic();
        }
    }

    void GameClearLogic()
    {
        // To Do: 실제 게임 클리어 화면 전환 및 보상 로직 구현
        Debug.Log("게임 클리어! " + totalTime / 60 + "분 버티기 성공!");
        // 여기서 Time.timeScale = 0; 을 설정하거나 Scene 전환 등을 할 수 있습니다.
    }



    void GameClear()
    {
        Debug.Log(" 5분 버티기 성공!");
        // 게임 클리어 로직 추가
    }
}
