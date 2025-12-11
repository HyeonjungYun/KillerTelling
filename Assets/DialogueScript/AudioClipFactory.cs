using UnityEngine;

public static class AudioClipFactory
{
    // 사인파(Sine Wave) 비프음 생성 함수
    public static AudioClip CreateBeep(string name, int frequency, float durationSeconds)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * durationSeconds);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // 사인파 공식
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);

            // 틱! 하는 잡음 방지를 위해 끝부분 부드럽게 처리 (Fade Out)
            if (i > sampleCount - 1000)
            {
                samples[i] *= (sampleCount - i) / 1000f;
            }
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}