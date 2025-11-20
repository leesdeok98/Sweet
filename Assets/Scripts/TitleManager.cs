using UnityEngine;

public class TitleManager : MonoBehaviour
{
    void Start()
    {
        // 타이틀 씬이 시작되면 무조건 타이틀 BGM 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(AudioManager.Bgm.Title_BGM);
        }
    }
}