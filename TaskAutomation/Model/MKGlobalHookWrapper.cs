using MKGlobalHook.MouseKeyboard;
using System;
using System.Windows.Forms;

namespace Automate4Me.Model
{
    public class MKGlobalHookWrapper
    {
        #region Singleton
        private static readonly Lazy<MKGlobalHookWrapper> lazy = new Lazy<MKGlobalHookWrapper>(() => new MKGlobalHookWrapper());
        public static MKGlobalHookWrapper Instance { get { return lazy.Value; } }
        #endregion Singleton

        #region Private members
        private IKeyboardMouseEvents _mkEvents;
        private System.Drawing.Point _cacheMousePosition;
        private MouseButtons _cacheMouseButton;
        private string _cacheTextInput;
        #endregion Private members

        #region Constructor
        private MKGlobalHookWrapper()
        {
        }
        #endregion Constructor

        #region Public properties
        public bool CaptureInProgress { get; set; }
        public string TextInput { get; set; }
        public System.Drawing.Point MousePosition { get; set; }
        public bool IsDoubleClick { get; set; }
        #endregion Public properties

        #region Events subscription methods
        public void Subscribe(IKeyboardMouseEvents events)
        {
            CaptureInProgress = true;

            _mkEvents = events;

            _mkEvents.KeyDown += OnKeyDown;
            _mkEvents.KeyUp += OnKeyUp;
            _mkEvents.KeyPress += OnKeyPressed;
            _mkEvents.MouseClick += OnMouseClick;
            _mkEvents.MouseDoubleClick += OnMouseDoubleClick;
            _mkEvents.MouseDown += OnMouseDown;
            _mkEvents.MouseMove += OnMouseMove;
            _mkEvents.MouseUp += OnMouseUp;
        }

        public void Unsubscribe()
        {
            if (_mkEvents == null) return;

            _mkEvents.KeyDown -= OnKeyDown;
            _mkEvents.KeyUp -= OnKeyUp;
            _mkEvents.KeyPress -= OnKeyPressed;
            _mkEvents.MouseClick -= OnMouseClick;
            _mkEvents.MouseDoubleClick -= OnMouseDoubleClick;
            _mkEvents.MouseDown -= OnMouseDown;
            _mkEvents.MouseMove -= OnMouseMove;
            _mkEvents.MouseUp -= OnMouseUp;

            _mkEvents.Dispose();
            _mkEvents = null;

            CaptureInProgress = false;
        }

        public void SubscribeApplication()
        {
            Unsubscribe();
            Subscribe(Hook.AppEvents());
        }

        public void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        public void Start(bool isGlobal = true)
        {
            if (isGlobal)
                SubscribeGlobal();
            else
                SubscribeApplication();
        }

        public void Stop()
        {
            Unsubscribe();

            if (OnStopCapturingAllEvents != null)
                OnStopCapturingAllEvents(this, EventArgs.Empty);
        }
        #endregion Events subscription methods

        #region Event handlers
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (CaptureInProgress)
            {
                _cacheMousePosition = new System.Drawing.Point(e.X, e.Y);
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CaptureInProgress)
            {
                CaptureMouseActionOnly(e, true);
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (CaptureInProgress)
            {
                CaptureMouseActionOnly(e);
            }
        }

        private void OnKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (CaptureInProgress)
            {
                _cacheTextInput += e.KeyChar.ToString();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (CaptureInProgress)
            {
                if (e.KeyCode == Keys.F12)
                    CaptureTextInputOnly();
                else if (e.KeyCode == Keys.F2)
                    Stop();
            }
        }
        #endregion Event handlers

        #region Event properties
        public event EventHandler<MouseButtons> OnCaptureMouseActionOnly;
        public event EventHandler<string> OnCaptureTextInputOnly;
        public event EventHandler OnStopCapturingAllEvents;
        #endregion Event properties

        #region Private methods
        private void CaptureMouseActionOnly(MouseEventArgs e, bool isDoubleClick = false)
        {
            MousePosition = _cacheMousePosition;
            IsDoubleClick = isDoubleClick;

            if (OnCaptureMouseActionOnly != null)
                OnCaptureMouseActionOnly(this, e.Button);
        }

        private void CaptureTextInputOnly()
        {
            TextInput = _cacheTextInput;
            _cacheTextInput = string.Empty;

            if (OnCaptureTextInputOnly != null)
                OnCaptureTextInputOnly(this, TextInput);
        }
        #endregion Private methods
    }
}