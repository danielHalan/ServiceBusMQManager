#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    TextInputLabelButton.xaml.cs
  Created: 2012-12-23

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System.Windows;
using System.Windows.Controls;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for TextInputLabelButton.xaml
  /// </summary>
  public partial class TextInputLabelButton : UserControl {
    public TextInputLabelButton() {
      InitializeComponent();
    }


    private void btn_Click(object sender, RoutedEventArgs e) {
      RaiseEvent(new RoutedEventArgs(ClickEvent));
    }

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TextInputLabelButton));

    public event RoutedEventHandler Click {
      add { AddHandler(ClickEvent, value); }
      remove { RemoveHandler(ClickEvent, value); }
    }



    public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(TextInputLabelButton), new UIPropertyMetadata(string.Empty));

    public string Text {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }



  }
}
