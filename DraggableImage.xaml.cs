using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BorderlessAlphaWin
{
    /// <summary>
    /// Interaction logic for DraggableImage.xaml
    /// </summary>
    public partial class SnippWindow : Window, INotifyPropertyChanged, DraggableViewModel.ICloseHandler
    {
        private Point startPoint;
        private Vector posDelta;
        private DraggableViewModel vm;
        private ICloseListener _closeListener;

        public SnippWindow()
        {
            InitializeComponent();
            vm = new DraggableViewModel(this);
            //if(DesignerProperties.GetIsInDesignMode(this))
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) 
            {
                vm.Tags.Add(new Tag() { tName = "TagTest" });
            }
            this.DataContext = vm;
            this.ShowActivated = true;
            this.WindowState = WindowState.Normal;
            //_closeListener = listener; 
        }

        public SnippWindow(ICloseListener listener)
        {
            InitializeComponent();
            vm = new DraggableViewModel(this);
            //if(DesignerProperties.GetIsInDesignMode(this))
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)) 
            {
                vm.Tags.Add(new Tag() { tName = "TagTest" });
            }
            this.DataContext = vm;
            this.ShowActivated = true;
            this.WindowState = WindowState.Normal;
            _closeListener = listener;
        }
        System.Windows.Media.Color BkgColor
        {
            get
            {
                return Color.FromRgb(255,0,255);
            }
        }

        private void StartDrag(object sender, MouseButtonEventArgs e)
        { 
            startPoint = e.GetPosition(root);
            this.DragMove();
        }

        private void UpdateDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            posDelta = e.GetPosition(root) - startPoint;
        }

        private void EndDrag(object sender, MouseButtonEventArgs e)
        {

        }

        private void root_Deactivated(object sender, EventArgs e)
        {
            Window win = (Window)sender;
            win.Topmost = true; 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void SetPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String name = null)
        {
            if(PropertyChanged != null)
            {
                if (name != null)
                    PropertyChanged(this,new PropertyChangedEventArgs(name));
            }
        }

        private void root_MouseEnter(object sender, MouseEventArgs e)
        {
            vm.Opacity = 1.0f;
            tagName.Focus();
        }

        private void root_MouseLeave(object sender, MouseEventArgs e)
        {
            vm.Opacity = vm.TargetOpacity;
        }

        private void root_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Math.Sign(e.Delta) > 0)
            {
                vm.Opacity += 0.1f;
            }
            else
                vm.Opacity += -0.1f;

            vm.TargetOpacity = vm.Opacity;
        }

        private void root_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void root_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine();
            _closeListener.onSnippCLosed(this);
        }

        public interface ICloseListener
        {
            void onSnippCLosed(SnippWindow win);
        } 
    }
}
