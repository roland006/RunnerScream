using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float immunityDuration = 5f;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float blinkSpeed = 0.15f;

    private int currentHealth;
    private bool isImmune;
    private Color originalColor;
    private Coroutine immunityRoutine;

    void Start()
    {
        currentHealth = maxHealth;

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
    }

    void OnTriggerEnter(Collider other) 
    {
        Debug.Log($"Столкновение с: {other.name} ({other.tag})");
        if (other.CompareTag("Enemy"))
        {
            TakeDamage();          
           
            var enemyReaction = other.GetComponent<EnemyReaction>();
            if (enemyReaction != null)
            {
                enemyReaction.ReactToHit(Vector3.right, 1);
            }
        }
       
    }

    void TakeDamage()
    {
        if (isImmune) return;

        currentHealth--;
        Debug.Log($"Player took damage! HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (immunityRoutine != null)
            StopCoroutine(immunityRoutine);
        immunityRoutine = StartCoroutine(ImmunityFlash());
    }

    IEnumerator ImmunityFlash()
    {
        isImmune = true;
        float elapsed = 0f;

        while (elapsed < immunityDuration)
        {
            elapsed += Time.deltaTime;

            if (playerRenderer != null)
            {
                playerRenderer.material.color = (Mathf.FloorToInt(elapsed / blinkSpeed) % 2 == 0) ? hitColor : originalColor;
                
                if (Mathf.FloorToInt(elapsed / blinkSpeed) % 2 == 0)
                {
                    playerRenderer.material.color = hitColor;
                }
                else
                {
                    playerRenderer.material.color = originalColor;
                }
            }

            yield return null;
        }

        if (playerRenderer != null)
            playerRenderer.material.color = originalColor;

        isImmune = false;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Здесь можно добавить respawn, UI “Game Over” и т.п.
    }
}