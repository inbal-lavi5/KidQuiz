using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public sealed class HomeScreen : UiScreen
    {
        private const string SoundMutedPrefKey = "KidQuiz.SoundMuted";

        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button scienceButton;
        [SerializeField] private Button generalKnowledgeButton;
        [SerializeField] private Button mathButton;
        [SerializeField] private GameObject scienceSelectionRing;
        [SerializeField] private GameObject generalKnowledgeSelectionRing;
        [SerializeField] private GameObject mathSelectionRing;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Image soundToggleFill;
        [SerializeField] private TMP_Text soundToggleLabel;

        private Topic _selectedTopic = Topic.Science;
        private bool _isMuted;
        private Action<string, Topic> _onStart;
        private Action _onViewLeaderboard;

        private void Awake()
        {
            _isMuted = PlayerPrefs.GetInt(SoundMutedPrefKey, 0) == 1;
            ApplyMuteState();
        }

        public void Initialize(Action<string, Topic> onStart, Action onViewLeaderboard)
        {
            _onStart = onStart;
            _onViewLeaderboard = onViewLeaderboard;
        }

        public override void Show()
        {
            base.Show();
            SelectTopic(Topic.Science);
            UpdateStartButtonState();
        }

        private void OnEnable()
        {
            scienceButton.onClick.AddListener(HandleScienceClicked);
            generalKnowledgeButton.onClick.AddListener(HandleGeneralKnowledgeClicked);
            mathButton.onClick.AddListener(HandleMathClicked);
            startButton.onClick.AddListener(HandleStart);
            playerNameInput.onValueChanged.AddListener(HandleNameChanged);
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.AddListener(HandleViewLeaderboard);
            }
            if (soundToggleButton != null)
            {
                soundToggleButton.onClick.AddListener(HandleSoundToggle);
            }
        }

        private void OnDisable()
        {
            scienceButton.onClick.RemoveListener(HandleScienceClicked);
            generalKnowledgeButton.onClick.RemoveListener(HandleGeneralKnowledgeClicked);
            mathButton.onClick.RemoveListener(HandleMathClicked);
            startButton.onClick.RemoveListener(HandleStart);
            playerNameInput.onValueChanged.RemoveListener(HandleNameChanged);
            if (leaderboardButton != null)
            {
                leaderboardButton.onClick.RemoveListener(HandleViewLeaderboard);
            }
            if (soundToggleButton != null)
            {
                soundToggleButton.onClick.RemoveListener(HandleSoundToggle);
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

        private void HandleNameChanged(string value)
        {
            UpdateStartButtonState();
        }

        private void UpdateStartButtonState()
        {
            startButton.interactable = !string.IsNullOrWhiteSpace(playerNameInput.text);
        }

        private void HandleStart()
        {
            string playerName = playerNameInput.text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            _onStart?.Invoke(playerName, _selectedTopic);
        }

        private void HandleViewLeaderboard()
        {
            _onViewLeaderboard?.Invoke();
        }

        private void HandleSoundToggle()
        {
            _isMuted = !_isMuted;
            PlayerPrefs.SetInt(SoundMutedPrefKey, _isMuted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState();
        }

        private void ApplyMuteState()
        {
            AudioListener.volume = _isMuted ? 0f : 1f;

            if (soundToggleFill != null)
            {
                soundToggleFill.color = _isMuted ? UiPalette.Silver : UiPalette.SkyBlue;
            }
            if (soundToggleLabel != null)
            {
                soundToggleLabel.text = _isMuted ? "OFF" : "ON";
            }
        }
    }
}
