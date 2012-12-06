#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    CreateTemplateDialog.xaml.cs
  Created: 2012-11-24

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
using System.Windows.Shapes;

using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for CreateTemplateDialog.xaml
  /// </summary>
  public partial class CreateTemplateDialog : Window {
    
    string[] _existing;
    
    public CreateTemplateDialog(string[] existing) {
      InitializeComponent();

      _existing = existing;

      tbName.Focus();
    }

    private void btnCreate_Click(object sender, RoutedEventArgs e) {
      
      if( !string.IsNullOrEmpty(tbName.Text) )
        DialogResult = true;
    }


    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }


    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }

    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }

    private void tbName_TextChanged(object sender, TextChangedEventArgs e) {
      bool exist = _existing.Any( s => string.Compare(s, tbName.Text, true) == 0 );

      if( exist ) {
        lbInfo.Content = "Template with that name already exists";
      } else { 
        if( lbInfo.Content != null )
          lbInfo.Content = null;
      }

      btnCreate.IsEnabled = tbName.Text.Length > 0 && !exist;
    }

  }
}
