using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class JellyPunk : Enemy
{
    public GameObject bulletPrefab;
    public float stopDistance = 10f;
    public float fireInterval = 3f;
    private float lastFireTime;

    public float bulletSpeed = 3f;
    public float bulletDuration = 3f;

    public override void FixedUpdate()
    {
        if (!isLive || target == null) return;

        float distance = Vector2.Distance(target.position, rb.position);

        if (distance > stopDistance)
        {
            base.FixedUpdate();
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (Time.time > lastFireTime + fireInterval)
            {
                Fire();
                lastFireTime = Time.time;
            }
        }
    }

    void Fire()
    {
        Vector2 fireDirection = (target.position - (Vector2)transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<EnemyBullet>().Initialize(fireDirection, dps, bulletSpeed, bulletDuration);
    }
}
