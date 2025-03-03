using UnityEngine;

public class SkillHandle : MonoBehaviour
{
    [SerializeField] private GameObject skillPrefab; // Prefab Skill1
    [SerializeField] private Transform skillSpawnPoint; // Điểm xuất hiện skill
    [SerializeField] private float skillCooldown = 5f; // Thời gian hồi chiêu 5 giây

    private Animator animator;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.F) && cooldownTimer >= skillCooldown) // Khi nhấn F và hết cooldown
        {
            UseSkill();
            cooldownTimer = 0f; // Reset cooldown
        }
    }

    private void UseSkill()
    {
        animator.SetTrigger("skill1"); // Kích hoạt animation nhân vật

        // Tạo skill tại vị trí skillSpawnPoint
        GameObject skill = Instantiate(skillPrefab, skillSpawnPoint.position, Quaternion.identity);

        // Hủy skill sau 0.5 giây
        Destroy(skill, 0.5f);
    }
}
