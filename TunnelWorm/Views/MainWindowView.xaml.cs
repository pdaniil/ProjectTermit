using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Termit.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView
    {
        #region Private Members

        private NotifyIcon _icon;

        #endregion

        public MainWindowView()
        {
            InitializeComponent();

            _icon = new NotifyIcon
            {
                Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Icons/Icon.ico")).Stream)
            };

            _icon.MouseDoubleClick += IconMouseDoubleClick;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();

                _icon.BalloonTipTitle = "Minimized";
                _icon.BalloonTipText = "Minimized";
                _icon.ShowBalloonTip(400);
                _icon.Visible = true;
            }
            if (WindowState == WindowState.Normal)
                _icon.Visible = false;

            base.OnStateChanged(e);
        }
        private void IconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
    }
}
