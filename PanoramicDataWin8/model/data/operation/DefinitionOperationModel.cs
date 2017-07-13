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
        private model.data.idea.IDEAAttributeComputedFieldModel _code;

        public DefinitionOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
            _code = new idea.IDEAAttributeComputedFieldModel("", "", "", IDEA_common.catalog.DataType.String, "numeric",
                   new List<IDEA_common.catalog.VisualizationHint>());
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
        public model.data.idea.IDEAAttributeComputedFieldModel Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
            }
        }
        public void SetRawName(string name)
        {
            _code.RawName = name;
            _code.DisplayName = name;
        }
        public void UpdateCode()
        {
            string code = "";
            int index = 0;
            foreach (var opModel in BrushOperationModels)
            {
                foreach (var filt in opModel.FilterModels)
                {
                    code += "(";
                    foreach (var vc in filt.ValueComparisons)
                        code += vc.ToPythonString() + " && ";
                    code = code.Substring(0, code.Length - 4);
                    code += ")";
                    var name = "\""+(index < BrushColors.Count ? GetDescriptorFromColor(BrushColors[index]) : index == BrushColors.Count ? BrushDescriptors[0] : BrushDescriptors[1]).Name + "\"";
                    code += "? " + name + ": ";
                }
                    index++;
            }
            code += "\"" + BrushDescriptors[0].Name + "\"";
            Code.VisualizationHints = new List<IDEA_common.catalog.VisualizationHint>(new IDEA_common.catalog.VisualizationHint[] { IDEA_common.catalog.VisualizationHint.TreatAsEnumeration});

            (Code.FuncModel as AttributeCodeFuncModel).Code = code;
        }
    }
}