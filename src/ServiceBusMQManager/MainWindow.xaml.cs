#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    MainWindow.xaml.cs
  Created: 2012-08-21

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using ServiceBusMQManager.Dialogs;

namespace ServiceBusMQManager {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    const int WM_APP = 0x8000;
    public static readonly int WM_SHOWWINDOW = WM_APP + 1;

    private static readonly string[] BUTTON_LABELS = new string[] { "COMMANDS", "EVENTS", "MESSAGES", "ERRORS" };
    private static readonly char SPACE_SEPARATOR = ' ';
    private static readonly List<QueueItem> EMPTY_LIST = new List<QueueItem>();

    private SbmqSystem _sys;
    private IMessageManager _mgr;
    private UIStateConfig _uiState;

    private System.Windows.Forms.NotifyIcon _notifyIcon;


    private ContentWindow _dlg;
    private bool _dlgShown = false;


    public MainWindow() {
      InitializeComponent();

      //SourceInitialized += Window_SourceInitialized;

      CreateNotifyIcon();
    }

    private void Window_SourceInitialized(object sender, EventArgs e) {

      lbTitle.Content = Title;

      InitSystem();


      HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
      source.AddHook(WndProc);


      if( _sys.Config.VersionCheck.Enabled ) {

        if( _sys.Config.VersionCheck.LastCheck < DateTime.Now.AddDays(-14) )
          CheckIfLatestVersion(false);

      }

    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {

      if( msg == WM_SHOWWINDOW ) {

        ShowMainWindow();

        handled = true;
        return new IntPtr(1);
      
      } else handled = false;

      return IntPtr.Zero;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {

      if( _mgr.EventQueues.Length == 0 && _mgr.CommandQueues.Length == 0 && _mgr.MessageQueues.Length == 0 && _mgr.ErrorQueues.Length == 0 ) {

        ShowConfigDialog();
      }

    }

    private void ShowConfigDialog() {
      ConfigWindow dlg = new ConfigWindow(_sys);

      if( dlg.ShowDialog() == true ) {
        RestartSystem();
      }
    }


    private void InitSystem() {
      _sys = SbmqSystem.Create();
      _sys.ItemsChanged += MessageMgr_ItemsChanged;

      _mgr = _sys.Manager;
      _uiState = _sys.UIState;

      _dlg = new ContentWindow();

      RestoreUIState();

      this.Icon = BitmapFrame.Create(this.GetImageResourceStream("main.ico"));

      lbItems.ItemsSource = _mgr.Items;

      SetupContextMenu();

      SetupQueueMonitorTimer(_sys.Config.MonitorInterval);

    }

    private void RestartSystem() {
      _timer.Stop();

      if( _sys != null )
        _sys.Manager.Dispose();

      _sys = SbmqSystem.Create();
      _sys.ItemsChanged += MessageMgr_ItemsChanged;

      _mgr = _sys.Manager;
      _uiState = _sys.UIState;

      RestoreMonitorQueueState();

      lbItems.ItemsSource = _mgr.Items;

      _timer.Interval = TimeSpan.FromMilliseconds(_sys.Config.MonitorInterval);
      _timer.Start();
    }

    private void RestoreMonitorQueueState() {
      _mgr.MonitorCommands = btnCmd.IsChecked == true;
      _mgr.MonitorEvents = btnEvent.IsChecked == true;
      _mgr.MonitorMessages = btnMsg.IsChecked == true;
      _mgr.MonitorErrors = btnError.IsChecked == true;
    }



    void CheckIfLatestVersion(bool startedByUser) {
      CheckVersionThread cvt = new CheckVersionThread();

      //if( startedByUser )
      //cvt.RunWorkerCompleted += new RunWorkerCompletedEventHandler(cvt_RunWorkerCompleted);
      //else 
      cvt.RunWorkerCompleted += new RunWorkerCompletedEventHandler(cvt_HiddenRunWorkerCompleted);

      List<CheckVersionObject> list = new List<CheckVersionObject>();

      list.Add(
        new CheckVersionObject() {
          ProductName = App.Info.Name,
          CurrentVersion = App.Info.Version
        });


      cvt.RunWorkerAsync(list);
    }

    void cvt_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {

      /*
      if( e.Error == null ) {
        List<HalanVersionInfo> inf = (List<HalanVersionInfo>)e.Result;

        if( inf.Any(v => v.Status == VersionStatus.Old) ) {

          ShowNewerVersionDialog(inf);

        } else if( inf.All(v => v.Status == VersionStatus.Latest) ) {
          lbVersionInfo.Text = "You have the latest version";
          lbVersionInfo.Visible = true;

        } else if( inf.Any(v => v.Status == VersionStatus.NoConnection) ) {
          lbVersionInfo.Text = "Could not connect to Server";
          lbVersionInfo.Visible = true;
        }

      } else LogError(("Failed to retrieve latest version information from server", e.Error));


      if( !btnCheckUpdate.Enabled ) {
        btnCheckUpdate.Text = CHECK_UPDATES_LABEL;
        btnCheckUpdate.Enabled = true;
      }
     */
    }
    void cvt_HiddenRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {

      if( e.Error == null ) {
        List<HalanVersionInfo> inf = (List<HalanVersionInfo>)e.Result;

        if( inf.Any(v => v.Status == VersionStatus.Old) ) {
          ShowNewerVersionDialog(inf);
        }

        _sys.Config.VersionCheck.LastCheck = DateTime.Today;
        _sys.Config.Save();
      }
    }

    private void ShowNewerVersionDialog(List<HalanVersionInfo> inf) {
      NewVersionDialog dlg = new NewVersionDialog(inf);
      dlg.ShowDialog();
    }


    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
      if( (bool)e.NewValue )
        SetSelectedItem((QueueItem)lbItems.SelectedItem);
    }


    protected override void OnStateChanged(EventArgs e) {
      if( WindowState == WindowState.Minimized ) {
        this.Hide();

        if( _dlg != null && _dlg.IsVisible )
          _dlg.Hide();
      }

      base.OnStateChanged(e);
    }

    private void CreateNotifyIcon() {
      _notifyIcon = new System.Windows.Forms.NotifyIcon();

      //ServiceBusMQManager.Properties.Resources.
      var mi = new System.Windows.Forms.MenuItem[1];

      mi[0] = new System.Windows.Forms.MenuItem();
      mi[0].Text = "Exit";
      mi[0].Click += miClose_Click;

      _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(mi);
      _notifyIcon.Icon = new System.Drawing.Icon(this.GetImageResourceStream("trayIcon.ico"));
      _notifyIcon.DoubleClick += (sender,args) => { ShowMainWindow(); };

      _notifyIcon.Visible = true;
    }

    private void ShowMainWindow() {
      if( !this.IsVisible ) {
        this.Show();
        this.WindowState = WindowState.Normal;
      } else {
        this.Activate();

        if( _dlg != null )
          _dlg.Activate();
      }
    }

    bool _showingActivityTrayIcon;
    private DispatcherTimer _timer;
    void ShowActivityTrayIcon() {

      if( !_showingActivityTrayIcon ) {
        _showingActivityTrayIcon = true;

        Thread thread = new Thread(new ThreadStart(delegate() {

          Thread.Sleep(200); // this is important ...
          try {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                new Action(delegate() {
              _notifyIcon.Icon = new System.Drawing.Icon(this.GetImageResourceStream("trayIconActivity.ico"));
            }));
            Thread.Sleep(500);

            this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                new Action(delegate() {
              _notifyIcon.Icon = new System.Drawing.Icon(this.GetImageResourceStream("trayIcon.ico"));
            }));

            _showingActivityTrayIcon = false;
          } catch { }
        }));
        thread.Name = "thread-updateTrayIcon";
        thread.Start();
      }
    }


    private void SetupQueueMonitorTimer(int ms) {
      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromMilliseconds(ms);
      _timer.Tick += timer_Tick;

      _timer.Start();
    }

    private void UpdateButtonLabel(ToggleButton btn) {
      int iType = Convert.ToInt32(btn.Tag);
      QueueType type = (QueueType)iType;

      if( btn.IsChecked == true ) {

        int iCount = _sys.Manager.Items.Count(i => i.QueueType == type && !i.Deleted);

        string count = string.Format("({0})", iCount);
        if( !( btn.Content as string ).Contains(count) )
          btn.Content = string.Concat(BUTTON_LABELS[iType], SPACE_SEPARATOR, count);

      } else {
        btn.Content = BUTTON_LABELS[iType];
      }

    }

    private void MessageMgr_ItemsChanged(object sender, EventArgs e) {

      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {

        // Update button labels
        UpdateButtonLabel(btnCmd);
        UpdateButtonLabel(btnEvent);
        UpdateButtonLabel(btnMsg);
        UpdateButtonLabel(btnError);

        // Update List View
        lbItems.Items.Refresh();

        ShowActivityTrayIcon();

        // Show Window
        if( _sys.Config.ShowOnNewMessages && !this.IsVisible )
          this.Show();

      }));
    }
    private void timer_Tick(object sender, EventArgs e) {
      _sys.Manager.RefreshQueueItems();
    }


    private void _UpdateContextMenuItem(MenuItem mi, QueueItem itm) {
      mi.IsEnabled = itm != null;

      if( itm != null )
        mi.Tag = itm;
    }

    private void SetupContextMenu() {
      var items = lbItems.ContextMenu.Items;

      // Return All error messages
      var mi = (MenuItem)items[6];
      mi.Items.Clear();
      foreach( var name in _mgr.ErrorQueues ) {
        var m2 = new MenuItem() { Header = name };
        m2.Click += (sender, e) => { _mgr.MoveAllErrorItemsToOriginQueue(name); };

        mi.Items.Add(m2);
      }

      // Purge all error messages
      mi = (MenuItem)items[7];
      mi.Items.Clear();
      foreach( var name in _mgr.ErrorQueues ) {
        var m2 = new MenuItem() { Header = name };
        m2.Click += (sender, e) => { _mgr.PurgeErrorMessages(name); };

        mi.Items.Add(m2);
      }

    }
    private void UpdateContextMenu(QueueItem itm) {
      var items = lbItems.ContextMenu.Items;

      // Copy to Clipboard
      _UpdateContextMenuItem((MenuItem)items[0], itm);

      // Remove message
      var mi = (MenuItem)items[2];
      _UpdateContextMenuItem((MenuItem)items[2], itm);
      mi.IsEnabled = itm != null && !itm.Deleted;

      // Return Error Message to Origin
      mi = (MenuItem)items[5];
      _UpdateContextMenuItem(mi, itm);

      mi.IsEnabled = ( itm != null && itm.QueueType == QueueType.Error );

    }
    private void SetSelectedItem(QueueItem itm) {

      if( itm != null && itm.Content != null ) {

        if( _dlgShown && !_dlg.IsVisible ) {
          _dlg = new ContentWindow();
          _dlg.Topmost = Topmost;

          _uiState.RestoreWindowState(_dlg);

          _dlgShown = false;
          UpdateContentWindow();
        }

        _dlg.SetContent(_mgr.LoadMessageContent(itm));

        //_dlg.Left = ( this.Left + this.Width ) - _dlg.Width;
        //_dlg.Top = this.Top - _dlg.Height;
        _dlg.SetTitle(itm.DisplayName);


        if( !_dlgShown ) {
          _dlg.Show();

          _dlgShown = true;
        } else {
          if( !Topmost ) {
            _dlg.Activate(); // Make sure its visible
            this.Activate();
          }
        }

        UpdateContextMenu(itm);
      } else {

        UpdateContextMenu(null);

        if( _dlg != null && _dlg.IsVisible )
          _dlg.Hide();

      }

    }
    private void SetAlwaysOnTop(bool value) {
      Topmost = value;

      if( _dlg != null )
        _dlg.Topmost = value;

      // Update Context Menu Item
      miToggleOnTop.IsChecked = value;

      // Update Icon
      imgAlwaysOnTop.Source = value ? BitmapFrame.Create(this.GetImageResourceStream("pinned.png")) : BitmapFrame.Create(this.GetImageResourceStream("unpinned.png"));
    }
    private void ChangedMonitorFlag(QueueType type, bool newState) {
      switch( type ) {
        case QueueType.Command: _mgr.MonitorCommands = newState; break;
        case QueueType.Event: _mgr.MonitorEvents = newState; break;
        case QueueType.Message: _mgr.MonitorMessages = newState; break;
        case QueueType.Error: _mgr.MonitorErrors = newState; break;
      }
    }
    private void SetDefaultWindowPosition() {
      //Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {
      var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
      var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
      var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

      this.Left = corner.X - this.ActualWidth;
      this.Top = corner.Y - this.ActualHeight;
      //}));
    }

    private void lbItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var itm = ( lbItems.SelectedItem as QueueItem );

      SetSelectedItem(itm);
    }

    private void lbItems_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var itm = ( lbItems.SelectedItem as QueueItem );

      if( itm != null && _dlg != null && !_dlg.IsVisible && _dlgShown ) {
        SetSelectedItem(itm);
      }

    }


    private void btnClearDeleted_Click(object sender, RoutedEventArgs e) {

      _mgr.ClearDeletedItems();

      lbItems.Items.Refresh();
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e) {
      ShowConfigDialog();
    }

    private void SnapWindowToEdge() {
      var s = WpfScreen.GetScreenFrom(this);

      var treshold = 7;

      // End Right
      var right = this.Left + this.Width;
      if( ( right > s.DeviceBounds.Right - treshold && right < s.DeviceBounds.Right + treshold ) ) {
        this.Left = s.DeviceBounds.Width - this.Width;
      }

      // End Left
      var left = this.Left;
      if( ( left > s.DeviceBounds.Left - treshold && left < s.DeviceBounds.Left + treshold ) ) {
        this.Left = s.DeviceBounds.Left;
      }

      var bottom = this.Top + this.Height;
      if( ( bottom > s.WorkingArea.Bottom - treshold && bottom < s.WorkingArea.Bottom + treshold ) ) {
        this.Top = s.WorkingArea.Height - this.Height;
      }

      var top = this.Top;
      if( ( top > s.WorkingArea.Top - treshold && top < s.WorkingArea.Top + treshold ) ) {
        this.Top = s.WorkingArea.Top;
      }


    }
    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }
    private void Window_LocationChanged(object sender, EventArgs e) {

      SnapWindowToEdge();

      UpdateContentWindow();
    }

    private void UpdateContentWindow() {
      if( _dlg != null ) {
        var s = WpfScreen.GetScreenFrom(this);

        if( this.Top < _dlg.Height ) {
          _dlg.Top = this.Top + this.Height;
        } else {
          _dlg.Top = this.Top - _dlg.Height;
        }

        _dlg.Left = ( this.Left + this.Width ) - _dlg.Width;


      }
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      e.Cancel = true;

      if( _dlg != null && _dlg.IsVisible )
        _dlg.Hide();

      this.Hide();
    }
    private void Window_Closed(object sender, EventArgs e) {

      if( _notifyIcon != null )
        _notifyIcon.Visible = false;

      StoreUIState();

      if( _sys != null )
        _sys.Manager.Dispose();
    }

    private void StoreUIState() {

      if( _uiState != null ) {

        _uiState.StoreControlState(btnCmd);
        _uiState.StoreControlState(btnEvent);
        _uiState.StoreControlState(btnMsg);
        _uiState.StoreControlState(btnError);

        //_uiCfg.UpdateButtonState(btnCmd.IsChecked, btnEvent.IsChecked, btnMsg.IsChecked, btnError.IsChecked);

        _uiState.StoreWindowState(this);
        _uiState.StoreWindowState(_dlg);

        _uiState.AlwaysOnTop = Topmost;

        _uiState.Save();

      }
    }
    private void RestoreUIState() {

      SetAlwaysOnTop(_uiState.AlwaysOnTop);

      if( !_uiState.RestoreWindowState(this) )
        SetDefaultWindowPosition();

      _uiState.RestoreWindowState(_dlg);

      _uiState.RestoreControlState(btnCmd, true);
      _uiState.RestoreControlState(btnEvent, true);
      _uiState.RestoreControlState(btnMsg, false);
      _uiState.RestoreControlState(btnError, false);
    }

    private void miClose_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    private void miToggleAlwaysOnTop_Click(object sender, RoutedEventArgs e) {

      _uiState.AlwaysOnTop = !Topmost;

      SetAlwaysOnTop(_uiState.AlwaysOnTop);
    }


    private void miReturnErrorMsg_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      _mgr.MoveErrorItemToOriginQueue(itm);
    }
    private void miCopyMessageID_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      Clipboard.SetData(DataFormats.Text, itm.Id);
    }
    private void miDeleteMessage_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      _mgr.PurgeMessage(itm);
    }
    private void miDeleteAllMessage_Click(object sender, RoutedEventArgs e) {

      _mgr.PurgeAllMessages();
    }
    private void miDeleteAllErrorMessage_Click(object sender, RoutedEventArgs e) {

      _mgr.PurgeErrorAllMessages();
    }


    private void HandleCloseClick(Object sender, RoutedEventArgs e) {

      StoreUIState();

      Close();
    }
    private void HandleMinimizeClick(Object sender, RoutedEventArgs e) {
      WindowState = WindowState.Minimized;
    }


    private void btn_Unchecked(object sender, RoutedEventArgs e) {
      var btn = ( sender as ToggleButton );
      QueueType type = (QueueType)Convert.ToInt32(btn.Tag);

      ChangedMonitorFlag(type, false);

      UpdateButtonLabel(btn);

      lbItems.Items.Refresh();
    }
    private void btn_Checked(object sender, RoutedEventArgs e) {
      var btn = ( sender as ToggleButton );
      QueueType type = (QueueType)Convert.ToInt32(btn.Tag);

      ChangedMonitorFlag(type, true);

      UpdateButtonLabel(btn);
    }

    private void btnSendCommand_Click(object sender, RoutedEventArgs e) {
      var dlg = new SendCommandWindow(_sys);

      dlg.Show();
    }

    private void btnViewSubscriptions_Click(object sender, RoutedEventArgs e) {
      var dlg = new ViewSubscriptionsWindow(_sys);

      dlg.Show();
    }

    private void frmMain_Activated(object sender, EventArgs e) {

      if( _dlg != null && _dlg.IsVisible )
        _dlg.EnsureVisibility();

    }




  }
}
