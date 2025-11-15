using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UI 포커스 해제용

public class Stage1Setting : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드시 상태 복구
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);

        // 혹시 멈춰있을 수 있으니 복구 + 입력축 리셋 + UI 포커스 해제
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // 재시작/씬 전환 직후에도 확실히 복구
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);

        if (pausePanel) pausePanel.SetActive(false);
        isPaused = false;

        // ★ 플레이어 체력/상태 보장 (DontDestroyOnLoad 대응)
        var p = FindObjectOfType<Player>();
        if (p) p.ResetForRetry();
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

            // 일시정지 해제 시 입력 축/포커스 초기화
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

    private System.Collections.IEnumerator GoMainRoutine()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // ★ 메인 넘어가기 전에 남아있는 Player(또는 여러 개) 전부 제거
        var players = FindObjectsOfType<Player>(true);
        foreach (var p in players)
            Destroy(p.gameObject);

        // ★ 파괴가 프레임 끝에 반영되므로 한 프레임 쉬고 로드
        yield return null;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void OnClickRetry()
    {
        StartCoroutine(LoadRoutine("Stage1"));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        // 정지 해제/사운드/입력 축 초기화 + UI 포커스 해제
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        var es = EventSystem.current; if (es) es.SetSelectedGameObject(null);

        // 한 프레임 쉬고 로드(축/포커스 초기화 안정화)
        yield return null;

        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
