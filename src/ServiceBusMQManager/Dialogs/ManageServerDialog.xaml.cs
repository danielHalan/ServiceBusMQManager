#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ManageServerDialog.xaml.cs
  Created: 2013-10-11

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ServiceBusMQ;
using ServiceBusMQ.Configuration;
using ServiceBusMQ.Manager;
using ServiceBusMQManager.Controls;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for AddServerDialog.xaml
  /// </summary>
  public partial class ManageServerDialog : Window {

    enum ActionType { Edit, Add, Copy }

    private SbmqSystem _sys;
    private ServiceBusMQ.Configuration.SystemConfig3 _config;
    private ServiceBusFactory.ServiceBusManagerType[] _managerTypes;

    IServiceBusDiscovery _discoverySvc = null;
    private bool _nameEdited;

    public ConfigWindow.AddServerResult Result { get; set; }

    ActionType DialogActionType;
    private ServerConfig3 _server;

    public ManageServerDialog(SbmqSystem system, ServerConfig3 server, bool copy = false) {
      InitializeComponent();

      _sys = system;
      _config = system.Config;
      _server = server;

      _managerTypes = ServiceBusFactory.AvailableServiceBusManagers();

      Result = new ConfigWindow.AddServerResult();

      Result.Server = new ServerConfig3();
      if( server != null ) {
        server.CopyTo(Result.Server);
      
        if( copy ) {
          Result.Server.Name = string.Empty;
          Result.Server.ConnectionSettings = new Dictionary<string, string>();
        }
      }
      DialogActionType = server == null ? ActionType.Add : ( copy ? ActionType.Copy : ActionType.Edit );

      string verb = ( DialogActionType == ActionType.Edit ) ? "Edit" : "Add"; 
      lbTitle.Content = Title = "{0} Server".With(verb);
      lbInfo.Content = string.Empty;

      cbServiceBus.ItemsSource = _managerTypes.GroupBy(g => g.DisplayName).Select(x => x.Key).ToArray();
      //cbServiceBus.DisplayMemberPath = "DisplayName";
      //cbServiceBus.SelectedValuePath = "Name";

      tbName.Init(Result.Server.Name, typeof(string), false);

      //var s = cbServiceBus.SelectedValue as string;
      //cbTransport.ItemsSource = _managerTypes.GroupBy(g => g.Name).Single(x => x.Key == s).Select(x => x.QueueType);

      //tbInterval.Init(750, typeof(int), false);

      if( DialogActionType == ActionType.Edit ) {
        _nameEdited = GetDefaultServerName(_server.ConnectionSettings.First().Value, _server.ServiceBus, _server.ServiceBusVersion, _server.ServiceBusQueueType) != _server.Name;
      }

      BindServerInfo();
    }

    private void BindServerInfo() {
      var s = Result.Server;

      cbServiceBus.SelectedValue = s.FullMessageBusName;
      cbTransport.SelectedValue = s.ServiceBusQueueType;
      tbInterval.Init(s.MonitorInterval, typeof(int), false);
    }


    private void frmConfig_SourceInitialized(object sender, EventArgs e) {
      //SbmqSystem.UIState.RestoreWindowState(this);

      ValidateHeight();

      //GetAllAvailableQueueNamesForServer(_config.CurrentServer.Name);
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
    private void frmConfig_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      //SbmqSystem.UIState.StoreWindowState(this);
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      //SaveConfig();

      if( _sys.Config.Servers.Any( s => s.Name == tbName.RetrieveValue<string>() ) ) {
        
        MessageBox.Show("A Service Bus Connection with same name already exists", "Name Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      Result.Server.ConnectionSettings = GetConnectionSettings();

      TryAccessServer(Result.Server.ConnectionSettings, () => { // Success

        var sb = _managerTypes.First(x => x.DisplayName == (string)cbServiceBus.SelectedValue);

        var s = Result.Server;
        s.Name = tbName.RetrieveValue<string>();
        s.MonitorInterval = tbInterval.RetrieveValue<int>();
        s.ServiceBus = sb.Name;
        s.ServiceBusVersion = sb.Version;
        s.ServiceBusQueueType = cbTransport.SelectedItem as string;

        if( DialogActionType == ActionType.Edit ) {
          s.CopyTo(_server);

        } else { // Add or Copy
          s.MonitorQueues = new QueueConfig[0];

          _sys.Config.Servers.Add(Result.Server);
          _sys.Config.MonitorServerName = s.Name;
        }

        DialogResult = true;
      });

    }

    private void UpdateNameLabel() {

      if( !_nameEdited ) {

        if( cbServiceBus.SelectedValue == null )
          return;

        var sb = _managerTypes.First(x => x.DisplayName == (string)cbServiceBus.SelectedValue);

        var ctl = parameters.Children.Cast<ServerConnectionParamControl>().FirstOrDefault();
        var name = ctl != null ? ctl.Value : string.Empty;

        tbName.Init(GetDefaultServerName(name, sb.Name, sb.Version, cbTransport.SelectedValue as string), typeof(string), false);
      }

    }

    private string GetDefaultServerName(string name, string serviceBus, string version, string queueType) {
      int index = name.IsValid() ? name.IndexOf("//") : -1;

      if( index != -1 )
        name = name.Substring(index + 2);

      return "{0} ( {1} / {2} )".With(name.CutEnd(20), ServerConfig3.GetFullMessageBusName(serviceBus, version), queueType);
    }


    private Dictionary<string, string> GetConnectionSettings() {
      return parameters.Children.Cast<ServerConnectionParamControl>().ToDictionary(n => n.Param.SchemaName, v => v.Value);
    }



    private void TryAccessServer(Dictionary<string, string> connectionSettings, Action onSuccess) {
      //btnServerAction.Visibility = System.Windows.Visibility.Hidden;
      imgInfo.Visibility = System.Windows.Visibility.Visible;

      this.IsEnabled = false;

      if( DialogActionType == ActionType.Add ) {

        if( _config.Servers.Any(x => x.Name == Result.Server.Name) ) {
          lbInfo.Content = "Server Connection already exists with same Name " + Result.Server.Name;
        }
      }

      Result.Server.Name = tbName.RetrieveValue<string>();
      Result.Server.ServiceBus = cbServiceBus.SelectedValue as string;
      Result.Server.ServiceBusQueueType = cbTransport.SelectedValue as string;

      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += worker_TryAccessServer;
      worker.RunWorkerCompleted += (s, e) => {
        //UpdateConfigWindowUIState();

        //cbServers.Visibility = System.Windows.Visibility.Visible;
        //tbServer.Visibility = System.Windows.Visibility.Hidden;
        //tbServer.UpdateValue(string.Empty);

        if( !( (bool)e.Result ) ) { // failed
          lbInfo.Content = "Could not access server";

        } else onSuccess();

        imgInfo.Visibility = System.Windows.Visibility.Hidden;

        this.IsEnabled = true;
      };

      worker.RunWorkerAsync(connectionSettings);
    }


    void worker_TryAccessServer(object sender, DoWorkEventArgs e) {
      var connectionSettings = e.Argument as Dictionary<string, string>;
      try {

        //_discoverySvc = _sys.GetDiscoveryService(Result.MessageBus, Result.QueueType);
        Result.AllQueueNames = _discoverySvc.GetAllAvailableQueueNames(connectionSettings);

        e.Result = true;

      } catch {
        e.Result = false;
      }
    }

    bool _updatingServiceBus = false;
    private void cbServiceBus_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if( e.AddedItems.Count > 0 ) {
        _updatingServiceBus = true;
        try {
          var s = e.AddedItems[0] as string;
          var sb = _managerTypes.First(x => x.DisplayName == s);

          var itms = _managerTypes.GroupBy(g => g.DisplayName).Single(x => x.Key == s).Select(x => x.QueueType).ToArray();
          cbTransport.ItemsSource = itms;

          if( itms.Length == 1 )
            cbTransport.SelectedIndex = 0;

          UpdateServiceBus(sb.Name, sb.Version, cbTransport.SelectedValue as string);
          UpdateNameLabel();
        } finally {
          _updatingServiceBus = false;
        }
      }
    }

    private void cbTransport_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var queueType = e.AddedItems[0] as string;
      Result.Server.ServiceBusQueueType = queueType;

      if( !_updatingServiceBus ) {
        UpdateServiceBus(null, null, queueType);
        UpdateNameLabel();
      }
    }

    private void UpdateServiceBus(string messageBus = null, string version = null, string queueType = null) {

      if( messageBus == null ) {
        var sb = _managerTypes.First(x => x.DisplayName == (string)cbServiceBus.SelectedValue);
        messageBus = sb.Name;
        version = sb.Version;
      }

      if( queueType == null )
        queueType = cbTransport.SelectedValue as string;

      if( messageBus != null && queueType != null ) {
        _discoverySvc = _sys.GetDiscoveryService(messageBus, version, queueType);

        parameters.Children.Clear();
        foreach( var prm in _discoverySvc.ServerConnectionParameters ) {
          var value = Result.Server != null ? Result.Server.ConnectionSettings.GetValue(prm.SchemaName, prm.DefaultValue) : null;
          var ctl = new ServerConnectionParamControl(prm, value);
          ctl.ValueChanged += ctl_ValueChanged;
          ctl.LostFocus += ctl_LostFocus;
          parameters.Children.Add(ctl);
        }

      }
    }

    void ctl_LostFocus(object sender, RoutedEventArgs e) {
      if( parameters.Children.IndexOf((UIElement)sender) == 0 ) {
        var ctl = parameters.Children.Cast<ServerConnectionParamControl>().FirstOrDefault();
        var v = ctl.Value.ToLower();
        if( v == "localhost" || v == "127.1.1.1" )
          ctl.Value = Environment.MachineName;

        UpdateNameLabel();
      }
    }

    void ctl_ValueChanged(object sender, EventArgs e) {
      if( parameters.Children.IndexOf((UIElement)sender) == 0 )
        UpdateNameLabel();
    }

    private void tbName_ValueChanged(object sender, EventArgs e) {
      if( tbName.tb.IsFocused )
        _nameEdited = true;
    }


  }
}
