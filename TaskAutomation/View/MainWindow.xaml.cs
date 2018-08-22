using Automate4Me.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Automate4Me
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();
            this.DataContext = vm;

            // Event to make ListBox scroll to bottom when new item is added
            vm.OnActionListUpdated += (obj, e) =>
            {
                // column resize
                foreach (GridViewColumn c in gvActionList.Columns)
                {
                    // Code below was found in GridViewColumnHeader.OnGripperDoubleClicked() event handler (using Reflector)
                    // i.e. it is the same code that is executed when the gripper is double clicked
                    // if (adjustAllColumns || App.StaticGabeLib.FieldDefsGrid[colNum].DispGrid)
                    if (double.IsNaN(c.Width))
                    {
                        c.Width = c.ActualWidth;
                    }
                    c.Width = double.NaN;
                }

                // scroll to bottom
                var listview = this.lvActionList;
                if (listview == null) throw new ArgumentNullException("listview", "Argument listview cannot be null");
                if (!listview.IsInitialized) throw new InvalidOperationException("ListView is in an invalid state: IsInitialized == false");

                if (listview.Items.Count == 0)
                    return;

                listview.ScrollIntoView(listview.Items[listview.Items.Count - 1]);
            };
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}