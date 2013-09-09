#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ConfigWindow.xaml.cs
  Created: 2012-12-02

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Ookii.Dialogs.Wpf;
using ServiceBusMQ;
using ServiceBusMQ.Configuration;
using ServiceBusMQ.Manager;
using ServiceBusMQ.Model;
using ServiceBusMQManager.Controls;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for ConfigWindow.xaml
  /// </summary>
  public partial class ConfigWindow : Window {

    const int ROW_QUEUES_INFO = 3;
    const int ROW_QUEUES1 = 4;
    const int ROW_QUEUES2 = 5;
    const int ROW_SENDCMD_INFO = 7;
    const int ROW_ASMPATH = 8;

    SbmqSystem _sys;
    SystemConfig2 _config;
    ServiceBusFactory.ServiceBusManagerType[] _managerTypes;
    Dictionary<string, string[]> _allQueueNames = new Dictionary<string, string[]>();

    bool _updatingServer = false;


    ObservableCollection<ServerConfig2> _servers = new ObservableCollection<ServerConfig2>();


    public ConfigWindow(SbmqSystem system, bool showSendCommand = false) {
      InitializeComponent();

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      _sys = system;
      _config = system.Config;


      _managerTypes = ServiceBusFactory.AvailableServiceBusManagers();

      cbServiceBus.ItemsSource = _managerTypes;
      cbServiceBus.DisplayMemberPath = "Name";
      cbServiceBus.SelectedValuePath = "Name";
      cbServiceBus.SelectedIndex = 0;

      cbTransport.ItemsSource = _managerTypes[0].QueueTypes;

      cbContentFormat.ItemsSource = _managerTypes[0].MessageContentTypes;
      cbContentFormat.SelectedItem = _config.CommandContentType;

      cShowOnNewMessages.IsChecked = _config.ShowOnNewMessages;
      cCheckForNewVer.IsChecked = _config.VersionCheck.Enabled;


      asmPaths.BindItems(_config.CommandsAssemblyPaths);


      BindServers(_config.Servers);

      tbInterval.Init(750, typeof(int), false);

      SelectServer(_config.MonitorServer);


      tbServer.Init(string.Empty, typeof(string), false);
      tbServer.Visibility = System.Windows.Visibility.Hidden;

      tbNamespace.Init(_config.CommandDefinition.NamespaceContains, typeof(string), true);

      tbCmdInherits.Text = _config.CommandDefinition.InheritsType;

	  tbSubscriptionServiceQueue.Text = _config.MassTransitServiceSubscriptionQueue;

      UpdateSendCommandInfo(false);
      UpdateQueueuInfo(false);

      if( showSendCommand )
        scroller.ScrollToBottom();

    }

    private void BindServers(List<ServerConfig2> list) {
      _servers.Clear();

      list.ForEach(s => _servers.Add(s));

      cbServers.ItemsSource = _servers;
      cbServers.DisplayMemberPath = "Name";
      cbServers.SelectedValuePath = "Name";
    }

    private void frmConfig_SourceInitialized(object sender, EventArgs e) {
      SbmqSystem.UIState.RestoreWindowState(this);

      ValidateHeight();

      GetAllAvailableQueueNamesForServer(_config.CurrentServer.Name);
    }

    private void GetAllAvailableQueueNamesForServer(string serverName) {

      if( !_allQueueNames.ContainsKey(serverName) ) {

        _SetAccessingServer(true);
        UpdateConfigWindowUIState();

        BackgroundWorker w = new BackgroundWorker();
        w.DoWork += (s, arg) => {
          _allQueueNames.Add(serverName, GetDiscoveryService().GetAllAvailableQueueNames(serverName) );
        };
        w.RunWorkerCompleted += (s, arg) => {
          _SetAccessingServer(false);
        };

        w.RunWorkerAsync();

      }
    }

    private void ValidateHeight() {
      var s = WpfScreen.GetScreenFrom(this);

      if( this.Height > s.WorkingArea.Height ) {
        this.Top = s.WorkingArea.Top;
        this.Height = s.WorkingArea.Height;
      }
    }
    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }
    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }
    private void HandleMaximizeClick(object sender, RoutedEventArgs e) {
      var s = WpfScreen.GetScreenFrom(this);

      this.Top = s.WorkingArea.Top;
      this.Height = s.WorkingArea.Height;
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }


    IServiceBusDiscovery _disc = null;

    private IServiceBusDiscovery GetDiscoveryService() {
      if( _disc == null )
        _disc = _sys.GetDiscoveryService();

      return _disc;
    }



    private void Queue_AddItem_1(object sender, QueueListItemRoutedEventArgs e) {
      QueueListControl s = sender as QueueListControl;

      SelectQueueDialog dlg = new SelectQueueDialog(GetDiscoveryService(), cbServers.SelectedValue as string, GetAllQueueNames().Except(s.GetItems().Select(i => i.Name).ToList()).OrderBy( name => name ).ToArray());
      dlg.Title = "Select " + s.Title.Remove(s.Title.Length - 1);
      dlg.Owner = this;
      
      if( dlg.ShowDialog() == true ) {
        e.Handled = true;

        dlg.SelectedQueueNames.ForEach(queueName =>
            {
                var color = !s.Name.EndsWith("Errors") ? QueueColorManager.GetRandomAvailableColor() : Color.FromArgb(QueueColorManager.RED);
                e.Items.Add(new QueueListControl.QueueListItem(queueName, color));
            });
      }
    }

    private string[] GetAllQueueNames() {
      var name = _config.CurrentServer.Name;

      return _allQueueNames[name];
    }

    private void QueueListControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      QueueListControl s = sender as QueueListControl;
      var grid = s.Parent as Grid;

      int ROW = ROW_QUEUES1;

      double[] max = new double[2] { 0, 0 };
      foreach( var c in grid.Children.OfType<QueueListControl>().Where(c => c.Name.StartsWith("queue")) ) {

        if( !double.IsNaN(c.Height) ) {
          int row = Grid.GetRow(c) - ROW;
          max[row] = Math.Max(max[row], c.Height);
        }
      }

      for( int i = 0; i < max.Length; i++ ) {
        if( max[i] > 0 )
          grid.RowDefinitions[ROW + i].Height = new GridLength(max[i]);
      }

    }

    private void asmPaths_SizeChanged(object sender, SizeChangedEventArgs e) {
      StringListControl s = sender as StringListControl;
      var grid = s.Parent as Grid;

      double max = 0;

      if( !double.IsNaN(s.Height) ) {
        max = Math.Max(max, s.Height);
      }

      if( max > 0 )
        grid.RowDefinitions[ROW_ASMPATH].Height = new GridLength(max);
    }
    private void asmPaths_AddItem(object sender, StringListItemRoutedEventArgs e) {

      VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
      dialog.Description = "Please select Command Assembly Folder";
      dialog.UseDescriptionForTitle = true;

      if( (bool)dialog.ShowDialog(this) ) {


        if( !asmPaths.GetItems().Any( p => string.Compare(p, dialog.SelectedPath, true) == 0 ) ) {

          e.Item = dialog.SelectedPath;
          e.Handled = true;
        }
      }

    }

    private void UpdateSendCommandInfo(bool animate = true) {
      StringBuilder sb = new StringBuilder();

      if( asmPaths.ItemsCount > 0 ) {

        CommandDefinition cmdDef = new CommandDefinition();

        if( tbCmdInherits.Text.IsValid() )
          cmdDef.InheritsType = tbCmdInherits.Text;

        var cmdNamespace = tbNamespace.RetrieveValue<string>();
        if( cmdNamespace.IsValid() )
          cmdDef.NamespaceContains = cmdNamespace;

        var mw = App.Current.MainWindow as MainWindow;

        var cmds = _sys.GetAvailableCommands(asmPaths.GetItems(), cmdDef, !animate); // !animate = on dialog startup

        lbCmdsFound.Content = "{0} Commands Found".With(cmds.Length);
          
        if( cmds.Length == 0 ) {

          sb.Append("No commands found ");

          if( cmdDef.InheritsType.IsValid() )
            sb.Append("that inherits " + cmdDef.InheritsType.Substring(0, cmdDef.InheritsType.IndexOf(',')).CutBeginning(40));

          if( cmdDef.NamespaceContains.IsValid() ) {

            if( cmdDef.InheritsType.IsValid() )
              sb.Append(" or ");
            else sb.Append("that ");

            sb.AppendFormat("contains '{0}' in Namespace", cmdDef.NamespaceContains);

          }

          sb.Append(", make sure your Command Definition is correct");
        }

      } else {
        sb.Append("You need to add atleast one assembly path to be able to send commands");
        lbCmdsFound.Content = string.Empty;
      }


      if( sb.Length > 0 ) {
        lbSendCommandInfo.Text = sb.ToString();
      }

      UpdateInfoBox(sb.Length > 0, animate, ROW_SENDCMD_INFO, ConfigWindow.SendCommandInfoHeightProperty);
    }
    private void UpdateQueueuInfo(bool animate = true) {
      bool valid = queueCommands.ItemsCount == 0 &&
                    queueEvents.ItemsCount == 0 &&
                    queueMessages.ItemsCount == 0 &&
                    queueErrors.ItemsCount == 0;


      UpdateInfoBox(valid, animate, ROW_QUEUES_INFO, ConfigWindow.QueuesInfoHeightProperty);
    }

    private void UpdateInfoBox(bool expand, bool animate, int rowIndex, DependencyProperty property) {
      var row = theGrid.RowDefinitions[rowIndex];

      if( expand ) {

        if( row.Height.Value == 0 ) {

          if( animate )
            AnimateControlHeight(0, 70, property);
          else row.Height = new GridLength(70);
        }


      } else {
        if( row.Height.Value > 0 ) {

          if( animate )
            AnimateControlHeight(row.Height.Value, 0, property);
          else row.Height = new GridLength(0);
        }
      }


    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {

      SaveConfig();

      DialogResult = true;
    }

    private void SelectServer(string name) {

      var s = _config.Servers.SingleOrDefault(sv => sv.Name == name);

      if( s != null ) {

        _updatingServer = true;

        cbServers.SelectedValue = name;

        cbServiceBus.SelectedValue = s.MessageBus;
        cbTransport.SelectedValue = s.MessageBusQueueType;

        tbInterval.UpdateValue(s.MonitorInterval);

        queueCommands.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Command).Select(
                                        q => new QueueListControl.QueueListItem(q.Name, q.Color)));
        queueEvents.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Event).Select(
                                        q => new QueueListControl.QueueListItem(q.Name, q.Color)));
        queueMessages.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Message).Select(
                                        q => new QueueListControl.QueueListItem(q.Name, q.Color)));
        queueErrors.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Error).Select(
                                        q => new QueueListControl.QueueListItem(q.Name, q.Color)));

        _updatingServer = false;
      }

      UpdateServerButtonState(name);
    }


    private void SaveServerConfig(string name) {
      var currServer = _config.Servers.Single(s => s.Name == name);

      currServer.MessageBus = cbServiceBus.SelectedValue as string;
      currServer.MessageBusQueueType = cbTransport.SelectedItem as string;

      currServer.MonitorInterval = (int)tbInterval.RetrieveValue();

      List<QueueConfig> monitorQueues = new List<QueueConfig>();
      monitorQueues.AddRange(queueCommands.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Command, n.Color.ToArgb())));
      monitorQueues.AddRange(queueEvents.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Event, n.Color.ToArgb())));
      monitorQueues.AddRange(queueMessages.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Message, n.Color.ToArgb())));
      monitorQueues.AddRange(queueErrors.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Error, n.Color.ToArgb())));

      currServer.MonitorQueues = monitorQueues.ToArray();
    }

    private void SaveConfig() {

      var serverName = cbServers.SelectedValue as string;
      SaveServerConfig(serverName);
      _config.MonitorServer = serverName;

      _config.ShowOnNewMessages = cShowOnNewMessages.IsChecked == true;
      _config.VersionCheck.Enabled = cCheckForNewVer.IsChecked == true;

      _config.CommandDefinition.InheritsType = tbCmdInherits.Text;
      _config.CommandDefinition.NamespaceContains = tbNamespace.RetrieveValue() as string;

      _config.CommandsAssemblyPaths = asmPaths.GetItems();
      _config.CommandContentType = cbContentFormat.SelectedItem as string;
	  _config.MassTransitServiceSubscriptionQueue = tbSubscriptionServiceQueue.Text;

      _config.Save();

    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      SelectDataTypeDialog dlg = new SelectDataTypeDialog(_sys, asmPaths.GetItems());
      dlg.Owner = this;

      if( dlg.ShowDialog() == true ) {

        tbCmdInherits.Text = dlg.SelectedType.QualifiedName;

        UpdateSendCommandInfo();
      }

    }

    private void frmConfig_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      SbmqSystem.UIState.StoreWindowState(this);
    }

    void worker_TryAccessServer(object sender, DoWorkEventArgs e) {
      var name = e.Argument as string;
      try {
        if( _allQueueNames.ContainsKey(name) )
          _allQueueNames.Remove(name);

        _allQueueNames.Add(name, GetDiscoveryService().GetAllAvailableQueueNames(name));
        e.Result = true;

      } catch {
        e.Result = false;
      }
    }


    bool _creatingServer = false;
    private bool _accessingServer;

    private void AddServer_Click(object sender, RoutedEventArgs e) {
      tbServer.Visibility = System.Windows.Visibility.Visible;
      cbServers.Visibility = System.Windows.Visibility.Hidden;
      lbServerInfo.Content = string.Empty;

      _creatingServer = true;

      queueCommands.BindItems(null);
      queueEvents.BindItems(null);
      queueMessages.BindItems(null);
      queueErrors.BindItems(null);

      UpdateServerButtonState(string.Empty);

      UpdateConfigWindowUIState();


      tbServer.Focus();
    }

    System.Windows.Visibility _prevServerActionVisibility;
    void _SetAccessingServer(bool value) {
      _accessingServer = value;

      if( _accessingServer ) {

        cbServers.IsEnabled = false;
        imgServerLoading.Visibility = System.Windows.Visibility.Visible;

        _prevServerActionVisibility = btnServerAction.Visibility;
        btnServerAction.Visibility = System.Windows.Visibility.Hidden;

      } else {
        cbServers.IsEnabled = true;

        imgServerLoading.Visibility = System.Windows.Visibility.Hidden;
        btnServerAction.Visibility = _prevServerActionVisibility;
      }

      UpdateConfigWindowUIState();
    }

    private void UpdateConfigWindowUIState() {

      lbQueues.IsEnabled = !_creatingServer && !_accessingServer;
      queueCommands.IsEnabled = !_creatingServer && !_accessingServer;
      queueEvents.IsEnabled = !_creatingServer && !_accessingServer;
      queueMessages.IsEnabled = !_creatingServer && !_accessingServer;
      queueErrors.IsEnabled = !_creatingServer && !_accessingServer;
    }
    private void ServerAction_Click(object sender, RoutedEventArgs e) {
      var btn = sender as RoundMetroButton;

      if( (int?)btn.Tag == 1 ) { // Save

        SaveNewServer();

      } else if( (int?)btn.Tag == 2 ) { // Delete 

        DeleteCurrentServer();
      }

    }

    private void DeleteCurrentServer() {
      _servers.Remove(_sys.Config.CurrentServer);

      _sys.Config.Servers.Remove(_sys.Config.CurrentServer);
      _sys.Config.MonitorServer = _sys.Config.Servers[0].Name;

      cbServers.SelectedValue = _sys.Config.MonitorServer;

      SelectServer(_sys.Config.MonitorServer);
    }

    private void SaveNewServer() {
      string name = tbServer.RetrieveValue<string>();

      if( _sys.Config.Servers.Any( s => string.Compare(s.Name, name, true) == 0 ) ) {
        
        MessageBox.Show("Connection for this server already exist, Aborting...", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

        cbServers.Visibility = System.Windows.Visibility.Visible;
        tbServer.Visibility = System.Windows.Visibility.Hidden;
        tbServer.UpdateValue(string.Empty);
        _HideServerButton();

        SelectServer(name);
        return;
      }

      TryAccessServer(name, () => { // Success
        var s = new ServerConfig2();
        s.Name = name;
        s.MonitorInterval = tbInterval.RetrieveValue<int>();
        s.MessageBus = cbServiceBus.SelectedValue as string;
        s.MessageBusQueueType = cbTransport.SelectedItem as string;
        s.MonitorQueues = new QueueConfig[0];

        _sys.Config.Servers.Add(s);
        _sys.Config.MonitorServer = s.Name;

        _servers.Add(s);
      });
    }

    void _ShowSaveServerButton() {
      btnServerAction.Source = "/ServiceBusMQManager;component/Images/save-white.png";
      btnServerAction.Tag = 1;
      btnServerAction.Visibility = System.Windows.Visibility.Visible;
    }
    void _ShowDeleteServerButton() {
      btnServerAction.Source = "/ServiceBusMQManager;component/Images/delete-item-white.png";
      btnServerAction.Tag = 2;
      btnServerAction.Visibility = System.Windows.Visibility.Visible;
    }
    void _HideServerButton() {
      btnServerAction.Visibility = System.Windows.Visibility.Hidden;
    }

    void UpdateServerButtonState(string serverName) {

      if( !_creatingServer ) {

        if( Tools.IsLocalHost(serverName) )
          _HideServerButton();
        else _ShowDeleteServerButton();

      } else _ShowSaveServerButton();

    }
    private void TryAccessServer(string name, Action onSuccess) {
      btnServerAction.Visibility = System.Windows.Visibility.Hidden;
      imgServerLoading.Visibility = System.Windows.Visibility.Visible;

      this.IsEnabled = false;

      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += worker_TryAccessServer;
      worker.RunWorkerCompleted += (s, e) => {
        _creatingServer = false;
        UpdateConfigWindowUIState();

        cbServers.Visibility = System.Windows.Visibility.Visible;
        tbServer.Visibility = System.Windows.Visibility.Hidden;
        tbServer.UpdateValue(string.Empty);

        if( !( (bool)e.Result ) ) { // failed
          lbServerInfo.Content = "Could not access server " + name;

          SelectServer(cbServers.SelectedValue as string);

        } else { 
          onSuccess();
          
          SelectServer(name);
        }


        imgServerLoading.Visibility = System.Windows.Visibility.Hidden;
        this.IsEnabled = true;
      };

      worker.RunWorkerAsync(name);
    }


    private void cbServers_SelectionChanged(object sender, SelectionChangedEventArgs e) {

      if( !_updatingServer ) {


        SaveServerConfig(_config.MonitorServer);

        _config.MonitorServer = cbServers.SelectedValue as string;

        var s = e.AddedItems[0] as ServerConfig;

        if( GetDiscoveryService().CanAccessServer(s.Name) ) {

          SelectServer(s.Name);

          GetAllAvailableQueueNamesForServer(s.Name);
        } else throw new Exception("Can not access Server, " + s.Name);
      }
    }

    private void asmPaths_RemovedItem(object sender, StringListItemRoutedEventArgs e) {
      UpdateSendCommandInfo();
    }

    private void asmPaths_AddedItem(object sender, StringListItemRoutedEventArgs e) {
      UpdateSendCommandInfo();
    }



    void AnimateControlHeight(double from, double to, DependencyProperty heightAttribute) {

      var sb = new Storyboard();

      DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();

      EasingDoubleKeyFrame key = new EasingDoubleKeyFrame();
      key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
      key.Value = from;
      anim.KeyFrames.Add(key);

      key = new EasingDoubleKeyFrame();
      key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));
      key.Value = to;
      anim.KeyFrames.Add(key);

      sb.Children.Add(anim);

      Storyboard.SetTarget(anim, this);
      Storyboard.SetTargetProperty(anim, new PropertyPath(heightAttribute));

      sb.Begin();
    }



    public static readonly DependencyProperty SendCommandInfoHeightProperty = DependencyProperty.Register(
             "SendCommandInfoHeight", typeof(double), typeof(ConfigWindow), new PropertyMetadata(0.0));

    public static readonly DependencyProperty QueuesInfoHeightProperty = DependencyProperty.Register(
             "QueuesInfoHeight", typeof(double), typeof(ConfigWindow), new PropertyMetadata(0.0));

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
      base.OnPropertyChanged(e);

      if( ReferenceEquals(e.Property, SendCommandInfoHeightProperty) )
        theGrid.RowDefinitions[ROW_SENDCMD_INFO].Height = new GridLength((double)e.NewValue);

      else if( ReferenceEquals(e.Property, QueuesInfoHeightProperty) )
        theGrid.RowDefinitions[ROW_QUEUES_INFO].Height = new GridLength((double)e.NewValue);

    }



    private void queue_AddedItem(object sender, QueueListItemRoutedEventArgs e) {
      UpdateQueueuInfo();
    }

    private void queue_RemovedItem(object sender, QueueListItemRoutedEventArgs e) {
      UpdateQueueuInfo();
    }

    private void tbNamespace_LostFocus(object sender, RoutedEventArgs e) {
      UpdateSendCommandInfo();
    }


  }
}
