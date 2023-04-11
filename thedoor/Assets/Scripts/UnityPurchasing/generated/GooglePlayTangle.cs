// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ny22pX9wXqo5VMafPd42lyI7KMEh6b9sGWjO1KzWTZMPaW5PN9j8DROhIgETLiUqCaVrpdQuIiIiJiMgDqX1HYYV4Zps4p+I2nv6Aos9OVTe5v8XLAzwbJMjc946oME5VOKonEH0Ac6kSnVoVLEYlb/F/eCniRk6IvJEELLUULg2KryT5NTfzL1JoE6hIiwjE6EiKSGhIiIjlRP4tvFAVHO/51+73DjX9tPSfnisX7WUag4FGJK8I9smPqoDP4qSq9Epovequ3+YxXYYh6FzZqCLOPaht12a4W6+fEyD7+0FeGdsOJY5nFSPpnogklXbv1OTV3pLrA2uoRnubEODXjBVHMEm8hWbsVv7gCoUwTowEke0PBfrGnveQ7dAh3glUiEgIiMi");
        private static int[] order = new int[] { 11,7,11,10,11,12,6,7,10,12,13,11,12,13,14 };
        private static int key = 35;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
