namespace Automate4Me.Model
{
    public class OActionData
    {
        public int Id { get; set; }
        public int ActionDelay { get; set; }
        public System.Drawing.Point MousePosition { get; set; }
        public MouseActionType MouseAction { get; set; }
        public bool IsTaskPerformed { get; set; }
        public string TextInput { get; set; }
        public string ActionNumberString { get { return Id.ToString(); } }
        public string ActionDelayString { get { return ActionDelay.ToString(); } }
        public string MousePositionString
        {
            get
            {
                if (TextInput.Length == 0)
                    return string.Format("({0},{1})", MousePosition.X, MousePosition.Y);
                else
                    return "-";
            }
        }
        public string MouseActionString
        {
            get
            {
                switch (MouseAction)
                {
                    case MouseActionType.LeftClick:
                        return "LEFTCLICK";
                    case MouseActionType.RightClick:
                        return "RIGHTCLICK";
                    case MouseActionType.DoubleClick:
                        return "DOUBLECLICK";
                    default:
                        return "-";
                }
            }
        }
        public string TextInputString
        {
            get
            {
                return TextInput;
            }
        }
    }

    public enum MouseActionType
    {
        LeftClick,
        RightClick,
        DoubleClick,
        Undefined
    }
}