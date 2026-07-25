using System;
using System.Collections.Generic;
using UnityEngine;

namespace KidQuiz.Data
{
    // Offline fallback data, hardcoded in the editor. Each entry is tagged with the
    // same trivia category id used to query the live API (see QuizConfig.triviaCategoryId),
    // so the two sources agree on what "Science"/"Math"/etc. means without a shared enum.
    [CreateAssetMenu(menuName = "KidQuiz/Question Bank", fileName = "QuestionBank")]
    public sealed class QuestionBank : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string prompt;
            public string correctAnswer;
            public List<string> incorrectAnswers;
            public int categoryId;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;
    }
}
