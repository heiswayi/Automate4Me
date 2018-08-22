using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Automate4Me.Model
{
    public class WindowsInputWrapper
    {
        #region Singleton

        private static readonly Lazy<WindowsInputWrapper> lazy = new Lazy<WindowsInputWrapper>(() => new WindowsInputWrapper());
        public static WindowsInputWrapper Instance { get { return lazy.Value; } }

        #endregion Singleton

        private WindowsInput.InputSimulator _simulator;
        private BackgroundWorker _worker;
        private Logger _logger;
        private ManualResetEvent _busy;

        #region Constructor
        private WindowsInputWrapper()
        {
            _simulator = new WindowsInput.InputSimulator();
            _logger = LogManager.GetCurrentClassLogger();
            _busy = new ManualResetEvent(false);
        }
        #endregion Constructor

        #region Properties
        public ObservableCollection<OActionData> ActionList { get; private set; }
        public int RepeatCount { get; private set; }
        public bool IsRunning { get; private set; }
        public int StatusCode { get; private set; }
        #endregion Properties

        #region Methods

        public void DelayAction(int seconds)
        {
            if (seconds < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < _desired)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public void DelayActionWithUIFrozen(int seconds)
        {
            if (seconds < 1) return;
            _simulator.Mouse.Sleep(seconds * 1000);
        }

        public void MoveMouseTo(System.Drawing.Point point)
        {
            double x = point.X;
            double y = point.Y;

            var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var outputX = x * 65535 / screenBounds.Width;
            var outputY = y * 65535 / screenBounds.Height;

            _simulator.Mouse.MoveMouseTo(outputX, outputY);
        }

        public void MoveMouseTo(double x, double y)
        {
            var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var outputX = x * 65535 / screenBounds.Width;
            var outputY = y * 65535 / screenBounds.Height;

            _simulator.Mouse.MoveMouseTo(outputX, outputY);
        }

        public void MouseClickAction(MouseActionType clickType)
        {
            switch (clickType)
            {
                case MouseActionType.LeftClick:
                    _simulator.Mouse.LeftButtonClick();
                    break;

                case MouseActionType.RightClick:
                    _simulator.Mouse.RightButtonClick();
                    break;

                case MouseActionType.DoubleClick:
                    _simulator.Mouse.LeftButtonDoubleClick();
                    break;
                default:
                    throw new ArgumentException("Invalid MouseActionType");
            }
        }

        public void InsertTextInput(string textInput)
        {
            _simulator.Keyboard.TextEntry(textInput);
        }

        public void Start(ObservableCollection<OActionData> actionList, int repeat = 1)
        {
            if (ActionList == null)
                ActionList = new ObservableCollection<OActionData>();

            ActionList = actionList;
            RepeatCount = repeat;

            MoveMouseTo(0, 0);

            _worker = new BackgroundWorker();
            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += _worker_ProgressChanged;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            if (!_worker.IsBusy)
            {
                _busy.Set();
                _worker.RunWorkerAsync();
                StatusCode = 1;
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Automation tasks finished, send signal back to UI
            if (OnAutomationTasksFinished != null)
                OnAutomationTasksFinished(this, EventArgs.Empty);
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //TODO: Update number of actions performed per total...
            int performed = ActionList.Where(x => x.IsTaskPerformed == true).Count();
            int total = ActionList.Count();

            if (OnUpdateNumberOfTasksPerformed != null)
                OnUpdateNumberOfTasksPerformed(this, string.Format("{0}|{1}", performed, total));
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (RepeatCount < 1 || RepeatCount > int.MaxValue)
                return;

            int i = 1;
            while (i <= RepeatCount)
            {
                try
                {
                    foreach (var action in ActionList)
                    {
                        _busy.WaitOne();
                        if (_worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (action.TextInputString.Length > 0 && action.MouseActionString == "")
                        {
                            // text input
                            DelayAction(action.ActionDelay);
                            //MoveMouseTo(action.MousePosition);
                            RealisticMouseMoveTo(action.MousePosition);
                            MouseClickAction(MouseActionType.LeftClick);
                            Thread.Sleep(10);
                            InsertTextInput(action.TextInput);
                        }
                        else if (action.TextInputString == "" && action.MouseActionString != "")
                        {
                            // mouse event only
                            DelayAction(action.ActionDelay);
                            //MoveMouseTo(action.MousePosition);
                            RealisticMouseMoveTo(action.MousePosition);
                            MouseClickAction(action.MouseAction);
                        }

                        if (action.IsTaskPerformed == false) action.IsTaskPerformed = true;

                        _worker.ReportProgress(0);
                        Thread.Sleep(1);
                    }

                    i++;
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    //TODO: Handle exception...
                }
            }
        }

        public void Stop()
        {
            if (_worker.IsBusy)
            {
                _busy.Set();
                _worker.CancelAsync();
                StatusCode = 0;
            }
        }

        public void PauseAutomation()
        {
            _busy.Reset();
            StatusCode = 2;
        }

        public void ResumeAutomation()
        {
            if (!_worker.IsBusy)
            {
                _busy.Set();
                _worker.RunWorkerAsync();
                StatusCode = 3;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out System.Drawing.Point p);

        public void RealisticMouseMoveTo(System.Drawing.Point newPosition, int steps = 100)
        {
            System.Drawing.Point start = new System.Drawing.Point();
            GetCursorPos(out start);
            System.Drawing.PointF iterPoint = start;

            // Find the slope of the line segment defined by start and newPosition
            System.Drawing.PointF slope = new System.Drawing.PointF(newPosition.X - start.X, newPosition.Y - start.Y);

            // Divide by the number of steps
            slope.X = slope.X / steps;
            slope.Y = slope.Y / steps;

            // Move the mouse to each iterative point.
            for (int i = 0; i < steps; i++)
            {
                iterPoint = new System.Drawing.PointF(iterPoint.X + slope.X, iterPoint.Y + slope.Y);
                SetCursorPos(System.Drawing.Point.Round(iterPoint).X, System.Drawing.Point.Round(iterPoint).Y);
                Thread.Sleep(5);
            }

            // Move the mouse to the final destination.
            SetCursorPos(newPosition.X, newPosition.Y);
        }

        #endregion Methods

        #region Public events
        public event EventHandler OnAutomationTasksFinished;
        public event EventHandler<string> OnUpdateNumberOfTasksPerformed;
        #endregion Public events
    }
}