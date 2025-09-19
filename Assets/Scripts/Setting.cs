using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //�̰� ��� �� ���� ����
using UnityEngine.UI;

public class Setting : MonoBehaviour
{

    [SerializeField] private GameObject pausePanel; //�ν�����â�� ������Ʈ ���� �� ǥ��
    [SerializeField] private GameObject quitPanel; 
    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //ó���� ����â �Ⱥ��̰� ����
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
        pausePanel.SetActive(false); // ���� �г� ����
    }

    public void QuitGame()
    {
        Debug.Log("���� ����"); // �����Ϳ����� Ȯ�ο�
        Application.Quit();
    }

    public void Quit()
    {
        quitPanel.SetActive(true);
    }

    public void CloseQuit()
    {
        quitPanel.SetActive(false); // ���� �г� ����
    }
}



