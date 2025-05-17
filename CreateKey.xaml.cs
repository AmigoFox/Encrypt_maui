namespace Encrypt;

public partial class CreateKey : ContentPage
{
    public string PublicKey { get; set; }
    public string PrivateKey1 { get; set; }
    public string PrivateKey2 { get; set; }
    public string AESKEY { get; set; }
    public string IVKEY { get; set; }

    public CreateKey()
    {
        InitializeComponent();
    }

    public void UpdateKeys(string publicKey, string privateKey1, string privateKey2)
    {
        PublicKeyRSA.Text = publicKey;
        PrivatKeyRSA1.Text = privateKey1;
        PrivatKeyRSA2.Text = privateKey2;
    }
    private void OnKeySizeSelected(object sender, EventArgs e)
    {
        if (KeySizePicker.SelectedIndex != -1)
        {
            string selectedValue = KeySizePicker.Items[KeySizePicker.SelectedIndex];
            int keySize = GetKeySizeFromString(selectedValue);
        }
    }

    private int GetKeySizeFromString(string keySizeDescription)
    {
        if (keySizeDescription.Contains("512"))
            return 512;
        else if (keySizeDescription.Contains("1024"))
            return 1024;
        else if (keySizeDescription.Contains("2048"))
            return 2048;
        else if (keySizeDescription.Contains("4096"))
            return 4096;
        return 0;
    }

    private void CreateKeyRSA(object sender, EventArgs e)
    {
        if (KeySizePicker.SelectedIndex != -1)
        {
            string selectedValue = KeySizePicker.Items[KeySizePicker.SelectedIndex];
            int keySize = GetKeySizeFromString(selectedValue);

            if (keySize > 0)
            {
                var (publicKey, privateKey1, privateKey2) = EncryptionService.GenerateTwoLargePrimes(keySize);

                UpdateKeys(publicKey.ToString(), privateKey1.ToString(), privateKey2.ToString());
            }
        }
    }

    public void UpdateKeysAES(string AESKEY, string IVKEY)
    {
        AESKey.Text = AESKEY;
        IVKey.Text = IVKEY;
    }

    private void CreateKeyAES(object sender, EventArgs e)
    {
        (AESKEY, IVKEY) = EncryptionService.CreateKeyAESandIV();
        UpdateKeysAES(AESKEY, IVKEY);

    }
} 