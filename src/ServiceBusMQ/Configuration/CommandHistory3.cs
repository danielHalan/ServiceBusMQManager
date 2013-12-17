#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    CommandHistory3.cs
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
  public class SavedCommandItems3 {
    public SavedCommandItem3[] Items { get; set; }
  }

  [Serializable]
  public class SavedCommandItem3 {
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string FileName { get; set; }
    public bool Pinned { get; set; }
    public DateTime LastSent { get; set; }


    [JsonConstructor]
    public SavedCommandItem3() {
      IsNew = false;
    }

    public SavedCommandItem3(string displayName, string fileName, bool pinned, DateTime lastSent) {
      Id = Guid.NewGuid();

      DisplayName = displayName;
      FileName = fileName;
      Pinned = pinned;
      LastSent = lastSent;

      IsNew = true;
    }

    public SavedCommandItem3(Guid id, string displayName, string fileName, bool pinned, DateTime lastSent) {
      Id = id;
      IsNew = false;

      DisplayName = displayName;
      FileName = fileName;
      Pinned = pinned;
      LastSent = lastSent;
    }


    SavedCommand3 _sentCommand = null;

    [JsonIgnore]
    public SavedCommand3 SentCommand {
      get {
        if( _sentCommand == null ) {
          _sentCommand = JsonFile.Read<SavedCommand3>(FileName);
        }

        return _sentCommand;
      }
    }

    public void SetCommand(SavedCommand3 cmd) {
      _sentCommand = cmd;

      IsNew = true;
    }

    [JsonIgnore]
    public bool IsNew { get; private set; }
  }



}
