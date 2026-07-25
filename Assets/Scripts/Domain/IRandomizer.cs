using System.Collections.Generic;

namespace KidQuiz.Domain
{
    public interface IRandomizer
    {
        void Shuffle<T>(IList<T> list);
    }
}
