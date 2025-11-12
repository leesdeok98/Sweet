using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    // public Image healthBar;
    public float healthAmount = 100f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void TakeDamage(float damage)
    {
        healthAmount -= damage;
        // healthBar.fillAmoint = healthAmount / 100f;
    }
}
