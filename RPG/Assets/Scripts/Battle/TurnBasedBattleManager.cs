using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum BattleTurn { EnemyTurn, PlayerTurnWaiting, PlayerTurnActive, BattleEnd }

public class TurnBasedBattleManager : MonoBehaviour
{
    public static TurnBasedBattleManager instance { get; private set; }

    [Header("Battle Configuration")]
    [SerializeField] private BattleSceneData dragonBattleData;
    [SerializeField] private BattleSceneData mayorBattleData;

    [Header("Scene References")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject enemyObject;
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private HealthBar enemyHealthBar;

    [Header("UI Panels")]
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private GameObject turnPromptPanel;
    [SerializeField] private Text turnPromptText;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject attackIndicatorsPanel;

    [Header("Attack Name Display")]
    [SerializeField] private TextMeshProUGUI attackNameText;

    [Header("Attack Indicators")]
    [SerializeField] private Text ascendingUsesText;
    [SerializeField] private Text descendingUsesText;
    [SerializeField] private Text stableUsesText;

    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject playerProjectilePrefab;
    [SerializeField] private GameObject enemyProjectilePrefab;

    [Header("Attack Sprites")]
    [SerializeField] private Sprite ascendingSprite;
    [SerializeField] private Sprite descendingSprite;
    [SerializeField] private Sprite stableSprite;
    [SerializeField] private Sprite dragonProjectileSprite;

    [Header("Enemy Projectile Spawn")]
    [SerializeField] private Transform enemyProjectileSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip playerAttackSound;
    [SerializeField] private AudioClip enemyAttackSound;
    [SerializeField] private AudioClip hitSound;

    private BattleSceneData currentBattleData;
    private BattleTurn currentTurn;
    private bool battleEnded = false;

    private float playerCurrentHP;
    private float enemyCurrentHP;

    private Dictionary<VoiceController.PitchTrend, AttackType> playerAttacks;
    private bool isWaitingForVoiceInput = false;

    private int winEndingID;
    private int loseEndingID;
    private string winResumeKnot;
    private string loseResumeKnot;
    private string resumeScene;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one TurnBasedBattleManager in the scene.");
        }
        instance = this;

        if (attackNameText != null)
        {
            attackNameText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        InitializeBattle();
        PreWarmVoiceController();
    }

    private void PreWarmVoiceController()
    {
        if (VoiceController.instance != null)
        {
            Debug.Log("Pre-warming VoiceController...");
            VoiceController.instance.StartListening();
            StartCoroutine(StopPreWarmAfterDelay());
        }
    }

