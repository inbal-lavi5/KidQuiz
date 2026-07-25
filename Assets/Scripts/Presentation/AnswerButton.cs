using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidQuiz.Presentation
{
    public enum AnswerButtonState
    {
        Neutral,
        Correct,
        Incorrect
    }

    [RequireComponent(typeof(Button))]
    public sealed class AnswerButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;
        [SerializeField] private Color neutralColor = Color.white;
        [SerializeField] private Color correctColor = new(0.35f, 0.75f, 0.35f);
        [SerializeField] private Color incorrectColor = new(0.85f, 0.35f, 0.35f);

        private Button _button;
        private Action<string> _onClick;

        public string AnswerText { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void Bind(string text, Action<string> onClick)
        {
            AnswerText = text;
            _onClick = onClick;
            label.text = text;
            SetState(AnswerButtonState.Neutral);
        }

        public void SetState(AnswerButtonState state)
        {
            background.color = state switch
            {
                AnswerButtonState.Correct => correctColor,
                AnswerButtonState.Incorrect => incorrectColor,
                _ => neutralColor
            };
        }

        private void HandleClick()
        {
            _onClick?.Invoke(AnswerText);
        }
    }
}
