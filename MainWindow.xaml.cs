using MVVMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace BorderlessAlphaWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged, SnippWindow.ICloseListener
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _windows = new List<Window>();
            _searchResult = new List<Window>();

            if(File.Exists(SnippWriter.HistoryFile))
            {
                var text = File.ReadAllText(SnippWriter.HistoryFile);
                var jobj = JArray.Parse(text);

                foreach(var item in jobj)
                {
                    SnippWindow win = createWindowFromJSON(item);
                    win.Show();
                    win.Focus();
                    _windows.Add(win);
                    Console.WriteLine();
                }
            }
        }

        private List<SnippWindow> GetWindowsWithTag(String tag)
        {
            var res = new List<SnippWindow>();

            if (File.Exists(SnippWriter.RepoFile))
            {
                var text = File.ReadAllText(SnippWriter.RepoFile);
                var jobj = JArray.Parse(text);
                foreach (var win in jobj)
                {
                    var Tags = JsonConvert.DeserializeObject<ObservableCollection<Tag>>(win["Tags"].ToString());
                    var matchingTags = Tags.Where(x => x.tName.ToLower().Contains(tag.ToLower()));
                    if (matchingTags.Any())
                    {
                        res.Add(createWindowFromJSON(win));
                    }
                }
            }

            return res;
        }

        private SnippWindow createWindowFromJSON(JToken item)
        {
            var win = new SnippWindow(this);
            var b64 = item["Image64"].ToString();
            var bytes = Convert.FromBase64String(b64);
            var vm = (win.DataContext as DraggableViewModel);

            System.Drawing.Bitmap bmp;
            BitmapImage img = new BitmapImage();
            using (var stream = new MemoryStream(bytes))
            {
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = stream;
                img.EndInit();
            }

            vm.IsUnsaved =      false;
            vm.Opacity =        float.Parse(item["Opacity"].ToString());
            vm.ImageSource =    item["ImageSource"].ToString();
            vm.PosTop =         Int32.Parse(item["PosTop"].ToString());
            vm.Image64 =        b64;
            vm.PosLeft =        Int32.Parse(item["PosLeft"].ToString());
            vm.Tags =           JsonConvert.DeserializeObject<ObservableCollection<Tag>>(item["Tags"].ToString());
            win.imgSrc.Source = img;
            win.Width =         img.Width;
            win.Height =        img.Height;
            vm.UniqueIdentifier = b64.GetHashCode();
            return win;
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk
            );

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id
            );

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        private bool wasHotkeyed;

        private System.Windows.Shapes.Rectangle rect;
        private Point startPoint;
        private BitmapImage _imgSrc;
        private int _counter;
        private List<Window> _windows;
        private List<Window> _searchResult;
        private string _searchText;
        private bool _searching;

        public event PropertyChangedEventHandler PropertyChanged;
        public void SetPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String name = null)
        {
            if(PropertyChanged != null)
            {
                if (name != null)
                    PropertyChanged(this,new PropertyChangedEventArgs(name));
            }
        }

        public bool IsSearching
        {
            get
                //do this shit
            {
                return _searching;
            }
            set
            {
                _searching = value;
                SetPropertyChanged();
            }
        }

        public String SearchField
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                if (!String.IsNullOrWhiteSpace(value))
                {
                    foreach (var win in _searchResult)
                    {
                        win.Close();
                        _windows.Remove(win);
                    }

                    var wins = GetWindowsWithTag(value);
                    foreach (var win in wins)
                    {
                        win.Show();
                        _searchResult.Add(win);
                    }

                    TagSearch.Focus();
                    _windows.AddRange(wins);
                }
                else
                {
                    foreach (var win in _searchResult)
                        win.Close();
                }
                SetPropertyChanged();
            }
        }

        public System.Drawing.Size RectSize {
            get
            {
                var p = System.Drawing.Size.Empty; 

                p.Width = (int)rect.Width;
                p.Height = (int)rect.Height;

                return p;
            }
        }

        public System.Drawing.Point RectStart {

            get
            {
                var p = this.PointToScreen( new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect)) );
                return new System.Drawing.Point((int)p.X, (int)p.Y);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint VK_F10 = 0x79;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            // do stuff 
            if (this.WindowState.Equals(WindowState.Minimized))
            {
                this.Activate();
                this.WindowState = WindowState.Normal;
                wasHotkeyed = true;
                TagSearch.Focus();
            }
            else
                this.WindowState = WindowState.Minimized;

            Console.WriteLine("SHIT WAS PRESSED");
        } 
 
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.LeftCtrl && wasHotkeyed)
            {
                if(WindowState == WindowState.Normal)
                {
                //    this.WindowState = WindowState.Minimized;
                }
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window win = (Window)sender;
            win.Topmost = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(ctxCanvas);
            if(rect != null)
            {
                ctxCanvas.Children.Remove(rect);
            }

            rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 2;

            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.Y);
            ctxCanvas.Children.Add(rect); 
        }

        public void ToggleWindow()
        { 
            foreach(var win in Application.Current.Windows.OfType<MainWindow>())
            {
                if (win.WindowState.Equals(WindowState.Minimized))
                {
                    win.Activate();
                    win.WindowState = WindowState.Normal;
                    wasHotkeyed = true;
                }
                else
                    win.WindowState = WindowState.Minimized; 
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (rect == null)
                return;

            if(rect.Width * rect.Height > 4)
            {
                //imgCapture.Source = null;
                var dt = new System.Windows.Threading.DispatcherTimer();
                //ToggleWindow();
                var x = RectStart.X;
                var y = RectStart.Y;
                var w = RectSize.Width;
                var h = RectSize.Height;

                var location = BitmapLocation;

                ToggleWindow(); 
                Task.Delay(5).ContinueWith(task =>
                {
                    DoTakeScreenShot(x,y,w,h,location); // Maybe set a timer to minimize and reactivate window for a few millisecs
                    _counter++;

               }).Wait();

                //TODO Clean shit up
                var oldOp = this.Opacity;
                this.Opacity = 0.0f;
                ToggleWindow();
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(location);
                img.EndInit();
                ctxCanvas.Children.Remove(rect);
                rect = null;

                var win = new SnippWindow(this);
                win.imgSrc.Source = img;
                win.Width = img.Width;
                win.Height = img.Height;
                win.Show();
                win.Focus();
                ((DraggableViewModel)win.DataContext).BitmapImage = img;
                ((DraggableViewModel)win.DataContext).Image64 = BitmapImageToBase64(img);
                ((DraggableViewModel)win.DataContext).UniqueIdentifier = ((DraggableViewModel)win.DataContext).Image64.GetHashCode();
                ((DraggableViewModel)win.DataContext).ImageSource = location;
                ((DraggableViewModel)win.DataContext).ImgHeight = img.Height;
                ((DraggableViewModel)win.DataContext).ImgWidth = img.Width;
                win.tagName.Focus();
                _windows.Add(win); 
                ToggleWindow();
                this.Opacity = oldOp;
                //CurrentImage = img;
            }

        }

        public String BitmapImageToBase64(BitmapImage img)
        {
            String b64 = String.Empty;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                b64 = Convert.ToBase64String(stream.ToArray());
            }
            return b64; 
        }

        public BitmapImage CurrentImage
        {
            get
            {
                return _imgSrc;
            }

            set
            {
                _imgSrc = value;
                SetPropertyChanged();
            }
        }

        public string BitmapLocation { get { return System.IO.Path.Combine(Environment.CurrentDirectory, String.Format("tmpImg{0}.jpg",Counter)); } }

        public int Counter {
            get
            {
                return _counter;
            }
        }

        //use task but send params through func , as final i would guess  
        private void DoTakeScreenShot(System.Windows.Shapes.Rectangle rect)
        {
            var bounds = System.Windows.Forms.Screen.GetBounds(System.Drawing.Point.Empty);
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)rect.Width, (int)rect.Height))
            {
                using (var g  = System.Drawing.Graphics.FromImage(bitmap))
                { 
                    g.CopyFromScreen(RectStart, System.Drawing.Point.Empty, RectSize);
                    bitmap.Save(BitmapLocation, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            } 
        }

        private void DoTakeScreenShot(int x, int y, int w , int h,string savePath)
        {
            var bounds = System.Windows.Forms.Screen.GetBounds(System.Drawing.Point.Empty);
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(w,h))
            {
                using (var g  = System.Drawing.Graphics.FromImage(bitmap))
                { 
                    g.CopyFromScreen(new System.Drawing.Point(x,y), System.Drawing.Point.Empty, new System.Drawing.Size(w,h));
                    bitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            } 
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            var pos = e.GetPosition(ctxCanvas);
            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Save edited or created snippets?", "Save before quiting?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(res == MessageBoxResult.Yes)
            {
                ToggleWindow();
                SnippWriter.SaveAllSnippets(_windows.Select((x) => (x.DataContext as DraggableViewModel)).ToList());
            }
            foreach(var i in Enumerable.Range(0,_windows.Count))
            {
                var win = _windows[i];
                win.Close();
            }

            foreach (var i in Enumerable.Range(0, _searchResult.Count))
            {
                var win = _searchResult[i];
                win.Close();
            }

            this.Close(); 
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindow();
            SnippWriter.SaveAllSnippets(_windows.Select((x) => (x.DataContext as DraggableViewModel)).ToList());

        }

        public void onSnippCLosed(SnippWindow win)
        {
            _windows.Remove(win);
        }
    }
}
