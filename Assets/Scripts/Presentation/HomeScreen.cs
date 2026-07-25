using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class HomeScreen : UiScreen
    {
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button scienceButton;
        [SerializeField] private Button generalKnowledgeButton;
        [SerializeField] private Button mathButton;
        [SerializeField] private GameObject scienceSelectionRing;
        [SerializeField] private GameObject generalKnowledgeSelectionRing;
        [SerializeField] private GameObject mathSelectionRing;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaderboardButton;

        private Topic _selectedTopic = Topic.Science;
        private Action<string, Topic> _onStart;
        private Action _onViewLeaderboard;

        public void Initialize(Action<string, Topic> onStart, Action onViewLeaderboard)
        {
            _onStart = onStart;
            _onViewLeaderboard = onViewLeaderboard;
        }

        public override void Show()
        {
            base.Show();
            SelectTopic(Topic.Science);
        }

        private void OnEnable()
        {
            scienceButton.onClick.AddListener(HandleScienceClicked);
            generalKnowledgeButton.onClick.AddListener(HandleGeneralKnowledgeClicked);
            mathButton.onClick.AddListener(HandleMathClicked);
            startButton.onClick.AddListener(HandleStart);
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.AddListener(HandleViewLeaderboard);
            }
        }

        private void OnDisable()
        {
            scienceButton.onClick.RemoveListener(HandleScienceClicked);
            generalKnowledgeButton.onClick.RemoveListener(HandleGeneralKnowledgeClicked);
            mathButton.onClick.RemoveListener(HandleMathClicked);
            startButton.onClick.RemoveListener(HandleStart);
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.RemoveListener(HandleViewLeaderboard);
            }
        }

        private void HandleScienceClicked() => SelectTopic(Topic.Science);
        private void HandleGeneralKnowledgeClicked() => SelectTopic(Topic.GeneralKnowledge);
        private void HandleMathClicked() => SelectTopic(Topic.Math);

        private void SelectTopic(Topic topic)
        {
            _selectedTopic = topic;

            if (scienceSelectionRing != null)
            {
                scienceSelectionRing.SetActive(topic == Topic.Science);
            }
            if (generalKnowledgeSelectionRing != null)
            {
                generalKnowledgeSelectionRing.SetActive(topic == Topic.GeneralKnowledge);
            }
            if (mathSelectionRing != null)
            {
                mathSelectionRing.SetActive(topic == Topic.Math);
            }
        }

        private void HandleStart()
        {
            string playerName = string.IsNullOrWhiteSpace(playerNameInput.text)
                ? "Player"
                : playerNameInput.text.Trim();

            _onStart?.Invoke(playerName, _selectedTopic);
        }

        private void HandleViewLeaderboard()
        {
            _onViewLeaderboard?.Invoke();
        }
    }
}
