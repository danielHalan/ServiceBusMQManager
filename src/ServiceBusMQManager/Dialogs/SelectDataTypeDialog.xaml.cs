#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    SelectDataTypeDialog.xaml.cs
  Created: 2012-12-05

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

  public class DataTypeItem {
    private string _name;
    
    public string Name {
      get { return _name; }
      set { _name = value; 
        NameLower = value.ToLower(); }
    }

    public string NameLower { get; private set; }

    public string Namespace { get; set; }
    public Type Type { get; set; }
    public string QualifiedName {
      get {
        return string.Format("{0}, {1}", Type.FullName, Type.Assembly.GetName().Name);
      }
    }
  }


  /// <summary>
  /// Interaction logic for SelectDataTypeDialog.xaml
  /// </summary>
  public partial class SelectDataTypeDialog : Window {


    List<DataTypeItem> _Alltypes = new List<DataTypeItem>();
    ObservableCollection<DataTypeItem> _types = new ObservableCollection<DataTypeItem>();



    string[] _asmPaths;

    public SelectDataTypeDialog(string[] asmPaths) {
      InitializeComponent();

      _asmPaths = asmPaths;

      Topmost = SbmqSystem.Instance.UIState.AlwaysOnTop;

      LoadTypes();

      lvTypes.ItemsSource = _types;

      WindowTools.SetSortColumn(lvTypes, "Name");

      tbFilter.Focus();
    }


    void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e) {
      GridViewColumnHeader h = e.OriginalSource as GridViewColumnHeader;

      if( ( h != null ) && ( h.Role != GridViewColumnHeaderRole.Padding ) ) {
        WindowTools.SetSortColumn(lvTypes, ( h.Column.DisplayMemberBinding as Binding ).Path.Path);
      }

    }


    private void LoadTypes() {
      _types.Clear();

      List<string> files = new List<string>(1000);

      foreach( var path in _asmPaths )
        files.AddRange(Directory.GetFiles(path, "*.dll"));

      files.Add(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\NServiceBus.dll");


      List<string> processed = new List<string>(files.Count);

      foreach( var dll in files ) {
        var fileName = System.IO.Path.GetFileName(dll);

        if( processed.Contains(fileName) )
          continue;

        try {
          var asm = Assembly.LoadFrom(dll);

          foreach( Type t in asm.GetTypes() ) {
            if( ( t.IsClass || t.IsInterface ) && !IsCompilerGenerated(t) ) {

              var item = new DataTypeItem() { Type = t, Name = t.Name, Namespace = t.Namespace };

              _types.Add(item);
            }
          }

          processed.Add( fileName );

        } catch { }

      }

      _Alltypes.AddRange(_types.ToArray());

    }


    private bool IsCompilerGenerated(Type type) {
      if( type == null )
        return false;

      return type.IsDefined(typeof(CompilerGeneratedAttribute), false) || IsCompilerGenerated(type.DeclaringType);
    }

    public DataTypeItem SelectedType { get; set; }

    private void btnOK_Click(object sender, RoutedEventArgs e) {

      SelectedType = lvTypes.SelectedItem as DataTypeItem;
      DialogResult = true;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      this.MoveOrResizeWindow(e);
    }


    private void Window_MouseMove(object sender, MouseEventArgs e) {
      Cursor = this.GetBorderCursor();
    }

    private void HandleMaximizeClick(object sender, RoutedEventArgs e) {
      var s = WpfScreen.GetScreenFrom(this);

      this.Top = s.WorkingArea.Top;
      this.Height = s.WorkingArea.Height;
    }
    private void HandleCloseClick(Object sender, RoutedEventArgs e) {
      Close();
    }

    private void lvTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      btnOK.IsEnabled = lvTypes.SelectedItem != null;
    }

    private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e) {

      Filter(tbFilter.Text);
    }

    private void Filter(string str) {
      str = str.ToLower();

      foreach( var itm in _Alltypes.Where(t => !t.NameLower.Contains(str)) )
        _types.Remove(itm);

      foreach( var itm in _Alltypes.Where(t => t.NameLower.Contains(str)) ) {
        if( _types.IndexOf(itm) == -1 )
          _types.Add(itm);
      }

    }

    private void lvTypes_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      if( lvTypes.SelectedIndex > -1 ) {
        SelectedType = lvTypes.SelectedItem as DataTypeItem;
        DialogResult = true;
      }

    }

  }
}
