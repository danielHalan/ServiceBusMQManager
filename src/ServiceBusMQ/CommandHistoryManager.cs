using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;

namespace ServiceBusMQ {
  public class CommandHistoryManager {


    string _itemsFile;

    List<SavedCommand> _items;

    public IEnumerable<SavedCommand> Items { get { return _items.OrderByDescending( i => i.LastSent ); } }

    public CommandHistoryManager() {

      _itemsFile = SbmqSystem.AppDataPath + @"\commands.dat";

      Load();
    }

    private void Load() {
      _items = JsonFile.Read<List<SavedCommand>>(_itemsFile);

      if( _items == null )
        _items = new List<SavedCommand>();
    }
    private void Save() {
      JsonFile.Write(_itemsFile, _items);
    }

    public void RenameCommand(string displayName, object command) {

      var co = new CompareObjects();

      foreach( var c in _items ) {

        if( co.Compare(c.Command, command) ) {
          c.DisplayName = displayName;

          Save();
          break;
        }
      }

    }

    public SavedCommand CommandSent(object command, string serviceBus, string transport, string server, string queue) {
      SavedCommand cmd = null;

      var co = new CompareObjects();

      foreach( var c in _items ) {
      
        if( co.Compare(c.Command, command) ) {
          cmd = c; // TODO: when we show what SB/Server/Q the command has been sent to, then also compare those values
          break;
        }
      }

      if( cmd == null ) {
        cmd = new SavedCommand();      
        
        cmd.DisplayName = command.GetType().GetDisplayName(command).CutEnd(70);
        cmd.Command = command;
      
        cmd.ServiceBus = serviceBus;
        cmd.Transport = transport;
        cmd.Server = server;
        cmd.Queue = queue;

        _items.Add(cmd);
      }

      cmd.LastSent = DateTime.Now;

      Save();

      return cmd;
    }




  }
}
