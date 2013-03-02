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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for ArrayInputControl.xaml
  /// </summary>
  public partial class ArrayInputControl : UserControl, IInputControl {
    private Type _type;
    string _attributeName;

    ServiceBusMQManager.UIControlFactory.InputControl _currCtl;
    private bool _isListItem;
    private bool _isNull = false;

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
        var complexCtl = ( ctl.Control as ComplexDataInputControl );
        complexCtl.ShowContentInName = false;
        complexCtl.DefineComplextType += cd_DefineComplextType;
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
      

      } else Nullify();
 
    }

    public void LoadArray(object value) {
      theValueStack.Children.Clear();

      _isNull = (value == null);

      if( !_isNull ) {

        if( value.GetType().IsArray ) {

          foreach( var obj in (Array)value ) {
            AddListItem(obj);
          }
        }
      }

      UpdateCountLabel();
      RecalcSize();
    }

    public void Nullify() {
      theValueStack.Children.Clear();
      
      _isNull = true;
      
      UpdateCountLabel();
      RecalcSize();
    }


    private void UpdateCountLabel() {
      lbCount.Content = !_isNull ? string.Format("{0} Items", theValueStack.Children.Count) : "<< NULL >>";
    }


    public object RetrieveValue() {

      if( !_isNull ) {
        Array list = Array.CreateInstance(_type, theValueStack.Children.Count);

        for(int i = 0; i < theValueStack.Children.Count; i++ ) {
          IInputControl c = ((Grid)theValueStack.Children[i]).Children[0] as IInputControl;
          list.SetValue(c.RetrieveValue(), i);
        }

        return list;
      } else return null;
    
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

      IInputControl inCtl = i.Control as IInputControl;
      inCtl.UpdateValue(value);

      AddControlToValueStack(i.Control);
    }

    const float LISTITEM_HEIGHT = 30;

    private void AddControlToValueStack(Control control) {
      Grid g = new Grid();
      g.Height = LISTITEM_HEIGHT;
      g.Background = Brushes.Gray;
      g.Margin = new Thickness(0,0,0,2);

      control.Margin = new Thickness(0, 0, 40, 0);

      IInputControl inCtl = control as IInputControl;
      inCtl.IsListItem = true;
      g.Children.Add(control);

      var btn = new RemoveItemButton();
      //btn.Width = 35;
      btn.Tag = g;
      btn.Click += btnRemove_Click;
      btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
      g.Children.Add(btn);

      theValueStack.Children.Add(g);

      if( _isNull )
        _isNull = false;

      RecalcSize();
    }

    void btnRemove_Click(object sender, RoutedEventArgs e) {
      var s = sender as RemoveItemButton;

      theValueStack.Children.Remove(s.Tag as Grid);
      
      RecalcSize();
      UpdateCountLabel();
    }

    private void RecalcSize() {
      this.Height = ( theValueStack.Children.Count * LISTITEM_HEIGHT ) + 60 + 15;
    }


    private void AddItem_Click(object sender, RoutedEventArgs e) {

      if( _currCtl.DataType == DataType.Complex ) {
        (_currCtl.Control as ComplexDataInputControl).ShowComplextDataType();
        return;
      }


      theGrid.Children.Remove(_currCtl.Control);
      
      AddControlToValueStack(_currCtl.Control);

      _currCtl = CreateInpuControl(); 
      theGrid.Children.Add(_currCtl.Control);


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
