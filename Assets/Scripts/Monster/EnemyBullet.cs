using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private float speed;
    private float duration;
    private float damage;
    private Vector2 direction;

    public void Initialize(Vector2 direction, float damage, float speed, float duration)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.speed = speed;
        this.duration = duration;

        Destroy(gameObject, this.duration);
    }

    private void Update()
    {
        transform.Translate(direction * this.speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
