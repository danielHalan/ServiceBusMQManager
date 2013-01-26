#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    CommandTextEditor.xaml.cs
  Created: 2013-01-19

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
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
//using ScintillaNET;
using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {

  public enum CommandTextType { Xml, Json }

  /// <summary>
  /// Interaction logic for CommandTextEditor.xaml
  /// </summary>
  public partial class CommandTextEditor : UserControl {

    public CommandTextEditor() {
      InitializeComponent();
    }

    public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(CommandTextEditor), new UIPropertyMetadata(string.Empty));

    public string Text {
      get { return tb.Text; }
      set { tb.SetText(value); }
    }


    CommandTextType _textType;
    public CommandTextType TextType {
      get { return _textType; }
      set { _textType = value; 
      
        if( _textType == CommandTextType.Xml ) 
          tb.CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Xml;
        else tb.CodeLanguage = NServiceBus.Profiler.Common.CodeParser.CodeLanguage.Json;
      }

    }

  }
}
