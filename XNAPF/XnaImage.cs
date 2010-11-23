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

namespace XNAPF
{
    public class XnaImage<T> : System.Windows.Controls.Image, INotifyPropertyChanged
        where T : Game, new()
    {
        #region Fields

        #region Static, Const

        private const int FPSTIME = 1000 / 50;

        #endregion

        private static bool? _isInDesignMode;

        private bool m_running = true;
        protected D3DImage m_source;
        protected BackgroundWorker m_back;

        protected GraphicsDeviceManager m_manager;

        protected RenderTarget2D m_current;
        private RenderTarget2D m_target;

        private Int32Rect m_rect;

        private static FieldInfo m_isActive;
        private static MethodInfo m_initialize;

        private bool m_drawing = false;

        protected int m_width;
        protected int m_height;

        #endregion

        #region Property


        public T Game { get; private set; }

        #endregion

        #region Event

        public event PropertyChangedEventHandler PropertyChanged;

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
        }

        #endregion

        #region Method

        private void InitializeNewGame()
        {
            #region Game

            Game = new T();
            m_manager = Game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
            if (m_manager != null)
            {
                m_manager.IsFullScreen = false;
                m_manager.ApplyChanges();
            }

            m_width = Game.GraphicsDevice.Viewport.Width;
            m_height = Game.GraphicsDevice.Viewport.Height;

            m_target = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height);
            m_current = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height);

            Game.Exiting += (e, a) =>
            {
                m_running = false;
            };

            #endregion

            #region Img

            m_rect = new Int32Rect(m_target.Bounds.X, m_target.Bounds.Y, m_target.Bounds.Width, m_target.Bounds.Height);
            m_source = new D3DImage();

            #endregion

            #region Size

            this.SizeChanged += (e, a) =>
            {
                a.Handled = true;
                m_width = (int)a.NewSize.Width;
                m_height = (int)a.NewSize.Height;
                while (!m_drawing)
                    Thread.Sleep(1);
                ResizeGame(m_width, m_height);
                m_target = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                m_current = new RenderTarget2D(Game.GraphicsDevice, m_width, m_height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                m_rect = new Int32Rect(m_target.Bounds.X, m_target.Bounds.Y, m_target.Bounds.Width, m_target.Bounds.Height);
            };

            #endregion

            #region Background

            m_back = new BackgroundWorker();
            m_back.WorkerReportsProgress = true;

            Game.Window.AllowUserResizing = true;

            m_source.Changed += (p, u) =>
            {
                m_current = m_target;
                m_drawing = false;
            };

            #region DoWorkEnded

            #endregion

            #region DoWork

            m_back.DoWork += (e, a) =>
            {
                m_initialize.Invoke(this.Game, new object[] { });

                DateTime _save = DateTime.Now;
                while (m_running)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _TimeDiff = (_now - _save);
                    if (_TimeDiff.TotalMilliseconds < FPSTIME)
                    {
                        Thread.Sleep((int)(FPSTIME - _TimeDiff.TotalMilliseconds));
                        continue;
                    }

                    _save = _now;

                    if (m_drawing)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    Game.GraphicsDevice.SetRenderTarget(m_current);

                    if (!Game.IsActive)
                        m_isActive.SetValue(Game, true);

                    Game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                    Game.Tick();

                    Game.GraphicsDevice.SetRenderTarget(null);

                    m_drawing = true;
                    m_target = m_current;
                    m_back.ReportProgress(0);
                }

                Game.Exit();
            };

            #endregion

            #region ProgressChanged

            m_back.ProgressChanged += (e, a) =>
            {
                unsafe
                {
                    m_source.Lock();

                    m_source.SetBackBuffer(D3DResourceType.IDirect3DSurface9, m_target.GetPtr());
                    m_source.AddDirtyRect(m_rect);

                    m_source.Unlock();
                }

                this.Source = m_source;
            };

            #endregion

            m_back.RunWorkerAsync();

            #endregion
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

        public virtual void ResizeGame(int _newWidth, int _newHeitgh)
        {
            if (Game is IXnapfGameResizeable)
                (Game as IXnapfGameResizeable).Resize(_newWidth, _newHeitgh);
        }
        #endregion
    }
}
