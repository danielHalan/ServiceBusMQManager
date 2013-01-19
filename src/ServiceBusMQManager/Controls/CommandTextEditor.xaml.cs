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
//using ScintillaNET;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for CommandTextEditor.xaml
  /// </summary>
  public partial class CommandTextEditor : UserControl {
 
    public CommandTextEditor() {
      InitializeComponent();

      //Scintilla t = w32.Child as Scintilla;
      //t.LineWrapping.IndentMode = LineWrappingIndentMode.Same;
      //t.LineWrapping.Mode = LineWrappingMode.Word;
      //t.ConfigurationManager.Language = "xml";
      //foreach( var m in t.Margins )
      //  m.Width = 0;

    }

    public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(CommandTextEditor), new UIPropertyMetadata(string.Empty));

    public string Text {
      get { return tb.Text; }
      set {  tb.SetText(value); }
    }

  }
}
