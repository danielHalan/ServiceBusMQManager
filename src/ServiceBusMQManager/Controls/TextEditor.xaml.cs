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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NServiceBus.Profiler.Common.CodeParser;
using ServiceBusMQ;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace ServiceBusMQManager.Controls {

  /// <summary>
  /// Interaction logic for TextEditor.xaml
  /// </summary>
  public partial class TextEditor : UserControl {

    FoldingManager foldingManager;
    //BraceFoldingStrategy foldingStrategy;

    public TextEditor() {
      InitializeComponent();

      // CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Plain;

      // -- AvalonEdit
      foldingManager = FoldingManager.Install(doc.TextArea);
      //foldingStrategy = new BraceFoldingStrategy();
      SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
      doc.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
    }


    public void SetText(string text) {
      if( text.StartsWith("<?xml version=\"1.0\"") )
        doc.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
      else
        doc.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");

      /*
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
        //t.Inlines.Add(new Run(text)); // used when we need to skip coloring (temp)
        doc.Document.Blocks.Add(t);
      } 
      */
      Display(text);
    }


    public virtual void Display(string message) {
      if( message == null ) {
        return;
      }

      var text = message;
      try {
        var jObject = JObject.Parse(message);
        text = jObject.GetFormatted();
      } catch( JsonReaderException ) {
        // It looks like we having issues parsing the json
        // Best to do in this circunstances is to still display the text
      }

      doc.Document.Text = text;
      //foldingStrategy.UpdateFoldings(foldingManager, document.Document);
    }

    public virtual void Clear() {
      doc.Document.Text = string.Empty;
      //foldingStrategy.UpdateFoldings(foldingManager, document.Document);
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
      //get { return new TextRange(doc.Document.ContentStart, doc.Document.ContentEnd).Text; }
      get { return doc.Document.Text; }
      set {
        SetText(value);
      }
    }


  }
}
