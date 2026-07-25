using System.Collections.Generic;
using KidQuiz.Domain;

namespace KidQuiz.Domain.Tests
{
    // Leaves order untouched so tests stay deterministic.
    public sealed class FakeRandomizer : IRandomizer
    {
        public void Shuffle<T>(IList<T> list)
        {
        }
    }
}
