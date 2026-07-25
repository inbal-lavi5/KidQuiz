namespace KidQuiz.Domain
{
    public sealed class ScoreEntry
    {
        public string PlayerName { get; }
        public int Score { get; }
        public long TimestampUnixSeconds { get; }
        public string Category { get; }

        public ScoreEntry(string playerName, int score, long timestampUnixSeconds, string category)
        {
            PlayerName = playerName;
            Score = score;
            TimestampUnixSeconds = timestampUnixSeconds;
            Category = category;
        }
    }
}
