// SpineOneShot.cs
using System.Collections;
using UnityEngine;
using Spine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class SpineOneShot : MonoBehaviour
{
    [Header("Play Settings")]
    public string animationName = "burst"; // 프리팹 기본 재생 애니
    public bool loop = false;
    public float extraLifetime = 0.1f;     // 애니 끝나고 조금 더 살려두는 시간
    public float spineTimeScale = 1f;      // 스파인 자체 재생속도

    SkeletonAnimation sa;

    void Awake()
    {
        sa = GetComponent<SkeletonAnimation>();
        if (sa == null) { Debug.LogError("[SpineOneShot] SkeletonAnimation이 없음"); Destroy(gameObject); return; }

        sa.timeScale = spineTimeScale;

        var entry = sa.AnimationState.SetAnimation(0, animationName, loop);
        if (!loop) StartCoroutine(KillAfter(entry));
    }

    IEnumerator KillAfter(TrackEntry entry)
    {
        float duration = (entry != null && entry.Animation != null) ? entry.Animation.Duration : 0.5f;
        float t = 0f;
        while (t < duration + extraLifetime)
        {
            t += Time.unscaledDeltaTime; // 일시정지 영향 X
            yield return null;
        }
        Destroy(gameObject);
    }
}
