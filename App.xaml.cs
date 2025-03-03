
namespace Encrypt
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }

        public static string? CurrentProfile { get; internal set; }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        internal Task SaveCurrentProfileKeysAsync()
        {
            throw new NotImplementedException();
        }
    }
}