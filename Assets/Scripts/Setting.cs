using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; //�̰� ��� �� ���� ����
using UnityEngine.UI;

public class Setting : MonoBehaviour
{

    [SerializeField] private GameObject pausePanel; //����â
    [SerializeField] private GameObject quitPanel; //��������
    [SerializeField] private GameObject mapSelectionPanel; // �ʼ���
    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //ó���� ����â �Ⱥ��̰� ����
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
        pausePanel.SetActive(false); // ���� �г� ����
        mapSelectionPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("���� ���� ��ư Ŭ����!");

#if UNITY_EDITOR
        // ����Ƽ �����Ϳ��� Play ��� ����
        EditorApplication.isPlaying = false;
#else
        // ����� ���ӿ��� ����
        Application.Quit();
#endif
    }

    public void Quit()
    {
        quitPanel.SetActive(true);
    }

    public void CloseQuit()
    {
        quitPanel.SetActive(false); // ���� �г� ����
    }
    public void OnStartButtonClicked()
    {
        // �� ���� ȭ�� �г��� Ȱ��ȭ
        mapSelectionPanel.SetActive(true);
    }
}



