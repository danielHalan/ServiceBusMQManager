#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    CommandHistory2.cs
  Created: 2013-10-23

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
using Newtonsoft.Json;

namespace ServiceBusMQ.Configuration {

  [Serializable]
  public class SavedCommandItems {
    public int Version { get; set; }
    public SavedCommandItem[] Items { get; set; }
  }

  [Serializable]
  public class SavedCommandItem {
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string FileName { get; set; }
    public bool Pinned { get; set; }
    public DateTime LastSent { get; set; }


    [JsonConstructor]
    public SavedCommandItem() {
      IsNew = false;
    }

    public SavedCommandItem(string displayName, string fileName, bool pinned, DateTime lastSent) {
      Id = Guid.NewGuid();

      DisplayName = displayName;
      FileName = fileName;
      Pinned = pinned;
      LastSent = lastSent;

      IsNew = true;
    }

    SavedCommand2 _sentCommand = null;

    [JsonIgnore]
    public SavedCommand2 SentCommand {
      get {
        if( _sentCommand == null ) {
          _sentCommand = JsonFile.Read<SavedCommand2>(FileName);
        }

        return _sentCommand;
      }
    }

    public void SetCommand(SavedCommand2 cmd) {
      _sentCommand = cmd;

      IsNew = true;
    }

    [JsonIgnore]
    public bool IsNew { get; private set; }
  }



}
