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
    public sealed partial class ParticipantDialog : ContentDialog
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

        private string _participant = "";

        public string Participant
        {
            set
            {
                _participant = value;
                if (tbIp != null)
                {
                    tbIp.Text = _participant;
                }
            }
            get { return _participant; }
        }

        public ParticipantDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _participant = tbIp.Text.Trim();
        }
    }
}
