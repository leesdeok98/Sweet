using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SpriteRenderer 필수
[RequireComponent(typeof(SpriteRenderer))]
public class ObjectLayerSetter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null;

    private void Awake()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        this.StopAllCoroutines();
        this.StartCoroutine(this.SetLayer());
    }
    private IEnumerator SetLayer()
    {
        while (true)
        {
            // sortingOrder를 y값으로 계속 변경해준다.
            yield return new WaitForEndOfFrame();
            this.spriteRenderer.sortingOrder = -(int)this.transform.position.y;
        }
    }

    private void OnDisable()
    {
        this.StopAllCoroutines();
    }
}
