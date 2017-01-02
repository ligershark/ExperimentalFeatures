using System;
using System.Linq;
using System.Windows;

namespace ExperimentalFeatures.Commands
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Title = Vsix.Name;

                description.Text = "The Experimental Web Features contain early previews of features from the Visual Studio Web Team.";

                var logs = InstallerPackage.Installer.Store.Log.Select(l => l.ToString());
                log.Text = string.Join(Environment.NewLine, logs);

                reset.Content = "Reset...";
                reset.Click += ResetClick;
            };
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            var vsVersion = InstallerPackage.GetVisualStudioVersion();
            InstallerPackage.Installer.ResetAsync(vsVersion).ConfigureAwait(false);

            Close();
        }
    }
}
