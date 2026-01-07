using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText;

    private float maxHealth;
    private float currentHealth;

    public void Initialize(float maxHP)
    {
        maxHealth = maxHP;
        currentHealth = maxHP;
        UpdateDisplay();
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateDisplay();
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void UpdateDisplay()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"<b>{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}</b>";
        }
    }
}
