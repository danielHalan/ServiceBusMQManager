#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ArrayInputControl.xaml.cs
  Created: 2012-11-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
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

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for ArrayInputControl.xaml
  /// </summary>
  public partial class ArrayInputControl : UserControl, IInputControl {
    private Type _type;
    string _attributeName;

    ServiceBusMQManager.UIControlFactory.InputControl _currCtl;
    private bool _isListItem;

    public ArrayInputControl(Type type, object value, string attributeName) {
      InitializeComponent();
      
      _attributeName = attributeName;

      if( type.IsArray ) {
        _type = type.GetElementType();

      } else throw new Exception("Not an array type");

      BindControl();

      LoadArray(value);
    }

    ServiceBusMQManager.UIControlFactory.InputControl CreateInpuControl() {
      var ctl = UIControlFactory.CreateControl(null, _type, null);
      ctl.Control.Margin = new Thickness(0, 4, 50, 0);
      ctl.Control.Width = this.Width - 40;
      ctl.Control.Height = 30;
      ctl.Control.VerticalAlignment = System.Windows.VerticalAlignment.Top;
      //_currCtl.Control.Height = 25;

      if( ctl.DataType == DataType.Complex ) {
        ( ctl.Control as ComplexDataInputControl ).DefineComplextType += cd_DefineComplextType;
      }


      return ctl;
    }

    private void BindControl() {

      // First create the Input control
      _currCtl = CreateInpuControl();    
      theGrid.Children.Add(_currCtl.Control);

    }

    void cd_DefineComplextType(object sender, ComplexTypeEventArgs e) {      
      OnDefineComplextType(sender, _attributeName, e.Type, e.Value);
    }
    public event EventHandler<ComplexTypeEventArgs> DefineComplextType;
    private void OnDefineComplextType(object sender, string attribName, Type t, object value) {
      if( DefineComplextType != null )
        DefineComplextType(sender, new ComplexTypeEventArgs(attribName, t, value));
    }


    public void UpdateValue(object value) {

      if( value != null ) { 
      
        if( value.GetType().IsArray ) 
          LoadArray(value);
        else { 
          
          if( _currCtl.DataType == DataType.Complex ) {
            AddListItem(value);
            UpdateCountLabel();

          } else (_currCtl.Control as IInputControl).UpdateValue(value);
        }
      

      } else (_currCtl.Control as IInputControl).UpdateValue(value);
 
    }

    public void LoadArray(object value) {
      theValueStack.Children.Clear();

      if( value != null ) {

        if( value.GetType().IsArray ) {

          foreach( var obj in (Array)value ) {
            AddListItem(obj);
          }
        }

      }

      UpdateCountLabel();
    }


    private void UpdateCountLabel() {
      lbCount.Content = string.Format("{0} Items", theValueStack.Children.Count);
    }


    public object RetrieveValue() {

      //Type tp = typeof(List<>).MakeGenericType(itemType);

      //IList list = (IList)Activator.CreateInstance(tp);
      //IList list = (IList)Activator.CreateInstance("System.Collections.Generic", "List<" + _type.Name + ">");

      Array list = Array.CreateInstance(_type, theValueStack.Children.Count);

      for(int i = 0; i < theValueStack.Children.Count; i++ ) {
        IInputControl c = theValueStack.Children[i] as IInputControl;
        list.SetValue(c.RetrieveValue(), i);
      }

      return list;
    
    }

    public bool IsListItem {
      get {
        return _isListItem;
      }
      set {
        _isListItem = value;
      }
    }

    public void AddListItem(object value) {
      var i = CreateInpuControl();
      i.Control.Margin = new Thickness(0, 0, 10, 0);
      
      IInputControl inCtl = i.Control as IInputControl;
      inCtl.UpdateValue(value);
      inCtl.IsListItem = true;

      theValueStack.Children.Add(i.Control);

      this.Height = ( theValueStack.Children.Count * 30 ) + 60 + 10;
    }

    private void AddItem_Click(object sender, RoutedEventArgs e) {


      theGrid.Children.Remove(_currCtl.Control);

      _currCtl.Control.Width = theValueStack.Width;
      _currCtl.Control.Margin = new Thickness(0, 0, 10, 0);
      (_currCtl.Control as IInputControl).IsListItem = true;
      
      theValueStack.Children.Add(_currCtl.Control);


      _currCtl = CreateInpuControl(); 
      theGrid.Children.Add(_currCtl.Control);

      this.Height = (theValueStack.Children.Count * 30) + 60 + 10;

      UpdateCountLabel();

      OnValueChanged();
    }

    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }

  }
}
