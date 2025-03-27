using UnityEngine;

public class BulletController1 : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            // Đặt tất cả parameter về false trước khi chọn ngẫu nhiên
            animator.SetBool("S1", false);
            animator.SetBool("S2", false);
            animator.SetBool("S3", false);

            // Chọn ngẫu nhiên một parameter để bật
            int randomIndex = Random.Range(0, 3);
            string selectedParam = randomIndex == 0 ? "S1" : randomIndex == 1 ? "S2" : "S3";
            animator.SetBool(selectedParam, true);
        }
    }
}