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
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private TMP_Text leaderboardStatusText;

        [Header("Leaderboard row building blocks")]
        [SerializeField] private TMP_FontAsset rowNameFont;
        [SerializeField] private TMP_FontAsset rowScoreFont;
        [SerializeField] private Sprite circleSprite;

        private static readonly Color[] RankColors = { UiPalette.SunYellow, UiPalette.Silver, UiPalette.Bronze };

        private readonly List<GameObject> _leaderboardRows = new();
        private Action _onPlayAgain;
        private Action _onHome;
        private string _currentPlayerName;

        public void Initialize(Action onPlayAgain, Action onHome)
        {
            _onPlayAgain = onPlayAgain;
            _onHome = onHome;
        }

        public void ShowResult(QuizResult result, string playerName)
        {
            _currentPlayerName = playerName;
            scoreText.text = $"{result.CorrectCount}/{result.TotalQuestions}";
            correctText.text = $"Correct Answers · {result.Score} pts";

            if (subtitleText != null)
            {
                subtitleText.text = $"Great exploring, {playerName}!";
            }

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

            for (int i = 0; i < entries.Count; i++)
            {
                CreateLeaderboardRow(entries[i], i);
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

        private void CreateLeaderboardRow(ScoreEntry entry, int rank)
        {
            bool isCurrentPlayer = !string.IsNullOrEmpty(_currentPlayerName) &&
                                    string.Equals(entry.PlayerName, _currentPlayerName, StringComparison.OrdinalIgnoreCase);

            var row = new GameObject("LeaderboardRow", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(leaderboardContainer, false);
            row.GetComponent<Image>().color = isCurrentPlayer ? UiPalette.DeepSky : new Color(0, 0, 0, 0);

            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 20;
            rowLayout.padding = new RectOffset(10, 10, 6, 6);
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            var rowLayoutElement = row.AddComponent<LayoutElement>();
            rowLayoutElement.preferredHeight = 76;

            Color rankColor = rank < RankColors.Length ? RankColors[rank] : Color.white;
            var rankBadge = CreateCircle(row.transform, 56, rankColor, (rank + 1).ToString());

            var name = new GameObject("Name", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            name.transform.SetParent(row.transform, false);
            name.font = rowNameFont;
            name.fontSize = 30;
            name.color = UiPalette.Ink;
            name.text = isCurrentPlayer ? $"{entry.PlayerName} (You)" : entry.PlayerName;
            name.alignment = TextAlignmentOptions.MidlineLeft;
            var nameLayout = name.gameObject.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1;
            nameLayout.minWidth = 100;

            var score = new GameObject("Score", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            score.transform.SetParent(row.transform, false);
            score.font = rowScoreFont;
            score.fontSize = 30;
            score.color = UiPalette.Ink;
            score.text = entry.Score.ToString();
            score.alignment = TextAlignmentOptions.MidlineRight;
            var scoreLayout = score.gameObject.AddComponent<LayoutElement>();
            scoreLayout.preferredWidth = 100;

            _leaderboardRows.Add(row);
        }

        private GameObject CreateCircle(Transform parent, float diameter, Color color, string label)
        {
            var go = new GameObject("RankBadge", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().sprite = circleSprite;
            go.GetComponent<Image>().color = color;
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = diameter;
            layoutElement.preferredHeight = diameter;

            var text = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(go.transform, false);
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            text.font = rowNameFont;
            text.fontSize = 24;
            text.color = UiPalette.Ink;
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;

            return go;
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
