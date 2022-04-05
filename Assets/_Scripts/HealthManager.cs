using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthManager : MonoBehaviour
{
    public BossAI1 boss;
    public Slider HealthBar;
    public float health;
    const float MAX_HEALTH = 100;
    // Start is called before the first frame update
    void Start()
    {
        health = MAX_HEALTH;
        HealthBar.value = health / 100;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            health = 0;
            boss.RemovePlayer(transform);
            Destroy(gameObject);
            
        }
        HealthBar.value = health / 100;
    }
}
