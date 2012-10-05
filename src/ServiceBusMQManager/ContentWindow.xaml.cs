#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    ContentWindow.xaml.cs
  Created: 2012-08-21

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using ScintillaNET;
using ServiceBusMQ;

namespace ServiceBusMQManager {
  /// <summary>
  /// Interaction logic for ContentWindow.xaml
  /// </summary>
  public partial class ContentWindow : Window {
    
    private HwndSource _hwndSource;
    
    
    public ContentWindow() {
      InitializeComponent();

      Scintilla t = w32.Child as Scintilla;
      t.LineWrapping.IndentMode = LineWrappingIndentMode.Same;
      t.LineWrapping.Mode = LineWrappingMode.Word;
      t.ConfigurationManager.Language = "xml";

      SourceInitialized += ContentWindow_SourceInitialized;
    }

    void ContentWindow_SourceInitialized(object sender, EventArgs e) {
      _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
    }


    string FormatXml(string xml) {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(xml);

      StringBuilder sb = new StringBuilder();
      using( XmlTextWriter wr = new XmlTextWriter(new StringWriter(sb)) ) {

        wr.Indentation = 2;
        wr.Formatting = Formatting.Indented;

        doc.Save(wr);
      }

      return sb.ToString();
    }


    private static FlowDocument GetFlowDocument(string xml) {
      StringReader stringReader = new StringReader(xml);

      XmlReader xmlReader = XmlReader.Create(stringReader);

      Section sec = XamlReader.Load(xmlReader) as Section;

      FlowDocument doc = new FlowDocument();

      while( sec.Blocks.Count > 0 )
        doc.Blocks.Add(sec.Blocks.FirstBlock);

      return doc;
    }


    public void SetContent(string xml) {
      Scintilla t = w32.Child as Scintilla;

      t.Text = FormatXml(xml);

      //tbContent.Text = xml;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      
      CursorPosition pos = this.GetCursorPosition();

      if( e.LeftButton == MouseButtonState.Pressed ) {
        if( pos == CursorPosition.Body )
          DragMove();
        else ResizeWindow(pos);
      }

    }

    private void ResizeWindow(CursorPosition pos) {
      Native.SendMessage(_hwndSource.Handle, Native.WM_SYSCOMMAND,
          (IntPtr)( 61440 + pos ), IntPtr.Zero);
    }

    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }


    private void Window_MouseMove_1(object sender, MouseEventArgs e) {

      switch( this.GetCursorPosition() ) {
        case CursorPosition.Top:
          Cursor = Cursors.SizeNS;
          break;
        case CursorPosition.Bottom:
          Cursor = Cursors.SizeNS;
          break;
        case CursorPosition.Left:
          Cursor = Cursors.SizeWE;
          break;
        case CursorPosition.Right:
          Cursor = Cursors.SizeWE;
          break;
        case CursorPosition.TopLeft:
          Cursor = Cursors.SizeNWSE;
          break;
        case CursorPosition.TopRight:
          Cursor = Cursors.SizeNESW;
          break;
        case CursorPosition.BottomLeft:
          Cursor = Cursors.SizeNESW;
          break;
        case CursorPosition.BottomRight:
          Cursor = Cursors.SizeNWSE;
          break;

      default:
          Cursor = Cursors.Arrow;
          break;


      }


    }


    internal void SetTitle(string str) {
      lbTitle.Content = str;
      this.Title = str;
    }
  }
}
