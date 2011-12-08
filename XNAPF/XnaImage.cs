using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Interop;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Threading;
using System.Windows.Input;
using XNAPF.Tools;
using System.Windows.Media;

namespace XNAPF
{
    public class XnaImage<T> : System.Windows.Controls.Image, INotifyPropertyChanged
        where T : Game, new()
    {
        #region Fields

        private static DependencyProperty dp_lock = DependencyProperty.Register("IsLock", typeof(bool), typeof(XnaImage<T>), new PropertyMetadata(true, IsLockPropertyChangedCallback));

        #region Static, Const

        private const int FPSTIME = 1000 / 240;

        #endregion

        private RasterizerState m_rasterisation;
        private static bool? _isInDesignMode;

        private bool m_running = true;
        private bool m_isLock = true;

        protected D3DImage m_source;
        protected BackgroundWorker m_back;

        protected GraphicsDeviceManager m_manager;

        protected RenderTarget2D m_current;
        private RenderTarget2D m_target;

        private TimeSpan m_lastRender;

        private Int32Rect m_rect;

        private static FieldInfo m_isActive;
        private static MethodInfo m_initialize;

        private bool m_resizing = false;
        private object m_locker = new object();

        private bool m_isExited = false;
        private bool m_drawing = false;

        protected int m_width;
        protected int m_height;

        #endregion

        #region DependencyProperty

        #endregion

        #region Property

        public bool IsLock
        {
            get { return (bool)GetValue(dp_lock); }
            set
            {
                SetValue(dp_lock, value);
                m_isLock = value;
            }
        }

        public T Game { get; private set; }
        public ICommand Start { get; private set; }

        #endregion

        #region Event

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler GameLoaded;

        #endregion

        #region CTOR

        static XnaImage()
        {
            m_isActive = typeof(Game).GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);
            m_initialize = typeof(Game).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public XnaImage()
        {
            if (IsInDesignMode)
            {
                base.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(XNAPF.Resources.logoXna.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                base.Stretch = System.Windows.Media.Stretch.Fill;
                return;
            }
            InitializeNewGame();

            Start = new CActionCommand() { Action = () => { m_back.RunWorkerAsync(); } };

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (m_target == null)
                return;

            RenderingEventArgs args = (RenderingEventArgs)e;

            if ((args.RenderingTime - m_lastRender).TotalSeconds * 1000 < FPSTIME)
                return;

            if (this.m_source.IsFrontBufferAvailable && args.RenderingTime != m_lastRender)
            {
                lock (m_locker)
                {
                    this.m_source.Lock();
                    unsafe
                    {
                        //byte[] _data = new byte[4 * m_current.Width * m_current.Height];
                        //m_current.GetData<byte>(_data);
                        //m_target.SetData<byte>(_data);
                        if (!m_target.IsContentLost && !m_target.IsDisposed)
                            m_source.SetBackBuffer(D3DResourceType.IDirect3DSurface9, m_target.GetPtr());
                        m_source.AddDirtyRect(m_rect);
                    }
                    this.m_source.Unlock();
                }

                m_lastRender = args.RenderingTime;
            }
        }

        #endregion

        #region Method

        private void InitializeNewGame()
        {
            #region Game
            #endregion

            #region Img

            m_source = new D3DImage();
            this.Source = m_source;

            m_source.IsFrontBufferAvailableChanged += new DependencyPropertyChangedEventHandler(m_source_IsFrontBufferAvailableChanged);

            #endregion

            #region Size

            this.SizeChanged += (e, a) =>
            {
                a.Handled = true;
                m_width = (int)a.NewSize.Width;
                m_height = (int)a.NewSize.Height;
                m_resizing = true;

                ////while (!m_drawing)
                ////    Thread.Sleep(1);
                ////ResizeGame(m_width, m_height);
                ////if (Game is IXnapfGameResizeable)
                ////    (Game as IXnapfGameResizeable).Resize(m_width, m_height);
                //m_target = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                ////m_current = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                //m_rect = new Int32Rect(m_target.Bounds.X, m_target.Bounds.Y, m_target.Bounds.Width, m_target.Bounds.Height);
            };

            #endregion

            #region Background

            m_back = new BackgroundWorker();
            m_back.WorkerReportsProgress = true;

            #region DoWorkEnded

            #endregion

            #region DoWork

            m_back.DoWork += (e, a) =>
            {
                Game = new T();
                m_manager = Game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
                if (m_manager != null)
                {
                    m_manager.IsFullScreen = false;
                    m_manager.ApplyChanges();
                }

                //Game.Window.AllowUserResizing = true;

                m_rasterisation = Game.GraphicsDevice.RasterizerState;

                m_width = Game.GraphicsDevice.Viewport.Width;
                m_height = Game.GraphicsDevice.Viewport.Height;

                ProduceTarget(Game.GraphicsDevice, m_width, m_height);

                m_rect = new Int32Rect(m_target.Bounds.X, m_target.Bounds.Y, m_target.Bounds.Width, m_target.Bounds.Height);

                m_initialize.Invoke(this.Game, new object[] { });

                Dispatcher.Invoke(new Action(() =>
                    {
                        m_source_IsFrontBufferAvailableChanged(null, new DependencyPropertyChangedEventArgs());

                        if (GameLoaded != null)
                            GameLoaded.Invoke(this, EventArgs.Empty);
                    }));

                DateTime _save = DateTime.Now;
                while (m_running && this.Dispatcher.Thread.IsAlive)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _TimeDiff = (_now - _save);
                    if (_TimeDiff.TotalMilliseconds < FPSTIME)
                    {
                        Thread.Sleep((int)(FPSTIME - _TimeDiff.TotalMilliseconds));
                        continue;
                    }

                    _save = _now;

                    if (!m_isLock)
                        continue;

                    if (m_resizing)
                    {
                        //while (!m_drawing)
                        //    Thread.Sleep(1);
                        //ResizeGame(m_width, m_height);
                        //if (Game is IXnapfGameResizeable)
                        //    (Game as IXnapfGameResizeable).Resize(m_width, m_height);
                        //m_resizing = false;
                        //ResizeGame(m_width, m_height);
                        //m_drawing = false;
                        ////Game.Window.ClientSizeChanged += (x, y) =>
                        ////        {
                        //lock (m_locker)
                        //{
                        //    m_target = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height, false,
                        //                                  SurfaceFormat.Color, DepthFormat.Depth24, m_target.MultiSampleCount, RenderTargetUsage.PreserveContents);
                        //    m_rect = new Int32Rect(m_target.Bounds.X, m_target.Bounds.Y, m_target.Bounds.Width, m_target.Bounds.Height);
                        //}
                        //m_drawing = true;
                        //};
                        //m_current = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                    }

                    //if (!m_drawing)
                    //    continue;

                    lock (m_locker)
                    {
                        if (m_target.IsContentLost || m_target.IsDisposed)
                            ProduceTarget(Game.GraphicsDevice, m_width, m_height);
                        Game.GraphicsDevice.SetRenderTarget(m_target);

                        if (!Game.IsActive)
                            m_isActive.SetValue(Game, true);

                        Game.Tick();
                        //Game.GraphicsDevice.SetRenderTarget(null);
                    }

                }

                if (!m_isExited)
                    Game.Exit();
            };

            GameLoaded += (e, a) =>
                {
                    Game.Exiting += (obj, args) =>
                    {
                        m_running = false;
                        m_isExited = true;
                    };
                };

            #endregion

            #region ProgressChanged
            #endregion

            m_back.RunWorkerAsync();

            #endregion
        }

        void m_source_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_source.IsFrontBufferAvailable)
            {
                //lock (m_drawing)
                //{
                unsafe
                {
                    m_source.Lock();
                    lock (m_locker)
                    {
                        if (!m_target.IsContentLost && !m_target.IsDisposed)
                            m_source.SetBackBuffer(D3DResourceType.IDirect3DSurface9, m_target.GetPtr());
                    }
                    m_source.Unlock();
                }
                m_drawing = true;
                //}
            }
            else
                m_drawing = false;
        }

        private void ProduceTarget(GraphicsDevice device, int width, int height)
        {
            if (m_target != null && m_target.IsContentLost)
            {
                m_target.Dispose();
                var _tmp = Game.GraphicsDevice.PresentationParameters.Clone();
                ;
                Game.GraphicsDevice.Reset(_tmp, Game.GraphicsDevice.Adapter);
            }
            m_target = new RenderTarget2D(device, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            //m_current = new RenderTarget2D(device, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            //m_target.ContentLost += (e, a) => { ProduceTarget(device, width, height); };
        }

        protected void ClosedGame()
        {
            m_running = false;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
                    _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement))
                                      .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }

        private static void IsLockPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as XnaImage<T>).m_isLock = (bool)e.NewValue;
        }

        public virtual void ResizeGame(int _newWidth, int _newHeitgh)
        {
            if (Game is IXnapfGameResizeable)
                (Game as IXnapfGameResizeable).Resize(_newWidth, _newHeitgh);
        }

        #endregion
    }
}
