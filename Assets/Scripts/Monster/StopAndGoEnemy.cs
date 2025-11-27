using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class StopAndGoEnemy : Enemy
{
    [Header("Movement Pattern")]
    public float moveTime = 0.833f;
    public float stopTime = 0.5f;

    private float timer;
    private bool isMoving = true;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (skeletonAnimation != null)
        {
            skeletonAnimation.timeScale = 1f;
            skeletonAnimation.AnimationState.SetAnimation(0, runAnimName, true);
        }

        timer = 0f;
        isMoving = true;
    }

    public override void FixedUpdate()
    {
        if (!isLive || target == null || isStunned || isFrozen)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        timer += Time.fixedDeltaTime;

        if (isMoving)
        {
            base.FixedUpdate();

            if (timer >= moveTime)
            {
                timer = 0f;
                isMoving = false;

                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (timer >= stopTime)
            {
                timer = 0f;
                isMoving = true;
            }
        }
    }
}
