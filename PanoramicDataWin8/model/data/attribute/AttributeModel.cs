using System;
using System.Collections.Generic;
using IDEA_common.catalog;
using PanoramicDataWin8.utils;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data.idea;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PanoramicDataWin8.model.data.attribute
{
    public abstract class AttributeModel : ExtendedBindableBase
    {
        private List<VisualizationHint> _visualizationHints = new List<VisualizationHint>();
        private string  _displayName = "";
        private string  _rawName = "";
        private bool    _isTarget = false;
        private bool    _isDisplayed = true;
        private OriginModel        _originModel = null;
        private AttributeFuncModel _funcModel = null;

        public AttributeModel() { }
        public AttributeModel(string rawName, string displayName, AttributeFuncModel funcModel, DataType dataType, string inputVisualizationType,
            List<VisualizationHint> visualizationHints, OriginModel originModel, bool isTarget)
        {
            _rawName = rawName;
            _displayName = displayName;
            _funcModel = funcModel;
            _visualizationHints = visualizationHints;
            _originModel = originModel;
            InputVisualizationType = inputVisualizationType;
            _isTarget = isTarget;
            DataType = dataType;
        }

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }
        public bool IsTarget
        {
            get { return _isTarget; }
            set { SetProperty(ref _isTarget, value); }
        }
        public OriginModel OriginModel
        {
            get { return _originModel; }
            set { SetProperty(ref _originModel, value); }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }
        public string RawName
        {
            get { return _rawName; }
            set { SetProperty(ref _rawName, value); }
        }
        public DataType DataType { get; set; } = DataType.Object;

        public AttributeFuncModel FuncModel
        {
            get { return _funcModel; }
            set { SetProperty(ref _funcModel, value); }
        }
        public List<VisualizationHint> VisualizationHints
        {
            get { return _visualizationHints; }
            set { SetProperty(ref _visualizationHints, value); }
        }

        public string InputVisualizationType { get; set; } = "";

        public override bool Equals(object obj)
        {
            if (obj is AttributeModel)
            {
                var am = obj as AttributeModel;
                return
                    am.RawName.Equals(RawName) &&
                    am.InputVisualizationType.Equals(InputVisualizationType) &&
                    am.DataType.Equals(DataType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= RawName.GetHashCode();
            code ^= InputVisualizationType.GetHashCode();
            code ^= DataType.GetHashCode();
            return code;
        }

        public class AttributeFuncModel : BindableBase
        {
            public enum AttributeModelType {
                Group,
                Column,
                Code,
                Assigned,
                Backend
            };

            public virtual AttributeModelType ModelType { get; set;  }
            public class AttributeAssignedValueFuncModel : AttributeCodeFuncModel
            {
                public class Key
                {
                    public List<object> values;
                    public Key(List<object> vals) { values = vals; }
                    public override int GetHashCode()
                    {
                        var code = 0;
                        foreach (var v in values)
                            code ^= v.GetHashCode();
                        return code;
                    }
                    public override bool Equals(object obj)
                    {
                        var otherKey = obj as Key;
                        if (otherKey == null || otherKey.values.Count != values.Count)
                            return false;
                        for (int i = 0; i < values.Count; i++)
                            if (!otherKey.values[i].Equals(values[i]))
                                return false;
                        return true;
                    }
                }

                public class AssignmentDictionary
                {
                    [NonSerialized]
                    List<Tuple<Key, object>> _dict = new List<Tuple<Key, object>>();
                    // Dictionary<Key, object> _dict = new Dictionary<Key, object>();
                    public void Add(Key key, object value) { _dict.Add(new Tuple<Key,object>(key, value)); } //  _dict[key] = value; }
                    // public Dictionary<Key, object> GetDict() { return _dict; }
                    public List<Tuple<Key, object>> GetDict() { return _dict; }
                    [NonSerialized]
                    public List<AttributeModel> PrimaryKeys = new List<AttributeModel>();
                }

                [NonSerialized]
                AssignmentDictionary _dict;
                public void SetData(AssignmentDictionary d)
                {
                    _dict = d;
                    _code = "0";
                }
                public void Add(List<AttributeModel> primaryKeys, Key key, object value)
                {
                    var d = _dict ?? new AssignmentDictionary();
                    d.PrimaryKeys = primaryKeys;
                    d.Add(key, value);
                    _dict = d;
                }

                public string ComputeCode(DataType dtype)
                {
                    var code = "";
                    if (_dict?.PrimaryKeys.Count != 0)
                    {
                        foreach (var di in _dict.GetDict().ToArray().Reverse())
                        {
                            code += "(";
                            foreach (var primaryKey in _dict.PrimaryKeys)
                            {
                                if (_dict.PrimaryKeys.IndexOf(primaryKey) < di.Item1.values.Count)
                                    code += primaryKey.RawName + " == " + di.Item1.values[_dict.PrimaryKeys.IndexOf(primaryKey)] + "&&";
                            }
                            code = code.Substring(0, code.Length - 2) + ")";
                            if (dtype == DataType.String)
                                code += " ? \"" + di.Item2 + "\" : ";
                            else code += " ? " + di.Item2 + " : ";
                        }
                    }
                    if (dtype == DataType.String)
                        code += "\"\"";
                    else code += "0";
                    Debug.WriteLine("<<<code>>>" + code);
                    return code;
                }
                public AttributeAssignedValueFuncModel():base("0")
                {
                }
            }

            public class AttributeCodeFuncModel : AttributeFuncModel
            {
                [NonSerialized]
                protected string _code;
                public AttributeCodeFuncModel(string code)
                {
                    _code = code;
                }
                public override AttributeModelType ModelType { get; set; } = AttributeModelType.Code;
                public List<string> Terms { get { return TransformCode(Code).Item2;  }  }
                public string RefactorVariable(string oldName, string newName)
                {
                    return TransformCode(Code, oldName, newName).Item1;
                }
                public string Code { get => _code; set => _code = value; }

                public static Tuple<string, List<string>> TransformCode(string expression, string oldName = null, string newName = null)
                {
                    var sortedRawNames = AttributeTransformationModel.ExistingFieldList().ToList();
                    if (oldName != null && !sortedRawNames.Contains(oldName))
                        sortedRawNames.Add(oldName);
                    var newExpression = expression;

                    foreach (var rawName1 in sortedRawNames.ToArray())
                    {
                        var index = -1;
                        foreach (var rawName2 in sortedRawNames.ToArray())
                        {
                            if (rawName2.Contains(rawName1))
                            {
                                index = sortedRawNames.IndexOf(rawName2);
                            }
                        }
                        if (index != -1)
                        {
                            sortedRawNames.Remove(rawName1);
                            sortedRawNames.Insert(index, rawName1);
                        }
                    }

                    var replacedRawNames = new List<string>();
                    var sections = new Regex("\"[^\"]*\"").Split(expression);
                    var inners   = new Regex("\"[^\"]*\"").Matches(expression);
                    var transforms = sections.Select((str) => str.StartsWith("\"") ? str :
                               transformCodeRecursive(str, sortedRawNames, 0, replacedRawNames, oldName, newName)).ToList();
                    var resultStr = transforms.First();
                    if (inners.Count > 0)
                        for (int i = 0; i < inners.Count; i++)
                        {
                            resultStr += inners[i];
                            if (i < sections.Length - 1)
                                resultStr += transforms[i + 1];
                        }
                    return new Tuple<string, List<string>>(resultStr, replacedRawNames.Distinct().ToList());
                }

                static string transformCodeRecursive( string expression, List<string> sortedRawNames, int rawNameIndex, List<string> replacedRawNames, string oldName, string newName)
                {
                    if (rawNameIndex < sortedRawNames.Count)
                    {
                        string rawName = sortedRawNames[rawNameIndex];
                        var pattern = rawName.Replace("(", "\\(").Replace(")", "\\)").Replace("?", "\\?").Replace(".","\\.");
                        var strings = Regex.Split(expression, pattern);

                        if (strings.Length > 1)
                        {
                            replacedRawNames.Add(rawName);
                            var pieces = new List<string>();
                            bool anythingchanged = false;
                            for (int i = 1; i < strings.Count(); i++)
                            {
                                var s = strings[i];
                                var startBoundaryOk = i == 0 || "()[]<> {}.*/+-%^$#@!\0".Contains(strings[i - 1].LastOrDefault());
                                var endBoundaryOk = "()[]<> {}.*/+-%^$#@!\0".Contains(s.FirstOrDefault());
                                if (startBoundaryOk && endBoundaryOk)
                                {
                                    if (rawName == oldName)
                                        pieces.Add(newName);
                                    else pieces.Add(rawName);
                                    pieces.Add(transformCodeRecursive(s, sortedRawNames, rawNameIndex + 1, replacedRawNames, oldName, newName));
                                    anythingchanged = true;
                                }
                                else
                                {
                                    pieces.Add(rawName);
                                    pieces.Add(s);
                                }
                            }
                            if (anythingchanged)
                            {
                                if (strings[0] != "")
                                    pieces.Insert(0, transformCodeRecursive(strings[0], sortedRawNames, rawNameIndex + 1, replacedRawNames, oldName, newName));
                                return string.Join("", pieces);
                            }
                        }
                        return transformCodeRecursive(expression, sortedRawNames, rawNameIndex + 1, replacedRawNames, oldName, newName);
                    }
                    return expression;
                }

            }
            public class AttributeBackendFuncModel : AttributeFuncModel
            {
                string _id;
                public override AttributeModelType ModelType => AttributeModelType.Backend;
                public AttributeBackendFuncModel(string id)
                {
                    _id = id;
                }
                public string Id { get => _id; set => _id = value; }
            }
            public class AttributeColumnFuncModel : AttributeFuncModel
            {
                public override AttributeModelType ModelType => AttributeModelType.Column;
            }
            public class AttributeGroupFuncModel : AttributeFuncModel
            {
                private ObservableCollection<AttributeModel> _inputModels = new ObservableCollection<AttributeModel>();
                public override AttributeModelType ModelType => AttributeModelType.Group;
                public ObservableCollection<AttributeModel> InputModels
                {
                    get { return _inputModels; }
                    //set { SetProperty(ref _inputModels, value); }
                }
            }
        };
    }
}