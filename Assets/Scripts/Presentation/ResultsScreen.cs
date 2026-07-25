using System;
using System.Collections.Generic;
using KidQuiz.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class ResultsScreen : UiScreen
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text correctText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text leaderboardTitleText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private LeaderboardView leaderboardView;

        private Action _onPlayAgain;
        private Action _onHome;

        public void Initialize(Action onPlayAgain, Action onHome)
        {
            _onPlayAgain = onPlayAgain;
            _onHome = onHome;
        }

        public void ShowResult(QuizResult result, string playerName, string category)
        {
            scoreText.text = $"{result.CorrectCount}/{result.TotalQuestions}";
            correctText.text = $"Correct Answers · {result.Score} pts";

            if (subtitleText != null)
            {
                subtitleText.text = $"Great exploring, {playerName}!";
            }
            if (leaderboardTitleText != null)
            {
                leaderboardTitleText.text = $"{category} Leaderboard";
            }

            leaderboardView.SetCurrentPlayerName(playerName);
            leaderboardView.ShowLoading();
        }

        public void ShowLeaderboardUnavailable()
        {
            leaderboardView.ShowUnavailable();
        }

        public void ShowLeaderboardLoaded(IReadOnlyList<ScoreEntry> entries)
        {
            leaderboardView.ShowLoaded(entries);
        }

        private void OnEnable()
        {
            playAgainButton.onClick.AddListener(HandlePlayAgain);
            if (homeButton != null)
            {
                homeButton.onClick.AddListener(HandleHome);
            }
        }

        private void OnDisable()
        {
            playAgainButton.onClick.RemoveListener(HandlePlayAgain);
            if (homeButton != null)
            {
                homeButton.onClick.RemoveListener(HandleHome);
            }
        }

        private void HandlePlayAgain()
        {
            _onPlayAgain?.Invoke();
        }

        private void HandleHome()
        {
            _onHome?.Invoke();
        }
    }
}
