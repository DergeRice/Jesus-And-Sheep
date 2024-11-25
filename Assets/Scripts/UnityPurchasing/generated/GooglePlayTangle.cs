// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("eODYaO+hCqohg/2kIxuQmJoifh6LM391N6XM/F69Nc1F9tPtcj6JYXi7Gm3wsD2MpFCLRZ3sJp/cpEKcJ0v9oMk3q1w4a3CY0odq+d+2GgXnjTPztIiRQTmQGIbu+odEmjNldj16UvUmh3Gx+AqK1ybbiYCnyHKHe8lKaXtGTUJhzQPNvEZKSkpOS0jJSkRLe8lKQUnJSkpL7Pix5+062YgUYDTuOin9MzBxFcbqtuAk5IMsVQTH3h3EMSFHMi/eysJa+9Me6+FdnYPjBNYTaEAz9pAJsGy5WrYxaexpJrvaCf+phhQyZ5I4Arn01dXqhpE2J15FbKxyvNAexWzHhO3jErLyKOXVoOqgGtx1+p4Ism5kiUPaQpD49Bn0DuMl5klISktK");
        private static int[] order = new int[] { 12,3,11,5,13,11,12,11,10,12,10,11,13,13,14 };
        private static int key = 75;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
