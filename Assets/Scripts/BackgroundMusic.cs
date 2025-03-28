using UnityEngine;
using System.Collections;

public class BackgroundMusic : MonoBehaviour
{
    // Mảng chứa các bài nhạc (AudioClip)
    [SerializeField] private AudioClip[] musicClips;

    private AudioSource audioSource;

    // Singleton để đảm bảo chỉ có 1 BackgroundMusic tồn tại xuyên suốt các scene
    private static BackgroundMusic instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Không bị hủy khi chuyển scene
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Nếu chưa có AudioSource, thêm vào GameObject
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Đảm bảo AudioSource không loop vì ta sẽ tự chọn track mới sau khi hoàn thành clip hiện tại
        audioSource.loop = false;
        audioSource.volume = 0.5f; // Điều chỉnh volume theo ý muốn

        PlayRandomTrack();
    }

    // Phát một bài nhạc ngẫu nhiên
    void PlayRandomTrack()
    {
        if (musicClips.Length == 0)
        {
            Debug.LogWarning("Chưa có bài nhạc nào được gán vào mảng MusicClips!");
            return;
        }

        int randomIndex = Random.Range(0, musicClips.Length);
        audioSource.clip = musicClips[randomIndex];
        audioSource.Play();

        // Bắt đầu coroutine đợi cho đến khi bài nhạc hiện tại kết thúc
        StartCoroutine(WaitForTrackEnd());
    }

    IEnumerator WaitForTrackEnd()
    {
        // Chờ cho đến khi bài nhạc hiện tại chạy xong
        yield return new WaitForSeconds(audioSource.clip.length);
        // Thêm một chút delay nếu cần, ví dụ 0.5s
        yield return new WaitForSeconds(0.5f);
        PlayRandomTrack();
    }
}
