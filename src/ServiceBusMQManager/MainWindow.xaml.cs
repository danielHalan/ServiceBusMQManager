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
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using NLog;
using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using ServiceBusMQ.ViewModel;
using ServiceBusMQManager.Dialogs;

namespace ServiceBusMQManager {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    const int WM_APP = 0x8000;
    public const int WM_SHOWWINDOW = WM_APP + 1;

    private static readonly string[] BUTTON_LABELS = new string[] { "COMMANDS", "EVENTS", "MESSAGES", "ERRORS" };
    private static readonly char SPACE_SEPARATOR = ' ';
    private static readonly List<QueueItem> EMPTY_LIST = new List<QueueItem>();

    private Logger _log = LogManager.GetCurrentClassLogger();

    private SbmqSystem _sys;

    private IServiceBusManager _mgr;
    private ServiceBusFeature[] _features;

    private UIStateConfig _uiState;
    string _loadingText = null;

    private System.Windows.Forms.NotifyIcon _notifyIcon;

    private bool _isMinimized;

    bool _firstLoad = true;

    private ContentWindow _dlg;
    private bool _dlgShown = false;

    string _titleStr;

    public MainWindow() {
      InitializeComponent();

      var ver = App.Info.Version;
      _titleStr = string.Format("Service Bus MQ Manager {0}.{1} - (C)2012-2014 ITQ.COM, Daniel Halan", ver.Major, ver.Minor.ToString("D2"));


      imgLoadingQueues.Visibility = System.Windows.Visibility.Hidden;

      UpdateTitle();

      CreateNotifyIcon();

      SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

      tbSearchString.Visibility = System.Windows.Visibility.Collapsed;
    }

    private void UpdateTitle() {
      if( _sys != null )
        lbTitle.Content = Title = "{0}  -  [{1}]".With(_titleStr, _sys.Config.MonitorServerName.CutEnd(20));
      else lbTitle.Content = Title = _titleStr;
    }

    private void Window_SourceInitialized(object sender, EventArgs e) {

      InitSystem();

      HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
      source.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {

      switch( msg ) {
        case WM_SHOWWINDOW:

          ShowMainWindow();

          handled = true;
          return new IntPtr(1);
      }


      return IntPtr.Zero;
    }


    private void Window_Loaded(object sender, RoutedEventArgs e) {

      if( _uiState.IsMinimized || App.StartMinimized )
        Close();

      this.Focus();
    }

    private void ShowConfigDialog() {
      ConfigWindow dlg = new ConfigWindow(_sys);

      _sys.StopMonitoring();

      if( dlg.ShowDialog() == true ) {
        RestartSystem();

      } else _sys.StartMonitoring();
    }


    private void InitSystem() {

      this.Icon = BitmapFrame.Create(this.GetImageResourceStream("main.ico"));

      _uiState = SbmqSystem.UIState;
      RestoreWindowState();

      this.IsEnabled = false;
      lbLoading.Visibility = System.Windows.Visibility.Visible;

      BackgroundWorker w = new BackgroundWorker();
      w.DoWork += (s, e) => {
        try {
          _sys = SbmqSystem.Create();
          _sys.ItemsChanged += sys_ItemsChanged;
          _sys.ErrorOccured += _sys_ErrorOccured;
          _sys.WarningOccured += _sys_WarningOccured;

          _sys.StartedLoadingQueues += _sys_StartedLoadingQueues;
          _sys.FinishedLoadingQueues += _sys_FinishedLoadingQueues;

          _features = _sys.GetDiscoveryService().Features;

          _mgr = _sys.Manager;

        } catch( Exception ex ) {

          Dispatcher.Invoke(DispatcherPriority.Normal,
                            new Action(() => { LogError("Failed to initialize System", ex, true); }));

        }
      };

      w.RunWorkerCompleted += (s, e) => {

        RestoreQueueButtonsState();
        this.IsEnabled = true;

        btnSendCommand.IsEnabled = _sys.CanSendCommand;
        btnViewSubscriptions.IsEnabled = _sys.CanViewSubscriptions;

        lbItems.ItemsSource = _sys.Items;
        if( !lbItems.IsEnabled )
          lbItems.IsEnabled = true;

        SetupContextMenu();

        UpdateNotifyIconText();

        if( _sys.Config.StartCount == 1 ) {
          ShowConfigDialog();

        } else if( _sys.Config.VersionCheck.Enabled ) {
          if( _sys.Config.VersionCheck.LastCheck < DateTime.Now.AddDays(-14) )
            CheckIfLatestVersion(false);
        }

        UpdateTitle();

        _sys.StartMonitoring();

        lbLoading.Visibility = System.Windows.Visibility.Hidden;
      };

      w.RunWorkerAsync();
    }

    void _sys_StartedLoadingQueues(object sender, EventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
          new Action(delegate() {

        if( _loadingText.IsValid() ) {
          DisableListView(_loadingText);
        }

        imgLoadingQueues.Visibility = System.Windows.Visibility.Visible;
      }));
    }

