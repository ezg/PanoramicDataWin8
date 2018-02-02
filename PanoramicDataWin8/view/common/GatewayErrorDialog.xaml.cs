using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PanoramicDataWin8.controller.view;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.common
{
    public sealed partial class GatewayErrorDialog : ContentDialog
    {
        private string _content = "";

        public string Content
        {
            set
            {
                _content = value;
                if (tbError != null)
                {
                    tbError.Text = _content;
                }
            }
            get { return _content; }
        }


        private string _stackTrace = "";
        public string StackTrace
        {
            set
            {
                _stackTrace = value;
                if (tbStackTrace != null)
                {
                    tbStackTrace.Text = _stackTrace;
                }
            }
            get { return _stackTrace; }
        }

        private string _ip = "";

        public string Ip
        {
            set
            {
                _ip = value;
                if (tbIp != null)
                {
                    tbIp.Text = _ip;
                }
            }
            get { return _ip; }
        }

        private string _username = "";
        public string Username
        {
            set { _username = value.Trim(); }
            get { return _username; }
        }

        private string _password = "";
        public string Password
        {
            set { _password = value.Trim(); }
            get { return _password; }
        }

        public GatewayErrorDialog()
        {
            this.InitializeComponent();

            if (MainViewController.Instance.MainModel.IsDarpaSubmissionMode)
            {
                scrollViewer.Visibility = Visibility.Collapsed;
            }
            this.Loaded += GatewayErrorDialog_Loaded;
        }

        private void GatewayErrorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp += GatewayErrorDialog_KeyUp;
        }

        private void GatewayErrorDialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.Hide();
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _ip = tbIp.Text.Trim();
            _username = tbUsername.Text;
            _password = tbPassword.Password;
        }
    }
}
