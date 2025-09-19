using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //이걸 써야 씬 변경 가능
using UnityEngine.UI;

public class Setting : MonoBehaviour
{

    [SerializeField] private GameObject pausePanel; //인스펙터창에 오브젝트 넣을 거 표시
    [SerializeField] private GameObject quitPanel; 
    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //처음에 설정창 안보이게 해줌
        quitPanel.SetActive(false);
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
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료"); // 에디터에서만 확인용
        Application.Quit();
    }

    public void Quit()
    {
        quitPanel.SetActive(true);
    }

    public void CloseQuit()
    {
        quitPanel.SetActive(false); // 설정 패널 끄기
    }
}



