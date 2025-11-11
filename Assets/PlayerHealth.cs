using System.Collections;
using UnityEngine;

namespace Game
{
    using UI;

    public class PlayerHealth : MonoBehaviour
    {
        [Header("Player Stats")] [Header("Health")] [SerializeField]
        private static int maxHealth = 3;

        [SerializeField] private float immunityDuration = 5f;

        [Header("Visual Feedback")] [SerializeField]
        private Renderer playerRenderer;

        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float blinkSpeed = 0.15f;

        private static int currentHealth;
        private bool isImmune;
        private Color originalColor;
        private Coroutine immunityRoutine;


        public static int DeathsCount;
        public UIController uiControllerObj;


        void Start()
        {
            DeathsCount = 0;
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

            if (immunityRoutine != null)
                StopCoroutine(immunityRoutine);
            immunityRoutine = StartCoroutine(ImmunityFlash());

            UIController.SetHealth(currentHealth);

            if (currentHealth <= 0)
            {
                DeathsCount++;
                uiControllerObj.OnGameOver();

                return;
            }
            
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
                    playerRenderer.material.color =
                        (Mathf.FloorToInt(elapsed / blinkSpeed) % 2 == 0) ? hitColor : originalColor;

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

        public static void Revive()
        {
            currentHealth = maxHealth;
            
           
            UIController.SetHealth(currentHealth);
        }
    }
}