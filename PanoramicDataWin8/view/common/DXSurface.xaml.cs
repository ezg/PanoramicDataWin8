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
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using System.Diagnostics;
using Windows.Graphics.Display;

namespace PanoramicDataWin8.view.common
{
    public sealed partial class DXSurface : UserControl
    {
        private const D2D.DebugLevel D2DDebugLevel = D2D.DebugLevel.Information; //D2D.DebugLevel.Error

        // will hold all disposable resources to dispose them all at once
        private readonly DisposeCollector _disposeCollector = new DisposeCollector();

        // indicates if the frame needs to be redrawn
        private bool _isDirty;

        private GraphicsDevice _graphicsDevice; // encapsulates Direct3D11 Device and DeviceContext
        private GraphicsPresenter _presenter; // encapsulates the SwapChain, Backbuffer and DepthStencil buffer

        // ReSharper disable InconsistentNaming
        private D2D.Device _d2dDevice;
        private D2D.DeviceContext _d2dDeviceContext;
        // ReSharper restore InconsistentNaming
        private DW.Factory1 _dwFactory;

        // main render target (the backbuffer) - needs to be disposed before resizing the SwapChain otherwise the resize call will fail.
        private D2D.Bitmap1 _bitmapTarget;

        private DXSurfaceContentProvider _contentProvider = null;
        public DXSurfaceContentProvider ContentProvider
        {
            get
            {
                return _contentProvider;
            }
            set
            {
                _contentProvider = value;
                _contentProvider.Load(_d2dDeviceContext, _disposeCollector, _dwFactory);
            }
        }

        public DXSurface()
        {
            this.InitializeComponent();

            swapChainPanel.Loaded += HandleLoaded;
            swapChainPanel.Unloaded += HandleUnloaded;
            swapChainPanel.SizeChanged += HandleSizeChanged;
        }

        public void Dispose()
        {
            UnloadContent();
            _d2dDeviceContext = null;
            _d2dDevice = null;
            _dwFactory = null;
            _presenter = null;
            _graphicsDevice = null;
        }

        public void Redraw()
        {
            _isDirty = true;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            // create the device only on first load after that it can be reused
            if (_graphicsDevice == null)
                CreateDevice();

            StartRendering();
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            StopRendering();
        }

        private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException("e");

            ResetSize(e.NewSize);
        }

        private void HandleRendering(object sender, object e)
        {
            PerformRendering();
        }

        private void CreateDevice()
        {
            Debug.Assert(_graphicsDevice == null);

            var flags = GetDeviceCreationFlags();
            Debug.Assert(flags.HasFlag(DeviceCreationFlags.BgraSupport)); // mandatory for D2D support

            // initialize Direct3D11 - this is the bridge between Direct2D and control surface
            _graphicsDevice = _disposeCollector.Collect(GraphicsDevice.New(flags));

            // get the low-level DXGI device reference
            using (var dxgiDevice = ((Device)_graphicsDevice).QueryInterface<SharpDX.DXGI.Device>())
            {
                // create D2D device sharing same GPU driver instance
                _d2dDevice = _disposeCollector.Collect(new D2D.Device(dxgiDevice, new D2D.CreationProperties { DebugLevel = D2DDebugLevel }));

                // create D2D device context used in drawing and resource creation
                // this allows us to not recreate the resources if render target gets recreated because of size change
                _d2dDeviceContext = _disposeCollector.Collect(new D2D.DeviceContext(_d2dDevice, D2D.DeviceContextOptions.EnableMultithreadedOptimizations));
            }

            // device-independent factory used to create all DirectWrite resources
            _dwFactory = _disposeCollector.Collect(new SharpDX.DirectWrite.Factory1());
        }

        private DeviceCreationFlags GetDeviceCreationFlags()
        {
            return DeviceCreationFlags.BgraSupport;
            //#if DEBUG
            // | DeviceCreationFlags.Debug
            //#endif
            //;
        }

        private void UnloadContent()
        {
            _disposeCollector.DisposeAndClear();
        }

        private void StartRendering()
        {
            Debug.Assert(_presenter == null);
            Debug.Assert(_graphicsDevice.Presenter == null);
            Debug.Assert(_graphicsDevice != null);

            Redraw();

            var parameters = new PresentationParameters((int)ActualWidth, (int)ActualHeight, swapChainPanel);

            _presenter = _disposeCollector.Collect(new SwapChainGraphicsPresenter(_graphicsDevice, parameters));
            _graphicsDevice.Presenter = _presenter;

            Debug.Assert(_bitmapTarget == null);

            CreateD2DRenderTarget();

            CompositionTarget.Rendering += HandleRendering;
        }

