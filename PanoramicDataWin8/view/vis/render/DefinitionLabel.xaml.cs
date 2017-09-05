using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class DefinitionLabel : UserControl
    {
        public DefinitionLabel()
        {
            this.InitializeComponent();
            DataContextChanged += DefinitionLabel_DataContextChanged;
            MainViewController.Instance.MainPage.AddHandler(PointerPressedEvent, new PointerEventHandler(PointerPressedHandler), true);
        }

        void PointerPressedHandler(object sender, PointerRoutedEventArgs args)
        {
            var boolme = VisualTreeHelper.FindElementsInHostCoordinates(args.GetCurrentPoint(null).Position, null).Contains(TextInputBox);
            if (FocusManager.GetFocusedElement() == TextInputBox && TextInputBox.Visibility == Visibility.Visible && !boolme)
                MainViewController.Instance.MainPage.addAttributeButton.Focus(FocusState.Pointer);
        }

        private void DefinitionLabel_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var color = (DataContext as BrushDescriptor).Color;
            Background = new SolidColorBrush(color);
            if (color == Colors.Black)
                color = Colors.Gray;
            TextInputBox.SelectionHighlightColor = new SolidColorBrush(Color.FromArgb(0xff, (byte)(color.R * .7), (byte)(color.G * .7), (byte)(color.B * .7)));
            TextInputBox.Background = new SolidColorBrush(Color.FromArgb(0xff, (byte)(color.R * .9), (byte)(color.G * .9), (byte)(color.B * .9)));
        }

        public void Activate()
        {
            if (TextInputBox.Visibility == Visibility.Collapsed)
            {
                TextInputBox.Visibility = Visibility.Visible;
                TextInputBox.Focus(FocusState.Keyboard);
                TextInputBox.Focus(FocusState.Pointer);
            }
        }
        public DefinitionOperationModel DefinitionOperationModel { get; set; }

        private void TextInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextInputBox.Visibility = Visibility.Collapsed;
        }

        private void TextInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var model = DefinitionOperationModel;
            var senderBrush = DataContext as BrushDescriptor;
            (sender as TextBox).FontStyle = Windows.UI.Text.FontStyle.Normal;
            if (PanoramicDataWin8.model.data.attribute.AttributeTransformationModel.MatchesExistingField((sender as TextBox).Text, true) == null)
                model.SetDescriptorForColor(senderBrush.Color, new BrushDescriptor(senderBrush.Color, (sender as TextBox).Text));
            else (sender as TextBox).FontStyle = Windows.UI.Text.FontStyle.Italic;

            model.UpdateCode();
        }
    }
}
