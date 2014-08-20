using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;

using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;

using PrintMap;


//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace PrintRestSOE
{
    [ComVisible(true)]
    [Guid("c3c8d0cd-0165-4be5-87b9-e1c82f54c53d")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "Insert SOE Description here",
        DisplayName = "PrintRestSOE",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class PrintRestSOE : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;

        public PrintRestSOE()
        {
            soe_name = this.GetType().Name;
            logger = new ServerLogger();
            reqHandler = new SoeRestImpl(soe_name, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        public void Init(IServerObjectHelper pSOH)
        {
            //System.Diagnostics.Debugger.Launch();
            serverObjectHelper = pSOH;
        }

        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        public void Construct(IPropertySet props)
        {
            configProps = props;
        }

        #endregion

        #region IRESTRequestHandler Members

        public string GetSchema()
        {
            return reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource rootRes = new RestResource(soe_name, false, RootResHandler);

            RestOperation printOper = new RestOperation("Print",
                                                      new string[] { "printParameters" },
                                                      new string[] { "json" },
                                                      PrintOperHandler);

            rootRes.operations.Add(printOper);

            RestOperation deleteOper = new RestOperation("Delete",
                                                      new string[] { "fileNames" },
                                                      new string[] { "json" },
                                                      DeleteOperHandler);
            rootRes.operations.Add(deleteOper);

            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("hello", "world");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] PrintOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            JsonObject info;
            bool found = operationInput.TryGetJsonObject("printParameters", out info);
            if (!found || info == null)
                throw new ArgumentNullException("printParameters");

            ExportMapInfo printInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ExportMapInfo>(info.ToJson());

            JsonObject result = new JsonObject();

            ExportMap export = new ExportMap();
            string outmsg = string.Empty;
            bool isPrinted = export.Export(printInfo, out outmsg);

            PrintResult pr = new PrintResult();
            if (isPrinted)
            {
                pr.IsPrinted = isPrinted;
                pr.FileName = outmsg;
                pr.OutMsg = "制图成功！";
            }
            else
            {
                pr.IsPrinted = isPrinted;
                pr.FileName = "未找到！";
                pr.OutMsg = outmsg;
            }
            result.AddObject("result", pr);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] DeleteOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            object[] fileNames;
            bool found = operationInput.TryGetArray("fileNames", out fileNames);
            if (!found || fileNames == null || fileNames.Length == 0)
                throw new ArgumentNullException("fileNames");
            JsonObject result = new JsonObject();

            ExportMap export = new ExportMap();
            bool tmpIsOk = false;
            string outMsg = string.Empty;
            List<string> fileList = new List<string>();
            foreach (object o in fileNames)
            {
                fileList.Add(o.ToString());
            }
            tmpIsOk = export.DeleteFile(fileList, out outMsg);
            result.AddBoolean("deleted", tmpIsOk);
            result.AddString("errMsg", outMsg);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }
    }
}
