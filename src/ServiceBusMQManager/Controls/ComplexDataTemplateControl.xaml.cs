#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ComplexDataTemplateControl.xaml.cs
  Created: 2012-11-24

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

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

using KellermanSoftware.CompareNetObjects;

using ServiceBusMQ;
using ServiceBusMQManager.Dialogs;


namespace ServiceBusMQManager.Controls {

  public class TemplateEventArgs : EventArgs {

    public string Name;
    public Type Type;

    public object Value;

    public TemplateEventArgs(string name, Type type) {
      Name = name;
      Type = type;
    }
  }


  public class TemplateSelectedEventArgs : EventArgs {

    public string Name;
    public Type Type;
    public object Data;

    public TemplateSelectedEventArgs(string name, Type type, object data) {
      Name = name;
      Type = type;
      Data = data;
    }
  }


  /// <summary>
  /// Interaction logic for ComplexDataTemplateControl.xaml
  /// </summary>
  public partial class ComplexDataTemplateControl : UserControl {

    Type _type;
    private ServiceBusMQ.DataTemplateManager _tempMgr;
    
    bool _updating = false;

    ObservableCollection<DataTemplateManager.DataTemplate> _items;

    public ComplexDataTemplateControl(Type t) {

      _type = t;
    }

    public ComplexDataTemplateControl(Type type, ServiceBusMQ.DataTemplateManager tempMgr) {
      InitializeComponent();

      _type = type;
      _tempMgr = tempMgr;

      BindItems();
    }

    private void BindItems() {

      _updating = true; 

      try {
        _items = new ObservableCollection<DataTemplateManager.DataTemplate>();
        var nullItem = new DataTemplateManager.DataTemplate() { Name = "<< none >>", Object = null };
        _items.Add(nullItem);

        foreach( var a in _tempMgr.Templates.Where(t => t.TypeName == _type.FullName) )
          _items.Add(a);

        cbTemps.ItemsSource = _items;
        cbTemps.DisplayMemberPath = "Name";
        cbTemps.SelectedValuePath = "Object";
        cbTemps.SelectedValue = nullItem;

        cbTemps.SelectedIndex = 0;
      
      } finally {
        _updating = false;
      }
    }

    private void CreateTemplate_Click(object sender, RoutedEventArgs e) {

      CreateTemplateDialog dlg = new CreateTemplateDialog();
      dlg.Owner = Application.Current.Windows.OfType<SendCommandWindow>().Single();

      if( dlg.ShowDialog() == true ) {
        OnCreateTemplate(dlg.tbName.Text);
      }

    }


    public event EventHandler<TemplateEventArgs> CreateTemplate;
    private void OnCreateTemplate(string name) {

      var args = new TemplateEventArgs(name, _type);

      if( CreateTemplate != null )
        CreateTemplate(this, args);

      var newItem = new DataTemplateManager.DataTemplate() { Name = name, TypeName = _type.FullName, Object = args.Value };
      _items.Add(newItem);

      cbTemps.SelectedValue = newItem.Object;
    }


    public event EventHandler<TemplateEventArgs> DeleteTemplate;
    private void OnDeleteTemplate(string name) {

      var args = new TemplateEventArgs(name, _type);

      if( DeleteTemplate != null )
        DeleteTemplate(this, args);
      
      var index = cbTemps.SelectedIndex;
      cbTemps.SelectedIndex = 0;

      _items.RemoveAt(index);
    }



    public event EventHandler<TemplateSelectedEventArgs> TemplateSelected;
    private void OnTemplateSelected(string name, object data) {


      if( TemplateSelected != null )
        TemplateSelected(this, new TemplateSelectedEventArgs(name, _type, data));
    }

    private void cbTemps_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      
      if( !_updating ) {
        var item = ( (DataTemplateManager.DataTemplate)cbTemps.SelectedItem );

        if( item != null )
          OnTemplateSelected(item.Name, item.Object);
        else OnTemplateSelected(null, null);
      }
    }


    internal void SelectTemplate(object value) {
      _updating = true; 

        try {
        var co = new CompareObjects();


        if( value != null ) {
          foreach( var itm in _items.Where(i => i.Object != null) ) {

            if( co.Compare(itm.Object, value) )
              cbTemps.SelectedValue = itm.Object;

          }

        } else cbTemps.SelectedIndex = 0;

      } finally {
        _updating = false;
      }
    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      if( cbTemps.SelectedIndex > 0 ) {
        var item = ( (DataTemplateManager.DataTemplate)cbTemps.SelectedItem );

        OnDeleteTemplate(item.Name);

      }
    }
  }
}
