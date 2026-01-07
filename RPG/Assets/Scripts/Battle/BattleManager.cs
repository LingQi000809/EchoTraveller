using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance { get; private set; }

    [Header("Battle Configuration")]
    [SerializeField] private BattleSceneData battleData;

    [Header("Scene References")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private BattlePlayerController player;
    [SerializeField] private BattleEnemy enemy;
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private HealthBar enemyHealthBar;

    [Header("End Screen")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Text victoryText;
    [SerializeField] private Text defeatText;

    [Header("Audio")]
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;

    private bool battleEnded = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one BattleManager in the scene.");
        }
        instance = this;
    }

    private void Start()
    {
        InitializeBattle();
    }

    private void InitializeBattle()
    {
        if (battleData == null)
        {
            Debug.LogError("BattleSceneData not assigned!");
            return;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = battleData.backgroundSprite;
        }

        if (playerHealthBar != null)
        {
            playerHealthBar.Initialize(battleData.playerMaxHP);
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.Initialize(battleData.enemyMaxHP);
        }

        if (player != null)
        {
            player.SetArenaBounds(battleData.arenaBoundsMin, battleData.arenaBoundsMax);
        }

        if (enemy != null)
        {
            enemy.Initialize(battleData, player.transform, battleData.arenaBoundsMin, battleData.arenaBoundsMax);

            if (battleData.enemyAnimatorController != null)
            {
                Animator enemyAnimator = enemy.GetComponent<Animator>();
                if (enemyAnimator != null)
                {
                    enemyAnimator.runtimeAnimatorController = battleData.enemyAnimatorController;
                }
            }
        }

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (GameController.instance != null)
        {
            GameController.instance.ChangeGameState(GameState.Battle);
        }

        if (VoiceController.instance != null)
        {
            VoiceController.instance.StartListening();
        }

        // if (AudioManager.instance != null && battleMusic != null)
        // {
        //     AudioManager.instance.StopAmbientMusic();
        // }
    }

    public void DamageEnemy(float damage)
    {
        if (battleEnded || enemyHealthBar == null) return;

        float newHealth = enemyHealthBar.GetCurrentHealth() - damage;
        enemyHealthBar.SetHealth(newHealth);

#if UNITY_EDITOR
        Debug.Log($"Enemy took {damage} damage! Remaining HP: {newHealth}");
#endif

        if (newHealth <= 0)
        {
            Victory();
        }
    }

    public void DamagePlayer(float damage)
    {
        if (battleEnded || playerHealthBar == null) return;

        float newHealth = playerHealthBar.GetCurrentHealth() - damage;
        playerHealthBar.SetHealth(newHealth);

#if UNITY_EDITOR
        Debug.Log($"Player took {damage} damage! Remaining HP: {newHealth}");
#endif

        if (newHealth <= 0)
        {
            Defeat();
        }
    }

    public bool CheckRayHitEnemy(Vector3 rayStart, Vector3 rayDirection, float rayDistance)
    {
        if (enemy == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, rayDistance);

        if (hit.collider != null && hit.collider.gameObject == enemy.gameObject)
        {
            float damage = battleData != null ? battleData.playerRayDamage : 20f;
            enemy.TakeDamage(damage);
            return true;
        }
        return false;
    }


    private void Victory()
    {
        if (battleEnded) return;
        battleEnded = true;

#if UNITY_EDITOR
        Debug.Log("VICTORY!");
#endif

        if (enemy != null)
        {
            enemy.Die();
        }

        if (VoiceController.instance != null)
        {
            VoiceController.instance.StopListening();
        }

        if (AudioManager.instance != null && victorySound != null)
        {
            AudioManager.instance.PlaySound(victorySound);
        }

        StartCoroutine(ShowVictoryScreen());
    }

    private void Defeat()
    {
        if (battleEnded) return;
        battleEnded = true;

#if UNITY_EDITOR
        Debug.Log("DEFEAT!");
#endif

        if (VoiceController.instance != null)
        {
            VoiceController.instance.StopListening();
        }

        if (AudioManager.instance != null && defeatSound != null)
        {
            AudioManager.instance.PlaySound(defeatSound);
        }

        StartCoroutine(ShowDefeatScreen());
    }

    private IEnumerator ShowVictoryScreen()
    {
        yield return new WaitForSeconds(2f);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    private IEnumerator ShowDefeatScreen()
    {
        yield return new WaitForSeconds(2f);

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
    }

    public void ReturnToScene(string sceneName)
    {
        if (GameController.instance != null)
        {
            GameController.instance.ChangeGameState(GameState.FreeRoam);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAmbientMusic();
        }

        SceneManager.LoadScene(sceneName);
    }
}
