using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace TypPostriku
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static AppRuntimeData appRuntimeData = new AppRuntimeData();
        private const string UniqueEventName = "TypPostrikuEvent";
        private const string UniqueMutexName = "TypPostrikuMutex";
        private static Mutex _mutex = null;
        private static EventWaitHandle _eventWaitHandle;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, UniqueMutexName, out createdNew);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            if (createdNew)
            {
                base.OnStartup(e);

                Thread listenerThread = new Thread(ListenForShowWindowEvents);
                listenerThread.SetApartmentState(ApartmentState.STA);
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            else
            {
                _eventWaitHandle.Set();

                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
             
                Environment.Exit(0);
            }
        }


        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
            base.OnExit(e);
        }


        private void ListenForShowWindowEvents()
        {
            while (true)
            {
                _eventWaitHandle.WaitOne();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (MainWindow != null)
                    {
                        if (MainWindow.WindowState == WindowState.Minimized)
                        {
                            MainWindow.WindowState = WindowState.Normal;
                        }
                        MainWindow.Activate();
                        MainWindow.Topmost = true;  
                        MainWindow.Topmost = false; 
                        MainWindow.Show();
                    }
                });
            }
        }

    }

}
