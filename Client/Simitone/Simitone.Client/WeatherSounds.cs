using Microsoft.Xna.Framework.Audio;
using System;
using System.IO;

namespace Simitone.Client
{
    public static class WeatherSounds
    {
        private static SoundEffect RainLoopSound;
        private static SoundEffect ThunderSound;
        private static SoundEffectInstance RainLoopInstance;

        private static bool IsLoaded = false;
        private static bool IsRainPlaying = false;

        public static void Load(string contentPath)
        {
            try
            {
                var rainPath = Path.Combine(contentPath, "Sounds", "rain_loop.wav");
                var thunderPath = Path.Combine(contentPath, "Sounds", "thunder.wav");

                if (File.Exists(rainPath))
                {
                    using (var stream = File.OpenRead(rainPath))
                    {
                        RainLoopSound = SoundEffect.FromStream(stream);
                    }
                }

                if (File.Exists(thunderPath))
                {
                    using (var stream = File.OpenRead(thunderPath))
                    {
                        ThunderSound = SoundEffect.FromStream(stream);
                    }
                }

                IsLoaded = RainLoopSound != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load weather sounds: {ex.Message}");
                IsLoaded = false;
            }
        }

        public static void PlayRain(float intensity)
        {
            if (!IsLoaded || RainLoopSound == null) return;

            if (!IsRainPlaying)
            {
                RainLoopInstance = RainLoopSound.CreateInstance();
                RainLoopInstance.IsLooped = true;
                RainLoopInstance.Volume = Math.Clamp(intensity, 0f, 1f) * 0.5f;
                RainLoopInstance.Play();
                IsRainPlaying = true;
            }
            else
            {
                if (RainLoopInstance != null)
                {
                    RainLoopInstance.Volume = Math.Clamp(intensity, 0f, 1f) * 0.5f;
                }
            }
        }

        public static void PauseRain()
        {
            if (RainLoopInstance != null && IsRainPlaying)
                RainLoopInstance.Pause();
        }

        public static void ResumeRain()
        {
            if (RainLoopInstance != null && IsRainPlaying)
                RainLoopInstance.Resume();
        }

        public static void StopRain()
        {
            if (RainLoopInstance != null && IsRainPlaying)
            {
                RainLoopInstance.Stop();
                RainLoopInstance.Dispose();
                RainLoopInstance = null;
                IsRainPlaying = false;
            }
        }

        public static void PlayThunder(float volume = 0.7f)
        {
            if (!IsLoaded || ThunderSound == null) return;

            ThunderSound.Play(Math.Clamp(volume, 0f, 1f), 0f, 0f);
        }

        public static void Unload()
        {
            StopRain();
            RainLoopSound?.Dispose();
            ThunderSound?.Dispose();
            RainLoopSound = null;
            ThunderSound = null;
            IsLoaded = false;
        }
    }
}
