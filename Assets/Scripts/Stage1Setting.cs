using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Stage1Setting : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 이벤트 등록
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);

        // 화면 들어올 때 기본 상태 세팅
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // 씬 전환 직후 기본값 재설정
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);

        if (pausePanel) pausePanel.SetActive(false);
        isPaused = false;

        // 🔹 GameManager 상태 초기화
        if (GameManager.instance != null)
            GameManager.instance.ResetState();

        // 🔹 Player 찾기
        var p = FindObjectOfType<Player>();
        if (p != null)
        {
            // HP/상태 리셋
            p.ResetForRetry();

            // 🔹 이번 씬(Canvas)에서 새로 생긴 "사망 패널"을 찾아서 Player에 다시 연결
            //    - Canvas 이름: "Canvas"
            //    - 사망 패널 이름: "사망 패널"  (스크린샷 기준)
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                var diePanelTr = canvas.transform.Find("사망 패널");
                if (diePanelTr != null)
                {
                    p.SetDiePanel(diePanelTr.gameObject);
                }
                else
                {
                    Debug.LogWarning("[Stage1Setting] Canvas 안에서 '사망 패널'을 찾지 못했습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[Stage1Setting] 'Canvas' 오브젝트를 찾지 못했습니다.");
            }
        }

        // (필요하면 여기서 KillCounter.Instance.ResetCount() 같은 것도 호출 가능)
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel) pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pausePanel) pausePanel.SetActive(false);

            // 재개할 때 입력/선택 초기화
            Input.ResetInputAxes();
            var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);
    }

    public void QuitToMain()
    {
        StartCoroutine(GoMainRoutine());
    }

    private IEnumerator GoMainRoutine()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 현재 씬에 남아 있는 Player(또는 복수) 제거
        var players = FindObjectsOfType<Player>(true);
        foreach (var p in players)
            Destroy(p.gameObject);

        // 파괴 반영을 위해 한 프레임 대기
        yield return null;

        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void OnClickRetry()
    {
        StartCoroutine(LoadRoutine("Stage1"));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        // 씬 전환 전 상태 초기화
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);

        // 한 프레임 건너뛰기(UI/입력 정리 후 로드)
        yield return null;

        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
