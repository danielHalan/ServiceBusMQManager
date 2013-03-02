#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    TextEditor.xaml.cs
  Created: 2013-01-19

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using NServiceBus.Profiler.Common.CodeParser;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {

  /// <summary>
  /// Interaction logic for TextEditor.xaml
  /// </summary>
  public partial class TextEditor : UserControl {
    public TextEditor() {
      InitializeComponent();

      CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Plain;
    }


    public void SetText(string text) {
      doc.Document.Blocks.Clear();

      if( text.StartsWith("<?xml version=\"1.0\"") )
        CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Xml;


      if( text.IsValid() ) {
        var presenter = new CodeBlockPresenter(CodeLanguage);
        var t = new Paragraph();

        if( CodeLanguage == NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Xml )
          text = Tools.FormatXml(text);
        else if( CodeLanguage == NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Json )
          text = Tools.FormatJson(text);

        presenter.FillInlines(text, t.Inlines);
        doc.Document.Blocks.Add(t);
      } 
    }

    public static readonly DependencyProperty ReadOnlyProperty =
      DependencyProperty.Register("ReadOnlyProperty", typeof(bool), typeof(TextEditor), new UIPropertyMetadata(false));

    public bool ReadOnly {
      get { return (bool)GetValue(ReadOnlyProperty); }
      set { SetValue(ReadOnlyProperty, value); }
    }


    public static readonly DependencyProperty CodeLanguageProperty =
      DependencyProperty.Register("CodeLanguageProperty", typeof(CodeLanguage), typeof(TextEditor), new UIPropertyMetadata(CodeLanguage.Plain));
    
    public CodeLanguage CodeLanguage {
      get { return (CodeLanguage)GetValue(CodeLanguageProperty); }
      set { SetValue(CodeLanguageProperty, value); }
    }

    public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(TextEditor), new UIPropertyMetadata(string.Empty));

    public string Text {
      get { return new TextRange(doc.Document.ContentStart, doc.Document.ContentEnd).Text; }
      set {
        SetText(value);
      }
    }


  }
}
