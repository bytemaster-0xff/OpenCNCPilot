using System.Windows.Data;

namespace OpenCNCPilot.Util
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = App.Current.Settings;
            this.Mode = BindingMode.TwoWay;
        }
    }
}