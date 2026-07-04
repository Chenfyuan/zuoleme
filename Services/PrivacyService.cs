using System.Security.Cryptography;
using System.Text;

namespace zuoleme.Services
{
    /// <summary>
    /// 密码保护服务：PIN 以哈希形式保存在系统安全存储中，不落地明文
    /// </summary>
    public class PrivacyService
    {
        private const string PinHashKey = "privacy_pin_hash";
        private const string EnabledKey = "privacy_pin_enabled";

        public async Task<bool> IsEnabledAsync()
        {
            try
            {
                var enabled = await SecureStorage.Default.GetAsync(EnabledKey);
                return enabled == "true";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取密码保护状态失败: {ex.Message}");
                return false;
            }
        }

        public async Task SetPinAsync(string pin)
        {
            await SecureStorage.Default.SetAsync(PinHashKey, HashPin(pin));
            await SecureStorage.Default.SetAsync(EnabledKey, "true");
        }

        public async Task DisableAsync()
        {
            SecureStorage.Default.Remove(PinHashKey);
            await SecureStorage.Default.SetAsync(EnabledKey, "false");
        }

        public async Task<bool> VerifyPinAsync(string pin)
        {
            try
            {
                var storedHash = await SecureStorage.Default.GetAsync(PinHashKey);
                if (string.IsNullOrEmpty(storedHash)) return false;
                return storedHash == HashPin(pin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"校验密码失败: {ex.Message}");
                return false;
            }
        }

        private static string HashPin(string pin)
        {
            var bytes = Encoding.UTF8.GetBytes(pin);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
