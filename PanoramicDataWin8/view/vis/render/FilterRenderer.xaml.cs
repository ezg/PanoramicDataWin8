using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Input;
using System.Diagnostics;
using Windows.UI.Input.Inking;
using GeoAPI.Geometries;
using IDEA_common.operations;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.inq;
using InkStroke = PanoramicDataWin8.view.inq.InkStroke;
using PanoramicDataWin8.model.data.attribute;
using System.Globalization;
using Windows.UI;
using System.Runtime.InteropServices;
using IDEA_common.operations.recommender;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class FilterRenderer : Renderer, IScribbable
    {
        private readonly Gesturizer _gesturizerField = new Gesturizer();
        private readonly Gesturizer _gesturizerScene1 = new Gesturizer();
        private readonly Gesturizer _gesturizerScene2 = new Gesturizer();

        public FilterRenderer()
        {
            this.InitializeComponent();

            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += FilterRenderer_Loaded;

            inkableScene.IsHitTestVisible = false;
            inkableScene2.IsHitTestVisible = false;
            inkableField.IsHitTestVisible = false;
            _gesturizerScene1.AddGesture(new EraseGesture(inkableScene));
            _gesturizerScene2.AddGesture(new EraseGesture(inkableScene2));
            _gesturizerField.AddGesture(new EraseGesture(inkableField));
        }

        void FilterRenderer_Loaded(object sender, RoutedEventArgs e)
        {

            //_filterRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            //_filterRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            //dxSurface.ContentProvider = _filterRendererContentProvider;
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as FilterOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as FilterOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
            //if (dxSurface != null)
            //{
            //    dxSurface.Dispose();
            //}
        }

        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (true && (this.DataContext as FilterOperationViewModel).UseTypingUI)
            {
                TypingPanel.Visibility = Visibility.Visible;
                InkPanels.Visibility = Visibility.Collapsed;
                MinHeight = OperationViewModel.MIN_HEIGHT / 2;
            } else
            {
                MinHeight = OperationViewModel.MIN_HEIGHT / 2;
                TypingPanel.Visibility = Visibility.Collapsed;
                InkPanels.Visibility = Visibility.Visible;

            }
            if (args.NewValue != null)
            {
                (DataContext as FilterOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
                (DataContext as FilterOperationViewModel).OperationModel.OperationModelUpdated += OperationModelUpdated;
                (DataContext as FilterOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
                (DataContext as FilterOperationViewModel).OperationModel.PropertyChanged += OperationModel_PropertyChanged;


            }
        }
        void OperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterOperationModel operationModel = (FilterOperationModel)((OperationViewModel)DataContext).OperationModel;

        }

        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            if (e is VisualOperationModelUpdatedEventArgs)
            {
                render();
            }
        }

        public override void StartSelection(Windows.Foundation.Point point)
        {
            _pressed = DateTime.Now;
        }


        DateTime _pressed;

        public override void MoveSelection(Windows.Foundation.Point point)
        {
        }

        public override bool EndSelection()
        {
            if (DateTime.Now.Subtract(_pressed).TotalMilliseconds < 300)
            {
                ExpressionTextBox.IsEnabled = true;
                ExpressionTextBox.Focus(FocusState.Keyboard);
                return true;
            }
            return false;
        }
        void render(bool sizeChanged = false)
        {
            //dxSurface?.Redraw();
        }


        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                return this.GetBounds(MainViewController.Instance.InkableScene).GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                FilterOperationViewModel model = this.DataContext as FilterOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public bool Consume(InkStroke inkStroke)
        {
            var tg             = MainViewController.Instance.InkableScene.TransformToVisual(this);
            var transInkStroke = inkStroke.GetTranslated(tg.TransformPoint(new Point()));
            Op1Val.Text = this.Op2Val.Text = "";
            
            if (OpExpr.Visibility == Visibility.Visible && OpExpr.GetBoundingRect(this).Contains(transInkStroke.Points.First()))
            {
                return tapOnOperator(transInkStroke);
            }
            else if (Field.Visibility == Visibility.Visible && Field.GetBoundingRect(this).Contains(transInkStroke.Points.First()))
            {
                return ProcessStroke(inkStroke, inkableField, _gesturizerField);
            }
            else if (Op1.Visibility == Visibility.Visible && Op1.GetBoundingRect(this).Contains(transInkStroke.Points.First()))
            {
                return ProcessStroke(inkStroke, inkableScene, _gesturizerScene1);
            }
            else if (Op2.Visibility == Visibility.Visible && Op2.GetBoundingRect(this).Contains(transInkStroke.Points.First()))
            {
                return ProcessStroke(inkStroke, inkableScene2, _gesturizerScene2);
            }
            return false;
        }

        bool ProcessStroke(InkStroke inkStroke, InkableScene scene, Gesturizer gesturizer)
        {
            var tg2 = MainViewController.Instance.InkableScene.TransformToVisual(scene);
            var offset2 = tg2.TransformPoint(new Point());
            var transInkStroke2 = inkStroke.GetTranslated(offset2);

            var recognizedGestures = gesturizer.Recognize(transInkStroke2.Clone());
            foreach (var recognizedGesture in recognizedGestures.ToList())
            {
                if (recognizedGesture is HitGesture)
                {
                    var hitGesture = recognizedGesture as HitGesture;
                    foreach (var hitScribbable in hitGesture.HitScribbables)
                    {
                        if (hitScribbable is InkStroke)
                        {
                            scene.Remove(hitScribbable as InkStroke);
                        }
                    }
                }
            }

            if (!transInkStroke2.IsErase && !recognizedGestures.Any())
                scene.Add(transInkStroke2);
            recognize();
            return true;
        }

        bool tapOnOperator(InkStroke transInkStroke)
        {
            var op1text = Op1Text.Text;
            var op2text = Op2Text.Text;
            var pt = transInkStroke.Points.First();
            if (Op1Text.GetBoundingRect(this).Contains(pt))
            {
                Op1Text.Text = op1text = Op1Text.Text == "<" ? "<=" : "<";
            }
            else if (Op2Text.GetBoundingRect(this).Contains(pt))
            {
                Op2Text.Text = op2text = Op2Text.Text == "<" ? "<=" : Op2Text.Text == "<=" ? "=" : "<";
                recognize();
            }

            Op1.Visibility     = Op2Text.Text == "=" ? Visibility.Collapsed : Visibility.Visible;
            Op1Val.Visibility  = Op2Text.Text == "=" ? Visibility.Collapsed : Visibility.Visible;
            Op1Text.Visibility = Op2Text.Text == "=" ? Visibility.Collapsed : Visibility.Visible;
            Grid.SetColumnSpan(Op2, Op2Text.Text == "=" ? 2 : 1);
            Grid.SetColumn    (Op2, Op2Text.Text == "=" ? 0 : 1);
            return true;
        }

        AttributeTransformationModel validField(string str)
        {
            var inputModels = (MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel.OriginModels.First()
                     .InputModels.Where(am => am.IsDisplayed) /*.OrderBy(am => am.RawName)*/;
            AttributeTransformationModel attributeTransformationModel = null;
            foreach (var im in inputModels)
                if (im.RawName.ToLower().StartsWith(str.ToLower()))
                    attributeTransformationModel = new AttributeTransformationModel(im as AttributeFieldModel);
            foreach (var im in inputModels)
                if (im.RawName.ToLower() == str.ToLower())
                    attributeTransformationModel = new AttributeTransformationModel(im as AttributeFieldModel);
            return attributeTransformationModel;
        }

        public static string FieldType(string str)
        {
            var inputModels = (MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel.OriginModels.First()
                     .InputModels.Where(am => am.IsDisplayed) /*.OrderBy(am => am.RawName)*/;
            AttributeFieldModel fieldModel = null;
            foreach (var im in inputModels)
                if (im.RawName.ToLower().StartsWith(str.ToLower()))
                    fieldModel = im as AttributeFieldModel;
            foreach (var im in inputModels)
                if (im.RawName.ToLower() == str.ToLower())
                    fieldModel = im as AttributeFieldModel; 
            return fieldModel != null ? fieldModel.InputDataType : null;
        }

        InkManager AddStrokesToInkManager(InkableScene scene)
        {
            InkManager imgr = new InkManager();
            var b = new InkStrokeBuilder();
            foreach (var inkStroke in scene.InkStrokes)
            {
                var pc = new PointCollection();
                foreach (var pt in inkStroke.Points)
                    pc.Add(pt);
                var stroke = b.CreateStroke(pc);
                imgr.AddStroke(stroke);
            }
            return imgr;
        }
        async void recognize()
        {
            AttributeTransformationModel attributeTransformationModel = null;
            string  recognizedText   = null;
            double? recognizedValue1 = null;
            double? recognizedValue2 = null;

            OpField.Text = Op1Val.Text = Op2Val.Text = "";

            var imgr = AddStrokesToInkManager(inkableField);
            var imgr1 = AddStrokesToInkManager(inkableScene);
            var imgr2 = AddStrokesToInkManager(inkableScene2);
            
            if (imgr.GetStrokes().Count > 0)
            {
                var result = await imgr.RecognizeAsync(InkRecognitionTarget.All);
                recognizedText = result[0].GetTextCandidates().First().Replace(" ", "");
                attributeTransformationModel = validField(recognizedText);
                OpField.Text = attributeTransformationModel != null ? attributeTransformationModel.AttributeModel.DisplayName : recognizedText;
                OpField.Foreground = validField(recognizedText) != null ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
            }
            if (imgr1.GetStrokes().Count > 0)
            {
                double val;
                var result1 = await imgr1.RecognizeAsync(InkRecognitionTarget.All);
                if (double.TryParse(result1[0].GetTextCandidates().First(), out val))
                {
                    recognizedValue1 = val;
                    Op1Val.Text = val.ToString();
                    Op1Val.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    Op1Val.Text = result1[0].GetTextCandidates().First().Replace(" ", "");
                    Op1Val.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            if (imgr2.GetStrokes().Count > 0)
            {
                double val;
                var result2 = await imgr2.RecognizeAsync(InkRecognitionTarget.All);
                if (double.TryParse(result2[0].GetTextCandidates().First(), out val))
                {
                    recognizedValue2 = val;
                    Op2Val.Text = val.ToString();
                    Op2Val.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    Op2Val.Text = result2[0].GetTextCandidates().First().Replace(" ", "");
                    Op2Val.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            (DataContext as FilterOperationViewModel).FilterOperationModel.ClearFilterModels();
            if (attributeTransformationModel != null)
            {
                if (recognizedValue1.HasValue)
                    AddFilterModel(attributeTransformationModel, ToLvalPredicate(Op1Text.Text), (double)recognizedValue1);
                if (recognizedValue2.HasValue)
                    AddFilterModel(attributeTransformationModel, ToRvalPredicate(Op2Text.Text), (double)recognizedValue2);
            }
            UpdateFilterDisplay();
        }

        string ToString(Predicate p)
        {
            switch (p)
            {
                case Predicate.EQUALS: return "=";
                case Predicate.GREATER_THAN: return ">";
                case Predicate.GREATER_THAN_EQUAL: return ">=";
                case Predicate.LESS_THAN: return "<";
                case Predicate.LESS_THAN_EQUAL: return "<=";
                case Predicate.LIKE: return "Like";
                case Predicate.STARTS_WITH: return "^=";
                case Predicate.ENDS_WITH: return "$=";
                case Predicate.CONTAINS: return "~=";

            }
            return "";
        }
        Predicate ToRvalPredicate(string s)
        {
            switch (s)
            {
                case "=": return Predicate.EQUALS;
                case ">": return Predicate.GREATER_THAN;
                case ">=": return Predicate.GREATER_THAN_EQUAL;
                case "<": return Predicate.LESS_THAN;
                case "<=": return Predicate.LESS_THAN_EQUAL;

            }
            return Predicate.EQUALS;
        }
        Predicate ToLvalPredicate(string s)
        {
            switch (s)
            {
                case "=": return Predicate.EQUALS;
                case "<": return Predicate.GREATER_THAN;
                case "<=": return Predicate.GREATER_THAN_EQUAL;
                case ">": return Predicate.LESS_THAN;
                case ">=": return Predicate.LESS_THAN_EQUAL;

            }
            return Predicate.EQUALS;
        }
        Predicate ToStringPredicate(string s)
        {
            switch (s)
            {
                case "=": return Predicate.EQUALS;
                case "<": return Predicate.GREATER_THAN;
                case "<=": return Predicate.GREATER_THAN_EQUAL;
                case ">": return Predicate.LESS_THAN;
                case ">=": return Predicate.LESS_THAN_EQUAL;
                case "^=": return Predicate.STARTS_WITH;
                case "$=": return Predicate.ENDS_WITH;
                case "~=": return Predicate.CONTAINS;

            }
            return Predicate.STARTS_WITH;
        }


        void AddFilterModel(AttributeTransformationModel attributeTransformationModel, Predicate p, string value)
        {
            var filterModel = (this.DataContext as FilterOperationViewModel).FilterOperationModel.FilterModels.LastOrDefault();
            if (filterModel == null)
            {
                filterModel = new FilterModel();
                (DataContext as FilterOperationViewModel).FilterOperationModel.AddFilterModel(filterModel);
            }

            if (attributeTransformationModel != null)
                OpField.Text = attributeTransformationModel.AttributeModel.DisplayName;
            
            Op2Text.Text = ToString(ToRvalPredicate(ToString(p)));
            if (attributeTransformationModel == null)
                filterModel = null;
            else filterModel.ValueComparisons.Add(new ValueComparison(attributeTransformationModel, p, value));

            UpdateFilterDisplay();
            render();
        }
        void AddFilterModel(AttributeTransformationModel attributeTransformationModel, Predicate p, double value)
        {
            var filterModel = (this.DataContext as FilterOperationViewModel).FilterOperationModel.FilterModels.LastOrDefault();
            if (filterModel == null)
            {
                filterModel = new FilterModel();
            }

            if (attributeTransformationModel != null)
                OpField.Text = attributeTransformationModel.AttributeModel.DisplayName;

            if (p == Predicate.GREATER_THAN || p == Predicate.GREATER_THAN_EQUAL)
            {
                Op1Text.Text = ToString(ToLvalPredicate(ToString(p)));
            }
            else
            {
                Op2Text.Text = ToString(ToRvalPredicate(ToString(p)));
            }
            if (attributeTransformationModel == null)
                filterModel = null;
            else filterModel.ValueComparisons.Add(new ValueComparison(attributeTransformationModel, p, value));
            if (!(DataContext as FilterOperationViewModel).FilterOperationModel.FilterModels.Contains(filterModel))
                (DataContext as FilterOperationViewModel).FilterOperationModel.AddFilterModel(filterModel);

            UpdateFilterDisplay();
            render();
        }

        void UpdateFilterDisplay()
        {
            var filterModels = (this.DataContext as FilterOperationViewModel).FilterOperationModel.FilterModels;
            if (filterModels.Count > 0 && filterModels.First().ValueComparisons.Count > 0)
            {
                this.ExpressionTextBox.Text = "";
                if (filterModels.Count == 1 && filterModels.First().ValueComparisons.Count == 2)
                {
                    var vop1 = filterModels.First().ValueComparisons.First();
                    var vop2 = filterModels.First().ValueComparisons.Last();
                    this.ExpressionTextBox.Text = vop1.Value + " " + ToString(ToLvalPredicate(ToString(vop1.Predicate))) + " " +
                        (vop1.AttributeTransformationModel == null ? "??" : vop1.AttributeTransformationModel.AttributeModel.DisplayName) +
                        " " + ToString(vop2.Predicate) + " " + vop2.Value;
                }
                else
                {
                    foreach (var fm in filterModels)
                        foreach (var vop in fm.ValueComparisons)
                            this.ExpressionTextBox.Text += " && " + (vop.AttributeTransformationModel == null ? OpField.Text : vop.AttributeTransformationModel.AttributeModel.DisplayName) + " " +
                                ToString(vop.Predicate) + " " + vop.Value;
                    if (ExpressionTextBox.Text != "")
                        ExpressionTextBox.Text = ExpressionTextBox.Text.Substring(4);
                }
                ExpressionTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
                ExpressionTextBox.Foreground = new SolidColorBrush(Colors.Red);
        }

        public void SetFilter(string field, Predicate p, double value)
        {
            Op1Val.Text = Op2Val.Text = "";
            if (p == Predicate.LESS_THAN || p == Predicate.LESS_THAN_EQUAL || p == Predicate.EQUALS)
                 Op2Val.Text = value.ToString();
            else Op1Val.Text = value.ToString();

            (DataContext as FilterOperationViewModel).FilterOperationModel.ClearFilterModels();
            AddFilterModel(validField(field), p, value);
        }
        public void SetFilter(string field, Predicate p, string value)
        {
            Op1Val.Text = Op2Val.Text = "";
            Op2Val.Text = value;

            (DataContext as FilterOperationViewModel).FilterOperationModel.ClearFilterModels();
            AddFilterModel(validField(field), p, value);
        }

        string tokenize(ref string accum, char letter)
        {
            var token = "";
            switch (letter)
            {
                case ' ':
                    token = accum;
                    accum = "";
                    return token;
                case '=':
                    if (accum != "" && (accum.Last() == '<' || accum.Last() == '>'))
                    {
                        token = accum + '=';
                        accum = "";
                    }
                    else
                        accum += letter;
                    return token;
                case '>':
                case '<':
                    token = accum;
                    accum = "" + letter;
                    return token;
                default:
                    if (accum != "" && (accum.Last() == '<' || accum.Last() == '>' || accum.Last() == '='))
                    {
                        token = accum;
                        accum = "" + letter;
                        return token;
                    }
                    accum += letter;
                    break;
            }
            return "";
        }

        void ExpressionTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                (DataContext as FilterOperationViewModel).FilterOperationModel.ClearFilterModels();

                var splits = new List<String>();
                var exprText = ExpressionTextBox.Text;
                string accum = "";
                for (int i = 0; i < exprText.Length; i++)
                {
                    var token = tokenize(ref accum, exprText[i]);
                    if (token != "")
                        splits.Add(token);
                }
                if (accum != "")
                    splits.Add(accum);

                double val = 0;
                if (splits.Count() > 2)
                {
                    var vfield = FieldType(splits.First());
                    if (vfield != null && vfield == InputDataTypeConstants.NVARCHAR)
                    {
                        var p = ToStringPredicate(splits[1]);
                        AddFilterModel(validField(splits[0]), p, splits[2]);
                    }
                    else
                    {
                        if (double.TryParse(splits.First(), out val))
                        {
                            var p = ToLvalPredicate(splits[1]);
                            AddFilterModel(validField(splits[2]), p, val);
                            splits.RemoveAt(0);
                            splits.RemoveAt(0);
                        }
                        if (splits.Count() > 2)
                        {
                            var p = ToRvalPredicate(splits[1]);
                            if (double.TryParse(splits[2], out val))
                            {
                                AddFilterModel(validField(splits[0]), p, val);
                            }
                        }
                    }
                }
                e.Handled = true;
            }
        }
        
        void ExpressionTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            if (!ExpressionTextBox.GetBounds().Contains(e.GetCurrentPoint(ExpressionTextBox).Position))
            {
                ExpressionTextBox.IsEnabled = false;
                MainViewController.Instance.MainPage.FocusSink.Focus(FocusState.Keyboard);
            }
        }
    }
}