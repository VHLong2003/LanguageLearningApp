namespace LanguageLearningApp.Helpers
{
    public class FirebaseConfig
    {
        public string ApiKey { get; set; }
        public string AuthDomain { get; set; }
        public string ProjectId { get; set; }
        public string StorageBucket { get; set; }
        public string DatabaseUrl { get; set; }

        public FirebaseConfig()
        {
            ApiKey = "AIzaSyDcRMJ3I57o6EP1dzGCHorqmubiY6LAZVM";
            AuthDomain = "languagelearningapp-f88e7.firebaseapp.com";
            ProjectId = "languagelearningapp-f88e7";
            StorageBucket = "languagelearningapp-f88e7.firebasestorage.app";
            DatabaseUrl = "https://languagelearningapp-f88e7-default-rtdb.firebaseio.com/";
        }
    }
}
