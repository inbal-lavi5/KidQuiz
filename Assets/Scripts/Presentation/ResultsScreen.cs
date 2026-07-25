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
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private TMP_Text leaderboardStatusText;

        private readonly List<GameObject> _leaderboardRows = new();
        private Action _onPlayAgain;

        public void Initialize(Action onPlayAgain)
        {
            _onPlayAgain = onPlayAgain;
        }

        public void ShowResult(QuizResult result)
        {
            scoreText.text = $"Score: {result.Score}";
            correctText.text = $"{result.CorrectCount} / {result.TotalQuestions} correct";
            ShowLeaderboardLoading();
        }

        public void ShowLeaderboardLoading()
        {
            ClearLeaderboardRows();
            SetStatus("Loading leaderboard...");
        }

        public void ShowLeaderboardUnavailable()
        {
            ClearLeaderboardRows();
            SetStatus("Leaderboard unavailable right now.");
        }

        // entries == null means the fetch failed; empty means it succeeded with no scores yet.
        public void ShowLeaderboardLoaded(IReadOnlyList<ScoreEntry> entries)
        {
            ClearLeaderboardRows();

            if (entries == null)
            {
                SetStatus("Leaderboard unavailable right now.");
                return;
            }

            if (entries.Count == 0)
            {
                SetStatus("No scores yet - be the first!");
                return;
            }

            SetStatus(null);

            foreach (ScoreEntry entry in entries)
            {
                CreateLeaderboardRow(entry);
            }
        }

        private void SetStatus(string message)
        {
            if (leaderboardStatusText == null)
            {
                return;
            }

            bool hasMessage = !string.IsNullOrEmpty(message);
            leaderboardStatusText.gameObject.SetActive(hasMessage);
            leaderboardStatusText.text = hasMessage ? message : string.Empty;
        }

        private void CreateLeaderboardRow(ScoreEntry entry)
        {
            var rowGO = new GameObject("LeaderboardRow", typeof(RectTransform));
            rowGO.transform.SetParent(leaderboardContainer, false);

            var text = rowGO.AddComponent<TextMeshProUGUI>();
            text.text = $"{entry.PlayerName} - {entry.Score}";
            text.fontSize = 36;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.MidlineLeft;

            var layoutElement = rowGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;

            _leaderboardRows.Add(rowGO);
        }

        private void ClearLeaderboardRows()
        {
            foreach (GameObject row in _leaderboardRows)
            {
                Destroy(row);
            }
            _leaderboardRows.Clear();
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
