#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    CursorPosition.cs
  Created: 2012-09-08

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion


namespace ServiceBusMQ {
  public enum CursorPosition {
    Body = 0,

    Left = 1,
    Right = 2,
    Top = 3,
    TopLeft = 4,
    TopRight = 5,
    Bottom = 6,
    BottomLeft = 7,
    BottomRight = 8,
  }
}
