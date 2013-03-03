#region File Information
/********************************************************************
  Project: ServiceBusMQ
  File:    ErrorReport.cs
  Created: 2013-03-02

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
using System.IO;
using ServiceBusMQ.HalanService;
using ServiceBusMQ.Manager;

namespace ServiceBusMQ.Model.HalanService {

  public class ErrorReport {

    ApplicationInfo _appData;
    Error _error;
    string[] _managerState;

    public ErrorReport(ApplicationInfo appData, string errorMsg, string[] managerState) {
      _error = new Error(errorMsg);
      _appData = appData;
      _managerState = managerState;
    }

    public ErrorReport(ApplicationInfo appData, Error error, string[] managerState) {
      _error = error;
      _appData = appData;
      _managerState = RemoveSensibleData(managerState);
    }

    void RemoveSensibleParamContent(List<string> data, string paramName) {
      int index = data.IndexOf(paramName);

      if( index != -1 && index + 1 < data.Count )
        data[index + 1] = "[removed]";
    }

    private string[] RemoveSensibleData(string[] managerState) {
      List<string> data = new List<string>(managerState);

      RemoveSensibleParamContent(data, "-w");
      RemoveSensibleParamContent(data, "-pwd");

      return data.ToArray();
    }

    public Guid Send() {
      ProductManagerClient pm = HalanServices.CreateProductManager();

      try {
        Guid id = Guid.NewGuid();
        ErrorReportRequest req = new ErrorReportRequest();
        req.Reference = id.ToString();
        req.Message = _error.Message;
        if( _error.Exception != null ) {
          Exception e = _error.Exception;
          ErrorReportException errorRepExcept = new ErrorReportException();
          req.Exception = errorRepExcept;
          while( e != null ) {
            errorRepExcept.Message = e.Message;
            errorRepExcept.StackTrace = e.StackTrace;
            errorRepExcept.Source = e.Source;
            errorRepExcept.Type = e.GetType().ToString();

            e = e.InnerException;
            if( e != null ) {
              errorRepExcept.InnerException = new ErrorReportException();
              errorRepExcept = errorRepExcept.InnerException;
            }
          }
        }

        req.ProductName = _appData.Product;
        req.ProductVersion = _appData.Version.ToString(4);
        req.DotNetFrameworkVersion = Environment.Version.ToString();
        req.OperatingSystem = Environment.OSVersion.VersionString;
        req.ReportID = Tools.EncryptSimple(_appData.Id);

        if( _managerState != null && _managerState.Length > 0 )
          req.ManagerState = Tools.EncryptSimple(_managerState.Concat(" "));

        ErrorReportResponse resp = pm.ReportError(req);

        if( !resp.Successful )
          throw new FailedToSendErrorReportException(resp.Message);

        return id;

      } finally {
        pm.Close();
      }
    }

    /*
    public void SendFile(Guid reportId, string filePath) {
      ProductManagerClient pm = ServiceManager.ProductManager;
      byte[] data = null;

      if( Path.GetExtension(filePath) != ".zip" )
        data = Tools.Zip(filePath);
      else data = File.ReadAllBytes(filePath);

      File.WriteAllBytes(@"c:\temp\ErrorReportTest.zip", data);

      ReportAttachmentRequest req = new ReportAttachmentRequest();
      req.ReportId = reportId.ToString();
      req.FileName = Path.GetFileName(filePath);
      req.Data = data;
      pm.ReportAttachment(req);
    }
    */
  }
}
