using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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

        public MainPage()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            CreateProfilesFolder();
            LoadProfiles();
            LoadProfilesNow();
        }

        public MainPage(string content)
        {
            this.content = content;
        }

        private async void OnEncryptClicked(object sender, EventArgs e)
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

            await Navigation.PushAsync(new Encrypt.FileSettings(OutputLabelEncrypt.Text));
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

        private void OnCreateProfileClicked(object sender, EventArgs e)
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
                LoadProfilesNow();

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
            LoadProfilesNow();
        }


        private void CreateProfilesFolder()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void LoadKeysForProfile(string profileName)
        {
            string profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder, profileName);

            try
            {
                PrivateKeyDEntry.Text = File.Exists(Path.Combine(profilePath, "privateKeyRSA.txt"))
                    ? File.ReadAllText(Path.Combine(profilePath, "privateKeyRSA.txt"))
                    : "";

                PublicKeyNEntry.Text = File.Exists(Path.Combine(profilePath, "publicKeyKeyRSA.txt"))
                    ? File.ReadAllText(Path.Combine(profilePath, "publicKeyKeyRSA.txt"))
                    : "";

                AesKeyEntry.Text = File.Exists(Path.Combine(profilePath, "AES.txt"))
                    ? File.ReadAllText(Path.Combine(profilePath, "AES.txt"))
                    : "";

                IvEntry.Text = File.Exists(Path.Combine(profilePath, "IV.txt"))
                    ? File.ReadAllText(Path.Combine(profilePath, "IV.txt"))
                    : "";
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Не удалось загрузить ключи: {ex.Message}", "OK");
            }
        }

private void LoadProfiles()
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
                LoadKeysForProfile(currentProfile);
            }
        }

        private void LoadProfilesNow()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProfilesFolder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var directories = Directory.GetDirectories(path);

            // Очищаем список перед обновлением
            profileNames.Clear();

            foreach (var directory in directories)
            {
                profileNames.Add(Path.GetFileName(directory));
            }

            // Обновляем источник данных для ProfilePicker
            ProfilePicker.ItemsSource = null;
            ProfilePicker.ItemsSource = profileNames;

            // Если остались профили, выбираем первый
            if (profileNames.Count > 0)
            {
                ProfilePicker.SelectedIndex = 0;
                currentProfile = profileNames[0];
                LoadKeysForProfile(currentProfile);
            }
            else
            {
                // Если профилей нет, сбрасываем текущий профиль
                currentProfile = null;
                ProfilePicker.SelectedIndex = -1;
            }
        }


        private async void OnProfileChanged(object sender, EventArgs e)
        {
            if (ProfilePicker.SelectedItem == null) return;
            int selectedIndex = ProfilePicker.SelectedIndex;
            if (selectedIndex != -1)
            {
                string selectedProfile = profileNames[selectedIndex];

                if (!string.IsNullOrEmpty(App.CurrentProfile))
                {
                    await SaveKeysForProfile(App.CurrentProfile);
                }

                App.CurrentProfile = selectedProfile;
                LoadKeysForProfile(App.CurrentProfile);
            }
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
            await SaveKeysForProfile(newText);

            LoadProfiles();
            LoadProfilesNow();
        }
        
        private async void PrivatKey_RSA(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;
            await SaveKeysForProfile(newText);

            LoadProfiles();
            LoadProfilesNow();
        }

        private async void AES(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;
            await SaveKeysForProfile(newText);

            LoadProfiles();
            LoadProfilesNow();
        }

        private async void IV(object sender, TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;
            await SaveKeysForProfile(newText);

            LoadProfiles();
            LoadProfilesNow();
        }
    }
}