#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    RoundMetroButton.xaml.cs
  Created: 2012-12-04

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
  /// Interaction logic for RoundMetroButton.xaml
  /// </summary>
  public partial class RoundMetroButton : UserControl {

    public RoundMetroButton() {
      InitializeComponent();

    }


    private void btn_Click(object sender, RoutedEventArgs e) {

      //if( _Click != null )
      //  _Click(this, e);


      RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
    //public static readonly DependencyProperty ClickProperty;

    //public event EventHandler<RoutedEventArgs> _Click;

    //public event EventHandler<RoutedEventArgs> Click {
    //  add {
    //    if( this._Click == null )
    //      this._Click += value;
    //  }
    //  remove {
    //    if( this._Click != null )
    //      this._Click -= value;
    //  }
    //}


    
    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(RoundMetroButton));

    public event RoutedEventHandler Click {
      add { AddHandler(ClickEvent, value); }
      remove { RemoveHandler(ClickEvent, value); }
    }
    

    //[Bindable(true)]
    //public string Source { get { return (string)btn.Tag; } set { btn.Tag = value; } }

    public static readonly DependencyProperty SourceProperty = 
      DependencyProperty.Register("Source", typeof(string), typeof(RoundMetroButton), new UIPropertyMetadata(string.Empty));

    public string Source {
      get { return (string)GetValue(SourceProperty); }
      set {
        btn.Tag = value;
        
        SetValue(SourceProperty, value);
      }
    }
  }
}
