using IDEA_common.catalog;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class BrushDescriptor : ExtendedBindableBase
    {
        string _name;
        public BrushDescriptor() { }
        public BrushDescriptor(Color color, string name)
        {
            Color = color;
            Name = name;
        }
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public Color Color { get; set; }
    }
    public class DefinitionOperationModel : ComputationalOperationModel, IBrushableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        

        public DefinitionOperationModel(SchemaModel schemaModel, string rawName, string displayName=null) : base(schemaModel, "0", DataType.String, "numeric", rawName, displayName)
        {
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
            BrushOperationModels.CollectionChanged += BrushOperationModels_CollectionChanged;
        }
        private void BrushOperationModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (GetAttributeModel() != null)
                UpdateName();
        }

        public List<BrushDescriptor> BrushDescriptors { get; set; } = new List<BrushDescriptor>();

        public void SetDescriptorForColor(Color c, BrushDescriptor d)
        {
            if (GetDescriptorFromColor(c) != null)
                GetDescriptorFromColor(c).Name = d.Name;
            else BrushDescriptors.Add(d);
        }
        public BrushDescriptor GetDescriptorFromColor(Color c, string nameIfNull=null)
        {
            foreach (var d in BrushDescriptors)
                if (d.Color == c)
                    return d;
            if (nameIfNull != null)
            {
                var newBrush = new BrushDescriptor(c, nameIfNull);
                SetDescriptorForColor(c, newBrush);
                return newBrush;
            }
            return null;
        }

        public List<Color> BrushColors { get; set; } = new List<Color>();
        
        public ObservableCollection<IBrusherOperationModel> BrushOperationModels
        {
            get { return _brushableOperationModelImpl.BrushOperationModels; }
            set { _brushableOperationModelImpl.BrushOperationModels = value;   }
        }
        public void UpdateCode(bool refactoring=false)
        {
            var expressions = new List<string>();
            foreach (var opModel in BrushOperationModels)
            {
                if (opModel.FilterModels.Count > 0)
                {
                    string code = "(";
                    bool first = true;
                    foreach (var filt in opModel.FilterModels)
                    {
                        if (first)
                        {
                            code += "(";
                            first = false;
                        }
                        else
                            code += "|| (";
                        foreach (var vc in filt.ValueComparisons)
                            code += vc.ToPythonString() + " && ";
                        code = code.Substring(0, code.Length - 4);
                        code += ")";
                    }
                    code += ")";
                    expressions.Add(code);
                }
            }

            string expression = "";
            string ORseparator = " || ";
            if (expressions.Count > 0) 
            {
                if (expressions.Count > 1)
                {
                    expression = "(";
                    for (int cind = 0; cind < expressions.Count-1; cind++)
                    {
                        var c = expressions[cind];
                        expression += "(" +  c + " && ";
                        var unions = "(";
                        for (int oind = cind+1; oind < expressions.Count; oind++)
                        {
                            var o = expressions[oind];
                            unions += o + ORseparator;
                        }
                        unions = unions.Substring(0, unions.Length - ORseparator.Length) + ")";
                        expression += unions + ")"+ ORseparator;
                    }
                    expression = expression.Substring(0, expression.Length - ORseparator.Length) +  ") ? \"" + BrushDescriptors[1].Name + "\" : ";
                }

                int index = 0;
                foreach (var c in expressions)
                {
                    var name = "\"" + GetDescriptorFromColor(BrushColors[index]).Name + "\"";
                    expression += c + "? " + name + " :";
                    index++;
                }
            }
            expression += "\"" + BrushDescriptors[0].Name + "\"";
            GetAttributeModel().VisualizationHints = new List<IDEA_common.catalog.VisualizationHint>(new IDEA_common.catalog.VisualizationHint[] { IDEA_common.catalog.VisualizationHint.TreatAsEnumeration});


            GetAttributeModel().SetCode(expression, DataType.String, refactoring);
        }
    }
}