using System;
using System.Security.Cryptography;


namespace Helpers
{
    public static class OtpGenerator
    {
        public static string GenerateOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);

                int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;

                int otp = value % 1000000;

                return otp.ToString("D6");
            }
        }
    }

}
