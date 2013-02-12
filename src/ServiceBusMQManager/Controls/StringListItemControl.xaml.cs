#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    StringListItemControl.xaml.cs
  Created: 2012-12-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
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


  public class DeleteStringListItemRoutedEventArgs : RoutedEventArgs {

    public int Id { get; set; }

    public DeleteStringListItemRoutedEventArgs(RoutedEvent e)
      : base(e) {
    }
  }



  /// <summary>
  /// Interaction logic for StringListItemControl.xaml
  /// </summary>
  public partial class StringListItemControl : UserControl {

    public StringListItemControl(string value, int id) {
      InitializeComponent();

      tb.Text = value;
      btn.Tag = id;
    }


    private void btnDelete_Click(object sender, RoutedEventArgs e) {
      var btn = sender as RoundMetroButton;

      var e2 = new DeleteStringListItemRoutedEventArgs(RemovedItemEvent);

      e2.Id = (int)btn.Tag;

      RaiseEvent(e2);
    }



    public static readonly RoutedEvent RemovedItemEvent = EventManager.RegisterRoutedEvent("RemovedItem",
       RoutingStrategy.Direct, typeof(EventHandler<DeleteStringListItemRoutedEventArgs>), typeof(StringListItemControl));

    public event EventHandler<DeleteStringListItemRoutedEventArgs> RemovedItem {
      add { AddHandler(RemovedItemEvent, value); }
      remove { RemoveHandler(RemovedItemEvent, value); }
    }



  }
}
