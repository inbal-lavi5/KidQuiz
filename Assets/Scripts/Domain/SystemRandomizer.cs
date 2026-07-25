using System;
using System.Collections.Generic;

namespace KidQuiz.Domain
{
    // Uses System.Random, not UnityEngine.Random - this layer never references UnityEngine.
    public sealed class SystemRandomizer : IRandomizer
    {
        private readonly Random _random = new Random();

        public void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