        private void CreateD2DRenderTarget()
        {
            var renderTarget = _presenter.BackBuffer;

            var dpi = DisplayProperties.LogicalDpi;

            // 1. Use same format as the underlying render target with premultiplied alpha mode
            // 2. Use correct DPI
            // 3. Deny drawing direct calls and specify that this is a render target.
            var bitmapProperties = new D2D.BitmapProperties1(new SharpDX.Direct2D1.PixelFormat(renderTarget.Format, D2D.AlphaMode.Premultiplied),
                                                             dpi,
                                                             dpi,
                                                             D2D.BitmapOptions.CannotDraw | D2D.BitmapOptions.Target);

            // create the bitmap render target and assign it to the device context
            _bitmapTarget = _disposeCollector.Collect(new D2D.Bitmap1(_d2dDeviceContext, renderTarget, bitmapProperties));
            _d2dDeviceContext.Target = _bitmapTarget;
        }

        private void StopRendering()
        {
            Debug.Assert(_presenter != null);
            Debug.Assert(_graphicsDevice.Presenter != null);

            CompositionTarget.Rendering -= HandleRendering;

            DisposeD2DRenderTarget();

            _graphicsDevice.Presenter = null;
            _disposeCollector.RemoveAndDispose(ref _presenter);
        }

        private void DisposeD2DRenderTarget()
        {
            _d2dDeviceContext.Target = null;
            _disposeCollector.RemoveAndDispose(ref _bitmapTarget);
        }

        private void ResetSize(Size size)
        {
            if (_presenter == null) return;

            Redraw();

            DisposeD2DRenderTarget();
            _presenter.Resize((int)size.Width, (int)size.Height, _presenter.Description.BackBufferFormat);
            CreateD2DRenderTarget();
        }

        private void PerformRendering()
        {
            Debug.Assert(_graphicsDevice.Presenter != null);

            if (!_isDirty) return;
            _isDirty = false;

            if (!BeginDrawFrame())
            {
                Redraw();
                return;
            }

            _contentProvider.Clear(_graphicsDevice);

            DrawContent();

            EndDrawFrame();
        }

        private bool BeginDrawFrame()
        {
            switch (_graphicsDevice.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Normal:
                    // graphics device is okay
                    _graphicsDevice.ClearState();
                    if (_graphicsDevice.BackBuffer != null)
                    {
                        _graphicsDevice.SetRenderTargets(_graphicsDevice.DepthStencilBuffer, _graphicsDevice.BackBuffer);
                        _graphicsDevice.SetViewport(0, 0, _graphicsDevice.BackBuffer.Width, _graphicsDevice.BackBuffer.Height);
                    }

                    return true;

                default:
                    // graphics device needs to be recreated - give GPU driver some time to recover
                    Utilities.Sleep(TimeSpan.FromMilliseconds(20));

                    StopRendering();
                    Dispose();
                    CreateDevice();
                    if (_contentProvider != null)
                    {
                        _contentProvider.Load(_d2dDeviceContext, _disposeCollector, _dwFactory);
                    }
                    StartRendering();

                    return false;
            }
        }

        private void EndDrawFrame()
        {
            try
            {
                _graphicsDevice.Present();
            }
            catch (SharpDXException ex)
            {
                Debug.WriteLine(ex);
                if (ex.ResultCode != SharpDX.DXGI.ResultCode.DeviceRemoved && ex.ResultCode != SharpDX.DXGI.ResultCode.DeviceReset)
                    throw;
            }
        }

        private void DrawContent()
        {
            bool beginDrawCalled = false;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                _d2dDeviceContext.BeginDraw();
                beginDrawCalled = true;
                _contentProvider.Draw(_d2dDeviceContext, _dwFactory);
                Debug.WriteLine("Render time: " + sw.ElapsedMilliseconds);
            }
            finally
            {
                // end a draw batch
                if (beginDrawCalled)
                    _d2dDeviceContext.EndDraw();

            }
        }
    }

    public class DXSurfaceContentProvider
    {
        public virtual void Clear(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.White);
        }

        public virtual void Draw(D2D.DeviceContext d2dDeviceContext, DW.Factory1 dwFactory)
        {

        }
        public virtual void Load(D2D.DeviceContext d2dDeviceContext, DisposeCollector disposeCollector, DW.Factory1 dwFactory)
        {

        }
    }
}
