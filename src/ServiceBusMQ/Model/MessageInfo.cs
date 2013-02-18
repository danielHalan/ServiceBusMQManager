using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceBusMQ.Model {
  public class MessageInfo {

    public string Name { get; private set; }
    public string AssemblyQualifiedName { get; private set; }
  
    public MessageInfo(string name, string asmName = null) { 
      Name = name;
      AssemblyQualifiedName = asmName;
    }

  }
}
