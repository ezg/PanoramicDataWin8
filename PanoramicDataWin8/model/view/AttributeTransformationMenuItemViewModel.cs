using System;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.view
{
    public class AttributeTransformationMenuItemViewModel : MenuItemComponentViewModel
    {
        private AttributeTransformationViewModel _attributeTransformationViewModel;

        private bool _canDrag = true;

        private bool _canDrop = true;

        private string _label = "";

        private Windows.UI.Xaml.Visibility _editing = Windows.UI.Xaml.Visibility.Collapsed;

        private double _textAngle;

        private Brush _textBrush = new SolidColorBrush(Colors.Black);
        public Action<AttributeTransformationModel> DroppedTriggered { get; set; }
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

        public AttributeTransformationViewModel AttributeTransformationViewModel
        {
            get { return _attributeTransformationViewModel; }
            set { SetProperty(ref _attributeTransformationViewModel, value); }
        }

        public double TextAngle
        {
            get { return _textAngle; }
            set { SetProperty(ref _textAngle, value); }
        }
    }
}