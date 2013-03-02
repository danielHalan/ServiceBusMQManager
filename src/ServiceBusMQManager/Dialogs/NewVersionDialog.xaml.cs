#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    NewVersionDialog.xaml.cs
  Created: 2012-12-09

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ServiceBusMQ;

namespace ServiceBusMQManager.Dialogs {
  /// <summary>
  /// Interaction logic for NewVersionDialog.xaml
  /// </summary>
  public partial class NewVersionDialog : Window {
    private string _url;
    
    
    public NewVersionDialog(List<HalanVersionInfo> products) {
      InitializeComponent();
    
      BindFeatures(products);
      
      btnOK.Focus();
    }

    private void BindFeatures(List<HalanVersionInfo> products) {

      foreach( HalanVersionInfo inf in products ) {

        string title = string.Format("{0} {1}.{2:D2}", inf.Product.Replace("ServiceBusMQManager", "Service Bus MQ Manager"), 
                                                        inf.LatestVersion.Major, inf.LatestVersion.Minor);
        
        Paragraph para = new Paragraph();
        para.Inlines.Add(new Bold(
                               new Run(title) { FontSize = 19 } ));

        if( inf.ReleaseDate > DateTime.MinValue )
          para.Inlines.Add( new Run(string.Concat(", Released: ", inf.ReleaseDate.ToShortDateString())) );

        tbFeatures.Document.Blocks.Add(para);


        var list = new System.Windows.Documents.List();
        
        foreach( var f in inf.Features )
          list.ListItems.Add( new ListItem(new Paragraph(new Run(f))) );

        tbFeatures.Document.Blocks.Add(list);

        if( !_url.IsValid() )
          _url = inf.Url;
      }

    }


    private void btnOK_Click(object sender, RoutedEventArgs e) {
      System.Diagnostics.Process.Start(_url);
      Close();
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



  }
}