    private IEnumerator StopPreWarmAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (VoiceController.instance != null)
        {
            VoiceController.instance.StopListening();
            Debug.Log("VoiceController pre-warmed and ready!");
        }
    }

    private void InitializeBattle()
    {
        if (BattleContext.instance != null)
        {
            string enemyName = BattleContext.instance.enemyName;
            winEndingID = BattleContext.instance.winEndingID;
            loseEndingID = BattleContext.instance.loseEndingID;
            winResumeKnot = BattleContext.instance.resumeWin;
            loseResumeKnot = BattleContext.instance.resumeLose;
            resumeScene = BattleContext.instance.returnScene;

            currentBattleData = enemyName.ToLower().Contains("dragon") ? dragonBattleData : mayorBattleData;

            Debug.Log($"Battle started: {enemyName}, Win={winEndingID}, Lose={loseEndingID}, WinResume={winResumeKnot}, LoseResume={loseResumeKnot}, ReturnScene={resumeScene}");
        }
        else
        {
            Debug.LogWarning("BattleContext not found! Using default dragon battle.");
            currentBattleData = dragonBattleData;
            winEndingID = 0;
            loseEndingID = 0;
        }

        SetupVisuals();
        SetupHealth();
        SetupPlayerAttacks();
        UpdateAttackIndicators();

        if (GameController.instance != null)
        {
            GameController.instance.ChangeGameState(GameState.Battle);
        }

        // if (AudioManager.instance != null && battleMusic != null)
        // {
        //     AudioManager.instance.StopAmbientMusic();
        // }

        ShowRulesPanel();
    }

    private void SetupVisuals()
    {
        if (backgroundRenderer != null && currentBattleData.backgroundSprite != null)
        {
            backgroundRenderer.sprite = currentBattleData.backgroundSprite;

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                float cameraHeight = mainCamera.orthographicSize * 2f;
                float cameraWidth = cameraHeight * mainCamera.aspect;

                float spriteWidth = currentBattleData.backgroundSprite.bounds.size.x;
                float spriteHeight = currentBattleData.backgroundSprite.bounds.size.y;

                float scaleX = cameraWidth / spriteWidth;
                float scaleY = cameraHeight / spriteHeight;
                float scale = Mathf.Max(scaleX, scaleY);

                backgroundRenderer.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        if (playerObject != null)
        {
            playerObject.transform.localScale = new Vector3(2f, 2f, 1f);
        }

        if (enemyObject != null)
        {
            Animator enemyAnimator = enemyObject.GetComponent<Animator>();
            SpriteRenderer enemySpriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
            Transform dragonPSB = enemyObject.transform.Find("DragonRedPSB");

            if (currentBattleData.enemyAnimatorController != null)
            {
                if (enemyAnimator != null)
                {
                    enemyAnimator.enabled = true;
                    enemyAnimator.runtimeAnimatorController = currentBattleData.enemyAnimatorController;
                }

                if (dragonPSB != null)
                {
                    dragonPSB.gameObject.SetActive(true);
                }

                if (enemySpriteRenderer != null)
                {
                    enemySpriteRenderer.enabled = false;
                }

                enemyObject.transform.localPosition = new Vector3(6.5f, -5.25f, 0f);
                enemyObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                if (enemyProjectileSpawnPoint != null)
                {
                    enemyProjectileSpawnPoint.localPosition = new Vector3(0f, 1.5f, 0f);
                }
            }
            else if (currentBattleData.enemySprite != null)
            {
                if (enemyAnimator != null)
                {
                    enemyAnimator.enabled = false;
                }

                if (dragonPSB != null)
                {
                    dragonPSB.gameObject.SetActive(false);
                }

                if (enemySpriteRenderer != null)
                {
                    enemySpriteRenderer.enabled = true;
                    enemySpriteRenderer.sprite = currentBattleData.enemySprite;
                }

                enemyObject.transform.localPosition = new Vector3(6.5f, -2.8f, 0f);
                enemyObject.transform.localScale = new Vector3(2f, 2f, 1f);

                if (enemyProjectileSpawnPoint != null)
                {
                    enemyProjectileSpawnPoint.localPosition = new Vector3(0f, 0.2f, 0f);
                }
            }
        }

        if (enemyProjectileSpawnPoint == null && enemyObject != null)
        {
            GameObject spawnPointObj = new GameObject("ProjectileSpawnPoint");
            spawnPointObj.transform.SetParent(enemyObject.transform);
            spawnPointObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            enemyProjectileSpawnPoint = spawnPointObj.transform;
        }
    }







    private void SetupHealth()
    {
        playerCurrentHP = currentBattleData.playerMaxHP;
        enemyCurrentHP = currentBattleData.enemyMaxHP;

        if (playerHealthBar != null)
        {
            playerHealthBar.Initialize(currentBattleData.playerMaxHP);
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.Initialize(currentBattleData.enemyMaxHP);
        }
    }

    private void SetupPlayerAttacks()
    {
        playerAttacks = new Dictionary<VoiceController.PitchTrend, AttackType>
        {
            {
                VoiceController.PitchTrend.Ascending,
                new AttackType("POWER STRIKE", VoiceController.PitchTrend.Ascending, 45f, 60f, 2, ascendingSprite)
                {
                    projectileColor = new Color(1f, 0.3f, 0.3f)
                }
            },
            {
                VoiceController.PitchTrend.Descending,
                new AttackType("SWIFT ATTACK", VoiceController.PitchTrend.Descending, 25f, 35f, 2, descendingSprite)
                {
                    projectileColor = new Color(0.3f, 0.5f, 1f)
                }
            },
            {
                VoiceController.PitchTrend.Stable,
                new AttackType("BASIC SHOT", VoiceController.PitchTrend.Stable, 15f, 25f, 2, stableSprite)
                {
                    projectileColor = new Color(0.3f, 1f, 0.3f)
                }
            }
        };
    }

    private void ShowRulesPanel()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
        }

        if (attackIndicatorsPanel != null)
        {
            attackIndicatorsPanel.SetActive(false);
        }
    }

    public void OnRulesPanelClosed()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(false);
        }

        if (attackIndicatorsPanel != null)
        {
            attackIndicatorsPanel.SetActive(true);
        }

        StartCoroutine(StartBattleSequence());
    }

    private IEnumerator StartBattleSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (!battleEnded)
        {
            StartEnemyTurn();
        }
    }

    private void StartEnemyTurn()
    {
        if (battleEnded) return;

        currentTurn = BattleTurn.EnemyTurn;
        Debug.Log("Enemy's turn!");
        StartCoroutine(EnemyAttackSequence());
    }

    private IEnumerator EnemyAttackSequence()
    {
        yield return new WaitForSeconds(1f);

        if (battleEnded) yield break;

        float damage = Random.Range(currentBattleData.enemyProjectileDamage, currentBattleData.enemyProjectileDamage + 15f);
        Debug.Log($"Dragon attacks for {damage} damage!");

        if (battleEnded) yield break;

        if (enemyProjectilePrefab != null && playerObject != null)
        {
            Vector3 spawnPos = enemyProjectileSpawnPoint != null ? enemyProjectileSpawnPoint.position : enemyObject.transform.position;

            GameObject projectile = Instantiate(enemyProjectilePrefab, spawnPos, Quaternion.identity);
            PlayerProjectile projScript = projectile.GetComponent<PlayerProjectile>();
            if (projScript == null)
            {
                projScript = projectile.AddComponent<PlayerProjectile>();
            }

            projScript.Initialize(playerObject.transform.position, damage, dragonProjectileSprite, Color.red, () => {
                if (!battleEnded)
                {
                    DamagePlayer(damage);
                    if (AudioManager.instance != null && hitSound != null)
                    {
                        AudioManager.instance.PlaySound(hitSound);
                    }
                }
            });

            if (AudioManager.instance != null && enemyAttackSound != null)
            {
                AudioManager.instance.PlaySound(enemyAttackSound);
            }

            yield return new WaitForSeconds(2f);
        }
        else
        {
            DamagePlayer(damage);
        }

        if (battleEnded) yield break;

        yield return new WaitForSeconds(1f);

        if (!battleEnded)
        {
            StartPlayerTurn();
        }
    }

    private void StartPlayerTurn()
    {
        if (battleEnded) return;

        currentTurn = BattleTurn.PlayerTurnWaiting;
        Debug.Log("Player's turn!");
        ShowTurnPrompt();
    }

    private void ShowTurnPrompt()
    {
        if (battleEnded) return;

        if (turnPromptPanel != null)
        {
            turnPromptPanel.SetActive(true);
            if (turnPromptText != null)
            {
                turnPromptText.text = "Your Turn! Ready?";
            }
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(OnReadyButtonPressed);
        }
    }

    private void OnReadyButtonPressed()
    {
        if (battleEnded) return;

        if (turnPromptPanel != null)
        {
            turnPromptPanel.SetActive(false);
        }

        currentTurn = BattleTurn.PlayerTurnActive;
        isWaitingForVoiceInput = true;

        if (VoiceController.instance != null)
        {
            VoiceController.instance.StartListening();
            Debug.Log("Voice listening activated immediately!");
        }
    }

    private void Update()
    {
        if (rulesPanel != null && rulesPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            OnRulesPanelClosed();
        }

        if (currentTurn == BattleTurn.PlayerTurnActive && isWaitingForVoiceInput && !battleEnded)
        {
            CheckForPlayerAttack();
        }

        if (currentTurn == BattleTurn.PlayerTurnWaiting && Input.GetKeyDown(KeyCode.Return) && !battleEnded)
        {
            OnReadyButtonPressed();
        }
    }

    private void CheckForPlayerAttack()
    {
        if (battleEnded) return;

        if (VoiceController.instance != null && VoiceController.instance.HasDetectedTrend())
        {
            VoiceController.PitchTrend detectedTrend = VoiceController.instance.GetLastDetectedTrend();

            if (playerAttacks.ContainsKey(detectedTrend))
            {
                AttackType attack = playerAttacks[detectedTrend];

                if (attack.CanUse())
                {
                    ExecutePlayerAttack(attack);
                }
                else
                {
                    string messageToShow = "";
                    if (detectedTrend == VoiceController.PitchTrend.Ascending)
                    {
                        messageToShow = "NO POWER STRIKES!";
                    }
                    else if (detectedTrend == VoiceController.PitchTrend.Descending)
                    {
                        messageToShow = "NO SWIFT ATTACKS!";
                    }
                    else if (detectedTrend == VoiceController.PitchTrend.Stable)
                    {
                        messageToShow = "NO BASIC SHOTS!";
                    }

                    Debug.Log($"{attack.attackName} has no uses remaining!");
                    ShowNoAttacksLeftMessage(messageToShow);
                }
            }
        }
    }

    private void ExecutePlayerAttack(AttackType attack)
    {
        if (battleEnded) return;

        isWaitingForVoiceInput = false;

        if (VoiceController.instance != null)
        {
            VoiceController.instance.StopListening();
        }

        attack.UseAttack();
        UpdateAttackIndicators();

        float damage = attack.GetRandomDamage();
        Debug.Log($"Player uses {attack.attackName} for {damage} damage!");

        StartCoroutine(PlayerAttackSequence(attack, damage));
    }

    private IEnumerator PlayerAttackSequence(AttackType attack, float damage)
    {
        ShowAttackName(attack.attackName);

        yield return new WaitForSeconds(0.5f);

        if (battleEnded) yield break;

        if (playerProjectilePrefab != null && enemyObject != null)
        {
            Vector3 targetPos = enemyProjectileSpawnPoint != null ? enemyProjectileSpawnPoint.position : enemyObject.transform.position;

            GameObject projectile = Instantiate(playerProjectilePrefab, playerObject.transform.position, Quaternion.identity);
            PlayerProjectile projScript = projectile.GetComponent<PlayerProjectile>();
            if (projScript == null)
            {
                projScript = projectile.AddComponent<PlayerProjectile>();
            }

            projScript.Initialize(targetPos, damage, attack.projectileSprite, attack.projectileColor, () => {
                if (!battleEnded)
                {
                    DamageEnemy(damage);
                    if (AudioManager.instance != null && hitSound != null)
                    {
                        AudioManager.instance.PlaySound(hitSound);
                    }
                }
            });

            if (AudioManager.instance != null && playerAttackSound != null)
            {
                AudioManager.instance.PlaySound(playerAttackSound);
            }

            yield return new WaitForSeconds(2f);
        }
        else
        {
            DamageEnemy(damage);
        }

        if (battleEnded) yield break;

        yield return new WaitForSeconds(1f);

        if (!battleEnded)
        {
            StartEnemyTurn();
        }
    }

    private void ShowAttackName(string attackName)
    {
        if (attackNameText != null)
        {
            StartCoroutine(AnimateAttackName(attackName));
        }
    }

    private void ShowNoAttacksLeftMessage(string message)
    {
        if (attackNameText != null)
        {
            StartCoroutine(AnimateAttackName(message));
        }
    }

    private IEnumerator AnimateAttackName(string attackName)
    {
        attackNameText.text = attackName;
        attackNameText.gameObject.SetActive(true);

        float duration = 1.5f;
        float elapsed = 0f;

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 1.5f;
        Color startColor = attackNameText.color;
        startColor.a = 0f;
        Color endColor = startColor;
        endColor.a = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (progress < 0.3f)
            {
                float scaleProgress = progress / 0.3f;
                attackNameText.transform.localScale = Vector3.Lerp(startScale, endScale, scaleProgress);
                attackNameText.color = Color.Lerp(startColor, endColor, scaleProgress);
            }
            else if (progress > 0.7f)
            {
                float fadeProgress = (progress - 0.7f) / 0.3f;
                attackNameText.color = Color.Lerp(endColor, startColor, fadeProgress);
            }

            yield return null;
        }

        attackNameText.gameObject.SetActive(false);
        attackNameText.transform.localScale = Vector3.one;
    }

    private void DamagePlayer(float damage)
    {
        if (battleEnded) return;

        playerCurrentHP -= damage;
        playerCurrentHP = Mathf.Max(0, playerCurrentHP);

        if (playerHealthBar != null)
        {
            playerHealthBar.SetHealth(playerCurrentHP);
        }

        Debug.Log($"Player HP: {playerCurrentHP}/{currentBattleData.playerMaxHP}");

        if (playerCurrentHP <= 0 && !battleEnded)
        {
            StopAllCoroutines();
            StartCoroutine(PlayDefeatAnimation(playerObject, false));
        }
    }

    private void DamageEnemy(float damage)
    {
        if (battleEnded) return;

        enemyCurrentHP -= damage;
        enemyCurrentHP = Mathf.Max(0, enemyCurrentHP);

        if (enemyHealthBar != null)
        {
            enemyHealthBar.SetHealth(enemyCurrentHP);
        }

        Debug.Log($"Enemy HP: {enemyCurrentHP}/{currentBattleData.enemyMaxHP}");

        if (enemyCurrentHP <= 0 && !battleEnded)
        {
            StopAllCoroutines();
            StartCoroutine(PlayDefeatAnimation(enemyObject, true));
        }
    }

    private IEnumerator PlayDefeatAnimation(GameObject character, bool playerWon)
    {
        battleEnded = true;
        currentTurn = BattleTurn.BattleEnd;

        if (VoiceController.instance != null)
        {
            VoiceController.instance.StopListening();
        }

        if (turnPromptPanel != null)
        {
            turnPromptPanel.SetActive(false);
        }

        if (character != null)
        {
            SpriteRenderer sr = character.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float duration = 1.5f;
                float elapsed = 0f;
                Vector3 originalScale = character.transform.localScale;
                Color originalColor = sr.color;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / duration;

                    sr.color = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0f), progress);
                    character.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.3f, progress);
                    character.transform.Rotate(0, 0, 360f * Time.deltaTime);

                    yield return null;
                }

                character.SetActive(false);
            }
        }

        yield return new WaitForSeconds(0.5f);
        EndBattle(playerWon);
    }

    private void UpdateAttackIndicators()
    {
        if (ascendingUsesText != null && playerAttacks.ContainsKey(VoiceController.PitchTrend.Ascending))
        {
            AttackType ascending = playerAttacks[VoiceController.PitchTrend.Ascending];
            ascendingUsesText.text = $"Power Strike: {ascending.remainingUses}/{ascending.maxUses}";
        }

        if (descendingUsesText != null && playerAttacks.ContainsKey(VoiceController.PitchTrend.Descending))
        {
            AttackType descending = playerAttacks[VoiceController.PitchTrend.Descending];
            descendingUsesText.text = $"Swift Attack: {descending.remainingUses}/{descending.maxUses}";
        }

        if (stableUsesText != null && playerAttacks.ContainsKey(VoiceController.PitchTrend.Stable))
        {
            AttackType stable = playerAttacks[VoiceController.PitchTrend.Stable];
            stableUsesText.text = $"Basic Shot: {stable.remainingUses}/{stable.maxUses}";
        }
    }

    private void EndBattle(bool playerWon)
    {
        if (currentTurn == BattleTurn.BattleEnd)
        {
            Debug.Log(playerWon ? "PLAYER VICTORY!" : "PLAYER DEFEATED!");
            StartCoroutine(TransitionToEnding(playerWon));
        }
    }

    private IEnumerator TransitionToEnding(bool playerWon)
    {
        yield return new WaitForSeconds(1f);
        
        if (!string.IsNullOrEmpty(winResumeKnot) && !string.IsNullOrEmpty(loseResumeKnot))
        {
            string resumeKnot = playerWon ? winResumeKnot : loseResumeKnot;
            BattleContext.instance.ResumeAfterBattle(resumeScene, resumeKnot);
        }
        else
        {
            int endingToLoad = playerWon ? winEndingID : loseEndingID;
            Debug.Log($"Loading ending {endingToLoad}");

            PlayerPrefs.SetInt("EndingID", endingToLoad);

            if (BattleContext.instance != null)
            {
                BattleContext.instance.ClearContext();
            }

            if (GameController.instance != null)
            {
                GameController.instance.ChangeGameState(GameState.End);
            }

            if (ScreenLoader.instance != null)
            {
                ScreenLoader.instance.LoadScene("EndingScene");
            }
            else
            {
                Debug.LogWarning("ScreenLoader not found! Loading scene directly...");
                SceneManager.LoadScene("EndingScene");
            }
        }        
    }
}
