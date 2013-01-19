#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ComplexDataTitleControl.xaml.cs
  Created: 2012-11-20

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
 
  public class ToggleContentViewEventArgs : EventArgs { 
    
    public ToggleContentViewEventArgs(bool viewAsText) {
      ViewAsText = viewAsText;
    }

    public bool ViewAsText { get; set; }
    public bool Cancel { get; set; }
  }
  
  /// <summary>
  /// Interaction logic for ComplexDataTitleControl.xaml
  /// </summary>
  public partial class ComplexDataTitleControl : UserControl {
 


    bool _viewAsText = false;


    public ComplexDataTitleControl(string title, bool hasParent) {
      InitializeComponent();

      lbTitle.Content = title;

      btnViewAsText.IsChecked = _viewAsText;

      if( !hasParent ) {
        btnBack.Visibility = System.Windows.Visibility.Hidden;
        lbTitle.Margin = new Thickness(0, lbTitle.Margin.Top, lbTitle.Margin.Right, lbTitle.Margin.Bottom);
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      btnBack.IsEnabled = false;

      OnBackClick();
    }

    private void btn_Checked(object sender, RoutedEventArgs e) {

      _viewAsText = !_viewAsText;
      
      if( !OnToggleContentViewClick() )
        _viewAsText = !_viewAsText;

    }


    public event EventHandler<EventArgs> BackClick;

    private void OnBackClick() {
      if( BackClick != null )
        BackClick(this, EventArgs.Empty);
    }

    public event EventHandler<ToggleContentViewEventArgs> ContentViewToggled;

    private bool OnToggleContentViewClick() {
      if( ContentViewToggled != null ) {
        var e = new ToggleContentViewEventArgs(_viewAsText);
        e.Cancel = false;

        ContentViewToggled(this, e);
        return !e.Cancel;
      }

      return true;
    }






  }
}