    void _sys_FinishedLoadingQueues(object sender, EventArgs e) {

      this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
          new Action(delegate() {
        imgLoadingQueues.Visibility = System.Windows.Visibility.Hidden;

        if( _loadingText.IsValid() ) {
          _loadingText = null;
          EnableListView();
        }

      }));
    }



    void _sys_ErrorOccured(object sender, ErrorArgs e) {

      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {

        MessageDialog.Show(MessageType.Error, e.Message, e.Exception);

      }));
    }
    void _sys_WarningOccured(object sender, WarningArgs e) {
      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {

        if( e.Type == WarningType.ConnectonFailed ) {

          DisableListView("Connection Failed: " + e.Message);

        } else MessageDialog.Show(MessageType.Warn, e.Message, e.Content);

      }));
    }

    private void RestartSystem() {

      this.IsEnabled = false;
      lbItems.ItemsSource = null;

      BackgroundWorker w = new BackgroundWorker();
      w.DoWork += (s, e) => {

        try {
          _sys.SwitchServiceBus(_sys.Config.ServiceBus, _sys.Config.ServiceBusVersion, _sys.Config.ServiceBusQueueType);
          _mgr = _sys.Manager;

        } catch( RestartRequiredException ) {
          e.Cancel = true;
        }
      };

      w.RunWorkerCompleted += (s, e) => {
        if( !e.Cancelled ) {
          this.IsEnabled = true;
          RestoreMonitorQueueState();

          btnSendCommand.IsEnabled = _sys.CanSendCommand;
          btnViewSubscriptions.IsEnabled = _sys.CanViewSubscriptions;

          lbItems.ItemsSource = _sys.Items;

          if( !lbItems.IsEnabled )
            lbItems.IsEnabled = true;

          SetupContextMenu();

          UpdateTitle();

          _sys.StartMonitoring();

        } else { // Restart needed

          System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, "-f");
          Application.Current.Shutdown();
        }
      };

      w.RunWorkerAsync();
    }


    private void RestoreMonitorQueueState() {

      _sys.MonitorCommands = btnCmd.IsChecked == true;
      _sys.MonitorEvents = btnEvent.IsChecked == true;
      _sys.MonitorMessages = btnMsg.IsChecked == true;
      _sys.MonitorErrors = btnError.IsChecked == true;
    }


    void CheckIfLatestVersion(bool startedByUser) {
      CheckVersionThread cvt = new CheckVersionThread();

      if( startedByUser ) {
        miCheckVersion.IsEnabled = false;
        cvt.RunWorkerCompleted += new RunWorkerCompletedEventHandler(cvt_RunWorkerCompleted);

      } else cvt.RunWorkerCompleted += new RunWorkerCompletedEventHandler(cvt_HiddenRunWorkerCompleted);

      List<CheckVersionObject> list = new List<CheckVersionObject>();

      list.Add(
        new CheckVersionObject() {
          ProductName = App.Info.Name,
          CurrentVersion = App.Info.Version
        });


      cvt.RunWorkerAsync(list);
    }

    void cvt_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {


      if( e.Error == null ) {
        List<HalanVersionInfo> inf = (List<HalanVersionInfo>)e.Result;

        if( inf.Any(v => v.Status == VersionStatus.Old) ) {

          ShowNewerVersionDialog(inf);

        } else if( inf.All(v => v.Status == VersionStatus.Latest) ) {
          LogInfo("You have the latest version");

        } else if( inf.Any(v => v.Status == VersionStatus.NoConnection) ) {
          LogInfo("Could not connect to Server");
        }

      } else LogError("Failed to retrieve latest version information from server", e.Error);


      if( !miCheckVersion.IsEnabled ) {
        miCheckVersion.IsEnabled = true;
      }

    }

    private void LogError(string msg, Exception exception, bool fatal = false) {
      MessageDialog.Show(MessageType.Error, msg, exception);

      if( fatal )
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    private void LogInfo(string msg) {
      MessageDialog.Show(MessageType.Info, msg);
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


    private void ChangeMinimizedState(bool value) {
      if( _isMinimized != value ) {
        _uiState.IsMinimized = _isMinimized = value;
        _uiState.Save();
      }
    }


    void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
      _uiState.RestoreWindowState(this);
    }
    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
      if( (bool)e.NewValue ) {

        ChangeMinimizedState(false);

        SetSelectedItem((QueueItem)lbItems.SelectedItem);
      }
    }
    private void frmMain_Activated(object sender, EventArgs e) {

      if( _dlg != null && _dlg.IsVisible )
        _dlg.EnsureVisibility();

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
      _notifyIcon.MouseMove += _notifyIcon_MouseMove;
      //ServiceBusMQManager.Properties.Resources.
      var mi = new System.Windows.Forms.MenuItem[1];

      mi[0] = new System.Windows.Forms.MenuItem();
      mi[0].Text = "Exit";
      mi[0].Click += miClose_Click;

      _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(mi);
      _notifyIcon.Icon = new System.Drawing.Icon(this.GetImageResourceStream("trayIcon.ico"));
      _notifyIcon.DoubleClick += (sender, args) => { ShowMainWindow(); };

      _notifyIcon.Visible = true;
    }


    private void ShowMainWindow() {
      if( !this.IsVisible ) {
        this.Show();
        this.WindowState = WindowState.Normal;

        ChangeMinimizedState(false);
      } else {
        this.Activate();

        if( _dlg != null )
          _dlg.Activate();
      }
    }

    bool _showingActivityTrayIcon;

    void ShowActivityTrayIcon() {

      if( !_showingActivityTrayIcon ) {
        _showingActivityTrayIcon = true;

        if( !_notifyIconTextDirty )
          _notifyIconTextDirty = true;

        Thread thread = new Thread(new ThreadStart(delegate() {

          Thread.Sleep(200); // wait a bit..
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

    bool _notifyIconTextDirty = true;
    void _notifyIcon_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {

      if( _notifyIconTextDirty )
        UpdateNotifyIconText();

    }

    void UpdateNotifyIconText() {
      _notifyIcon.Text = GetQueueStatusString();
      _notifyIconTextDirty = false;
    }


    private string GetQueueStatusString() {
      QueueType[] itemTypes = null;
      if( _sys != null )
        itemTypes = _sys.Items.Select(i => i.Queue.Type).ToArray();
      else itemTypes = new QueueType[0];

      return string.Format(" Commands: {0} \r\n Events: {1} \r\n Messages: {2} \r\n Errors: {3} ",
                  itemTypes.Count(i => i == QueueType.Command),
                  itemTypes.Count(i => i == QueueType.Event),
                  itemTypes.Count(i => i == QueueType.Message),
                  itemTypes.Count(i => i == QueueType.Error)
                  );
    }



    private void UpdateButtonLabel(ToggleButton btn) {
      int iType = Convert.ToInt32(btn.Tag);
      QueueType type = (QueueType)iType;

      if( btn.IsChecked == true ) {

        uint iCount = _sys.GetUnprocessedItemsCount(type);
        //int iCount = _sys.Items.Count(i => i.Queue.Type == type && !i.Processed);

        string count = string.Format("({0})", iCount);
        if( !( btn.Content as string ).Contains(count) )
          btn.Content = string.Concat(BUTTON_LABELS[iType], SPACE_SEPARATOR, count);

      } else {
        btn.Content = BUTTON_LABELS[iType];
      }

    }

    private void sys_ItemsChanged(object sender, ServiceBusMQ.ItemsChangedEventArgs e) {

      Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {

        EnableListView();

        // Update button labels
        UpdateButtonLabel(btnCmd);
        UpdateButtonLabel(btnEvent);
        UpdateButtonLabel(btnMsg);
        UpdateButtonLabel(btnError);

        // Update List View
        lock( _sys.ItemsLock ) {
          lbItems.ItemsSource = _sys.Items;
          lbItems.Items.Refresh();

          if( !lbItems.IsEnabled )
            lbItems.IsEnabled = true;

        }

        SetupContextMenu();

        if( e.Origin == ItemChangeOrigin.Queue )
          ShowActivityTrayIcon();

        // Show Window
        if( _sys.Config.ShowOnNewMessages && !_firstLoad && !this.IsVisible ) {
          this.Show();

          if( !Topmost ) {
            Topmost = true;
            Thread.Sleep(100);
            Topmost = false;
          }
        }

        _firstLoad = false;

      }));
    }
    private void timer_Tick(object sender, EventArgs e) {
      try {
        _sys.RefreshUnprocessedQueueItemList();
#if DEBUG
      } catch( Exception ex ) {
        MessageBox.Show("Failed when fetching messages " + ex.Message);
#else
      } catch {
#endif
      }
    }


    private void _BindContextMenuItem(MenuItem mi, QueueItem itm, Func<QueueItem, bool> eval = null) {
      mi.IsEnabled = itm != null && ( eval == null || eval(itm) );

      if( itm != null )
        mi.Tag = itm;
    }

    private void SetupContextMenu() {
      var items = lbItems.ContextMenu.Items;


      if( _sys.GetUnprocessedItemsCount(QueueType.Error) > 0 ) {
        miReturnAllErr.IsEnabled = true;
        miPurgeAllErr.IsEnabled = true;

        // Return All error messages
        var mi = miReturnAllErr;
        /*
        mi.Click += async (sender, e) => {
          try {
            _loadingText = "Processing...";
            await _sys.MoveAllErrorMessagesToOriginQueue(null);
          } catch( Exception ex ) {
            _sys_ErrorOccured(this, new ErrorArgs("Failed to move messages to Orgin Queues", ex));
          }
        };
        */
        mi.Items.Clear();
        foreach( var q in _mgr.MonitorQueues.Where(q => q.Type == QueueType.Error) ) {
          var m2 = new MenuItem() { Header = q.Name };
          m2.Click += async (sender, e) => {
            _loadingText = "Processing...";
            _sys.MoveAllErrorMessagesToOriginQueue(q.Name);
          };

          mi.Items.Add(m2);
        }

        // Purge all error messages
        mi = miPurgeAllErr;
        mi.Items.Clear();
        foreach( var q in _mgr.MonitorQueues.Where(q => q.Type == QueueType.Error) ) {
          var m2 = new MenuItem() { Header = q.Name };
          m2.Click += async (sender, e) => { _sys.PurgeErrorMessages(q.Name); };

          mi.Items.Add(m2);
        }

      } else {
        miReturnAllErr.IsEnabled = false;
        miPurgeAllErr.IsEnabled = false;
      }


      miPurgeMsg.IsEnabled = _features.Any(f => f == ServiceBusFeature.PurgeMessage);

      if( miReturnAllErr.IsEnabled )
        miPurgeAllErr.IsEnabled = _features.Any(f => f == ServiceBusFeature.PurgeAllMessages);

      miReturnErrToOrgin.IsEnabled = _features.Any(f => f == ServiceBusFeature.MoveErrorMessageToOriginQueue);

      if( miReturnAllErr.IsEnabled )
        miReturnAllErr.IsEnabled = _features.Any(f => f == ServiceBusFeature.MoveAllErrorMessagesToOriginQueue);

    }
    private bool HasFeature(ServiceBusFeature feat) {
      return _features.Any(f => f == feat);
    }

    private void UpdateContextMenu(QueueItem itm) {
      var items = lbItems.ContextMenu.Items;

      // Copy to Clipboard
      _BindContextMenuItem(miCopyMsgId, itm);
      _BindContextMenuItem(miCopyMsgName, itm);
      _BindContextMenuItem(miCopyMsgContent, itm);
      _BindContextMenuItem(miResendCommand, itm, qi => _sys.CanSendCommand && qi.Queue.Type == QueueType.Command);

      if( itm != null )
        miReturnErrToOrgin.FontWeight = itm.Queue.Type == QueueType.Error ? FontWeights.Bold : FontWeights.Normal;

      // Remove message
      _BindContextMenuItem(miPurgeMsg, itm, qi => !qi.Processed && HasFeature(ServiceBusFeature.PurgeMessage));

      // Return Error Message to Origin
      _BindContextMenuItem(miReturnErrToOrgin, itm, qi => qi.Queue.Type == QueueType.Error && HasFeature(ServiceBusFeature.MoveErrorMessageToOriginQueue));

//#if DEBUG
      MenuItem mi = null;
      if( ( ( items[items.Count - 1] as MenuItem ).Header as string ) != "Headers" ) {
        mi = new MenuItem();
        mi.Header = "Headers";
        items.Add(mi);

      } else mi = (MenuItem)items[items.Count - 1];

      mi.Items.Clear();

      if( itm != null && itm.Headers != null )
        foreach( var head in itm.Headers )
          mi.Items.Add(new MenuItem() { Header = string.Concat(head.Key, '=', head.Value) });
//#endif

    }

    private string GetQueueItemContent(QueueItem itm) {
      var content = string.Empty;

      if( itm.Content == null || itm.Content.StartsWith("**") )
        content = _mgr.LoadMessageContent(itm);
      else
        content = itm.Content;


      if( content != null && !itm.Content.StartsWith("**") ) {

        if( itm.Queue.ContentFormat == MessageContentFormat.Unknown ) {
          itm.Queue.SetContentFormat(content);
        }
      }

      return content;
    }

    ContentWindow CreateContentWindow() {
      ContentWindow w = new ContentWindow();
      w.Closed += _dlg_Closed;
      w.Topmost = Topmost;

      _uiState.RestoreWindowState(w);

      return w;
    }

    private void SetSelectedItem(QueueItem itm) {

      if( itm != null ) {

        if( _isMinimized )
          return;

        if( _dlg == null || ( _dlgShown && !_dlg.IsVisible ) ) {
          _dlg = CreateContentWindow();

          _dlgShown = false;
          UpdateContentWindow();
        }

        var content = GetQueueItemContent(itm);

        //_mgr.MessageContentFormat
        _dlg.SetContent(content, itm.Queue.ContentFormat, itm.Error);
        _dlg.SetTitle(itm.DisplayName);

        if( !_dlgShown ) {
          _dlg.Show();

          _dlgShown = true;

          this.Focus();
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

    void _dlg_Closed(object sender, EventArgs e) {
      lbItems.SelectedIndex = -1;
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
        case QueueType.Command: _sys.MonitorCommands = newState; break;
        case QueueType.Event: _sys.MonitorEvents = newState; break;
        case QueueType.Message: _sys.MonitorMessages = newState; break;
        case QueueType.Error: _sys.MonitorErrors = newState; break;
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

      if( itm != null ) {
        if( _dlg != null && !_dlg.IsVisible && _dlgShown ) {
          SetSelectedItem(itm);
        }

        if( itm.Queue.Type == QueueType.Error ) { // Move back to origin
          _sys.MoveErrorMessageToOriginQueue(itm);
        }
      }
    }  

    private void btnClearProcessed_Click(object sender, RoutedEventArgs e) {

      _sys.ClearProcessedItems();

      lbItems.ItemsSource = _sys.Items;
      lbItems.Items.Refresh();
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e) {
      ShowConfigDialog();
    }


    private void SnapWindowToEdge() {
      var s = WpfScreen.GetScreenFrom(this);

      var treshold = 30;

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

      //SnapWindowToEdge();

      UpdateContentWindow();
    }

    private void UpdateContentWindow() {
      if( _dlg != null ) {
        var s = WpfScreen.GetScreenFrom(this);

        if( this.Top < _dlg.Height ) { // no space above, place bellow

          var height = this.Top + this.ActualHeight;

          if( height + _dlg.Height < s.WorkingArea.Height )
            _dlg.Top = height;

          else { // Not fit bellow
            _dlg.Top = this.Top;
            _dlg.Left = this.Left - _dlg.Width;
            if( _dlg.Left < 0 )
              _dlg.Left = this.Left + this.ActualWidth;

            return;
          }

        } else {
          _dlg.Top = this.Top - _dlg.Height;
        }

        _dlg.Left = ( this.Left + this.ActualWidth ) - _dlg.Width;


      }
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

      CursorPosition pos = this.GetCursorPosition();

      if( e.LeftButton == MouseButtonState.Pressed ) {
        if( pos == CursorPosition.Body ) {
          this.DragMove();
          SnapWindowToEdge();

        } else WindowTools.ResizeWindow(this, pos);
      }


      //this.MoveOrResizeWindow(e);
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

      if( _sys != null ) {
        _sys.StopMonitoring();
        _sys.Manager.Terminate();
      }
    }

    private void StoreUIState() {

      if( _uiState != null ) {

        _uiState.IsMinimized = _isMinimized;

        _uiState.StoreControlState(btnCmd);
        _uiState.StoreControlState(btnEvent);
        _uiState.StoreControlState(btnMsg);
        _uiState.StoreControlState(btnError);

        _uiState.StoreWindowState(this);

        if( _dlg != null )
          _uiState.StoreWindowState(_dlg);

        _uiState.AlwaysOnTop = Topmost;

        _uiState.Save();

      }
    }
    private void RestoreWindowState() {

      _isMinimized = _uiState.IsMinimized || App.StartMinimized;

      SetAlwaysOnTop(_uiState.AlwaysOnTop);

      if( _dlg != null )
        _uiState.RestoreWindowState(_dlg);

      if( !_uiState.RestoreWindowState(this) )
        SetDefaultWindowPosition();

      UpdateContentWindow();


      if( _isMinimized ) {
        this.Hide();
        this.WindowState = System.Windows.WindowState.Minimized;

        if( _dlg != null ) {
          //_dlg.WindowState = System.Windows.WindowState.Minimized;
          _dlg.Hide();
        }

      }
    }

    private void RestoreQueueButtonsState() {
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

      _mgr.MoveErrorMessageToOriginQueue(itm);
    }
    private void miCopyMessageID_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      Clipboard.SetData(DataFormats.Text, itm.Id);
    }
    private void miCopyMessageName_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      Clipboard.SetData(DataFormats.Text, itm.DisplayName);
    }
    private void miCopyMessageContent_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      Clipboard.SetData(DataFormats.Text, GetQueueItemContent(itm));
    }
    private void miDeleteMessage_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;

      _sys.PurgeMessage(itm);
    }
    private void miDeleteAllMessage_Click(object sender, RoutedEventArgs e) {
      _loadingText = "Processing...";
      _sys.PurgeAllMessages();
    }

    private void miDeleteQueryMessage_Click(object sender, RoutedEventArgs e) {
      _loadingText = "Processing...";
      _sys.PurgeMessages(_sys.Items.Where( i => !i.Processed ));
    }


    private void miDeleteAllErrorMessage_Click(object sender, RoutedEventArgs e) {
      _loadingText = "Processing...";
      _sys.PurgeErrorAllMessages();
    }


    private void HandleMinimizeToTrayClick(Object sender, RoutedEventArgs e) {
      _isMinimized = true;

      StoreUIState();

      Close();
    }


    private void btn_Unchecked(object sender, RoutedEventArgs e) {
      var btn = ( sender as ToggleButton );
      QueueType type = (QueueType)Convert.ToInt32(btn.Tag);

      ChangedMonitorFlag(type, false);

      UpdateButtonLabel(btn);

      lbItems.ItemsSource = _sys.Items;
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

    private void CheckVersion_Click(object sender, RoutedEventArgs e) {
      CheckIfLatestVersion(true);

    }

    private void ShowProcessedMsg_Click(object sender, RoutedEventArgs e) {
      var min = Convert.ToInt32(( e.Source as MenuItem ).Tag as string);
      TimeSpan timeSpan = ( min != 0 ) ? new TimeSpan(0, min, 0) : TimeSpan.FromTicks(DateTime.Today.Ticks);

      LoadProcessedQueueItems(timeSpan);
    }
    private void ShowProcessedMsgPastDays_Click(object sender, RoutedEventArgs e) {
      var days = Convert.ToInt32(( e.Source as MenuItem ).Tag as string);

      LoadProcessedQueueItems(new TimeSpan(days, 0, 0, 0));
    }


    bool _openedByButton = false;

    private void LoadProcessedQueueItems(TimeSpan timeSpan) {
      lbLoading.Content = "Loading...";
      lbLoading.ToolTip = string.Empty;

      lbLoading.Visibility = System.Windows.Visibility.Visible;
      WindowTools.Sleep(10);
      try {
        _sys.RetrieveProcessedQueueItems(timeSpan);
      } finally {
        lbLoading.Visibility = System.Windows.Visibility.Hidden;
      }
    }

    private void DisableListView(string message) {
      lbLoading.Content = message.CutEnd(60);

      if( message.Length > 60 )
        lbLoading.ToolTip = message;

      lbLoading.Visibility = System.Windows.Visibility.Visible;

      lbItems.IsEnabled = false;
      lbItems.Opacity = 0.5;
    }
    private void EnableListView() {
      lbLoading.Visibility = System.Windows.Visibility.Hidden;

      lbItems.IsEnabled = true;
      lbItems.Opacity = 1.0;
    }

    private void btnShowProcessed_Click(object sender, RoutedEventArgs e) {
      var btn = sender as Button;
      var cm = ContextMenuService.GetContextMenu(sender as DependencyObject);
      if( cm != null ) {
        _openedByButton = true;

        var offset = btn.TranslatePoint(new Point(0, 0), this);

        cm.Placement = PlacementMode.Relative;
        cm.HorizontalOffset = this.Left + offset.X;
        cm.VerticalOffset = this.Top + offset.Y + btn.ActualHeight;

        if( FlowDirection.RightToLeft == FlowDirection )
          cm.HorizontalOffset *= -1;

        cm.IsOpen = true;

      }
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e) {

      var btn = btnShowProcessed;
      var cm = ContextMenuService.GetContextMenu(btn as DependencyObject);
      if( cm != null ) {

        if( !_openedByButton ) {
          var offset = btn.TranslatePoint(new Point(0, 0), this);

          cm.Placement = PlacementMode.AbsolutePoint;
          cm.HorizontalOffset = this.Left + offset.X;
          cm.VerticalOffset = this.Top + offset.Y + btn.ActualHeight;

          if( FlowDirection.RightToLeft == FlowDirection )
            cm.HorizontalOffset *= -1;
        }

      }
    }
    private void ContextMenu_Closed(object sender, RoutedEventArgs e) {

      _openedByButton = false;
    }

    private void miResendCommand_Click(object sender, RoutedEventArgs e) {
      QueueItem itm = ( (MenuItem)sender ).Tag as QueueItem;
      ISendCommand mgr = _sys.Manager as ISendCommand;

      if( itm.Messages.Length == 1 ) {

        object cmd = mgr.DeserializeCommand(GetQueueItemContent(itm), Type.GetType(itm.Messages[0].AssemblyQualifiedName));

        if( cmd != null ) {
          var dlg = new SendCommandWindow(_sys);

          dlg.Show();
          dlg.SetCommand(cmd);
        }

      } else MessageBox.Show("ServiceBus MQ Manager Don't support messages with multiple commands yet.", "Resend Command", MessageBoxButton.OK, MessageBoxImage.Asterisk);
    }

    private void tbSearchString_FilterChanged(object sender, Controls.FilterChangedRoutedEventArgs e) {
      if( _sys != null ) {
        if( tbSearchString.Visibility == System.Windows.Visibility.Collapsed )
          tbSearchString.Visibility = System.Windows.Visibility.Visible;

        _sys.FilterItems(e.Filter);
      }
    }
    private void tbSearchString_FilterCleared(object sender, RoutedEventArgs e) {
      if( _sys != null ) {
        if( tbSearchString.Visibility == System.Windows.Visibility.Visible )
          tbSearchString.Visibility = System.Windows.Visibility.Collapsed;

        _sys.ClearFilter();
      }
    }


  }
}
