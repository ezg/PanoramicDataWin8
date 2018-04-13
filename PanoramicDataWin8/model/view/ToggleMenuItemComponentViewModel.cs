﻿using System.Collections.Generic;

namespace PanoramicDataWin8.model.view
{
    public class ToggleMenuItemComponentViewModel : MenuItemComponentViewModel
    {
        private bool _isChecked;
        private bool _isVisible = true;
        private bool _canToggleOff = true;

        public bool TriState = false;
        public int _triStateCount = 0;

        private string _label = "";

        private List<ToggleMenuItemComponentViewModel> _otherToggles = new List<ToggleMenuItemComponentViewModel>();

        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
        public int TriStateCount
        {
            get { return _triStateCount; }
            set { SetProperty(ref _triStateCount, value); }
        }
        public bool CanToggleOff
        {
            get { return _canToggleOff; }
            set { SetProperty(ref _canToggleOff, value); }
        }

        public string Label
        {
            get { return _label; }
            set { SetProperty(ref _label, value); }
        }

        public object Data { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public List<ToggleMenuItemComponentViewModel> OtherToggles
        {
            get { return _otherToggles; }
            set { SetProperty(ref _otherToggles, value); }
        }
    }
}