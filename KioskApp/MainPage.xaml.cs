using System;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KioskApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        String uri;
        String currentUri;
        Regex validUrl;
        int timeout;
        // Required to get System Idle Time
        [DllImport("User32.dll")]
        private static extern bool
                GetLastInputInfo(ref LASTINPUTINFO plii);

        //required to get System Idle time
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }


        public MainPage()
        {
            // set timeout in milliseconds
            timeout = 300000;
            // set home uri
            uri = "https://www.wikipedia.org";
            // set regex for validUri's
            validUrl = new Regex(@"^https:\/\/(([a-zA-Z0-9\-\\_\.]*)\.|)wikipedia.org(\/.*|)$"); // all https sites, domains and sub-domains of wikipedia.org
            InitializeComponent();
            WVWebBrowser.Navigate(new Uri(uri));
            // setting current uri
            currentUri = uri;
            WVWebBrowser.NavigationStarting += WVWebBrowser_NavigationStarting;
            // Initializes the reset scheduler
            ResetOnIdle();

        }

        // Checks if navigation url is permitted or not
        private async void WVWebBrowser_NavigationStarting(object sender, WebViewNavigationStartingEventArgs args)
        {
            // Cancel navigation if URL is not allowed.
            if (!this.validUrl.IsMatch(args.Uri.ToString()))
            {
                args.Cancel = true;
                var dialog = new MessageDialog("Navigation not permitted");
                await dialog.ShowAsync();
            }
            else
            {
                currentUri = args.Uri.ToString();
            }
        }

        // WebView click events

        private void AppBarBack_Click(object sender, RoutedEventArgs e)
        {
            if (WVWebBrowser.CanGoBack) WVWebBrowser.GoBack();
        }
        private void AppBarForward_Click(object sender, RoutedEventArgs e)
        {
            if (WVWebBrowser.CanGoForward) WVWebBrowser.GoForward();
        }

        private void AppBarHome_Click(Object sender, RoutedEventArgs e)
        {
            WVWebBrowser.Navigate(new Uri(this.uri));
        }

        private void AppBarRefresh_Click(Object sender, RoutedEventArgs e)
        {
            WVWebBrowser.Refresh();
        }

        // Loads webview landing page after Idle time longer than this.timeout (+/- 1 minute)
        private async void ResetOnIdle()
        {
            // checks if idle time is longer than timeout and current Uri is not home
            if (timeout <= GetIdleTime() && uri != currentUri)
            {
                // loads landing page in webview
                WVWebBrowser.Navigate(new Uri(this.uri));
            }
            // waits 1 minute before re executing resetOnIdle();
            await Task.Delay(30000);
            ResetOnIdle();
        }

        // Gets the time without user interaction in In Ticks (millisconds)
        // source: https://stackoverflow.com/questions/1037595/c-sharp-detect-time-of-last-user-interaction-with-the-os
        private int GetIdleTime()
        {
            int LastInputTicks = 0;
            // time since lastinput (in ticks)
            int IdleTicks = 0;
            // num ticks since activity
            int systemUptime = Environment.TickCount;
            // system uptime (in ticks)
            LASTINPUTINFO LastInputInfo = new LASTINPUTINFO();
            LastInputInfo.cbSize = (uint)Marshal.SizeOf(LastInputInfo);
            // init LastInputInfo object
            LastInputInfo.dwTime = 0;
            // wait for 2 seconds to ensure no activity
            // If we have a value from the function
            if (GetLastInputInfo(ref LastInputInfo))
            {
                // Get the number of ticks at the point when the last activity was seen
                LastInputTicks = (int)LastInputInfo.dwTime;
                // Number of idle ticks = system uptime ticks - number of ticks at last input
                IdleTicks = systemUptime - LastInputTicks;
            }
            return IdleTicks;
        }
    }
}
