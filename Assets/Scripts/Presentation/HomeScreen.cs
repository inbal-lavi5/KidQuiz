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
        [SerializeField] private TMP_Text nameHintText;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;

        private Topic _selectedTopic = Topic.Science;
        private Action<string, Topic> _onStart;
        private Action _onViewLeaderboard;
        private Action _onToggleSound;
        private AudioManager _audioManager;

        public void Initialize(Action<string, Topic> onStart, Action onViewLeaderboard, Action onToggleSound, AudioManager audioManager)
        {
            _onStart = onStart;
            _onViewLeaderboard = onViewLeaderboard;
            _onToggleSound = onToggleSound;
            _audioManager = audioManager;
        }

        public void SetMuted(bool isMuted)
        {
            if (soundIcon != null)
            {
                soundIcon.sprite = isMuted ? soundOffSprite : soundOnSprite;
            }
        }

        public override void Show()
        {
            base.Show();
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

            _audioManager.PlayButton();
        }

        private void HandleNameChanged(string value)
        {
            UpdateStartButtonState();
        }

        private void UpdateStartButtonState()
        {
            bool hasName = !string.IsNullOrWhiteSpace(playerNameInput.text);
            startButton.interactable = hasName;

            if (nameHintText != null)
            {
                nameHintText.gameObject.SetActive(!hasName);
            }
        }

        private void HandleStart()
        {
            string playerName = playerNameInput.text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            _audioManager.PlayButton();
            _onStart?.Invoke(playerName, _selectedTopic);
        }

        private void HandleViewLeaderboard()
        {
            _onViewLeaderboard?.Invoke();
        }

        private void HandleSoundToggle()
        {
            _onToggleSound?.Invoke();
        }
    }
}
