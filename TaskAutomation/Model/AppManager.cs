using System;
using System.Collections.ObjectModel;

namespace Automate4Me.Model
{
    public class AppManager
    {
        #region Singleton

        private static readonly Lazy<AppManager> lazy = new Lazy<AppManager>(() => new AppManager());
        public static AppManager Instance { get { return lazy.Value; } }

        private AppManager()
        {
            Automation = WindowsInputWrapper.Instance;
            Macro = MKGlobalHookWrapper.Instance;
            Settings = new CurrentSettings();

            DefaultValues();
            HookupEvents();
        }

        #endregion Singleton

        #region Properties
        public WindowsInputWrapper Automation { get; private set; }
        public MKGlobalHookWrapper Macro { get; private set; }
        public CurrentSettings Settings { get; private set; }

        public bool AutomationIsRunning
        {
            get { return Automation.IsRunning; }
        }
        public bool AutomationIsPaused
        {
            get
            {
                switch (Automation.StatusCode)
                {
                    case 0:
                    case 1:
                    case 3:
                        return false;
                    case 2:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public class CurrentSettings
        {
            public ObservableCollection<OActionData> CurrentActionList { get; set; }
            public int CurrentActionDelay { get; set; }
            public int CurrentRepeatNumber { get; set; }
            public MouseActionType CurrentMouseAction { get; set; }
            public System.Drawing.Point CurrentMousePosition { get; set; }
            public string CurrentTextInput { get; set; }
        }

        #endregion Properties

        private void DefaultValues()
        {
            Settings.CurrentActionList = null;
            Settings.CurrentActionDelay = 2;
            Settings.CurrentRepeatNumber = 1;
        }

        private void HookupEvents()
        {
        }
    }
}