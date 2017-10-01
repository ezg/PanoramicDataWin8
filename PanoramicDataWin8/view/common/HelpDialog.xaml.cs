using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.common
{
    public sealed partial class HelpDialog : ContentDialog
    {
       
        private string _problem = "";
        public string Problem
        {
            set
            {
                _problem = value;
                if (tbProblem != null)
                {
                    tbProblem.Text = _problem;
                }
            }
            get { return _problem; }
        }

        public HelpDialog()
        {
            this.InitializeComponent();
            Loaded += HelpDialog_Loaded;
        }

        private async void HelpDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var installedLoc = Package.Current.InstalledLocation;
            var tutorialContent = await installedLoc.GetFileAsync(@"Assets\data\tutorials.json").AsTask()
                .ContinueWith(t => FileIO.ReadTextAsync(t.Result)).Result;
            var videos = JsonConvert.DeserializeObject<List<VideoVO>>(tutorialContent);

            this.PointerEntered += HelpDialog_PointerEntered;

            videoList.ItemsSource = videos;
            videoList.SelectedIndex = 0;
        }

        private void HelpDialog_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }


        private void VideoList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                LoadMediaFromString((e.AddedItems.First() as VideoVO).Video);
            }
        }

        private async void LoadMediaFromString(string path)
        {
            mediaPlayer.TransportControls.IsFullWindowButtonVisible = false;
            var installedLoc = Package.Current.InstalledLocation;
            var storageFile = await installedLoc.GetFileAsync(path);
            mediaPlayer.Source = MediaSource.CreateFromStorageFile(storageFile);
        }
    }
}
