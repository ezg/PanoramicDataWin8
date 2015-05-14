using System.Collections.Generic;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view

{
    public abstract class OrgItem : ExtendedBindableBase
    {
        private bool _isVisible;
        private bool _isExpanded;
        private bool _isSelected;
        private OrgItem _parent;

        protected OrgItem(object data, string name)
        {
            Data = data;
            Name = name;
        }

        public string Name { get; set; }

        public object Data { get; private set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                this.SetProperty(ref _isVisible, value);
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                this.SetProperty(ref _isExpanded, value);
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                this.SetProperty(ref _isSelected, value);
            }
        }

        public OrgItem Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public virtual IEnumerable<OrgItem> Children
        {
            get { return new List<OrgItem>(); }
        }
    }

    public class NamedAttributeRootOrgItem : OrgItem
    {
        private IEnumerable<OrgItem> _children = null;
        private SchemaModel _schemaModel = null;

        public NamedAttributeRootOrgItem(SchemaModel schemaModel, string name)
            : base(schemaModel, name)
        {
            _schemaModel = schemaModel;
        }

        public override IEnumerable<OrgItem> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = ConstructChildren();
                }
                return _children;
            }
        }

        private IEnumerable<OrgItem> ConstructChildren()
        {
            List<OrgItem> children = new List<OrgItem>();
            foreach (var key in _schemaModel.NamedAttributeModels.Keys)
            {
                OrgItem oi = new AttributeOrgItem(key);
                children.Add(oi);
            }
            return children;
        }
    }

    public class CaclculatedAttributeRootOrgItem : OrgItem
    {
        private IEnumerable<OrgItem> _children = null;
        private SchemaModel _schemaModel = null;

        public CaclculatedAttributeRootOrgItem(SchemaModel schemaModel, string name)
            : base(schemaModel, name)
        {
            _schemaModel = schemaModel;
        }

        public override IEnumerable<OrgItem> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = ConstructChildren();
                }
                return _children;
            }
        }

        private IEnumerable<OrgItem> ConstructChildren()
        {
            List<OrgItem> children = new List<OrgItem>();
            foreach (var key in _schemaModel.CalculatedAttributeModels.Keys)
            {
                OrgItem oi = new AttributeOrgItem(key);
                children.Add(oi);
            }
            return children;
        }
    }


    public class DatabaseRootOrgItem : OrgItem
    {
        private IEnumerable<OrgItem> _children = null;
        private SchemaModel _schemaModel = null;

        public DatabaseRootOrgItem(SchemaModel schemaModel, string name)
            : base(schemaModel, name)
        {
            _schemaModel = schemaModel;
        }

        public override IEnumerable<OrgItem> Children
        {
            get {
                if (_children == null)
                {
                    _children = ConstructChildren();
                }
                return _children;
            }
        }

        private IEnumerable<OrgItem> ConstructChildren()
        {
            List<OrgItem> children = new List<OrgItem>();
            foreach (var originModel in _schemaModel.OriginModels)
            {
                OrgItem oi = new OriginOrgItem(originModel, _schemaModel);
                children.Add(oi);
            }
            return children;
        }
    }

    public class OriginOrgItem : OrgItem
    {
        private IEnumerable<OrgItem> _children = null;
        private SchemaModel _schemaModel = null;

        public OriginOrgItem(OriginModel originModel, SchemaModel schemaModel)
            : base(originModel, originModel.Name)
        {
            _schemaModel = schemaModel;
        }

        public override IEnumerable<OrgItem> Children
        {
            get 
            {
                if (_children == null)
                {
                    _children = ConstructChildren(Data as OriginModel);
                }
                return _children;
            }
        }

        private IEnumerable<OrgItem> ConstructChildren(OriginModel originModel)
        {
            List<OrgItem> children = new List<OrgItem>();
            foreach (var childOriginModel in originModel.OriginModels)
            {
                OrgItem oi = new OriginOrgItem(childOriginModel, _schemaModel);
                children.Add(oi);
            }
            foreach (var attributeModel in originModel.AttributeModels)
            {
                OrgItem oi = new AttributeOrgItem(attributeModel);
                children.Add(oi);
            }
            return children;
        }
    }

    public class AttributeOrgItem : OrgItem
    {
        public AttributeOrgItem(AttributeModel attributeModel)
            : base(attributeModel, attributeModel.Name)
        {
        }
    }
}
