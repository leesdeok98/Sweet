using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class StopAndGoEnemy : Enemy
{
    [Header("Movement Pattern")]
    public float moveTime = 2.0f;
    public float stopTime = 1.0f;

    [SpineAnimation] public string idleAnimName = "Idle";

    private float timer;
    private bool isMoving = true;

    public override void FixedUpdate()
    {
        Debug.Log("몬스터 움직이는 중...");
        if (!isLive || target == null || isStunned) return;

        timer += Time.fixedDeltaTime;

        if (isMoving)
        {
            base.FixedUpdate();

            if (timer >= moveTime)
            {
                timer = 0;
                isMoving = false;
                rb.velocity = Vector2.zero;

                skeletonAnimation.AnimationState.SetAnimation(0, idleAnimName, true);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (timer >= stopTime)
            {
                timer = 0;
                isMoving = true;

                skeletonAnimation.AnimationState.SetAnimation(0, runAnimName, true);
            }
        }
    }
}
