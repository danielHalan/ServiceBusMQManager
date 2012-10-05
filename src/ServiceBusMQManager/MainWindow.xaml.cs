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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using ServiceBusMQ;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;

namespace ServiceBusMQManager {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    private static readonly string[] BUTTON_LABELS = new string[] { "COMMANDS", "EVENTS", "MESSAGES", "ERRORS" };
    private static readonly char SPACE_SEPARATOR = ' ';
    private static readonly List<QueueItem> EMPTY_LIST = new List<QueueItem>();

    private IMessageManager _mgr;
    private UIStateConfig _uiCfg = new UIStateConfig();

    private HwndSource _hwndSource;
    private System.Windows.Forms.NotifyIcon _notifyIcon;

    private bool _showOnNewMessages = true;

    private ContentWindow _dlg;
    private bool _dlgShown = false;


    public MainWindow() {
      InitializeComponent();

      SourceInitialized += Window_SourceInitialized;

      CreateNotifyIcon();
    }

    private void Window_SourceInitialized(object sender, EventArgs e) {

      lbTitle.Content = Title;

      _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
    }
    private void Window_Loaded(object sender, RoutedEventArgs e) {
      var appSett = ConfigurationManager.AppSettings;

      _mgr = MessageBusFactory.Create(appSett["messageBus"], appSett["messageBusQueueType"]);
      _dlg = new ContentWindow();
      _showOnNewMessages = Convert.ToBoolean(appSett["showOnNewMessages"] ?? "false");
      
      RestoreUIState();

      this.Icon = BitmapFrame.Create(_GetImageResourceStream("main.ico"));


      var serverName = !string.IsNullOrEmpty(appSett["server"]) ? appSett["server"] : Environment.MachineName;
      var watchEventQueues = GetQueueNamesFromConfig("event.queues");
      var watchCommandQueues = GetQueueNamesFromConfig("command.queues");
      var watchMessageQueues = GetQueueNamesFromConfig("message.queues");
      var watchErrorQueues = GetQueueNamesFromConfig("error.queues");

      if( watchEventQueues.Length == 0 && watchCommandQueues.Length == 0 && watchMessageQueues.Length == 0 ) {
        MessageBox.Show("No queues has been configured. \n\nPlease add the queues you want to monitor in ServiceBusMQManager.exe.config, and try again.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Application.Current.Shutdown();
      }


      _mgr.Init(serverName, watchCommandQueues, watchEventQueues, watchMessageQueues, watchErrorQueues);
      _mgr.ItemsChanged += MessageMgr_ItemsChanged;

      lbItems.ItemsSource = _mgr.Items;

      SetupContextMenu();

      SetupQueueMonitorTimer(Convert.ToInt32(appSett["interval"] ?? "700"));
    }
    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
      if( (bool)e.NewValue )
        SetSelectedItem((QueueItem)lbItems.SelectedItem);
    }


    private string[] GetQueueNamesFromConfig(string name) {
      return ConfigurationManager.AppSettings[name].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
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
      _notifyIcon.Icon = new System.Drawing.Icon(_GetImageResourceStream("trayIcon.ico"));
      _notifyIcon.DoubleClick +=
          delegate(object sender, EventArgs args) {
            if( !this.IsVisible ) {
              this.Show();
              this.WindowState = WindowState.Normal;
            } else {
              this.Activate();

              if( _dlg != null )
                _dlg.Activate();
            }

          };

      _notifyIcon.Visible = true;
    }

