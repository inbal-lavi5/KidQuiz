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
        [SerializeField] private Button scienceTabButton;
        [SerializeField] private Button generalKnowledgeTabButton;
        [SerializeField] private Button mathTabButton;
        [SerializeField] private Image scienceTabFill;
        [SerializeField] private Image generalKnowledgeTabFill;
        [SerializeField] private Image mathTabFill;
        [SerializeField] private LeaderboardView leaderboardView;

        private Action _onBack;
        private Action<Topic> _onSelectTopic;

        public void Initialize(Action onBack, Action<Topic> onSelectTopic)
        {
            _onBack = onBack;
            _onSelectTopic = onSelectTopic;
        }

        public void SelectTopic(Topic topic)
        {
            SetTabColor(scienceTabFill, topic == Topic.Science);
            SetTabColor(generalKnowledgeTabFill, topic == Topic.GeneralKnowledge);
            SetTabColor(mathTabFill, topic == Topic.Math);

            _onSelectTopic?.Invoke(topic);
        }

        private void SetTabColor(Image tab, bool selected)
        {
            if (tab == null)
            {
                return;
            }
            tab.color = selected ? UiPalette.SkyBlue : UiPalette.CardWhite;
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
            scienceTabButton.onClick.AddListener(HandleScienceTab);
            generalKnowledgeTabButton.onClick.AddListener(HandleGeneralKnowledgeTab);
            mathTabButton.onClick.AddListener(HandleMathTab);
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveListener(HandleBack);
            scienceTabButton.onClick.RemoveListener(HandleScienceTab);
            generalKnowledgeTabButton.onClick.RemoveListener(HandleGeneralKnowledgeTab);
            mathTabButton.onClick.RemoveListener(HandleMathTab);
        }

        private void HandleScienceTab() => SelectTopic(Topic.Science);
        private void HandleGeneralKnowledgeTab() => SelectTopic(Topic.GeneralKnowledge);
        private void HandleMathTab() => SelectTopic(Topic.Math);

        private void HandleBack()
        {
            _onBack?.Invoke();
        }
    }
}
