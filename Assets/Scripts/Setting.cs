using System.Collections;
using System.Collections.Generic;
// using Unity.VisualScripting.Antlr3.Runtime; // ❌ 불필요하면 제거
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;        // 일시정지/설정
    [SerializeField] private GameObject quitPanel;         // 종료 확인
    [SerializeField] private GameObject mapSelectionPanel; // 맵 선택

    void Start()
    {
        // 처음에는 모두 비활성화
        if (pausePanel) pausePanel.SetActive(false);
        if (quitPanel) quitPanel.SetActive(false);
        if (mapSelectionPanel) mapSelectionPanel.SetActive(false);

        // 혹시 타임스케일이 멈춰있을 수 있으니 복구
        Time.timeScale = 1f;
    }

    void Update()
    {
        // ESC 처리: 가장 위(활성) 패널을 우선적으로 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1) 맵 선택이 열려 있으면 그거부터 닫기
            if (mapSelectionPanel != null && mapSelectionPanel.activeSelf)
            {
                mapSelectionPanel.SetActive(false);
                return;
            }

            // 2) 종료 패널이 열려 있으면 닫기
            if (quitPanel != null && quitPanel.activeSelf)
            {
                CloseQuit();
                return;
            }

            // 3) 일시정지/설정 패널 토글
            if (pausePanel != null && pausePanel.activeSelf)
            {
                CloseSetting();
            }
            else
            {
                SettingOpen();
            }
        }
    }

    // === 씬 전환 ===
    public void LoadNextScene()
    {
        // 게임 재개 상태로 전환 후 씬 로드
        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage1");
    }

    // === 설정(일시정지) 열기/닫기 ===
    public void SettingOpen()
    {
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지
    }

    public void CloseSetting()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (mapSelectionPanel) mapSelectionPanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
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
        EditorApplication.isPlaying = false; // 에디터에서 Play 종료
#else
        Application.Quit();                  // 빌드에서 종료
#endif
    }

    // === 맵 선택 ===
    public void OnStartButtonClicked()
    {
        if (mapSelectionPanel) mapSelectionPanel.SetActive(true);
    }
}
