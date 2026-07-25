using System.Collections.Generic;

namespace KidQuiz.Data
{
    internal sealed class TriviaResponseDto
    {
        public int response_code;
        public List<TriviaQuestionDto> results;
    }

    internal sealed class TriviaQuestionDto
    {
        public string question;
        public string correct_answer;
        public List<string> incorrect_answers;
    }
}
