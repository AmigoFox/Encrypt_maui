using Microsoft.Maui.Storage;
using System.Text;

namespace Encrypt;
public partial class FileSettings : ContentPage
{
    private string? _filePath;
    private string? _fileName;
    private string _receivedEncryptedText; 

    public FileSettings(string encryptedText)
    {
        InitializeComponent(); 
        _receivedEncryptedText = encryptedText;
    }

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
        if (string.IsNullOrEmpty(_receivedEncryptedText))
        {
            await DisplayAlert("Ошибка", "Нет данных для сохранения.", "OK");
            return;
        }

        string result = await EncryptionService.SaveFileButtonEncrypt(_filePath, _receivedEncryptedText);
        await DisplayAlert("Результат", result, "OK");
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
            await DisplayAlert("Результат", result.message, "OK");
            await Navigation.PushAsync(new MainPage(result.content));
        }
        else
        {
            await DisplayAlert("Ошибка", result.message, "OK");
        }
    }
}
