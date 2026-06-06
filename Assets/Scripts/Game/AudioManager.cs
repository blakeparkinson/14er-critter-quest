using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float ambientVolume = 0.3f;

    private AudioSource musicSource;
    private AudioSource ambientSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private int maxSfxSources = 8;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = CreateSource("Music", true);
        ambientSource = CreateSource("Ambient", true);

        for (int i = 0; i < maxSfxSources; i++)
            sfxSources.Add(CreateSource($"SFX_{i}", false));

        GenerateProceduralSounds();
    }

    private AudioSource CreateSource(string name, bool loop)
    {
        var obj = new GameObject(name);
        obj.transform.parent = transform;
        var source = obj.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        return source;
    }

    public void PlaySFX(string clipName)
    {
        var clip = GetClip(clipName);
        if (clip == null) return;

        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
            {
                source.clip = clip;
                source.volume = sfxVolume * masterVolume;
                source.pitch = 1f + Random.Range(-0.1f, 0.1f);
                source.Play();
                return;
            }
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlayAmbient(AudioClip clip)
    {
        ambientSource.clip = clip;
        ambientSource.volume = ambientVolume * masterVolume;
        ambientSource.Play();
    }

    // procedurally generated placeholder sounds
    private Dictionary<string, AudioClip> proceduralClips = new Dictionary<string, AudioClip>();

    private void GenerateProceduralSounds()
    {
        proceduralClips["shutter"] = GenerateTone(0.1f, 800, 400, 0.3f);
        proceduralClips["jump"] = GenerateTone(0.15f, 300, 600, 0.2f);
        proceduralClips["land"] = GenerateTone(0.1f, 200, 80, 0.15f);
        proceduralClips["discovery"] = GenerateChime();
        proceduralClips["summit"] = GenerateFanfare();
        proceduralClips["squeak"] = GenerateTone(0.08f, 2000, 2500, 0.15f);
        proceduralClips["whistle"] = GenerateTone(0.3f, 1200, 800, 0.2f);
        proceduralClips["footstep"] = GenerateFootstep();
    }

    private AudioClip GenerateFootstep()
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float env = Mathf.Exp(-t * 30f);
            float noise = (Random.Range(-1f, 1f)) * 0.2f * env;
            float thud = Mathf.Sin(t * 200f) * 0.15f * env;
            samples[i] = noise + thud;
        }

        var clip = AudioClip.Create("footstep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateTone(float duration, float startFreq, float endFreq, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = 1f - t;
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate) * volume * envelope;
        }

        var clip = AudioClip.Create($"tone_{startFreq}_{endFreq}", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateChime()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f }; // C5 E5 G5 C6

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float sample = 0;
            for (int n = 0; n < notes.Length; n++)
            {
                float noteStart = n * 0.12f;
                float noteT = t - noteStart;
                if (noteT > 0)
                {
                    float env = Mathf.Max(0, 1f - noteT * 3f);
                    sample += Mathf.Sin(2 * Mathf.PI * notes[n] * i / sampleRate) * 0.15f * env;
                }
            }
            samples[i] = sample;
        }

        var clip = AudioClip.Create("chime", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateFanfare()
    {
        int sampleRate = 44100;
        float duration = 1.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[][] melody = {
            new[] { 0f, 0.2f, 392f },    // G4
            new[] { 0.2f, 0.4f, 523.25f },  // C5
            new[] { 0.4f, 0.6f, 659.25f },  // E5
            new[] { 0.6f, 1.4f, 783.99f },  // G5 (hold)
        };

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount * duration;
            float sample = 0;
            foreach (var note in melody)
            {
                if (t >= note[0] && t < note[1])
                {
                    float noteProgress = (t - note[0]) / (note[1] - note[0]);
                    float env = noteProgress < 0.1f ? noteProgress * 10f : Mathf.Max(0, 1f - (noteProgress - 0.1f) * 0.5f);
                    sample += Mathf.Sin(2 * Mathf.PI * note[2] * i / sampleRate) * 0.2f * env;
                    sample += Mathf.Sin(2 * Mathf.PI * note[2] * 2 * i / sampleRate) * 0.05f * env;
                }
            }
            samples[i] = sample;
        }

        var clip = AudioClip.Create("fanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GetClip(string name)
    {
        return proceduralClips.TryGetValue(name, out var clip) ? clip : null;
    }
}
