using UnityEngine;

namespace AI
{
    public static class NoiseChannel
    {
        public static event System.Action<Vector3, float> OnNoise;

        public static void Emit(Vector3 position, float loudness = 0f)
        {
            OnNoise?.Invoke(position, loudness);
        }
    }
}
