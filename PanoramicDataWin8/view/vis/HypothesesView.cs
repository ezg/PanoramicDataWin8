using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using IDEA_common.operations.risk;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.tilemenu;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.view.vis
{
    public class HypothesesView : UserControl
    {
        public static double GAP = 4;

        private readonly DispatcherTimer _activeTimer = new DispatcherTimer();
        private readonly Canvas _contentCanvas;
        
        private readonly Dictionary<HypothesisViewModel, HypothesisView> _views = new Dictionary<HypothesisViewModel, HypothesisView>();

        private HypothesesViewModel _hypothesesViewModel;

        private int _lastVisibleIndex = -1;

        public HypothesesView()
        {
            DataContextChanged += HypothesesView_DataContextChanged;
            _contentCanvas = new Canvas();
            this.Content = _contentCanvas;
            this.SizeChanged += _contentCanvas_SizeChanged;
            this.ManipulationStarted += HypothesesView_ManipulationStarted;
            this.ManipulationDelta += HypothesesView_ManipulationDelta;
            this.ManipulationCompleted += HypothesesView_ManipulationCompleted;
            this.ManipulationMode = ManipulationModes.All;
            this.ManipulationInertiaStarting += HypothesesView_ManipulationInertiaStarting;

            MainViewController.Instance.InkableScene.AddHandler(PointerPressedEvent, new PointerEventHandler(MainPage_PointerPressed), true);

            _activeTimer.Interval = TimeSpan.FromMilliseconds(10);
            _activeTimer.Tick += _activeTimer_Tick;
            _activeTimer.Start();
        }

        private void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            foreach (var hypo in _hypothesesViewModel.HypothesisViewModels)
            {
                hypo.TargetSize = new Vec(HypothesisViewModel.DefaultHeight, HypothesisViewModel.DefaultHeight);
                hypo.IsExpanded = false;
            }
            updateRendering();
        }

        private void HypothesesView_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
           e.TranslationBehavior.DesiredDeceleration = 0.1;

        }

        private void HypothesesView_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            //var moveIndexBy = -Math.Floor(e.Cumulative.Translation.Y/(HypothesisViewModel.DefaultHeight + GAP));
            //_lastVisibleIndex += (int) moveIndexBy;
            //updateRendering();
            updateRendering();
        }

        private double _currentCumulativeDelta = 0;
        private void HypothesesView_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            //_currentCumulativeDelta += e.Delta.Translation.Y;
            var moveIndexBy = Math.Floor(e.Cumulative.Translation.Y / (HypothesisViewModel.DefaultHeight + GAP));
            
            //_currentCumulativeDelta -= moveIndexBy*(HypothesisViewModel.DefaultHeight + GAP);

            var current = _lastVisibleIndex;
            updateLastVisibleIndex(_scrollStartLastIndex - (int) moveIndexBy);

            if (current != _lastVisibleIndex)
            {
                _currentCumulativeDelta = 0;
                updateRendering();
            }
            else
            {
                var movePixelBy = e.Cumulative.Translation.Y + (_lastVisibleIndex - _scrollStartLastIndex) * (HypothesisViewModel.DefaultHeight + GAP);
                //Debug.WriteLine(movePixelBy);
                //Debug.WriteLine(_lastVisibleIndex);
                //Debug.WriteLine(_scrollStartLastIndex);
                foreach (var hypo in _hypothesesViewModel.HypothesisViewModels)
                {
                    hypo.DeltaTargetPosition = new Pt(0, Math.Sign(movePixelBy) * Math.Sqrt(Math.Abs(movePixelBy)));
                }
            }
        }

        private int _scrollStartLastIndex  = -1;
        private void HypothesesView_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            _scrollStartLastIndex = _lastVisibleIndex;
            _currentCumulativeDelta = 0;
        }

        private Dictionary<HypothesisViewModel, Storyboard> _storyboards = new Dictionary<HypothesisViewModel, Storyboard>();
        void toggleDisplayed(HypothesisViewModel model)
        {
            var ts = TimeSpan.FromMilliseconds(300);

            // fade out
            if (!model.IsDisplayed)
            {
                if (_storyboards.ContainsKey(model))
                {
                    //_storyboards[kvp.Key].Stop();
                }
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = _views[model].Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, _views[model]);
                Storyboard.SetTargetProperty(animation, "Opacity");
               // storyboard.Duration = new Duration(ts);
               animation.Duration = new Duration(ts);
                storyboard.Begin();
                storyboard.Completed += (sender, o) =>
                {
                    _views[model].IsHitTestVisible = false;
                };
                _storyboards[model] = storyboard;
            }
            // fade in
            else
            {
                if (_storyboards.ContainsKey(model))
                {
                    // _storyboards[kvp.Key].Stop();
                }
                _views[model].IsHitTestVisible = true;

                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = _views[model].Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, _views[model]);
                Storyboard.SetTargetProperty(animation, "Opacity");
                animation.Duration = new Duration(ts);
                storyboard.Begin();
                storyboard.Completed += (sender, o) =>
                {
                    _views[model].IsHitTestVisible = true;
                };
                _storyboards[model] = storyboard;
            }
        }

        private void _contentCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateRendering();
        }

        private void _activeTimer_Tick(object sender, object e)
        {
            // animate all elements to target size, position
            if (_hypothesesViewModel != null)
            {
                foreach (var item in _hypothesesViewModel.HypothesisViewModels)
                {
                    // position
                    if ((item.Position.X == 0) && (item.Position.Y == 0))
                    {
                        item.Position = item.TargetPosition;
                    }
                    else
                    {
                        var delta = (item.TargetPosition + item.DeltaTargetPosition) - item.Position;
                        var deltaNorm = delta.Normalized();
                        var t = delta.Length;
                        item.Position = t <= 1 ? (item.TargetPosition + item.DeltaTargetPosition) : item.Position + deltaNorm*(t/item.DampingFactor);
                    }

                    // size
                    if ((item.Size.X == 0) && (item.Size.Y == 0))
                    {
                        item.Size = item.TargetSize;
                    }
                    else
                    {
                        var delta = item.TargetSize - item.Size;
                        var deltaNorm = delta.Normalized();
                        var t = delta.Length;
                        item.Size = t <= 1 ? item.TargetSize : item.Size + deltaNorm*(t/item.DampingFactor);
                    }
                }
            }
        }

        private void updateLastVisibleIndex(int newIndex)
        {
            int elementsToShow = getElementsToShow();
            IEnumerable<HypothesisViewModel> hypos = _hypothesesViewModel.HypothesisViewModels;
            _lastVisibleIndex = newIndex;
            _lastVisibleIndex = Math.Min(hypos.Count() - 1, _lastVisibleIndex);
            _lastVisibleIndex = Math.Max(elementsToShow - 1, _lastVisibleIndex);
        }

        private int getElementsToShow()
        {
            IEnumerable<HypothesisViewModel> hypos = _hypothesesViewModel.HypothesisViewModels;
            int elementsToShow = (int)Math.Floor(this.ActualHeight / (HypothesisViewModel.DefaultHeight + GAP));
            elementsToShow = Math.Min(8, hypos.Count());
            return elementsToShow;
        }

        private void updateRendering()
        {
            IEnumerable<HypothesisViewModel> hypos = _hypothesesViewModel.HypothesisViewModels;

            int elementsToShow = getElementsToShow();

            double currentX = 0;
            double currentY = this.ActualHeight - (elementsToShow * HypothesisViewModel.DefaultHeight + (elementsToShow ) * GAP);
            int hiddenElementsTop = Math.Max(_lastVisibleIndex - elementsToShow + 1, 0);
            if (hiddenElementsTop > 0)
            {
                currentY = currentY - (hiddenElementsTop*HypothesisViewModel.DefaultHeight + (hiddenElementsTop - 1)*GAP);
            }

            var count = 0;
            foreach (var hypo in hypos)
            {
                if (count <= _lastVisibleIndex - elementsToShow)
                {
                   
                    if (hypo.IsDisplayed)
                    {
                        hypo.IsDisplayed = false;
                        toggleDisplayed(hypo);
                    }
                }
                else if (count > _lastVisibleIndex)
                {
                    if (hypo.IsDisplayed)
                    {
                        hypo.IsDisplayed = false;
                        toggleDisplayed(hypo);
                    }
                }
                else
                {
                    if (!hypo.IsDisplayed)
                    {
                        hypo.IsDisplayed = true;
                        toggleDisplayed(hypo);
                    }
                }
                hypo.TargetPosition = new Pt(currentX + this.ActualWidth - hypo.TargetSize.X, currentY);
                hypo.DeltaTargetPosition = new Pt(0, 0);
                currentY += GAP + hypo.Size.Y;
                count++;
            }

            foreach (var hypo in hypos)
            {
                
            }

            
           
            foreach (var hypo in hypos)
            {
                //hypo.TargetPosition = new Pt(currentX + this.ActualWidth - hypo.Size.X - GAP, currentY);
                //currentY += GAP + hypo.Size.Y;
                    
            }
        }                           

        private void HypothesesView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_hypothesesViewModel != null)
            {
                _hypothesesViewModel.HypothesisViewModels.CollectionChanged -= HypothesisViewModels_CollectionChanged;
            }
            if ((args.NewValue != null) && args.NewValue is HypothesesViewModel)
            {
                _hypothesesViewModel = (HypothesesViewModel) args.NewValue;
                _hypothesesViewModel.HypothesisViewModels.CollectionChanged += HypothesisViewModels_CollectionChanged;
            }
        }

        private void HypothesisViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var hypo = (HypothesisViewModel) item;
                    
                    if (_views.ContainsKey(hypo))
                    {
                        _contentCanvas.Children.Remove(_views[hypo]);
                        _views[hypo].Tapped += hypothesisView_Tapped;
                        _views.Remove(hypo);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var hypo = (HypothesisViewModel) item; 
                    hypo.Position = new Pt(this.ActualWidth, this.ActualHeight + hypo.Size.Y);
                    var v = new HypothesisView();
                    v.DataContext = hypo;
                    _views.Add(hypo, v);
                    _contentCanvas.Children.Add(v);
                    v.Opacity = 0;
                    v.Tapped += hypothesisView_Tapped;
                    hypo.IsDisplayed = true;
                    toggleDisplayed(hypo);
                }
            }

            _lastVisibleIndex = ((IEnumerable<HypothesisViewModel>) sender).Count() - 1;
            updateRendering();
        }

        private void hypothesisView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HypothesisView view = (HypothesisView) sender;
            var hypo = (HypothesisViewModel) view.DataContext;
            if (hypo.IsExpanded)
            {
                hypo.TargetSize = new Vec(HypothesisViewModel.DefaultHeight, HypothesisViewModel.DefaultHeight);
            }
            else
            {
                hypo.TargetSize = new Vec(HypothesisViewModel.ExpandedWidth, HypothesisViewModel.DefaultHeight);
            }
            hypo.IsExpanded = !hypo.IsExpanded;
            updateRendering();
        }
    }
}