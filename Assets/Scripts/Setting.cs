using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject quitPanel;
    [SerializeField] private GameObject mapSelectionPanel;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (quitPanel) quitPanel.SetActive(false);
        if (mapSelectionPanel) mapSelectionPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();                 // ★ 입력 축 초기화
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();                 // ★ 새 씬 로드 직후 한 번 더 초기화

        if (pausePanel) pausePanel.SetActive(false);
        if (quitPanel) quitPanel.SetActive(false);
        if (mapSelectionPanel) mapSelectionPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mapSelectionPanel != null && mapSelectionPanel.activeSelf) { mapSelectionPanel.SetActive(false); return; }
            if (quitPanel != null && quitPanel.activeSelf) { CloseQuit(); return; }

            if (pausePanel != null && pausePanel.activeSelf) CloseSetting();
            else SettingOpen();
        }
    }

    // === 다이패널에서 연결할 재시작 버튼 ===
    public void RetryCurrentScene()
    {
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        // 1) 멈춤/사운드/입력 축 초기화
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();                 // ★ 여기서도 초기화

        // 2) UI 포커스가 키보드를 잡고 있던 경우 해제(선택)
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es) es.SetSelectedGameObject(null);

        // 3) 한 프레임 쉬고 로드 (UI/포커스 정리 후 로드가 더 안정적)
        yield return null;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // === 씬 전환 ===
    public void LoadNextScene()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Input.ResetInputAxes();
        SceneManager.LoadScene("Stage1");
    }

    // === 설정(일시정지) ===
    public void SettingOpen()
    {
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseSetting()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (mapSelectionPanel) mapSelectionPanel.SetActive(false);
        Time.timeScale = 1f;
        Input.ResetInputAxes();                 // ★ 일시정지 해제 시에도 축 초기화
    }

    // === 종료 ===
    public void Quit()
    {
        if (quitPanel) quitPanel.SetActive(true);
    }

    public void CloseQuit()
    {
        if (quitPanel) quitPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭됨!");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === 맵 선택 ===
    public void OnStartButtonClicked()
    {
        if (mapSelectionPanel) mapSelectionPanel.SetActive(true);
    }
}
