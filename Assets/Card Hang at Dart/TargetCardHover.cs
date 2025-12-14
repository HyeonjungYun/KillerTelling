using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TargetCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.6f;

    private Image img;

    // ✅ "호버 들어가기 직전" 스케일을 저장 (실제 런타임 스케일 기준)
    private Vector3 baseScale;
    private bool hasBaseScale = false;

    private Color originalColor;
    private bool isHovering = false;

    // 🔊 SFX
    [Header("Sound (SFX)")]
    public AudioClip hoverSound;
    public AudioClip exitSound;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // 🎵 (선택) Hover 중 루프 BGM
    [Header("Sound (BGM / Loop)")]
    [Tooltip("호버 중에만 루프로 깔리는 사운드(없으면 무시)")]
    public AudioClip hoverLoopBgm;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private void Awake()
    {
        img = GetComponent<Image>();
        originalColor = img.color;

        // 🔊 오디오 소스 2개: SFX / BGM
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
    }

    private void OnEnable()
    {
        // ✅ 활성화 시점마다 현재 스케일을 기본값으로 갱신 (레이아웃/Instantiate 타이밍 대응)
        if (!isHovering)
        {
            baseScale = transform.localScale;
            hasBaseScale = true;
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip);
    }

    private void StartHoverBgm()
    {
        if (hoverLoopBgm == null || bgmSource == null) return;
        if (bgmSource.isPlaying && bgmSource.clip == hoverLoopBgm) return;

        bgmSource.clip = hoverLoopBgm;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    private void StopHoverBgm()
    {
        if (bgmSource == null) return;
        if (!bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (img == null) return;
        if (!img.raycastTarget) return;
        if (isHovering) return;

        // ✅ "지금 이 순간"의 스케일을 기준으로 잡기
        baseScale = transform.localScale;
        hasBaseScale = true;

        isHovering = true;

        transform.localScale = baseScale * hoverScale;
        img.color = new Color(1f, 1f, 0.7f, 1f);

        PlaySfx(hoverSound);
        StartHoverBgm();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (img == null) return;
        if (!img.raycastTarget) return;
        if (!isHovering) return;

        isHovering = false;

        // ✅ 호버 진입 직전 스케일로 정확히 복구
        if (hasBaseScale)
            transform.localScale = baseScale;

        img.color = originalColor;

        PlaySfx(exitSound);
        StopHoverBgm();
    }

    private void OnDisable()
    {
        StopHoverBgm();

        // ✅ 꺼질 때도 "원래(호버 진입 전)" 스케일로 복구
        if (isHovering)
        {
            isHovering = false;
            if (hasBaseScale) transform.localScale = baseScale;
            if (img != null) img.color = originalColor;
        }
        else
        {
            // 호버 중이 아니면, 현재 스케일을 기본값으로 갱신(다음 활성화/호버 대비)
            baseScale = transform.localScale;
            hasBaseScale = true;
        }
    }

    private void OnDestroy()
    {
        StopHoverBgm();
    }
}
