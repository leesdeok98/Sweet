using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //이걸 써야 씬 변경 가능
using UnityEngine.UI;
public class Stage1Setting : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    // Start is called before the first frame update
    private bool isPaused = false;
    void Start()
    {
        pausePanel.SetActive(false); //처음에 설정창 안보이게 해줌 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
          
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f; 
            pausePanel.SetActive(true);

        }
        else
        {
            Time.timeScale = 1f; 
            pausePanel.SetActive(false);

        }
    }
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void QuitToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Stage1");
    }

   
}
