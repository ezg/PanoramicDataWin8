using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        public GatewayErrorDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _ip = tbIp.Text.Trim();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
