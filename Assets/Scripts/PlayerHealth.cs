using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;
    public Slider slider;
    public int health;


    void Start()
    {
        health = maxHealth;
        slider.maxValue = maxHealth;
        slider.value = health;  
    }
    public void ChangeHealth(int amount) 
    {
        currentHealth += amount;
        slider.value = currentHealth;
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);    
        }
    }
}
