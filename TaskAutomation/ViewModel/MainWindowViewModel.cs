using Automate4Me.Model;
using HeiswayiNrird.MVVM.Common;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Automate4Me.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private Members
        private int _counter;
        #endregion Private Members

        #region Constructor
        public MainWindowViewModel()
        {
            AppManager.Instance.Macro.OnCaptureMouseActionOnly += Instance_OnCaptureMouseActionOnly;
            AppManager.Instance.Macro.OnCaptureTextInputOnly += Instance_OnCaptureTextInputOnly;
            AppManager.Instance.Macro.OnStopCapturingAllEvents += Instance_OnStopCapturingAllEvents;

            AppManager.Instance.Automation.OnUpdateNumberOfTasksPerformed += Instance_OnUpdateNumberOfTasksPerformed;
            AppManager.Instance.Automation.OnAutomationTasksFinished += Automation_OnAutomationTasksFinished;
        }

        private void Automation_OnAutomationTasksFinished(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Instance_OnUpdateNumberOfTasksPerformed(object sender, string e)
        {
            if (AppManager.Instance.AutomationIsRunning)
            {
                string[] data = e.Split('|');
                BottomStatusText = string.Format("Action performed... {0}/{1}", data[0], data[1]);
            }
        }

        private void Instance_OnStopCapturingAllEvents(object sender, EventArgs e)
        {
            StopCaptureAllEvents();
        }

        private void Instance_OnCaptureTextInputOnly(object sender, string e)
        {
            if (!string.IsNullOrEmpty(e))
                AppManager.Instance.Settings.CurrentTextInput = e;

            AddTextInputToActionList();
        }

        private void Instance_OnCaptureMouseActionOnly(object sender, System.Windows.Forms.MouseButtons e)
        {
            if (AppManager.Instance.Macro.IsDoubleClick)
            {
                switch (e)
                {
                    case System.Windows.Forms.MouseButtons.Left:
                        AppManager.Instance.Settings.CurrentMouseAction = MouseActionType.DoubleClick;
                        break;
                    case System.Windows.Forms.MouseButtons.Right:
                    default:
                        AppManager.Instance.Settings.CurrentMouseAction = MouseActionType.Undefined;
                        break;
                }
            }
            else
            {
                switch (e)
                {
                    case System.Windows.Forms.MouseButtons.Left:
                        AppManager.Instance.Settings.CurrentMouseAction = MouseActionType.LeftClick;
                        break;
                    case System.Windows.Forms.MouseButtons.Right:
                        AppManager.Instance.Settings.CurrentMouseAction = MouseActionType.RightClick;
                        break;
                    default:
                        AppManager.Instance.Settings.CurrentMouseAction = MouseActionType.Undefined;
                        break;
                }
            }

            AppManager.Instance.Settings.CurrentMousePosition = AppManager.Instance.Macro.MousePosition;

            AddMouseEventToActionList();
        }
        #endregion Constructor

        #region Public Event Properties
        public event EventHandler OnActionListUpdated;
        public event EventHandler OnExitApp;
        #endregion Public Event Properties

        #region Global Settings Properties
        public int ActionDelayValue
        {
            get { return AppManager.Instance.Settings.CurrentActionDelay; }
            set
            {
                if (value > 0)
                    AppManager.Instance.Settings.CurrentActionDelay = value;
                else
                    AppManager.Instance.Settings.CurrentActionDelay = 2;
                RaisePropertyChanged(() => ActionDelayValue);
            }
        }
        public int RepeatValue
        {
            get { return AppManager.Instance.Settings.CurrentRepeatNumber; }
            set
            {
                if (value > 0)
                    AppManager.Instance.Settings.CurrentRepeatNumber = value;
                else
                    AppManager.Instance.Settings.CurrentRepeatNumber = 1;
                RaisePropertyChanged(() => RepeatValue);
            }
        }
        #endregion Global Settings Properties

        #region Action List Properties
        public ObservableCollection<OActionData> ActionListSource
        {
            get { return AppManager.Instance.Settings.CurrentActionList; }
            set
            {
                if (AppManager.Instance.Settings.CurrentActionList == null)
                    AppManager.Instance.Settings.CurrentActionList = new ObservableCollection<OActionData>();

                AppManager.Instance.Settings.CurrentActionList = value;
                RaisePropertyChanged(() => ActionListSource);
            }
        }

        private OActionData _SelectedAction;
        public OActionData SelectedAction
        {
            get { return _SelectedAction; }
            set { _SelectedAction = value; RaisePropertyChanged(() => SelectedAction); }
        }
        #endregion Action List Properties

        #region BottomStatus Properties
        private string _BottomStatusText;
        public string BottomStatusText
        {
            get { return _BottomStatusText; }
            set { _BottomStatusText = value; RaisePropertyChanged(() => BottomStatusText); }
        }
        #endregion BottomStatus Properties

        #region ICommand Properties
        private ICommand _StartCaptureMouseEventCommand;
        public ICommand StartCaptureMouseEventCommand
        {
            get
            {
                if (_StartCaptureMouseEventCommand == null)
                    _StartCaptureMouseEventCommand = new RelayCommand(param => StartCaptureMouseEvent());
                return _StartCaptureMouseEventCommand;
            }
        }

        private ICommand _CaptureTextInputCommand;
        public ICommand CaptureTextInputCommand
        {
            get
            {
                if (_CaptureTextInputCommand == null)
                    _CaptureTextInputCommand = new RelayCommand(param => CaptureTextInput());
                return _CaptureTextInputCommand;
            }
        }

        private ICommand _StartAutomationCommand;
        public ICommand StartAutomationCommand
        {
            get
            {
                if (_StartAutomationCommand == null)
                    _StartAutomationCommand = new RelayCommand(param => StartAutomation());
                return _StartAutomationCommand;
            }
        }

        private ICommand _StopAutomationCommand;
        public ICommand StopAutomationCommand
        {
            get
            {
                if (_StopAutomationCommand == null)
                    _StopAutomationCommand = new RelayCommand(param => StopAutomation());
                return _StopAutomationCommand;
            }
        }

        private ICommand _PauseResumeAutomationCommand;
        public ICommand PauseResumeAutomationCommand
        {
            get
            {
                if (_PauseResumeAutomationCommand == null)
                    _PauseResumeAutomationCommand = new RelayCommand(param => PauseResumeAutomation());
                return _PauseResumeAutomationCommand;
            }
        }

        private ICommand _ExitAppCommand;
        public ICommand ExitAppCommand
        {
            get
            {
                if (_ExitAppCommand == null)
                    _ExitAppCommand = new RelayCommand(param => ExitApp());
                return _ExitAppCommand;
            }
        }

        //private ICommand _StopCaptureAllEventsCommand;
        //public ICommand StopCaptureAllEventsCommand
        //{
        //    get
        //    {
        //        if (_StopCaptureAllEventsCommand == null)
        //            _StopCaptureAllEventsCommand = new RelayCommand(param => StopCaptureAllEvents());
        //        return _StopCaptureAllEventsCommand;
        //    }
        //}

        #endregion ICommand Properties

        #region ICommand Handlers
        private void StartCaptureMouseEvent()
        {
            if (WindowsInputWrapper.Instance.IsRunning)
                return;

            _counter = 0; // reset
            AppManager.Instance.Macro.Start();
            //TODO: Auto minimized to tray icon...

            BottomStatusText = "Capturing mouse event or text input...";
        }

        private void CaptureTextInput()
        {
            //Do nothing as pressing [F12] key automatically triggered by MKGlobalHookWrapper.
        }

        private void StartAutomation()
        {
            if (AppManager.Instance.Automation.IsRunning)
                AppManager.Instance.Automation.Stop();

            AppManager.Instance.Automation.Start(AppManager.Instance.Settings.CurrentActionList, AppManager.Instance.Settings.CurrentRepeatNumber);

            BottomStatusText = "Automation has started.";
        }

        private void StopAutomation()
        {
            if (AppManager.Instance.Automation.IsRunning)
                AppManager.Instance.Automation.Stop();

            BottomStatusText = "Automation has stopped.";
        }

        private void PauseResumeAutomation()
        {
            if (AppManager.Instance.Automation.IsRunning)
            {
                if (!AppManager.Instance.AutomationIsPaused)
                    AppManager.Instance.Automation.PauseAutomation();

                BottomStatusText = "Automation has paused.";
            }
            else
            {
                // resume
                if (AppManager.Instance.AutomationIsPaused)
                    AppManager.Instance.Automation.ResumeAutomation();

                BottomStatusText = "Automation has resumed.";
            }
        }

        private void ExitApp()
        {
            BottomStatusText = "Exiting application...";

            if (AppManager.Instance.Automation.IsRunning)
                AppManager.Instance.Automation.Stop();

            if (AppManager.Instance.Macro.CaptureInProgress)
                AppManager.Instance.Macro.Stop();

            if (OnExitApp != null)
                OnExitApp(this, EventArgs.Empty);
        }

        private void StopCaptureAllEvents()
        {
            //TODO: Reactive window to normal from tray icon...

            BottomStatusText = "Capturing mouse event or text input has stopped.";
        }
        #endregion ICommand Handlers

        #region Private Methods
        private void AddMouseEventToActionList()
        {
            if (AppManager.Instance.AutomationIsRunning)
                return;

            if (ActionListSource == null)
                ActionListSource = new ObservableCollection<OActionData>();

            _counter++;

            ActionListSource.Add(new OActionData
            {
                Id = _counter,
                ActionDelay = AppManager.Instance.Settings.CurrentActionDelay,
                MousePosition = AppManager.Instance.Settings.CurrentMousePosition,
                MouseAction = AppManager.Instance.Settings.CurrentMouseAction,
                IsTaskPerformed = false,
                TextInput = ""
            });

            if (OnActionListUpdated != null)
                OnActionListUpdated(this, EventArgs.Empty);

            BottomStatusText = "New mouse action added.";
        }

        private void AddTextInputToActionList()
        {
            if (AppManager.Instance.AutomationIsRunning)
                return;

            if (ActionListSource == null)
                ActionListSource = new ObservableCollection<OActionData>();

            _counter++;
            ActionListSource.Add(new OActionData
            {
                Id = _counter,
                ActionDelay = AppManager.Instance.Settings.CurrentActionDelay,
                MousePosition = AppManager.Instance.Settings.CurrentMousePosition,
                MouseAction = MouseActionType.Undefined,
                IsTaskPerformed = false,
                TextInput = AppManager.Instance.Settings.CurrentTextInput
            });

            if (OnActionListUpdated != null)
                OnActionListUpdated(this, EventArgs.Empty);

            BottomStatusText = "New text input added.";
        }

        #endregion Private Methods
    }
}