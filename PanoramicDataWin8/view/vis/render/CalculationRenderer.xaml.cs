﻿using PanoramicDataWin8.view.inq;
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
using Windows.UI.Xaml.Navigation;
using GeoAPI.Geometries;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.model.data.operation;
using System.ComponentModel;
using Windows.UI;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.controller.data.progressive;
using IDEA_common.operations;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.view;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.vis.render
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalculationRenderer : Renderer, IScribbable
    {
        public CalculationRenderer()
        {
            this.InitializeComponent();
            this.CodeBox.LostFocus += (sender, e) => Labels.IsHitTestVisible = false;
        }
        
        public CalculationOperationModel CalculationOperationModel {  get { return (DataContext as CalculationOperationViewModel)?.OperationModel as CalculationOperationModel; } }
        
        public override void StartSelection(Point point)
        {
            var bounds = CodeBox.GetBoundingRect(MainViewController.Instance.InkableScene);
            if (bounds.Contains(point))
            {
                CodeBox.Focus(FocusState.Keyboard);
                CodeBox.Focus(FocusState.Pointer);
                Labels.IsHitTestVisible = true;
            }
        }
        
        private SolidColorBrush _textBrush = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush;
        private readonly SolidColorBrush _lightBrush = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;

        public override void Refactor(string oldName, string newName)
        {
            CodeBox.Text = CalculationOperationModel.GetAttributeModel().GetCode();
        }

        async void CodeBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var codeText = CodeBox.Text;
            var newAttr = new AttributeCodeParameters() { Code = codeText, RawName = CalculationOperationModel?.GetAttributeModel().RawName };

            var originModel = CalculationOperationModel.OriginModel;
            var attributeCodeParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels(originModel).Select(a => IDEAHelpers.GetAttributeParameters(a)).OfType<AttributeCodeParameters>().ToList();
            if (attributeCodeParameters.Contains(newAttr))
                attributeCodeParameters.Remove(newAttr);
            attributeCodeParameters.Add(newAttr);
            
            var res = await new CodeCommand().CompileCode(
                new CodeParameters()
                {
                    AttributeCodeParameters = attributeCodeParameters,
                    AdapterName = ((IDEASchemaModel)MainViewController.Instance.MainModel.SchemaModel).RootOriginModel.Name
                });

            if (res.RawNameToCompileResult.Where((r) => !r.Value.CompileSuccess).Count() == 0)
            {
                foreach (var r in res.RawNameToCompileResult)
                    if (r.Key == newAttr.RawName)
                        CalculationOperationModel.SetCode(newAttr.Code, r.Value.DataType);
                CodeBoxFeedback.Text = "";
                CodeBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                foreach (var r in res.RawNameToCompileResult)
                    if (r.Key == newAttr.RawName)
                        CodeBoxFeedback.Text = r.Value.CompileMessage;
                CodeBox.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as CalculationOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }

        private void CodeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CodeBoxFeedback.Text != "" && CodeBox.IsInVisualTree())
                FlyoutBase.ShowAttachedFlyout(CodeBox);
        }
    }
}
