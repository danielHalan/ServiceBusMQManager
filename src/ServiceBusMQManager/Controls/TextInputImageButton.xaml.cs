#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    TextInputImageButton.xaml.cs
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
  /// Interaction logic for TextInputImageButton.xaml
  /// </summary>
  public partial class TextInputImageButton : UserControl {
    public TextInputImageButton() {
      InitializeComponent();
    }



    private void btn_Click(object sender, RoutedEventArgs e) {
      RaiseEvent(new RoutedEventArgs(ClickEvent));
    }

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
      RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TextInputImageButton));

    public event RoutedEventHandler Click {
      add { AddHandler(ClickEvent, value); }
      remove { RemoveHandler(ClickEvent, value); }
    }



    public static readonly DependencyProperty SourceProperty =
      DependencyProperty.Register("Source", typeof(string), typeof(TextInputImageButton), new UIPropertyMetadata(string.Empty));

    public string Source {
      get { return (string)GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }



  }
}
