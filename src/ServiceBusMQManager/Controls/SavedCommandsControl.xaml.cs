#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    SavedCommandsControl.xaml.cs
  Created: 2012-12-04

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using ServiceBusMQ;
using ServiceBusMQ.Configuration;

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

    ObservableCollection<SavedCommandItem3> _recent = new ObservableCollection<SavedCommandItem3>();

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
    internal void Unload() {
      //_mgr.Unload();
    }

    public SavedCommandItem3 SelectedItem {
      get { return cbRecent.SelectedItem as SavedCommandItem3; }
      set { cbRecent.SelectedItem = value; }
    }

    private void BindRecent() {
      foreach( var cmd in _mgr.Items )
        _recent.Add(cmd);

      cbRecent.ItemsSource = _recent;
      cbRecent.DisplayMemberPath = "DisplayName";
      cbRecent.SelectedValuePath = "SentCommand";

      //cbRecent.SelectedValue = null;
    }


    public static readonly RoutedEvent SavedCommandSelectedEvent = EventManager.RegisterRoutedEvent("SavedCommandSelected",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SavedCommandsControl));


    public event RoutedEventHandler SavedCommandSelected {
      add { AddHandler(SavedCommandSelectedEvent, value); }
      remove { RemoveHandler(SavedCommandSelectedEvent, value); }
    }


    public static readonly RoutedEvent EnterEditModeEvent = EventManager.RegisterRoutedEvent("EnterEditMode",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SavedCommandsControl));

    public event RoutedEventHandler EnterEditMode {
      add { AddHandler(EnterEditModeEvent, value); }
      remove { RemoveHandler(EnterEditModeEvent, value); }
    }

    public static readonly RoutedEvent ExitEditModeEvent = EventManager.RegisterRoutedEvent("ExitEditMode",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SavedCommandsControl));

    public event RoutedEventHandler ExitEditMode {
      add { AddHandler(ExitEditModeEvent, value); }
      remove { RemoveHandler(ExitEditModeEvent, value); }
    }




    private bool _editMode;

    private void OnSavedCommandSelected(SavedCommandItem3 cmd) {

      RaiseEvent(new RoutedEventArgs(SavedCommandSelectedEvent));
    }


    private void cbRecent_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommandItem3;

      btnEdit.IsEnabled = recent != null;

      OnSavedCommandSelected(recent);
    }


    private void UpdateView(bool editMode) {
      _editMode = editMode;

      if( _editMode )
        RaiseEvent(new RoutedEventArgs(EnterEditModeEvent));
      else
        RaiseEvent(new RoutedEventArgs(ExitEditModeEvent));


      if( _editMode ) {
        selectGrid.Visibility = System.Windows.Visibility.Hidden;
        editGrid.Visibility = System.Windows.Visibility.Visible;

      } else {
        selectGrid.Visibility = System.Windows.Visibility.Visible;
        editGrid.Visibility = System.Windows.Visibility.Hidden;
      }

    }

    private void btnEdit_Click(object sender, RoutedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommandItem;

      if( recent != null ) {
        tbName.UpdateValue(recent.DisplayName);

        UpdateView(true);

        tbName.FocusTextBox();

        Expand();
      }
    }


    private void Expand() {

      GetExpandParentGridStoryboard().Begin();
      GetExpandControlStoryboard().Begin();

    }


    Storyboard _storyExpandParentGrid;
    Storyboard _storyExpandControl;

    Storyboard GetExpandParentGridStoryboard() {

      if( _storyExpandParentGrid == null ) {
        _storyExpandParentGrid = new Storyboard();

        DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();

        EasingDoubleKeyFrame key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
        key.Value = 220;
        anim.KeyFrames.Add(key);

        key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));
        key.Value = 290; ;
        anim.KeyFrames.Add(key);

        _storyExpandParentGrid.Children.Add(anim);

        Storyboard.SetTarget(anim, this);
        Storyboard.SetTargetProperty(anim, new PropertyPath(SavedCommandsControl.GridHeightProperty));
      }

      return _storyExpandParentGrid;
    }
    Storyboard GetExpandControlStoryboard() {

      if( _storyExpandControl == null ) {
        _storyExpandControl = new Storyboard();

        DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();

        EasingDoubleKeyFrame key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
        key.Value = 76;
        anim.KeyFrames.Add(key);

        key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));
        key.Value = 145;
        anim.KeyFrames.Add(key);

        _storyExpandControl.Children.Add(anim);

        Storyboard.SetTarget(anim, this);
        Storyboard.SetTargetProperty(anim, new PropertyPath(UserControl.HeightProperty));
      }

      return _storyExpandControl;
    }

    Storyboard _storyCollapseParentGrid;
    Storyboard _storyCollapseControl;

    Storyboard GetCollapseParentGridStoryboard() {

      if( _storyCollapseParentGrid == null ) {
        _storyCollapseParentGrid = new Storyboard();

        DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();

        EasingDoubleKeyFrame key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
        key.Value = 290;
        anim.KeyFrames.Add(key);

        key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));
        key.Value = 220; ;
        anim.KeyFrames.Add(key);

        _storyCollapseParentGrid.Children.Add(anim);

        Storyboard.SetTarget(anim, this);
        Storyboard.SetTargetProperty(anim, new PropertyPath(SavedCommandsControl.GridHeightProperty));
      }

      return _storyCollapseParentGrid;
    }

    Storyboard GetCollapseControlStoryboard() {
      if( _storyCollapseControl == null ) {
        _storyCollapseControl = new Storyboard();
        //sb.Duration = new Duration(TimeSpan.FromSeconds(5));

        DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();
        //anim.Duration = new Duration(TimeSpan.FromMilliseconds(400));

        EasingDoubleKeyFrame key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
        key.Value = 145;
        anim.KeyFrames.Add(key);

        key = new EasingDoubleKeyFrame();
        key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));
        key.Value = 76;
        anim.KeyFrames.Add(key);

        _storyCollapseControl.Children.Add(anim);

        Storyboard.SetTarget(anim, this);
        Storyboard.SetTargetProperty(anim, new PropertyPath(UserControl.HeightProperty));

        _storyCollapseControl.Completed += (a, b) => { UpdateView(false); };
      }

      return _storyCollapseControl;
    }
    private void Collapse() {
      GetCollapseParentGridStoryboard().Begin();
      GetCollapseControlStoryboard().Begin();
    }


    public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register(
             "GridHeight", typeof(double), typeof(SavedCommandsControl), new PropertyMetadata(0.0));

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
      base.OnPropertyChanged(e);

      if( ReferenceEquals(e.Property, GridHeightProperty) ) {
        Grid g = this.Parent as Grid;
        g.RowDefinitions[0].Height = new GridLength((double)e.NewValue);
      }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommandItem;

      Updating = true;
      try {
        recent.DisplayName = tbName.RetrieveValue() as string;
        _mgr.Save();

        cbRecent.SelectedIndex = -1;
        CollectionViewSource.GetDefaultView(cbRecent.ItemsSource).Refresh();
        cbRecent.SelectedItem = recent;

        Collapse();

      } finally {
        Updating = false;
      }

    }
    private void btnDelete_Click(object sender, RoutedEventArgs e) {
      var recent = cbRecent.SelectedItem as SavedCommandItem3;
      Updating = true;
      try {

        Remove(recent);

        CollectionViewSource.GetDefaultView(cbRecent.ItemsSource).Refresh();

        Collapse();
      } finally {
        Updating = false;
      }

      if( _recent.Count > 0 )
        cbRecent.SelectedIndex = 0;
      else cbRecent.SelectedIndex = -1;

    }

    public void Remove(SavedCommandItem3 item) {
      if( item != null ) {
        _recent.Remove(item);
        _mgr.Remove(item);
      }
    }

    public bool Updating { get; set; }

    public SavedCommandItem3 CommandSent(object command, string serviceBus, string transport, Dictionary<string, string> connectionStrings, string queue) {
      var sentCmd = _mgr.AddCommand(command, serviceBus, transport, connectionStrings, queue);

      int pos = _recent.IndexOf(sentCmd);
      if( pos == -1 ) {
        _recent.Insert(0, sentCmd);

        if( cbRecent.SelectedItem != sentCmd )
          cbRecent.SelectedValue = sentCmd.SentCommand;

      } else if( pos != 0 ) {
        _recent.Move(_recent.IndexOf(sentCmd), 0);
      }

      return sentCmd;
    }


  }
}
