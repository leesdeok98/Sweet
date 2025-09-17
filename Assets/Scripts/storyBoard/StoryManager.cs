using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    [Header("�̹���")]
    [SerializeField] private Image image;

    [Header("�ؽ�Ʈ")]
    [SerializeField] private TMP_Text text;

    [Header("��ũ���ͺ�")]
    [SerializeField] private ChangeImageText[] images;

    private int currentImage = 1;   // �� ��° �̹������� ���� (ù ��° �̹����� Source image�� �ְ� ����)

    void Start()
    {
        //text.text = "";
        //image.sprite = images[0].StoryImage;  //Start�� �־��� �� ������ ������ ���� Source image�� ����
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextImage();
        }
    }

    void ShowNextImage()
    {
        image.sprite = images[currentImage].StoryImage;
        currentImage++;
    }

}
