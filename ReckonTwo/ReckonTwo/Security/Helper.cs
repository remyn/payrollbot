using System;
using System.Security.Cryptography;
using System.Text;

namespace ReckonTwo.Security
{
    public class Helper
    {
        public static string HashPassword(string Password)
        {
            Password = Password.PadRight(5, '{');
            Password = Password.ToLower().Substring(0, 2) + Password + Password.ToUpper().Substring(1, 3) + Password + "_&))(#@$";
            Password = BitConverter.ToString(new SHA512CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(Password))).Replace("-", String.Empty).ToUpper();
            Password = String.Concat("$", Password.Substring(0, 126), "$");

            return Password;
        }

        public static string GenerateTempPassword(int lowercase, int uppercase, int numerics)
        {
            string lowers = "abcdefghijklmnopqrstuvwxyz";
            string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string number = "0123456789";

            Random random = new Random();

            string generated = "!";
            for (int i = 1; i <= lowercase; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    lowers[random.Next(lowers.Length - 1)].ToString()
                );

            for (int i = 1; i <= uppercase; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    uppers[random.Next(uppers.Length - 1)].ToString()
                );

            for (int i = 1; i <= numerics; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    number[random.Next(number.Length - 1)].ToString()
                );

            return generated.Replace("!", string.Empty);

        }

        public static bool ConfirmLoginDetails(string EncPass, string Email)
        {
            //hard coded for now, in real life you'll validate to a DB

            return (EncPass == "$FC9FFF4C8D9EF05A78F613654E49C061F1A554A10A323169D400317ED47F8FE461E5460BCB9AAEAC30474971AEB7511087FDDAD245FB38F63C8E7C51DD2CDB$"
                 && Email == "payroll@reckon.com");
        }
    }
}