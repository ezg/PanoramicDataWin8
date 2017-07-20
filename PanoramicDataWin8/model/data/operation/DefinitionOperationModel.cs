﻿using PanoramicDataWin8.model.data.idea;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public class DefinitionOperationModel : OperationModel, IBrushableOperationModel
    {
        private readonly BrushableOperationModelImpl _brushableOperationModelImpl;
        string _rawName;

        public DefinitionOperationModel(SchemaModel schemaModel, string rawName, string displayName=null) : base(schemaModel)
        {
            _rawName = rawName;
            if (rawName != null && !IDEAAttributeComputedFieldModel.NameExists(rawName))
            {
                IDEAAttributeComputedFieldModel.Add(rawName, displayName == null ? rawName : displayName, "0", IDEA_common.catalog.DataType.String, "numeric",
                               new List<IDEA_common.catalog.VisualizationHint>());
            }
            _brushableOperationModelImpl = new BrushableOperationModelImpl(this);
        }
        public class BrushDescriptor
        {
            public BrushDescriptor()
            {

            }
            public BrushDescriptor(Color color, string name)
            {
                Color = color;
                Name = name;
            }
            public string Name { get; set; }
            public Color Color { get; set; }
        }
        public List<BrushDescriptor> BrushDescriptors { get; set; } = new List<BrushDescriptor>();

        public void SetDescriptorForColor(Color c, BrushDescriptor d)
        {
            if (GetDescriptorFromColor(c) != null)
                GetDescriptorFromColor(c).Name = d.Name;
            else BrushDescriptors.Add(d);
        }
        public BrushDescriptor GetDescriptorFromColor(Color c)
        {
            foreach (var d in BrushDescriptors)
                if (d.Color == c)
                    return d;
            return null;
        }

        public List<Color> BrushColors { get; set; } = new List<Color>();


        public ObservableCollection<IBrusherOperationModel> BrushOperationModels
        {
            get { return _brushableOperationModelImpl.BrushOperationModels; }
            set { _brushableOperationModelImpl.BrushOperationModels = value; }
        }
        public IDEAAttributeComputedFieldModel GetCode()
        {
            return IDEAAttributeComputedFieldModel.Function(_rawName);
        }
        public void SetRawName(string name)
        {
            GetCode().RawName = name;
            _rawName = name;
            GetCode().DisplayName = name;
        }
        public void UpdateCode()
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
            GetCode().VisualizationHints = new List<IDEA_common.catalog.VisualizationHint>(new IDEA_common.catalog.VisualizationHint[] { IDEA_common.catalog.VisualizationHint.TreatAsEnumeration});

            GetCode().SetCode(expression);
        }
    }
}