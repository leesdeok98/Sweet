using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //�̰� ��� �� ���� ����
using UnityEngine.UI;

public class Setting : MonoBehaviour
{

    [SerializeField] private GameObject pausePanel; //�ν�����â�� ������Ʈ ���� �� ǥ��
    // Start is called before the first frame update
    void Start()
    {
        pausePanel.SetActive(false); //ó���� ����â �Ⱥ��̰� ����

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
}



