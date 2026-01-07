using UnityEngine;

[CreateAssetMenu(fileName = "BattleSceneData", menuName = "Battle System/Battle Scene Data")]
public class BattleSceneData : ScriptableObject
{
    [Header("Visual Configuration")]
    public Sprite backgroundSprite;
    public RuntimeAnimatorController enemyAnimatorController;
    public Sprite enemySprite;

    [Header("Enemy Stats")]
    public string enemyName = "Dragon";
    public float enemyMaxHP = 150f;
    public float enemyMinDamage = 15f;
    public float enemyMaxDamage = 25f;
    public Sprite enemyProjectileSprite;
    public float enemyProjectileDamage = 15f;

    [Header("Old Battle System (Keep for compatibility)")]
    public float enemyMoveSpeed = 2f;
    public float enemyProjectileSpeed = 5f;
    public float enemyAttackInterval = 2f;

    [Header("Player Stats")]
    public float playerMaxHP = 100f;
    public float playerMoveSpeed = 5f;
    public float playerRayDamage = 20f;

    [Header("Battle Boundaries")]
    public Vector2 arenaBoundsMin = new Vector2(-8f, -4f);
    public Vector2 arenaBoundsMax = new Vector2(8f, 4f);
    public float enemyMinDistanceFromPlayer = 3f;

    [Header("Attack Balancing - Turn-Based System")]
    [Tooltip("Ascending attack: Strongest, 1 use")]
    public float ascendingMinDamage = 45f;
    public float ascendingMaxDamage = 60f;

    [Tooltip("Descending attack: Medium, 2 uses")]
    public float descendingMinDamage = 25f;
    public float descendingMaxDamage = 35f;

    [Tooltip("Stable attack: Weakest, 3 uses")]
    public float stableMinDamage = 15f;
    public float stableMaxDamage = 25f;
}
