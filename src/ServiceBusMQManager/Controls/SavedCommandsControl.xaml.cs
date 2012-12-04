using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {

  public class SavedCommandSelectedEventArgs : EventArgs {
  
    public SavedCommandSelectedEventArgs(SavedCommand cmd) {
      Command = cmd;
    }


    public SavedCommand Command { get; set; }
  }

  /// <summary>
  /// Interaction logic for SavedCommandsControl.xaml
  /// </summary>
  public partial class SavedCommandsControl : UserControl {
    
    CommandHistoryManager _mgr;

    ObservableCollection<SavedCommand> _recent = new ObservableCollection<SavedCommand>();

    public SavedCommandsControl() {
      InitializeComponent();

      tbName.Init(string.Empty, typeof(string), true);

      UpdateView(false);
    }

    public void Init(CommandHistoryManager mgr) {

      tbName.SelectAllTextOnFocus = true;

      _mgr = mgr;

      BindRecent();
    }

    public SavedCommand SelectedItem { 
      get { return cbRecent.SelectedItem as SavedCommand; } 
      set { cbRecent.SelectedItem = value; } 
    }

    private void BindRecent() {
      foreach( var cmd in _mgr.Items )
        _recent.Add(cmd);

      cbRecent.ItemsSource = _recent;
      cbRecent.DisplayMemberPath = "DisplayName";
      cbRecent.SelectedValuePath = "Command";

      cbRecent.SelectedValue = null;
    }


    public static readonly RoutedEvent SavedCommandSelectedEvent = EventManager.RegisterRoutedEvent("SavedCommandSelected",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SavedCommandsControl));

    public event RoutedEventHandler SavedCommandSelected {
      add { AddHandler(SavedCommandSelectedEvent, value); }
      remove { RemoveHandler(SavedCommandSelectedEvent, value); }
    }


    private bool _editMode;

    private void OnSavedCommandSelected(SavedCommand cmd) {

      RaiseEvent(new RoutedEventArgs(SavedCommandSelectedEvent));
    }


    private void cbRecent_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommand;

      OnSavedCommandSelected(recent);
    }


    private void UpdateView(bool editMode) {
      _editMode = editMode;

      if( _editMode ) {
        selectGrid.Visibility = System.Windows.Visibility.Hidden;
        editGrid.Visibility = System.Windows.Visibility.Visible;

      } else {
        selectGrid.Visibility = System.Windows.Visibility.Visible;
        editGrid.Visibility = System.Windows.Visibility.Hidden;
      }

    }

    private void btnEdit_Click(object sender, RoutedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommand;

      if( recent != null ) {
        tbName.UpdateValue(recent.DisplayName);

        UpdateView(true);

        tbName.FocusTextBox();
      }
    }
    private void btnSave_Click(object sender, RoutedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommand;
      
      Updating = true;
      try {
        recent.DisplayName = tbName.RetrieveValue() as string;
        _mgr.Save();

        cbRecent.SelectedIndex = -1;
        CollectionViewSource.GetDefaultView(cbRecent.ItemsSource).Refresh();

        UpdateView(false);
      
      } finally {
        Updating = false;
      }
      
      cbRecent.SelectedValue = recent.Command;
    }
    private void btnDelete_Click(object sender, RoutedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommand;
      Updating = true;
      try {

        _recent.Remove(recent);
        _mgr.Remove(recent);

        CollectionViewSource.GetDefaultView(cbRecent.ItemsSource).Refresh();

        UpdateView(false);
      } finally {
        Updating = false;
      }

      if( _recent.Count > 0 )
        cbRecent.SelectedIndex = 0;
      else cbRecent.SelectedIndex = -1;

    }

    public bool Updating { get; set; }

    public SavedCommand CommandSent(object command, string serviceBus, string transport, string server, string queue) {
      var sentCmd = _mgr.CommandSent(command, serviceBus, transport, server, queue);

      int pos = _recent.IndexOf(sentCmd);
      if( pos == -1 ) {
        _recent.Insert(0, sentCmd);

        if( cbRecent.SelectedItem != sentCmd )
          cbRecent.SelectedValue = sentCmd.Command;

      } else if( pos != 0 ) {
        _recent.Move(_recent.IndexOf(sentCmd), 0);
      }

      return sentCmd;
    }
  }
}
