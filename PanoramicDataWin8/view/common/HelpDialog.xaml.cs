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
    public sealed partial class HelpDialog : UserControl
    {
        public event EventHandler<EventArgs> CloseEvent;

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
            playerGrid.SizeChanged += PlayerGrid_SizeChanged;
        }
        
        private async void HelpDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var installedLoc = Package.Current.InstalledLocation;
            var tutorialContent = await installedLoc.GetFileAsync(@"Assets\data\tutorials.json").AsTask()
                .ContinueWith(t => FileIO.ReadTextAsync(t.Result)).Result;
            var videos = JsonConvert.DeserializeObject<List<VideoVO>>(tutorialContent);
            
            videoList.ItemsSource = videos;
            videoList.SelectedIndex = 0;
        }
        
        private void VideoList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                LoadMediaFromString((e.AddedItems.First() as VideoVO).Video);
                mediaPlayer.TransportControls.IsCompact = true;
                mediaPlayer.Stretch = Stretch.Uniform;
                mediaPlayer.HorizontalAlignment = HorizontalAlignment.Right;
                mediaPlayer.VerticalAlignment = VerticalAlignment.Top;
                //var d = 400;
                
                
               // ScrollViewer.Height = 190;
            }
        }

        private void PlayerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var h = playerGrid.ActualHeight;
            mediaPlayer.Height = h ;
        }

        private async void LoadMediaFromString(string path)
        {
            mediaPlayer.TransportControls.IsFullWindowButtonVisible = false;
            var installedLoc = Package.Current.InstalledLocation;
            var storageFile = await installedLoc.GetFileAsync(path);
            var stream = await storageFile.OpenReadAsync();
            mediaPlayer.SetSource(stream, stream.ContentType);
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            CloseEvent?.Invoke(this, null);
        }
    }
}
