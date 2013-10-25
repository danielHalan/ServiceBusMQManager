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

    public class AddServerResult {
      public string Name { get { return Server.Name; } }
      public string[] AllQueueNames { get; set; }

      public ServerConfig3 Server { get; set; }
    }

    const int ROW_QUEUES_INFO = 3;
    const int ROW_QUEUES1 = 4;
    const int ROW_QUEUES2 = 5;
    const int ROW_SENDCMD_INFO = 7;
    const int ROW_ASMPATH = 8;

    SbmqSystem _sys;
    SystemConfig3 _config;
    ServiceBusFactory.ServiceBusManagerType[] _managerTypes;
    Dictionary<string, string[]> _allQueueNames = new Dictionary<string, string[]>();

    bool _updatingServer = false;


    ObservableCollection<ServerConfig3> _servers = new ObservableCollection<ServerConfig3>();

    private ServerConfig3 CurrentServer {
      get {
        return cbServers.SelectedItem as ServerConfig3;
      }
    }

    public ConfigWindow(SbmqSystem system, bool showSendCommand = false) {
      InitializeComponent();

      Topmost = SbmqSystem.UIState.AlwaysOnTop;

      _sys = system;
      _config = system.Config;


      _managerTypes = ServiceBusFactory.AvailableServiceBusManagers();


      cbContentFormat.ItemsSource = _managerTypes[0].MessageContentTypes;
      cbContentFormat.SelectedItem = _config.CurrentServer.CommandContentType;

      cShowOnNewMessages.IsChecked = _config.ShowOnNewMessages;
      cCheckForNewVer.IsChecked = _config.VersionCheck.Enabled;


      BindServers(_config.Servers);

      SelectServer(_config.MonitorServerName, false);

      //tbServer.Init(string.Empty, typeof(string), false);
      //tbServer.Visibility = System.Windows.Visibility.Hidden;

      BindSendCommandView(_config.CurrentServer);

      UpdateSendCommandInfo(false);
      UpdateQueueuInfo(false);

      if( showSendCommand )
        scroller.ScrollToBottom();

    }

    private void BindSendCommandView(ServerConfig3 server) {
      asmPaths.BindItems(server.CommandsAssemblyPaths);

      tbNamespace.Init(server.CommandDefinition.NamespaceContains, typeof(string), true);

      tbCmdInherits.Text = server.CommandDefinition.InheritsType;
    }

    private void BindServers(List<ServerConfig3> list) {
      _servers.Clear();

      list.ForEach(s => _servers.Add(s));

      cbServers.ItemsSource = _servers;
      cbServers.DisplayMemberPath = "Name";
      cbServers.SelectedValuePath = "Name";
    }

    private void frmConfig_SourceInitialized(object sender, EventArgs e) {
      SbmqSystem.UIState.RestoreWindowState(this);

      ValidateHeight();

      GetAllAvailableQueueNamesForServer(_config.CurrentServer);

    }

    private void ValidateQueues(ServerConfig3 s) {
      if( _allQueueNames != null && _allQueueNames.ContainsKey(s.Name) ) {
        var allQueues = _allQueueNames[s.Name];

        var removedQueues = s.MonitorQueues.Where(q => !allQueues.Any(x => q.Name == x)).ToArray();
        if( removedQueues.Any() ) {
          MessageDialog.Show(MessageType.Info, "Removed Queues", "These queues could not be found at the server and therefore removed from monitoring:\n" + removedQueues.AsString(",\n"));

          s.MonitorQueues = s.MonitorQueues.Where(q => allQueues.Any(x => q.Name == x)).ToArray();
          _config.Save();

          UpdateQueueLists(s);
        }
      }
    }


    private void GetAllAvailableQueueNamesForServer(ServerConfig3 server) {

      if( !_allQueueNames.ContainsKey(server.Name) ) {

        _SetAccessingServer(true);

        BackgroundWorker w = new BackgroundWorker();
        w.DoWork += (s, arg) => {
          _allQueueNames.Add(server.Name, GetDiscoveryService(server).GetAllAvailableQueueNames(server.ConnectionSettings));
        };
        w.RunWorkerCompleted += (s, arg) => {
          ValidateQueues(server);
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


    Dictionary<string, IServiceBusDiscovery> _disc = new Dictionary<string, IServiceBusDiscovery>();

    private IServiceBusDiscovery GetDiscoveryService(string messageBus, string version, string queueType) {
      var disc = _disc.GetValue(messageBus + queueType);

      if( disc == null ) {
        disc = _sys.GetDiscoveryService(messageBus, version, queueType);
        _disc.Add(messageBus + queueType, disc);
      }

      return disc;
    }
    private IServiceBusDiscovery GetDiscoveryService(ServerConfig3 s) {
      return GetDiscoveryService(s.ServiceBus, s.ServiceBusVersion, s.ServiceBusQueueType);
    }




    private void Queue_AddItem_1(object sender, QueueListItemRoutedEventArgs e) {
      QueueListControl s = sender as QueueListControl;
      var srv = CurrentServer;

      SelectQueueDialog dlg = new SelectQueueDialog(GetDiscoveryService(srv), srv, GetAllQueueNames().Except(s.GetItems().Select(i => i.Name).ToList()).OrderBy(name => name).ToArray());
      dlg.Title = "Select " + s.Title.Remove(s.Title.Length - 1);
      dlg.Owner = this;

      if( dlg.ShowDialog() == true ) {
        e.Handled = true;

        dlg.SelectedQueueNames.ForEach(queueName => {
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


        if( !asmPaths.GetItems().Any(p => string.Compare(p, dialog.SelectedPath, true) == 0) ) {

          e.Item = dialog.SelectedPath;
          e.Handled = true;
        }
      }

    }

    private void UpdateSendCommandInfo(bool animate = true) {
      StringBuilder sb = new StringBuilder();

      try {

        if( asmPaths.ItemsCount > 0 ) {

          CommandDefinition cmdDef = new CommandDefinition();

          if( tbCmdInherits.Text.IsValid() )
            cmdDef.InheritsType = tbCmdInherits.Text;

          var cmdNamespace = tbNamespace.RetrieveValue<string>();
          if( cmdNamespace.IsValid() )
            cmdDef.NamespaceContains = cmdNamespace;

          var mw = App.Current.MainWindow as MainWindow;


          if( !ServiceBusFactory.CanSendCommand(CurrentServer.ServiceBus, CurrentServer.ServiceBusVersion, CurrentServer.ServiceBusQueueType) ) {
            sb.Append("Service Bus Adapter doesn't support Sending Commands");
            lbCmdsFound.Content = string.Empty;
            return;
          }

          var cmds = GetAvailableCommands(asmPaths.GetItems(), cmdDef, !animate); // !animate = on dialog startup

          lbCmdsFound.Content = "{0} Commands Found".With(cmds.Length);

          if( cmds.Length == 0 ) {

            sb.Append("No commands found");

            if( cmdDef.InheritsType.IsValid() )
              sb.Append(" that inherits " + cmdDef.InheritsType.Substring(0, cmdDef.InheritsType.IndexOf(',')).CutBeginning(40));

            if( cmdDef.NamespaceContains.IsValid() ) {

              if( cmdDef.InheritsType.IsValid() )
                sb.Append(" or");
              else sb.Append(" that");

              sb.AppendFormat(" contains '{0}' in Namespace", cmdDef.NamespaceContains);

            }

            sb.Append(", make sure your Command Definition is correct");
          }

        } else {
          sb.Append("You need to add at least one assembly path containing commands libraries to be able to Send Commands");
          lbCmdsFound.Content = string.Empty;
        }

      } finally {

        if( sb.Length > 0 ) {
          lbSendCommandInfo.Text = sb.ToString();
        }

        UpdateInfoBox(sb.Length > 0, animate, ROW_SENDCMD_INFO, ConfigWindow.SendCommandInfoHeightProperty);
      }
    }

    private Type[] GetAvailableCommands(string[] asmPaths, CommandDefinition cmdDef, bool suppressErrors) {
      var srv = CurrentServer;

      return _sys.GetAvailableCommands(srv.ServiceBus, srv.ServiceBusVersion, srv.ServiceBusQueueType, asmPaths, cmdDef, suppressErrors);
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

    private void SelectServer(string name, bool animate = true) {

      var s = _config.Servers.SingleOrDefault(sv => sv.Name == name);

      if( s != null ) {

        _updatingServer = true;

        cbServers.SelectedValue = name;

        //cbServiceBus.SelectedValue = s.MessageBus;
        //cbTransport.SelectedValue = s.MessageBusQueueType;

        //tbInterval.UpdateValue(s.MonitorInterval);

        ValidateQueues(s);

        UpdateQueueLists(s);

        BindSendCommandView(s);
        UpdateSendCommandInfo(animate);

        UpdateQueueuInfo(animate);

        _updatingServer = false;
      }

    }

    private void UpdateQueueLists(ServerConfig3 s) {
      queueCommands.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Command).Select(
                                      q => new QueueListControl.QueueListItem(q.Name, q.Color)));
      queueEvents.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Event).Select(
                                      q => new QueueListControl.QueueListItem(q.Name, q.Color)));
      queueMessages.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Message).Select(
                                      q => new QueueListControl.QueueListItem(q.Name, q.Color)));
      queueErrors.BindItems(s.MonitorQueues.Where(q => q.Type == QueueType.Error).Select(
                                      q => new QueueListControl.QueueListItem(q.Name, q.Color)));
    }


    private void SaveServerConfig(string name) {
      var s = _config.Servers.Single(sv => sv.Name == name);

      List<QueueConfig> monitorQueues = new List<QueueConfig>();
      monitorQueues.AddRange(queueCommands.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Command, n.Color.ToArgb())));
      monitorQueues.AddRange(queueEvents.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Event, n.Color.ToArgb())));
      monitorQueues.AddRange(queueMessages.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Message, n.Color.ToArgb())));
      monitorQueues.AddRange(queueErrors.GetItems().Select(n => new QueueConfig(n.Name, QueueType.Error, n.Color.ToArgb())));

      s.MonitorQueues = monitorQueues.ToArray();

      s.CommandDefinition.InheritsType = tbCmdInherits.Text;
      s.CommandDefinition.NamespaceContains = tbNamespace.RetrieveValue() as string;

      s.CommandsAssemblyPaths = asmPaths.GetItems();
      s.CommandContentType = cbContentFormat.SelectedItem as string;
     
    }

    private void SaveConfig() {

      var serverName = cbServers.SelectedValue as string;
      SaveServerConfig(serverName);
      _config.MonitorServerName = serverName;

      _config.ShowOnNewMessages = cShowOnNewMessages.IsChecked == true;
      _config.VersionCheck.Enabled = cCheckForNewVer.IsChecked == true;

      _config.Save();

    }

    private void SelectDataType_Click(object sender, RoutedEventArgs e) {
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



    bool _creatingServer = false;
    private bool _accessingServer;

    private void AddServer_Click(object sender, RoutedEventArgs e) {
      //tbServer.Visibility = System.Windows.Visibility.Visible;
      //cbServers.Visibility = System.Windows.Visibility.Hidden;
      lbServerInfo.Content = string.Empty;

      _creatingServer = true;

      UpdateConfigWindowUIState();

      var dlg = new ManageServerDialog(_sys, null);
      if( dlg.ShowDialog() == true ) {
        var s = dlg.Result.Server;

        _allQueueNames.Add(s.Name, dlg.Result.AllQueueNames);
        _servers.Add(s);

        UpdateServerButtonState();

        SelectServer(s.Name);

      }

      _creatingServer = false;
      UpdateConfigWindowUIState();
      //tbServer.Focus();
    }
    private void EditServer_Click(object sender, RoutedEventArgs e) {

      var dlg = new ManageServerDialog(_sys, _config.CurrentServer);
      if( dlg.ShowDialog() == true ) {

        _config.MonitorServerName = dlg.Result.Name;

        BindServers(_sys.Config.Servers);

        SelectServer(dlg.Result.Name);
      }

    }
    private void CopyServer_Click(object sender, RoutedEventArgs e) {

      var dlg = new ManageServerDialog(_sys, _config.CurrentServer, true);
      if( dlg.ShowDialog() == true ) {

        _config.MonitorServerName = dlg.Result.Name;

        BindServers(_sys.Config.Servers);

        SelectServer(dlg.Result.Name);
      }

    }



    System.Windows.Visibility _prevServerActionVisibility;
    void _SetAccessingServer(bool value) {
      _accessingServer = value;

      if( _accessingServer ) {

        cbServers.IsEnabled = false;
        imgServerLoading.Visibility = System.Windows.Visibility.Visible;

        _prevServerActionVisibility = btnDeleteServer.Visibility;
        btnDeleteServer.Visibility = System.Windows.Visibility.Hidden;

      } else {
        cbServers.IsEnabled = true;

        imgServerLoading.Visibility = System.Windows.Visibility.Hidden;
        btnDeleteServer.Visibility = _prevServerActionVisibility;
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
    private void DeleteServer_Click(object sender, RoutedEventArgs e) {

      DeleteCurrentServer();

      UpdateServerButtonState();
    }

    private void DeleteCurrentServer() {
      _servers.Remove(_sys.Config.CurrentServer);

      _sys.Config.Servers.Remove(_sys.Config.CurrentServer);
      _sys.Config.MonitorServerName = _sys.Config.Servers[0].Name;

      cbServers.SelectedValue = _sys.Config.MonitorServerName;

      SelectServer(_sys.Config.MonitorServerName);
    }


    //void _ShowSaveServerButton() {
    //  btnDeleteServer.Source = "/ServiceBusMQManager;component/Images/save-white.png";
    //  btnDeleteServer.Tag = 1;
    //  btnDeleteServer.Visibility = System.Windows.Visibility.Visible;
    //}
    //void _ShowDeleteServerButton() {
    //  btnDeleteServer.Source = "/ServiceBusMQManager;component/Images/delete-item-white.png";
    //  btnDeleteServer.Tag = 2;
    //  btnDeleteServer.Visibility = System.Windows.Visibility.Visible;
    //}
    //void _HideServerButton() {
    //  btnDeleteServer.Visibility = System.Windows.Visibility.Hidden;
    //}

    void UpdateServerButtonState() {

      btnDeleteServer.Visibility = ( _servers.Count > 1 ) ? Visibility.Visible : Visibility.Hidden;
    }


    private void cbServers_SelectionChanged(object sender, SelectionChangedEventArgs e) {

      if( !_updatingServer ) {

        if( e.AddedItems.Count > 0 ) {

          SaveServerConfig(_config.MonitorServerName);

          _config.MonitorServerName = cbServers.SelectedValue as string;

          var s = e.AddedItems[0] as ServerConfig3;

          _SetAccessingServer(true);

          BackgroundWorker bw = new BackgroundWorker();
          bw.DoWork += (object sr, DoWorkEventArgs arg) => {

            var disc = GetDiscoveryService(s);
            if( disc.CanAccessServer(s.ConnectionSettings) ) {

              if( !_allQueueNames.ContainsKey(s.Name) )
                _allQueueNames.Add(s.Name, disc.GetAllAvailableQueueNames(s.ConnectionSettings));

              arg.Result = true;

            } else arg.Result = new Exception("Can not access Server, " + s.Name);

          };
          bw.RunWorkerCompleted += (sr, arg) => {

            if( arg.Result is Exception ) {
              _SetAccessingServer(false);

              throw (Exception)arg.Result;
            } else if( arg.Result as bool? == true ) {

              SelectServer(s.Name);
              BindSendCommandView(s);
              UpdateSendCommandInfo();

              _SetAccessingServer(false);
            }

          };

          bw.RunWorkerAsync();

        }
      }
    }

    private void asmPaths_RemovedItem(object sender, StringListItemRoutedEventArgs e) {
      UpdateSendCommandInfo();
    }
    private void asmPaths_AddedItem(object sender, StringListItemRoutedEventArgs e) {
      // Work-around due Resolve Assemblies use Config.Servers for Paths, when spec. Cmd Definition
      var s = _config.Servers.Single(sv => sv.Name == cbServers.SelectedValue as string);
      s.CommandsAssemblyPaths = asmPaths.GetItems();
      
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
    private ServerConfig3 _initializedServer;

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

    private void RoundMetroButton_Loaded_1(object sender, RoutedEventArgs e) {

    }




  }
}
