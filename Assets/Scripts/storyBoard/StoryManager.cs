/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    [Header("이미지")]
    [SerializeField] private Image image;

    [Header("텍스트")]
    [SerializeField] private TMP_Text text;

    [Header("스크립터블")]
    [SerializeField] private ChangeImageText[] images;

    private int currentImage = 0;   // 첫 번째 이미지로 시작

    
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
