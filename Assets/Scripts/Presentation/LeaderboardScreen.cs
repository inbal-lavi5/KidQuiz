using System;
using System.Collections.Generic;
using KidQuiz.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class LeaderboardScreen : UiScreen
    {
        [SerializeField] private Button backButton;
        [SerializeField] private LeaderboardView leaderboardView;

        private Action _onBack;

        public void Initialize(Action onBack)
        {
            _onBack = onBack;
        }

        public void ShowLoading()
        {
            leaderboardView.ShowLoading();
        }

        public void ShowUnavailable()
        {
            leaderboardView.ShowUnavailable();
        }

        public void ShowLoaded(IReadOnlyList<ScoreEntry> entries)
        {
            leaderboardView.ShowLoaded(entries);
        }

        private void OnEnable()
        {
            backButton.onClick.AddListener(HandleBack);
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveListener(HandleBack);
        }

        private void HandleBack()
        {
            _onBack?.Invoke();
        }
    }
}
