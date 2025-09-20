using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; //이걸 써야 씬 변경 가능
using UnityEngine.UI;

public class Setting : MonoBehaviour
{

    [SerializeField] private GameObject pausePanel; //설정창
    [SerializeField] private GameObject quitPanel; //게임종료
    [SerializeField] private GameObject mapSelectionPanel; // 맵선택
    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //처음에 설정창 안보이게 해줌
        quitPanel.SetActive(false);
        mapSelectionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadNextScene()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void SettingLode() 
    {
        pausePanel.SetActive(true);
    }

    public void CloseSetting()
    {
        pausePanel.SetActive(false); // 설정 패널 끄기
        mapSelectionPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭됨!");

#if UNITY_EDITOR
        // 유니티 에디터에서 Play 모드 종료
        EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 종료
        Application.Quit();
#endif
    }

    public void Quit()
    {
        quitPanel.SetActive(true);
    }

    public void CloseQuit()
    {
        quitPanel.SetActive(false); // 설정 패널 끄기
    }
    public void OnStartButtonClicked()
    {
        // 맵 선택 화면 패널을 활성화
        mapSelectionPanel.SetActive(true);
    }
}



