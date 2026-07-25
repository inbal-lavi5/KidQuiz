using UnityEngine;

namespace KidQuiz.Config
{
    [CreateAssetMenu(menuName = "KidQuiz/Firebase Config", fileName = "FirebaseConfig")]
    public sealed class FirebaseConfig : ScriptableObject
    {
        [SerializeField] private string databaseUrl;

        public string DatabaseUrl => databaseUrl;
    }
}
