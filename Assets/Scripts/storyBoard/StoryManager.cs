using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
//using UnityEditor.Experimental.GraphView;    - 안쓰는 코드래서 주석처리 - 성덕

public class StoryManager : MonoBehaviour
{
    [Header("이미지")]
    [SerializeField] private Image image;

    [Header("텍스트")]
    [SerializeField] private TMP_Text text;

    [Header("스크립터블")]
    [SerializeField] private ChangeImageText[] images;

    [Header("타이핑 효과")]
    [SerializeField] private float TypingSpeed = 0.03f;

    private int currentImage = 0; 
    private bool isTyping = false;
    private bool isImageChanging = true;
    private Coroutine TypingCorutine;

    void Start()
    {
        //text.text = "";
        image.enabled = false;   // 이미지 안 보이게 꺼두기
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isImageChanging)   //이미지 바꾸기
            {
                text.text = "";   //새로운 이미지로 넘어갔을 때 텍스트 비우기
                ShowNextImage();
                isImageChanging = false;
            }
            
            else if (isTyping)
            {
                return;
            }

            else   //타이핑 효과
            {
                text.text = "";     //텍스트 초기화
                TypingCorutine = StartCoroutine(WriteText(images[currentImage].StoryText));  //타이핑 시작
                isTyping = true;    //상태변경
                isImageChanging = true; 
            }
        }
    }

    void ShowNextImage()
    {
        image.enabled = true; //이미지 보이게 하기
        image.sprite = images[currentImage].StoryImage;
        
    }

    private IEnumerator WriteText(string StoryText)
    {
        // 타이핑 효과 적용
        for (int i = 0; i <= StoryText.Length; i++)
        {
            text.text = StoryText.Substring(0, i);
            yield return new WaitForSeconds(TypingSpeed);
        }

        // 타이핑 완료
        isTyping = false;
        currentImage++;
    }

}
