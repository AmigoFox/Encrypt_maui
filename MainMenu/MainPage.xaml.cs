using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Encrypt
{
    public partial class MainPage : ContentPage
    {
        private const string ProfilesFolder = "Profiles";
        private List<string> profileNames = new List<string>();
        private string currentProfile = "";
        private string content;

        private bool _isFromAnotherPage;

        public MainPage()
        {
            InitializeComponent();
            CommonInit();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (!string.IsNullOrEmpty(App.CurrentProfile))
            {
                await SaveKeysForProfile(App.CurrentProfile);
            }
        }
        void CommonInit()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CreateProfilesFolder();
            LoadProfiles();
        }


        private void OnEncryptClicked(object sender, EventArgs e)
        {
            string PublicKey = PublicKeyNEntry.Text;
            string PrivatKey = PrivateKeyDEntry.Text;
            string InpytText = InputTextEntry.Text;
            string rsaEncryptedText = EncryptionService.RSA_Encrypt(PublicKey, PrivatKey, InpytText);

            OutputLabelEncrypt.Text = rsaEncryptedText;

            string AesKey = AesKeyEntry.Text;
            string IvKey = IvEntry.Text;
            string aesEncryptText = EncryptionService.AES_Encrypt(AesKey, IvKey, OutputLabelEncrypt.Text);
            OutputLabelEncrypt.Text = aesEncryptText;
        }

        private void OnDecryptClicked(object sender, EventArgs e)
        {
            string AesKey = AesKeyEntry.Text;
            string IvKey = IvEntry.Text;
            string aesEncryptText = EncryptionService.AES_Decrypt(AesKey, IvKey, OutputTextEntry.Text);
            OutputText_pr.Text = aesEncryptText;

            string PublicKey = PublicKeyNEntry.Text;
            string PrivatKey = PrivateKeyDEntry.Text;
            string InpytText = OutputText_pr.Text;
            string rsaEncryptedText = EncryptionService.RSA_Decrypt(PublicKey, PrivatKey, InpytText);

            OutputText.Text = rsaEncryptedText;
        }

        private  void OnCreateProfileClicked(object sender, EventArgs e)
        {
            try
            {
                string profileName = ProfileNameEntry.Text?.Trim();

                if (string.IsNullOrWhiteSpace(profileName))
                {
                    DisplayAlert("Ошибка", "Введите имя профиля.", "OK");
                    return;
                }

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder, profileName);

                if (Directory.Exists(path))
                {
                    DisplayAlert("Ошибка", "Профиль с таким именем уже существует.", "OK");
                    return;
                }

                Directory.CreateDirectory(path);

                string[] files = { "privateKeyRSA.txt", "publicKeyKeyRSA.txt", "AES.txt", "IV.txt" };
                foreach (string file in files)
                {
                    string filePath = Path.Combine(path, file);
                    using (File.Create(filePath)) { }
                }

                profileNames.Add(profileName);
                ProfileListView.ItemsSource = null;
                ProfileListView.ItemsSource = profileNames;
                ProfileNameEntry.Text = string.Empty;

                LoadProfiles();
                LoadKeysForProfileAsync(profileName);

            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Не удалось создать профиль: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteProfileClicked(object sender, EventArgs e)
        {
            var selectedProfiles = ProfileListView.SelectedItems;

            if (selectedProfiles == null || selectedProfiles.Count == 0)
            {
                await DisplayAlert("Ошибка", "Выберите хотя бы один профиль для удаления", "OK");
                return;
            }

            bool allDeleted = true;

            foreach (var selectedProfile in selectedProfiles.Cast<string>().ToList())
            {
                string profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder, selectedProfile);

                if (Directory.Exists(profilePath))
                {
                    try
                    {
                        Directory.Delete(profilePath, true);

                        if (!Directory.Exists(profilePath))
                        {
                            profileNames.Remove(selectedProfile);
                        }
                        else
                        {
                            allDeleted = false;
                            await DisplayAlert("Ошибка", $"Не удалось удалить профиль: {selectedProfile}", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        allDeleted = false;
                        await DisplayAlert("Ошибка", $"Не удалось удалить профиль {selectedProfile}: {ex.Message}", "OK");
                    }
                }
                else
                {
                    allDeleted = false;
                    await DisplayAlert("Ошибка", $"Папка профиля {selectedProfile} не найдена", "OK");
                }
            }

            if (allDeleted)
            {
                await DisplayAlert("Успех", "Все выбранные профили удалены", "OK");
            }

            LoadProfiles();
        }


        private void CreateProfilesFolder()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private async Task LoadKeysForProfileAsync(string profileName)
        {
            string profilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ProfilesFolder,
                profileName);

            // Убедимся, что папка существует (CreateDirectory ничего не делает, если есть) :contentReference[oaicite:3]{index=3}
            await Task.Run(() => Directory.CreateDirectory(profilePath));

            try
            {
                // Для каждого файла: если существует — читаем асинхронно и ждём результата :contentReference[oaicite:4]{index=4}
                string privPath = Path.Combine(profilePath, "privateKeyRSA.txt");
                PrivateKeyDEntry.Text = File.Exists(privPath)
                    ? await File.ReadAllTextAsync(privPath)
                    : string.Empty;

                string pubPath = Path.Combine(profilePath, "publicKeyKeyRSA.txt");
                PublicKeyNEntry.Text = File.Exists(pubPath)
                    ? await File.ReadAllTextAsync(pubPath)
                    : string.Empty;

                string aesPath = Path.Combine(profilePath, "AES.txt");
                AesKeyEntry.Text = File.Exists(aesPath)
                    ? await File.ReadAllTextAsync(aesPath)
                    : string.Empty;

                string ivPath = Path.Combine(profilePath, "IV.txt");
                IvEntry.Text = File.Exists(ivPath)
                    ? await File.ReadAllTextAsync(ivPath)
                    : string.Empty;
            }
            catch (Exception ex)
            {
                // DisplayAlert — UI-операция, но async void для обработчиков жизненного цикла допустим :contentReference[oaicite:5]{index=5}
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить ключи: {ex.Message}", "OK");
            }
        }

        private async void LoadProfiles()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            profileNames = Directory.GetDirectories(path).Select(Path.GetFileName).ToList();
            ProfilePicker.ItemsSource = profileNames;
            ProfileListView.ItemsSource = profileNames;

            if (profileNames.Count > 0)
            {
                ProfilePicker.SelectedIndex = 0;
                currentProfile = profileNames[0];
                await LoadKeysForProfileAsync(currentProfile);
            }
        }



        private async void OnProfileChanged(object sender, EventArgs e)
        {
            if (ProfilePicker.SelectedIndex < 0) return;
            string sel = profileNames[ProfilePicker.SelectedIndex];
            if (!string.IsNullOrEmpty(App.CurrentProfile))
                await SaveKeysForProfile(App.CurrentProfile);
            App.CurrentProfile = sel;
            await LoadKeysForProfileAsync(sel);
        }

        private async Task SaveKeysForProfile(string profileName)// сделать проверрку на null, а то ключи могут быть пустыми при смене
        {
            string profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Profiles", profileName);

            try
            {
                Directory.CreateDirectory(profilePath);
                await File.WriteAllTextAsync(Path.Combine(profilePath, "privateKeyRSA.txt"), PrivateKeyDEntry.Text);
                await File.WriteAllTextAsync(Path.Combine(profilePath, "publicKeyKeyRSA.txt"), PublicKeyNEntry.Text);
                await File.WriteAllTextAsync(Path.Combine(profilePath, "AES.txt"), AesKeyEntry.Text);
                await File.WriteAllTextAsync(Path.Combine(profilePath, "IV.txt"), IvEntry.Text);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить ключи: {ex.Message}", "OK");
            }
        }


        public async Task SaveTextToFile(string fileName, string text, bool isBigInteger = false)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            try
            {
                Encoding cp1251 = Encoding.GetEncoding(1251);
                if (isBigInteger && BigInteger.TryParse(text, out BigInteger bigIntValue))
                {
                    await File.WriteAllTextAsync(filePath, bigIntValue.ToString(), cp1251);
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, text, cp1251);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка при сохранении файла {fileName}: {ex.Message}", "OK");
            }
        }

        private async void PublicKey_RSA(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;

            // Если ключ был изменен и не пустой, сохраняем его
            if (!string.IsNullOrEmpty(currentProfile) && newText != PublicKeyNEntry.Text)
            {
                if (!string.IsNullOrEmpty(newText)) // Проверяем, чтобы ключ не был пустым
                {
                    await SaveKeysForProfile(currentProfile);
                }
            }
        }

        private async void PrivatKey_RSA(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;

            // Если ключ был изменен и не пустой, сохраняем его
            if (!string.IsNullOrEmpty(currentProfile) && newText != PrivateKeyDEntry.Text)
            {
                if (!string.IsNullOrEmpty(newText)) // Проверяем, чтобы ключ не был пустым
                {
                    await SaveKeysForProfile(currentProfile);
                }
            }
        }

        private async void AES(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;

            // Если ключ был изменен и не пустой, сохраняем его
            if (!string.IsNullOrEmpty(currentProfile) && newText != AesKeyEntry.Text)
            {
                if (!string.IsNullOrEmpty(newText)) // Проверяем, чтобы ключ не был пустым
                {
                    await SaveKeysForProfile(currentProfile);
                }
            }
        }
        private async void IV(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;

            // Если ключ был изменен и не пустой, сохраняем его
            if (!string.IsNullOrEmpty(currentProfile) && newText != IvEntry.Text)
            {
                if (!string.IsNullOrEmpty(newText)) // Проверяем, чтобы ключ не был пустым
                {
                    await SaveKeysForProfile(currentProfile);
                }
            }
        }


        // Работа с файлами

        private string? _filePath;
        private string? _fileName;


        private async void SelectFileButtonEncrypt(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    _fileName = result.FileName;
                    _filePath = result.FullPath;
                    await DisplayAlert("Выбранный файл", $"Имя файла: {_fileName}\nПуть: {_filePath}", "OK");
                    fileName.Text = _fileName;
                    filePath.Text = _filePath;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void ClearFileButtonEncrypt(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                await DisplayAlert("Ошибка", "Сначала выберите файл.", "OK");
                return;
            }

            string result = await EncryptionService.ClearFileAsync(_filePath);
            await DisplayAlert("Результат", result, "OK");
        }

        private async void SaveFileButtonEncrypt(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                await DisplayAlert("Ошибка", "Сначала выберите файл.", "OK");
                return;
            }
            
            if(string.IsNullOrEmpty(OutputLabelEncrypt.Text))
            {

                string textToSave = OutputLabelEncrypt.Text ?? string.Empty;
                string result = await EncryptionService.SaveFileButtonEncrypt(_filePath, textToSave);
                await DisplayAlert("Ошибка", "Текс в поле зашифрованное сообщение пуст", "OK");
                return;
            }
            else{
                string textToSave = OutputLabelEncrypt.Text ?? string.Empty;
                string result = await EncryptionService.SaveFileButtonEncrypt(_filePath, textToSave);
                await DisplayAlert("Ок", "Файл записан", "OK");
            }


        }

        private async void ReadFileButtonEncrypt(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                await DisplayAlert("Ошибка", "Сначала выберите файл.", "OK");
                return;
            }

            (string content, string message) result = await EncryptionService.ReadFileAsync(_filePath);

            if (!string.IsNullOrEmpty(result.content))
            {
                OutputTextEntry.Text = result.content;
            }
            else
            {
                await DisplayAlert("Ошибка", result.message, "OK");
            }
        }
    }
}