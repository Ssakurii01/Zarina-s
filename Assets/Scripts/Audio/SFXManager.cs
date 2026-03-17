using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Clips (assign in Inspector or auto-generated)")]
    [SerializeField] private AudioClip _jumpClip;
    [SerializeField] private AudioClip _bombTickClip;
    [SerializeField] private AudioClip _bombExplodeClip;
    [SerializeField] private AudioClip _bombTransferClip;
    [SerializeField] private AudioClip _portalClip;
    [SerializeField] private AudioClip _platformBreakClip;
    [SerializeField] private AudioClip _gameOverClip;
    [SerializeField] private AudioClip _roundStartClip;
    [SerializeField] private AudioClip _pickupClip;

    private AudioSource _sfxSource;
    private AudioSource _tickSource; // separate source for looping tick

    private float _lastTickTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _tickSource = gameObject.AddComponent<AudioSource>();
        _tickSource.playOnAwake = false;
        _tickSource.loop = false;

        GeneratePlaceholderClips();
    }

    private void GeneratePlaceholderClips()
    {
        // Generate simple procedural audio clips as placeholders
        // These work without any imported audio files
        if (_jumpClip == null) _jumpClip = GenerateTone(0.1f, 600f, 900f);
        if (_bombTickClip == null) _bombTickClip = GenerateTone(0.05f, 800f, 800f);
        if (_bombExplodeClip == null) _bombExplodeClip = GenerateNoise(0.4f);
        if (_bombTransferClip == null) _bombTransferClip = GenerateTone(0.12f, 400f, 700f);
        if (_portalClip == null) _portalClip = GenerateTone(0.2f, 300f, 1200f);
        if (_platformBreakClip == null) _platformBreakClip = GenerateNoise(0.25f);
        if (_gameOverClip == null) _gameOverClip = GenerateTone(0.5f, 500f, 200f);
        if (_roundStartClip == null) _roundStartClip = GenerateTone(0.15f, 500f, 800f);
        if (_pickupClip == null) _pickupClip = GenerateTone(0.1f, 800f, 1200f);
    }

    private AudioClip GenerateTone(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float amplitude = 1f - t; // fade out
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * amplitude * 0.5f;
        }

        var clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateNoise(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float amplitude = 1f - t; // fade out
            samples[i] = Random.Range(-1f, 1f) * amplitude * 0.4f;
        }

        var clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayJump()
    {
        if (_jumpClip != null)
            _sfxSource.PlayOneShot(_jumpClip, 0.4f);
    }

    public void PlayBombTick(float normalizedTime)
    {
        // Throttle ticks — play faster as timer decreases
        float interval = Mathf.Lerp(0.8f, 0.15f, 1f - normalizedTime);
        if (Time.time - _lastTickTime < interval) return;
        _lastTickTime = Time.time;

        if (_bombTickClip != null)
        {
            _tickSource.pitch = Mathf.Lerp(0.8f, 1.6f, 1f - normalizedTime);
            _tickSource.PlayOneShot(_bombTickClip, 0.5f);
        }
    }

    public void PlayBombExplode()
    {
        if (_bombExplodeClip != null)
            _sfxSource.PlayOneShot(_bombExplodeClip, 0.8f);
    }

    public void PlayBombTransfer()
    {
        if (_bombTransferClip != null)
            _sfxSource.PlayOneShot(_bombTransferClip, 0.5f);
    }

    public void PlayPortalTeleport()
    {
        if (_portalClip != null)
            _sfxSource.PlayOneShot(_portalClip, 0.5f);
    }

    public void PlayPlatformBreak()
    {
        if (_platformBreakClip != null)
            _sfxSource.PlayOneShot(_platformBreakClip, 0.6f);
    }

    public void PlayGameOver()
    {
        if (_gameOverClip != null)
            _sfxSource.PlayOneShot(_gameOverClip, 0.7f);
    }

    public void PlayRoundStart()
    {
        if (_roundStartClip != null)
            _sfxSource.PlayOneShot(_roundStartClip, 0.5f);
    }

    public void PlayPickup()
    {
        if (_pickupClip != null)
            _sfxSource.PlayOneShot(_pickupClip, 0.5f);
    }
}
