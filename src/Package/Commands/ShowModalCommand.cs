using EnvDTE;
using ExperimentalFeatures.Commands;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows.Interop;

namespace ExperimentalFeatures
{
    internal sealed class ShowModalCommand
    {
        private readonly Package _package;

        private ShowModalCommand(Package package, OleMenuCommandService commandService)
        {
            _package = package;

            var menuCommandID = new CommandID(PackageGuids.guidShowModalCommandPackageCmdSet, PackageIds.ShowModalCommandId);
            var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ShowModalCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package, OleMenuCommandService commandService)
        {
            Instance = new ShowModalCommand(package, commandService);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dialog = new LogWindow();
            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));

            var hwnd = new IntPtr(dte.MainWindow.HWnd);
            var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;

            var result = dialog.ShowDialog();
        }
    }
}
