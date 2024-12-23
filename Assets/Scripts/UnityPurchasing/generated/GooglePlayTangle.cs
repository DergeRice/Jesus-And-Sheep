// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("HgHZ1VwtCWX+HyK++gZaE1kdBCaVz83QoVm1lvt7yCD/YzhsVPQvJohJniMJFSepKKDY/j+odGkSwMBZNruvM3Uf6dVn0JPQjdS3AOnmKM5RVnf8szVzz2OoEvDBwN4AIsZuZ27t4+zcbu3m7m7t7exbOtvsvViN/wMpVia3e/EHYlj4/5zV0HWSwQANHt20oWor7mZ1NqVfBw1XH9+5aWKy5B294nTCiPl6kCuoEfjRKgKm+zxgf9sTcsA8nrAGH33g+03TCIYM15nfK1X0aK0vGZdKv15b2yYyL9xu7c7c4erlxmqkahvh7e3t6ezvguO6b2EquvVfQX7EKn1V6Tc2EGkiGg0C/XLrkfIaLOyjuxhajNzGdgj+gLKJwVef++7v7ezt");
        private static int[] order = new int[] { 10,8,6,12,9,8,11,9,11,12,11,11,12,13,14 };
        private static int key = 236;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
