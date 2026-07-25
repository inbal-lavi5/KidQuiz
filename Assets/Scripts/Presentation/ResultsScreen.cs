using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class ResultsScreen : UiScreen
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text correctText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private TMP_Text leaderboardStatusText;

        private Action _onPlayAgain;

        public Transform LeaderboardContainer => leaderboardContainer;

        public void Initialize(Action onPlayAgain)
        {
            _onPlayAgain = onPlayAgain;
        }

        public void ShowResult(QuizResult result)
        {
            scoreText.text = $"Score: {result.Score}";
            correctText.text = $"{result.CorrectCount} / {result.TotalQuestions} correct";

            if (leaderboardStatusText != null)
            {
                leaderboardStatusText.text = "Leaderboard coming soon.";
            }
        }

        private void OnEnable()
        {
            playAgainButton.onClick.AddListener(HandlePlayAgain);
        }

        private void OnDisable()
        {
            playAgainButton.onClick.RemoveListener(HandlePlayAgain);
        }

        private void HandlePlayAgain()
        {
            _onPlayAgain?.Invoke();
        }
    }
}
