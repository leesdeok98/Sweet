/* using System.Collections;
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

    private int currentImage = 0;   // ù ��° �̹����� ����

    
    void Start()
    {
        ShowImage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentImage++;

            ShowImage();
        }
    }

    void ShowImage()
    {
        image.sprite = images[currentImage].StoryImage;
        text.text = images[currentImage].StoryText;
    }
} */
