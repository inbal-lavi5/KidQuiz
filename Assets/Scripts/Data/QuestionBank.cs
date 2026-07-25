using System;
using System.Collections.Generic;
using KidQuiz.Domain;
using UnityEngine;

namespace KidQuiz.Data
{
    // Offline fallback data - 15 kid-friendly questions, hardcoded in the editor.
    [CreateAssetMenu(menuName = "KidQuiz/Question Bank", fileName = "QuestionBank")]
    public sealed class QuestionBank : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string prompt;
            public string correctAnswer;
            public List<string> incorrectAnswers;
            public Difficulty difficulty;
        }

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;
    }
}
