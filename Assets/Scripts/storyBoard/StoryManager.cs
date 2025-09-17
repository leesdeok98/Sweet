using System.Collections;
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

    private int currentImage = 1;   // 두 번째 이미지부터 시작 (첫 번째 이미지는 Source image에 넣고 시작)

    void Start()
    {
        //text.text = "";
        //image.sprite = images[0].StoryImage;  //Start에 넣었을 때 사진이 느리게 떠서 Source image에 넣음
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
