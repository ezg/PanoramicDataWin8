﻿using Newtonsoft.Json;

namespace PanoramicDataWin8.model.data.tuppleware
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TuppleWareFieldInputModel : InputFieldModel
    {
        public TuppleWareFieldInputModel(string name, string inputDataType, string inputVisualizationType, bool isBoolean)
        {
            _name = name;
            _inputDataType = inputDataType;
            _inputVisualizationType = inputVisualizationType;
            _isBoolean = isBoolean;
        }

        private string _name = "";
        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.SetProperty(ref _name, value);
            }
        }

        private bool _isBoolean = false;
        public bool IsBoolean
        {
            get
            {
                return _isBoolean;
            }
            set
            {
                this.SetProperty(ref _isBoolean, value);
            }
        }

        private string _inputVisualizationType = "";
        public override string InputVisualizationType
        {
            get
            {
                return _inputVisualizationType;
            }
        }

        private string _inputDataType = "";
        public override string InputDataType
        {
            get
            {
                return _inputDataType;
            }
        }
    }
}
