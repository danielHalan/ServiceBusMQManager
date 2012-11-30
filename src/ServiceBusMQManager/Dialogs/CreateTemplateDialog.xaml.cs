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

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for CreateTemplateDialog.xaml
  /// </summary>
  public partial class CreateTemplateDialog : Window {
    public CreateTemplateDialog() {
      InitializeComponent();

      tbName.Focus();
    }

    private void btnCreate_Click(object sender, RoutedEventArgs e) {
      
      if( !string.IsNullOrEmpty(tbName.Text) )
        DialogResult = true;
    }
  }
}
