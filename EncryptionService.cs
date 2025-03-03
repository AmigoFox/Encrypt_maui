using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace Encrypt
{
    class EncryptionService
    {
        public static string RSA_Encrypt(string PublicKeyNEntry, string PrivateKeyDEntry, string inputText)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding win1251 = Encoding.GetEncoding(1251);

                // Проверка ключей RSA
                if (!BigInteger.TryParse(PublicKeyNEntry, out BigInteger publicKeyN) ||
                    !BigInteger.TryParse(PrivateKeyDEntry, out BigInteger privateKeyE))
                {
                    return "Ошибка, некорекр=тный ключ RSA";
                }

                string inputtext = inputText;

                // Проверка входного текста
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    return "Введите текст для шифрования.";
                }

                List<BigInteger> encryptedValues = new List<BigInteger>();
                byte[] bytes = win1251.GetBytes(inputText);

                foreach (byte b in bytes)
                {
                    BigInteger asciiValue = b;
                    if (asciiValue >= publicKeyN)
                    {
                        return $"Символ '{(char)b}' слишком большой для шифрования.";
                    }

                    BigInteger encryptedValue = BigInteger.ModPow(asciiValue, privateKeyE, publicKeyN);
                    encryptedValues.Add(encryptedValue);
                }

                return string.Join(" ", encryptedValues);
            }
            catch (Exception ex)
            {
                return $"Ошибка RSA шифрования: {ex.Message}";
            }
        }

        public static string RSA_Decrypt(string PublicKeyNEntry, string PrivateKeyDEntry, string inputText)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Проверка ключей RSA
                if (!BigInteger.TryParse(PublicKeyNEntry, out BigInteger publicKeyN) ||
                    !BigInteger.TryParse(PrivateKeyDEntry, out BigInteger privateD))
                {
                    return "Ошибка, некорекр=тный ключ RSA";

                }

                StringBuilder finalMessage = new StringBuilder();
                string words = inputText;


                if (string.IsNullOrWhiteSpace(words))
                {
                    return "Зашифрованный текст пуст.";
                }

                Encoding win1251 = Encoding.GetEncoding(1251);
                string[] numbers = words.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string number in numbers)
                {
                    // Проверка корректности числа
                    if (!BigInteger.TryParse(number, out BigInteger num))
                    {
                        return $"Некорректное число: {number}";
                    }

                    BigInteger decryptedValue = BigInteger.ModPow(num, privateD, publicKeyN);

                    if (decryptedValue >= 0 && decryptedValue <= 255)
                    {
                        byte byteValue = (byte)decryptedValue;
                        finalMessage.Append(win1251.GetString(new byte[] { byteValue }));
                    }
                    else
                    {
                        return $"Некорректное значение ключа";

                    }
                }

                return finalMessage.ToString();
            }
            catch (Exception ex)
            {
                return $"Ошибка RSA расшифрования: {ex.Message}";
            }
        }

        public static string AES_Encrypt(string keyString, string ivString, string inputText)
        {
            try
            {
                string Inputtext = inputText;

                // Проверка входного текста
                if (string.IsNullOrWhiteSpace(Inputtext))
                {
                    return "Введите текст для шифрования.";
                }

                string KeyStringAES = keyString;
                string IVString = ivString;

                // Проверка ключа и IV
                if (string.IsNullOrWhiteSpace(keyString) || string.IsNullOrWhiteSpace(ivString))
                {
                    return "Введите ключ и IV для AES.";
                }

                byte[] key = Convert.FromHexString(KeyStringAES.Replace("-", ""));
                byte[] iv = Convert.FromHexString(IVString.Replace("-", ""));

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(Inputtext);
                            csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                            csEncrypt.FlushFinalBlock();
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка AES шифрования: {ex.Message}";
            }
        }

        public static string AES_Decrypt(string keyString, string ivString, string inputText)
        {
            try
            {
                string Inputtext = inputText;

                // Проверка зашифрованного текста
                if (string.IsNullOrWhiteSpace(Inputtext))
                {
                    return "Введите текст для расшифрования.";
                }

                string keyStringAES = keyString;
                string IVString = ivString;

                // Проверка ключа и IV
                if (string.IsNullOrWhiteSpace(keyString) || string.IsNullOrWhiteSpace(ivString))
                {
                    return "Введите ключ и IV для AES.";
                }

                byte[] key = Convert.FromHexString(keyString.Replace("-", ""));
                byte[] iv = Convert.FromHexString(ivString.Replace("-", ""));

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(Inputtext)))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка AES расшифрования: {ex.Message}";
            }
        }




        public static async Task<(string content, string message)> ReadFileAsync(string filePath)
        {
            try
            {
                string content = await File.ReadAllTextAsync(filePath); // Считываем текст
                return (content, "Данные успешно прочитаны!");
            }
            catch (Exception ex)
            {
                return (string.Empty, $"Ошибка: {ex.Message}");
            }
        }


        public static async Task<string> SaveFileButtonEncrypt(string filePath, string textToSave)
        {
            {
                try
                {

                    File.WriteAllText(filePath, textToSave);
                    return "Данные успешно сохранены";
                }
                catch (Exception ex)
                {
                    return $"Ошибка: {ex.Message}";
                }
            }
        }

        public static Task<string> ClearFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return Task.FromResult("Ошибка: Путь к файлу не указан.");
                }

                if (!File.Exists(filePath))
                {
                    return Task.FromResult("Ошибка: Файл не существует.");
                }

                File.WriteAllText(filePath, string.Empty);
                return Task.FromResult("Содержимое файла очищено!");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Ошибка: {ex.Message}");
            }
        }


        //create key RSA
        public static BigInteger GenerateLargeRandomNumber(int length)
        {
            // Преобразуем длину в количество байт
            int byteLength = (length + 7) / 8;  // Длина в байтах (округляется вверх)

            byte[] bytes = new byte[byteLength];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return BigInteger.Abs(new BigInteger(bytes));
        }


        public static (BigInteger, BigInteger, BigInteger) GenerateTwoLargePrimes(int bits)
        {
            BigInteger prime1 = GenerateLargePrime(bits);  // Генерация первого простого числа
            BigInteger prime2 = GenerateLargePrime(bits);  // Генерация второго простого числа

            // Public key: N = prime1 * prime2
            BigInteger PublicKey = prime1 * prime2;

            // F(N) = (prime1 - 1) * (prime2 - 1)
            BigInteger F_n_ = (prime1 - 1) * (prime2 - 1);

            // Mutually Prime Number (E)
            BigInteger PrivatKey1;

            // Генерация взаимно простого числа с F(N)
            do
            {
                PrivatKey1 = GenerateLargeRandomNumber(1000); // Генерация случайного числа длиной 1000 бит
            } while (NOD(F_n_, PrivatKey1) != 1);

            // Private key (D)
            BigInteger PrivatKey2;
            if (TryModInverse(PrivatKey1, F_n_, out PrivatKey2))
            {
                // Возвращаем пару (PublicKey, PrivatKey)
                return (PublicKey, PrivatKey1, PrivatKey2);
            }

            // Если не удалось найти приватный ключ, возвращаем ошибку или какие-то значения по умолчанию
            throw new Exception("Не удалось найти обратный элемент для приватного ключа.");
        }

        public static BigInteger NOD(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        // Алгоритм нахождения обратного числа по модулю (расчет приватного ключа)
        private static bool TryModInverse(BigInteger a, BigInteger m, out BigInteger result)
        {
            BigInteger m0 = m, t, q;
            BigInteger x0 = 0, x1 = 1;

            if (m == 1)
            {
                result = 0;
                return false;
            }

            while (a > 1)
            {
                q = a / m;
                t = m;

                m = a % m;
                a = t;
                t = x0;

                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            result = x1;
            return true;
        }
        public static BigInteger GenerateLargePrime(int bits)
        {
            Random random = new Random();
            while (true)
            {
                byte[] bytes = new byte[bits / 8];
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] |= 0x01;  // Устанавливаем старший бит в 1 (для нечетности)
                BigInteger candidate = new BigInteger(bytes);
                if (IsProbablePrime(candidate, 10))
                {
                    return candidate;
                }
            }
        }

        static bool IsProbablePrime(BigInteger number, int certainty)
        {
            if (number <= 1) return false;
            if (number <= 3) return true;
            if (number % 2 == 0) return false;

            BigInteger d = number - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            Random random = new Random();

            for (int i = 0; i < certainty; i++)
            {
                BigInteger a;
                do
                {
                    byte[] bytes = new byte[number.GetByteCount()];
                    random.NextBytes(bytes);
                    bytes[bytes.Length - 1] |= 0x01;  // Число нечетное
                    bytes[0] &= 0x7F;  // Старший бит равен 0 для положительных чисел
                    a = new BigInteger(bytes);
                } while (a < 2 || a >= number - 1);

                BigInteger x = BigInteger.ModPow(a, d, number);
                if (x == 1 || x == number - 1) continue;

                bool continueOuter = false;
                for (int j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, number);
                    if (x == number - 1)
                    {
                        continueOuter = true;
                        break;
                    }
                }
                if (continueOuter) continue;

                return false;
            }
            return true;
        }
        //create key RSA


        //create key AES
        public static (string Key, string IV) CreateKeyAESandIV()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256; // max 256
                aes.GenerateKey();
                aes.GenerateIV();
                string keyString = Convert.ToBase64String(aes.Key);
                string ivString = Convert.ToBase64String(aes.IV);
                string keyHex = BitConverter.ToString(aes.Key);
                string ivHex = BitConverter.ToString(aes.IV);
                return (keyHex, ivHex);
            }
        }//create key AES
    }
}// Encrypt