    bool _showingActivityTrayIcon;
    void ShowActivityTrayIcon() {
      
      if( !_showingActivityTrayIcon ) {     
        _showingActivityTrayIcon = true;

        Thread thread = new Thread(new ThreadStart(delegate() {
 
          Thread.Sleep(200); // this is important ...
          try {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                new Action(delegate() {
                  _notifyIcon.Icon = new System.Drawing.Icon(_GetImageResourceStream("trayIconActivity.ico"));
            }));
            Thread.Sleep(500); 

            this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                new Action(delegate() {
                  _notifyIcon.Icon = new System.Drawing.Icon(_GetImageResourceStream("trayIcon.ico"));
            }));

            _showingActivityTrayIcon = false;
          } catch { }
        }));
        thread.Name = "thread-updateTrayIcon";
        thread.Start();
      }
    }


    private void SetupQueueMonitorTimer(int ms) {
      var timer = new DispatcherTimer();
      timer.Interval = TimeSpan.FromMilliseconds(ms);
      timer.Tick += timer_Tick;

      timer.Start();
    }

    private void UpdateButtonLabel(ToggleButton btn) {
      int iType = Convert.ToInt32(btn.Tag);
      QueueType type = (QueueType)iType;

      if( btn.IsChecked == true ) {

        int iCount = _mgr.Items.Count( i => i.QueueType == type && !i.Deleted );

        string count = string.Format("({0})", iCount);
        if( !( btn.Content as string ).Contains(count) )
          btn.Content = string.Concat(BUTTON_LABELS[iType],SPACE_SEPARATOR, count);

      } else {
        btn.Content = BUTTON_LABELS[iType];
      }

    }

    private void MessageMgr_ItemsChanged(object sender, EventArgs e) {


      // Update button labels
      UpdateButtonLabel(btnCmd);
      UpdateButtonLabel(btnEvent);
      UpdateButtonLabel(btnMsg);
      UpdateButtonLabel(btnError);

      // Update List View
      lbItems.Items.Refresh();

      ShowActivityTrayIcon();

      // Show Window
      if( _showOnNewMessages && !this.IsVisible )
        this.Show();
    }
    private void timer_Tick(object sender, EventArgs e) {
      _mgr.RefreshQueueItems();
    }


    private Stream _GetImageResourceStream(string name) {
      return this.GetType().Assembly.GetManifestResourceStream("ServiceBusMQManager.Images." + name);
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

        if( !_dlg.IsVisible && _dlgShown ) {
          _dlg = new ContentWindow();
          _dlg.Topmost = Topmost;

          _dlg.Width = _uiCfg.ContentWindowRect.Width;
          _dlg.Height = _uiCfg.ContentWindowRect.Height;
        }

        _dlg.SetContent(_mgr.LoadMessageContent(itm));

        _dlg.Left = ( this.Left + this.Width ) - _dlg.Width;
        _dlg.Top = this.Top - _dlg.Height;
        _dlg.SetTitle(itm.Id);


        _dlg.Show();

        _dlgShown = true;

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
      imgAlwaysOnTop.Source = value ? BitmapFrame.Create(_GetImageResourceStream("pinned.png")) : BitmapFrame.Create(_GetImageResourceStream("unpinned.png"));
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

    private void btnClearDeleted_Click(object sender, RoutedEventArgs e) {

      _mgr.ClearDeletedItems();

      lbItems.Items.Refresh();
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

      switch( this.GetCursorPosition() ) {
        case CursorPosition.Top:
          Cursor = Cursors.SizeNS;
          break;
        case CursorPosition.Bottom:
          Cursor = Cursors.SizeNS;
          break;
        case CursorPosition.Left:
          Cursor = Cursors.SizeWE;
          break;
        case CursorPosition.Right:
          Cursor = Cursors.SizeWE;
          break;
        case CursorPosition.TopLeft:
          Cursor = Cursors.SizeNWSE;
          break;
        case CursorPosition.TopRight:
          Cursor = Cursors.SizeNESW;
          break;
        case CursorPosition.BottomLeft:
          Cursor = Cursors.SizeNESW;
          break;
        case CursorPosition.BottomRight:
          Cursor = Cursors.SizeNWSE;
          break;

        default:
          Cursor = Cursors.Arrow;
          break;
      }

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

      CursorPosition pos = this.GetCursorPosition();

      if( e.LeftButton == MouseButtonState.Pressed ) {
        if( pos == CursorPosition.Body )
          DragMove();
        else ResizeWindow(pos);
      }
    }
    private void ResizeWindow(CursorPosition pos) {
      Native.SendMessage(_hwndSource.Handle, Native.WM_SYSCOMMAND, (IntPtr)( 61440 + pos ), IntPtr.Zero);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      e.Cancel = true;

      if( _dlg != null && _dlg.IsVisible )
        _dlg.Hide();

      this.Hide();
    }
    private void Window_Closed(object sender, EventArgs e) {
      _notifyIcon.Visible = false;

      StoreUIState();
    }


    private void StoreUIState() {

      _uiCfg.UpdateButtonState(this);
      
      _uiCfg.UpdateMainWindowState(this);
      _uiCfg.UpdateContentWindowState(_dlg);
      
      _uiCfg.UpdateAlwaysOnTop(Topmost);
      
      _uiCfg.Save();

    }
    private void RestoreUIState() {

      _uiCfg.Load();

      SetAlwaysOnTop(_uiCfg.AlwaysOnTop);

      if( !_uiCfg.MainWindowRect.IsEmpty ) {

        this.Left = _uiCfg.MainWindowRect.Left;
        this.Top = _uiCfg.MainWindowRect.Top;
        this.Width = _uiCfg.MainWindowRect.Width;
        this.Height = _uiCfg.MainWindowRect.Height;
      
      } else SetDefaultWindowPosition();

      if( !_uiCfg.ContentWindowRect.IsEmpty ) {

        _dlg.Width = _uiCfg.ContentWindowRect.Width;
        _dlg.Height = _uiCfg.ContentWindowRect.Height;
      } 


      string selected = _uiCfg.SelectedQueues;

      btnCmd.IsChecked = selected.Contains("commands");
      btnEvent.IsChecked = selected.Contains("events");
      btnMsg.IsChecked = selected.Contains("messages");
      btnError.IsChecked = selected.Contains("errors");
    }

    private void miClose_Click(object sender, EventArgs e) {
      Application.Current.Shutdown();
    }

    private void miToggleAlwaysOnTop_Click(object sender, RoutedEventArgs e) {
    
      SetAlwaysOnTop(!Topmost);
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




  }
}
