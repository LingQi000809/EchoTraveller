using UnityEngine;
using UnityEngine.InputSystem;

public class BattlePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private VoiceRayAttack rayAttackPrefab;
    [SerializeField] private Transform raySpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip rayShootSound;
    [SerializeField] private AudioClip rayHitSound;

    private Vector2 moveInput;
    private Animator animator;
    private VoiceController.PitchTrend queuedAttack = VoiceController.PitchTrend.None;
    private bool canAttack = true;
    private float attackCooldown = 0.3f;
    private float lastAttackTime;

    private Vector2 arenaBoundsMin;
    private Vector2 arenaBoundsMax;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (raySpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("RaySpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = new Vector3(0, 0.5f, 0);
            raySpawnPoint = spawnPoint.transform;
        }
    }

    public void SetArenaBounds(Vector2 min, Vector2 max)
    {
        arenaBoundsMin = min;
        arenaBoundsMax = max;
    }

    private void Update()
    {
        HandleMovement();
        HandleVoiceAttack();
    }

    private void HandleMovement()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;

            newPosition.x = Mathf.Clamp(newPosition.x, arenaBoundsMin.x, arenaBoundsMax.x);
            newPosition.y = Mathf.Clamp(newPosition.y, arenaBoundsMin.y, arenaBoundsMax.y);

            transform.position = newPosition;

            if (animator != null)
            {
                animator.SetFloat("moveX", moveInput.x);
                animator.SetFloat("moveY", moveInput.y);
                animator.SetBool("isMoving", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    private void HandleVoiceAttack()
    {
        if (VoiceController.instance != null && VoiceController.instance.HasDetectedTrend())
        {
            queuedAttack = VoiceController.instance.GetLastDetectedTrend();
        }

        if (queuedAttack != VoiceController.PitchTrend.None && canAttack)
        {
            FireRayAttack(queuedAttack);
            queuedAttack = VoiceController.PitchTrend.None;
        }
    }

    private void FireRayAttack(VoiceController.PitchTrend trend)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        Vector3 direction = Vector3.zero;

        switch (trend)
        {
            case VoiceController.PitchTrend.Ascending:
                direction = new Vector3(1, 1, 0).normalized;
                break;
            case VoiceController.PitchTrend.Descending:
                direction = new Vector3(1, -1, 0).normalized;
                break;
            case VoiceController.PitchTrend.Stable:
                direction = Vector3.right;
                break;
        }

        if (rayShootSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound(rayShootSound);
        }

        if (rayAttackPrefab != null)
        {
            VoiceRayAttack ray = Instantiate(rayAttackPrefab, raySpawnPoint.position, Quaternion.identity);
            ray.FireRay(raySpawnPoint.position, direction, trend, 10f);

            if (BattleManager.instance != null)
            {
                bool hitEnemy = BattleManager.instance.CheckRayHitEnemy(raySpawnPoint.position, direction, 10f);
                if (hitEnemy && rayHitSound != null && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySound(rayHitSound);
                }
            }

            Destroy(ray.gameObject, 1f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (BattleManager.instance != null)
        {
            BattleManager.instance.DamagePlayer(damage);
        }
    }
}
