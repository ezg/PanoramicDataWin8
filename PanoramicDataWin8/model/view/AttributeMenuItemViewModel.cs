using System;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data.attribute;
using Windows.UI.Xaml;

namespace PanoramicDataWin8.model.view
{
    public class AttributeMenuItemViewModel : MenuItemComponentViewModel
    {
        private AttributeViewModel _attributeViewModel;

        private bool _canDrag = true;

        private bool _canDrop = true;

        private string _label = "";

        private bool _editNameOnTap;

        private Visibility _editing = Visibility.Collapsed;

        private double _textAngle;

        private Brush _textBrush = new SolidColorBrush(Colors.Black);
        public Action<AttributeViewModel> DroppedTriggered { get; set; }
        public Action TappedTriggered { get; set; }

        public bool CanDrag
        {
            get { return _canDrag; }
            set { SetProperty(ref _canDrag, value); }
        }

        public bool CanDrop
        {
            get { return _canDrop; }
            set { SetProperty(ref _canDrop, value); }
        }

        public Brush TextBrush
        {
            get { return _textBrush; }
            set { SetProperty(ref _textBrush, value); }
        }

        public string Label
        {
            get { return _label; }
            set { SetProperty(ref _label, value); }
        }
        public Windows.UI.Xaml.Visibility Editing
        {
            get { return _editing; }
            set { SetProperty(ref _editing, value); }
        }

        public AttributeViewModel AttributeViewModel
        {
            get { return _attributeViewModel; }
            set { SetProperty(ref _attributeViewModel, value); }
        }

        public double TextAngle
        {
            get { return _textAngle; }
            set { SetProperty(ref _textAngle, value); }
        }
        public bool EditNameOnTap 
        {
            get { return _editNameOnTap; }
            set {
                TappedTriggered -= (() => Editing = Visibility.Visible);
                SetProperty(ref _editNameOnTap, value);
                if (value)
                    TappedTriggered += (() => Editing = Visibility.Visible);
            }
        }
        public AttributeMenuItemViewModel()
        {

        }
    }
}