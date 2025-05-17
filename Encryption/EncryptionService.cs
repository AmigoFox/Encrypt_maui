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
        public static string RSA_Encrypt(string PublicKeyNEntry, string PrivateKeyDEntry, string inputText)//принимает аргументы
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding win1251 = Encoding.GetEncoding(1251); //Реистрируем кодировку 1251, чтобы можно было перевести из текста в неёё

                // Проверка ключей RSA
                if (!BigInteger.TryParse(PublicKeyNEntry, out BigInteger publicKeyN) ||
                    !BigInteger.TryParse(PrivateKeyDEntry, out BigInteger privateKeyE))
                {
                    return "Ошибка, некорекретный ключ RSA";
                }

                string inputtext = inputText;

                // Проверка входного текста
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    return "Введите текст для шифрования.";
                }

                List<BigInteger> encryptedValues = new List<BigInteger>();// создание нового списка BigInteger(числа с произвольной длинной)
                byte[] bytes = win1251.GetBytes(inputText);

                foreach (byte b in bytes)
                {
                    BigInteger asciiValue = b;
                    if (asciiValue >= publicKeyN)// проверка что символ не выходит за пределы кодировки
                    {
                        return $"Символ '{(char)b}' слишком большой для шифрования.";
                    }

                    BigInteger encryptedValue = BigInteger.ModPow(asciiValue, privateKeyE, publicKeyN);// возведение переведенное числа из символа в число в степень приватного ключа и сразу деление его по модулю на публичный ключ 
                    encryptedValues.Add(encryptedValue);// Добавление расчитанного набора цифр в список
                }

                return string.Join(" ", encryptedValues);
            }
            catch (Exception ex)
            {
                return $"Ошибка RSA шифрования: {ex.Message}";// обработка ошибок
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

                StringBuilder finalMessage = new StringBuilder();// создаем изменяемую строку, куда будем сохранять расшифровку текста
                string words = inputText;


                if (string.IsNullOrWhiteSpace(words))
                {
                    return "Зашифрованный текст пуст.";
                }

                Encoding win1251 = Encoding.GetEncoding(1251);// Регистрируем кодировку 1251
                string[] numbers = words.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);// удаляем пробелы в начале и в конце, разделяем строчку по пробелам

                foreach (string number in numbers)
                {
                    // Проверка корректности числа
                    if (!BigInteger.TryParse(number, out BigInteger num))
                    {
                        return $"Некорректное число: {number}";
                    }

                    BigInteger decryptedValue = BigInteger.ModPow(num, privateD, publicKeyN);// возведение переведенное числа из символа в число в степень приватного ключа и сразу деление его по модулю на публичный ключ 

                    if (decryptedValue >= 0 && decryptedValue <= 255)// Проверка на то, чтобы число не выходило за пределы чисел кодировка 1251
                    {
                        byte byteValue = (byte)decryptedValue;// Переводим из числвого значение обратно в символы, тобиш текст
                        finalMessage.Append(win1251.GetString(new byte[] { byteValue }));// добавляем в изменяемую строку результат
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

                // Проверка входного текста на пустоту
                if (string.IsNullOrWhiteSpace(Inputtext))
                {
                    return "Введите текст для шифрования."; // Если текст пустой, возвращаем сообщение об ошибке
                }

                string KeyStringAES = keyString;
                string IVString = ivString;

                // Проверка ключа и вектора инициализации (IV) на пустоту
                if (string.IsNullOrWhiteSpace(keyString) || string.IsNullOrWhiteSpace(ivString))
                {
                    return "Введите ключ и IV для AES."; // Если ключ или IV пустые, возвращаем сообщение об ошибке
                }

                // Преобразование строки ключа из шестнадцатеричного формата в массив байтов
                byte[] key = Convert.FromHexString(KeyStringAES.Replace("-", "")); // Убираем дефисы и преобразуем в байты
                byte[] iv = Convert.FromHexString(IVString.Replace("-", "")); // То же самое для IV

                using (Aes aes = Aes.Create()) // Создаем экземпляр AES
                {
                    aes.Key = key; // Устанавливаем ключ
                    aes.IV = iv;   // Устанавливаем вектор инициализации (IV)

                    using (MemoryStream msEncrypt = new MemoryStream()) // Создаем поток для записи зашифрованных данных
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(Inputtext); // Преобразуем входной текст в массив байтов
                            csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length); // Записываем данные в поток для шифрования
                            csEncrypt.FlushFinalBlock(); // Завершаем процесс шифрования
                            return Convert.ToBase64String(msEncrypt.ToArray()); // Возвращаем зашифрованный текст в формате Base64
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка AES шифрования: {ex.Message}"; // Обработка ошибок
            }
        }

        public static string AES_Decrypt(string keyString, string ivString, string inputText)
        {
            try
            {
                string Inputtext = inputText;

                // Проверка зашифрованного текста на пустоту
                if (string.IsNullOrWhiteSpace(Inputtext))
                {
                    return "Введите текст для расшифрования."; // Если текст пустой, возвращаем сообщение об ошибке
                }

                string keyStringAES = keyString;
                string IVString = ivString;

                // Проверка ключа и IV на пустоту
                if (string.IsNullOrWhiteSpace(keyString) || string.IsNullOrWhiteSpace(ivString))
                {
                    return "Введите ключ и IV для AES."; // Если ключ или IV пустые, возвращаем сообщение об ошибке
                }

                // Преобразование строки ключа из шестнадцатеричного формата в массив байтов
                byte[] key = Convert.FromHexString(keyString.Replace("-", "")); // Убираем дефисы и преобразуем в байты
                byte[] iv = Convert.FromHexString(ivString.Replace("-", "")); // То же самое для IV

                using (Aes aes = Aes.Create()) // Создаем экземпляр AES
                {
                    aes.Key = key; // Устанавливаем ключ
                    aes.IV = iv;   // Устанавливаем вектор инициализации (IV)

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(Inputtext))) // Преобразуем Base64-строку в байты
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read)) // Создаем поток для расшифровки
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt)) // Читаем расшифрованные данные
                    {
                        return srDecrypt.ReadToEnd(); // Возвращаем расшифрованный текст
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка AES расшифрования: {ex.Message}"; // Обработка ошибок
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
                rng.GetBytes(bytes); // Генерация случайных байтов
            }
            return BigInteger.Abs(new BigInteger(bytes)); // Возвращаем положительное число
        }

        public static (BigInteger, BigInteger, BigInteger) GenerateTwoLargePrimes(int bits)
        {
            BigInteger prime1 = GenerateLargePrime(bits);  // Генерация первого простого числа
            BigInteger prime2 = GenerateLargePrime(bits);  // Генерация второго простого числа

            // Public key: N = prime1 * prime2
            BigInteger PublicKey = prime1 * prime2; // Вычисление публичного ключа

            // F(N) = (prime1 - 1) * (prime2 - 1)
            BigInteger F_n_ = (prime1 - 1) * (prime2 - 1); // Функция Эйлера

            // Mutually Prime Number (E)
            BigInteger PrivatKey1;

            // Генерация взаимно простого числа с F(N)
            do
            {
                PrivatKey1 = GenerateLargeRandomNumber(1000); // Генерация случайного числа длиной 1000 бит
            } while (NOD(F_n_, PrivatKey1) != 1); // Проверка, что число взаимно просто с F(N)

            // Private key (D)
            BigInteger PrivatKey2;
            if (TryModInverse(PrivatKey1, F_n_, out PrivatKey2))
            {
                // Возвращаем публичный и приватные ключи
                return (PublicKey, PrivatKey1, PrivatKey2);
            }

            throw new Exception("Не удалось найти обратный элемент для приватного ключа.");
        }

        public static BigInteger NOD(BigInteger a, BigInteger b)
        {
            while (b != 0) // Алгоритм Евклида для нахождения НОД
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return a; // Возвращаем наибольший общий делитель
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

            while (a > 1) // Расширенный алгоритм Евклида
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

            result = x1; // Возвращаем обратный элемент
            return true;
        }

        public static BigInteger GenerateLargePrime(int bits)
        {
            Random random = new Random();
            while (true)
            {
                byte[] bytes = new byte[bits / 8];
                random.NextBytes(bytes); // Генерация случайных байтов
                bytes[bytes.Length - 1] |= 0x01;  // Устанавливаем старший бит в 1 (для нечетности)
                BigInteger candidate = new BigInteger(bytes);
                if (IsProbablePrime(candidate, 10)) // Проверка на вероятность простоты
                {
                    return candidate; // Возвращаем простое число
                }
            }
        }

        static bool IsProbablePrime(BigInteger number, int certainty)
        {
            if (number <= 1) return false; // Число должно быть больше 1
            if (number <= 3) return true;  // 2 и 3 — простые числа
            if (number % 2 == 0) return false; // Четные числа не являются простыми

            BigInteger d = number - 1;
            int r = 0;
            while (d % 2 == 0) // Находим d и r для теста Миллера-Рабина
            {
                d /= 2;
                r++;
            }

            Random random = new Random();

            for (int i = 0; i < certainty; i++) // Проводим тест Миллера-Рабина
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

                BigInteger x = BigInteger.ModPow(a, d, number); // Вычисляем a^d mod number
                if (x == 1 || x == number - 1) continue;

                bool continueOuter = false;
                for (int j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, number); // Повторяем возведение в квадрат
                    if (x == number - 1)
                    {
                        continueOuter = true;
                        break;
                    }
                }
                if (continueOuter) continue;

                return false; // Если тест не пройден, число не является простым
            }
            return true; // Если все тесты пройдены, число вероятно простое
        }
        //create key RSA


        //create key AES 
        public static (string Key, string IV) CreateKeyAESandIV()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
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