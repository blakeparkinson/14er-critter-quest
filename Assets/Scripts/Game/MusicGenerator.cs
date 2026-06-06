using UnityEngine;

public class MusicGenerator : MonoBehaviour
{
    private AudioSource musicSource;
    private float mapHeight = 100f;
    private Transform player;

    // pentatonic scale frequencies (C major pentatonic, multiple octaves)
    private float[] notes = {
        261.63f, 293.66f, 329.63f, 392.00f, 440.00f,  // C4 D4 E4 G4 A4
        523.25f, 587.33f, 659.25f, 783.99f, 880.00f    // C5 D5 E5 G5 A5
    };

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        var obj = new GameObject("MusicSource");
        obj.transform.parent = transform;
        musicSource = obj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.12f;
        musicSource.playOnAwake = false;

        GenerateAndPlay();
    }

    private void GenerateAndPlay()
    {
        int sampleRate = 44100;
        float bpm = 72f;
        float beatDuration = 60f / bpm;
        int beatsPerBar = 4;
        int totalBars = 16;
        int totalBeats = totalBars * beatsPerBar;
        int totalSamples = (int)(totalBeats * beatDuration * sampleRate);

        float[] samples = new float[totalSamples];

        // chord progression (I - vi - IV - V in C pentatonic)
        int[][] chordPatterns = {
            new[] { 0, 2, 4 },    // C E G
            new[] { 4, 6, 8 },    // A C E
            new[] { 3, 5, 7 },    // G B D (approx with pentatonic)
            new[] { 1, 3, 5 }     // D G B
        };

        // melody seed — repeatable pattern
        Random.InitState(14);

        for (int beat = 0; beat < totalBeats; beat++)
        {
            int bar = beat / beatsPerBar;
            int beatInBar = beat % beatsPerBar;
            int chordIdx = (bar % 4);
            int[] chord = chordPatterns[chordIdx];

            int startSample = (int)(beat * beatDuration * sampleRate);
            int beatSamples = (int)(beatDuration * sampleRate);

            // pad/drone (soft chord)
            foreach (int noteIdx in chord)
            {
                float freq = notes[Mathf.Clamp(noteIdx, 0, notes.Length - 1)] * 0.5f; // octave down
                for (int i = 0; i < beatSamples && startSample + i < totalSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float env = Mathf.Min(1, (beatSamples - i) / (float)(sampleRate * 0.3f));
                    env = Mathf.Min(env, i / (float)(sampleRate * 0.05f)); // attack
                    float wave = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.04f * env;
                    // softer triangle wave for warmth
                    wave += (2 * Mathf.Abs(2 * ((freq * t) % 1) - 1) - 1) * 0.02f * env;
                    samples[startSample + i] += wave;
                }
            }

            // melody (on beats 1 and 3, sometimes 2)
            bool playMelody = beatInBar == 0 || beatInBar == 2 || (beatInBar == 1 && Random.value > 0.6f);
            if (playMelody)
            {
                int melodyNote = chord[Random.Range(0, chord.Length)];
                // occasionally step to neighbor note
                if (Random.value > 0.5f)
                    melodyNote = Mathf.Clamp(melodyNote + Random.Range(-1, 2), 0, notes.Length - 1);

                float mFreq = notes[melodyNote];
                float noteDur = beatDuration * (Random.value > 0.3f ? 1f : 0.5f);
                int noteSamples = (int)(noteDur * sampleRate);

                for (int i = 0; i < noteSamples && startSample + i < totalSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float env = Mathf.Exp(-t * 3f); // plucky decay
                    env *= Mathf.Min(1, i / (float)(sampleRate * 0.01f)); // tiny attack
                    // sine + slight harmonics for a music-box / kalimba feel
                    float wave = Mathf.Sin(2 * Mathf.PI * mFreq * t) * 0.08f * env;
                    wave += Mathf.Sin(2 * Mathf.PI * mFreq * 2 * t) * 0.02f * env;
                    wave += Mathf.Sin(2 * Mathf.PI * mFreq * 3 * t) * 0.008f * env;
                    samples[startSample + i] += wave;
                }
            }

            // gentle bass on beat 1
            if (beatInBar == 0)
            {
                float bassFreq = notes[chord[0]] * 0.25f; // two octaves down
                int bassSamples = (int)(beatDuration * 2 * sampleRate);
                for (int i = 0; i < bassSamples && startSample + i < totalSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float env = Mathf.Exp(-t * 1.5f);
                    env *= Mathf.Min(1, i / (float)(sampleRate * 0.02f));
                    float wave = Mathf.Sin(2 * Mathf.PI * bassFreq * t) * 0.06f * env;
                    samples[startSample + i] += wave;
                }
            }
        }

        // gentle limiter
        float maxAmp = 0;
        for (int i = 0; i < samples.Length; i++)
            if (Mathf.Abs(samples[i]) > maxAmp) maxAmp = Mathf.Abs(samples[i]);
        if (maxAmp > 0.8f)
            for (int i = 0; i < samples.Length; i++)
                samples[i] *= 0.8f / maxAmp;

        var clip = AudioClip.Create("HikingMusic", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        musicSource.clip = clip;
        musicSource.Play();
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        // volume fades up as you climb, slight variation by biome
        float progress = Mathf.Clamp01(player.position.y / mapHeight);
        float targetVol = Mathf.Lerp(0.08f, 0.15f, progress);
        musicSource.volume = Mathf.Lerp(musicSource.volume, targetVol, Time.deltaTime);

        // pitch shifts very slightly with altitude for atmosphere
        musicSource.pitch = Mathf.Lerp(0.95f, 1.05f, progress);
    }
}
