using UnityEngine;
using Spine.Unity;

public class Cc : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation spinePlayer;
    [SerializeField] private string animationName = "animation";

    [Header("건너뛸 구간(초 단위)")]
    [SerializeField] private float skipStart = 0.3f;
    [SerializeField] private float skipEnd = 0.7f;

    private bool isPlaying = false;

    private void Start()
    {
        // 시작 시 아무 애니메이션도 재생하지 않게
        if (spinePlayer != null)
        {
            spinePlayer.AnimationState.ClearTrack(0);
        }
    }

    private void Update()
    {
        if (spinePlayer == null) return;

        // 스페이스바 입력 시 애니메이션 시작
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPlaying = true;
            spinePlayer.AnimationState.SetAnimation(0, animationName, false);
        }

        if (!isPlaying) return;

        var track = spinePlayer.AnimationState.GetCurrent(0);
        if (track == null) return;

        // 중간 구간 스킵
        if (track.TrackTime > skipStart && track.TrackTime < skipEnd)
        {
            track.TrackTime = skipEnd;
        }

        // 애니메이션이 끝나면 플래그 꺼주기
        if (track.IsComplete)
        {
            isPlaying = false;
        }
    }
}
