using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI healthText;
    [SerializeField] public TextMeshProUGUI attackText;
    [SerializeField] public TextMeshProUGUI speedText;
    [SerializeField] public TextMeshProUGUI statText;
    [SerializeField] public Button healthButton;
    [SerializeField] public Button speedButton;
    [SerializeField] public Button damageButton;
    [SerializeField] public Button revertButton;
    [SerializeField] public Button saveButton;
    [SerializeField] private PlayerController player;

    private int tempHealth, tempDamage;
    private float tempSpeed;
    private int tempStatPoints;
    void Start()
    {
        Update();
        LoadStats();
        healthButton.onClick.AddListener(() => UpgradeStat("health")); 
        damageButton.onClick.AddListener(() => UpgradeStat("damage"));
        speedButton.onClick.AddListener(() => UpgradeStat("speed"));
        revertButton.onClick.AddListener(RevertStats);
        saveButton.onClick.AddListener(AppliedStat);
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            healthText.text = player.currentHealth + "/" + tempHealth;
            attackText.text = tempDamage.ToString();
            speedText.text = tempSpeed.ToString();
            statText.text = tempStatPoints.ToString();
        }
    }
    private void LoadStats()
    {
        tempHealth = player.maxHealth;
        tempDamage = player.damage;
        tempSpeed = player.moveSpeed;
        tempStatPoints = player.statPoints;
    }
    private void UpgradeStat(string statType)
    {
        if (tempStatPoints > 0)
        {
            switch (statType)
            {
                case "health":
                    tempHealth += 10; break;
                case "damage":
                    tempDamage += 5; break;
                case "speed":
                    tempSpeed += 0.5f; break;
            }

            tempStatPoints--;
        }
    }

    private void AppliedStat()
    {
         player.maxHealth = tempHealth;
         player.damage = tempDamage;
         player.moveSpeed = tempSpeed;
        player.statPoints = tempStatPoints;
    }
    private void RevertStats()
    {
        LoadStats();
    }
    public void ShowUpgradeMenu()
    {
        player.statPoints = 5;
        gameObject.SetActive(true); // Show UI when map is completed
    } 
    public void Hide()
    {
        gameObject.SetActive(false); // Show UI when map is completed
    }
}
