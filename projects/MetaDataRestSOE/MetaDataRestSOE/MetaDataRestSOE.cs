using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;

using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;

using MetaDataSDE;

//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace MetaDataRestSOE
{
    [ComVisible(true)]
    [Guid("b5d3493b-5036-4d2b-9831-ba6dc8daeede")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "Insert SOE Description here",
        DisplayName = "MetaDataRestSOE",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class MetaDataRestSOE : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;

        public MetaDataRestSOE()
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

            RestOperation saveOper = new RestOperation("SaveFeatures",
                                                      new string[] { "connectionString", "featureClass", "features" },
                                                      new string[] { "json" },
                                                      SaveFeaturesOperHandler);

            rootRes.operations.Add(saveOper);

            RestOperation deleteOper = new RestOperation("DeleteFeatures",
                                                      new string[] { "connectionString", "featureClass", "filterWhere" },
                                                      new string[] { "json" },
                                                      DeleteFeaturesOperHandler);

            rootRes.operations.Add(deleteOper);

            RestOperation getOper = new RestOperation("GetFeatures",
                                                      new string[] { "connectionString", "featureClass", "filterWhere" },
                                                      new string[] { "json" },
                                                      GetFeaturesOperHandler);

            rootRes.operations.Add(getOper);

            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("hello", "world");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] SaveFeaturesOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            JsonObject joConn;
            bool found = operationInput.TryGetJsonObject("connectionString", out joConn);
            if (!found || joConn == null)
                throw new ArgumentNullException("connectionString can not parse to jsonobject");

            JsonObject joFC;
            found = operationInput.TryGetJsonObject("featureClass", out joFC);
            if (!found || joFC == null)
                throw new ArgumentNullException("featureClass can not parse to jsonobject");

            object[] joFeatures;
            found = operationInput.TryGetArray("features", out joFeatures);
            if (!found || joFeatures == null)
                throw new ArgumentNullException("features can not parse to jsonobject");

            JsonObject result = new JsonObject();

            ConnectionJSObject connJsObj = ParamsParser.ParseConn(joConn);
            FeatureClassJSObject fcObj = ParamsParser.ParseFeatrureClass(joFC);
            List<FeatureObject> features = ParamsParser.ParseFeatures(joFeatures);

            bool isok = false;
            int count = 0;
            string outMsg = string.Empty;
            FeaturesHelper helper = new FeaturesHelper();
            isok = helper.SaveFeatures(connJsObj, fcObj, features, out count, out outMsg);
            result.AddBoolean("isSucess", isok);
            result.AddLong("recordCount", count);
            result.AddString("outMsg", outMsg);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] DeleteFeaturesOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            JsonObject joConn;
            bool found = operationInput.TryGetJsonObject("connectionString", out joConn);
            if (!found || joConn == null)
                throw new ArgumentNullException("connectionString can not parse to jsonobject");

            JsonObject joFC;
            found = operationInput.TryGetJsonObject("featureClass", out joFC);
            if (!found || joFC == null)
                throw new ArgumentNullException("featureClass can not parse to jsonobject");

            JsonObject joFilter;
            found = operationInput.TryGetJsonObject("filterWhere", out joFilter);
            if (!found || joFilter == null)
                throw new ArgumentNullException("filterWhere can not parse to jsonobject");

            JsonObject result = new JsonObject();

            ConnectionJSObject connJsObj = ParamsParser.ParseConn(joConn);
            FeatureClassJSObject fcObj = ParamsParser.ParseFeatrureClass(joFC);
            FilterWhereJSObject filterObj = ParamsParser.ParseFilterWhere(joFilter);

            bool isok = false;
            int count = 0;
            string outMsg = string.Empty;
            FeaturesHelper helper = new FeaturesHelper();
            isok = helper.DeleteFeatures(connJsObj, fcObj, filterObj, out count, out outMsg);
            result.AddBoolean("isSucess", isok);
            result.AddLong("recordCount", count);
            result.AddString("outMsg", outMsg);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] GetFeaturesOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            JsonObject joConn;
            bool found = operationInput.TryGetJsonObject("connectionString", out joConn);
            if (!found || joConn == null)
                throw new ArgumentNullException("connectionString");

            JsonObject joFC;
            found = operationInput.TryGetJsonObject("featureClass", out joFC);
            if (!found || joFC == null)
                throw new ArgumentNullException("featureClass");

            JsonObject joFilter;
            found = operationInput.TryGetJsonObject("filterWhere", out joFilter);
            if (!found || joFilter == null)
                throw new ArgumentNullException("filterWhere");

            JsonObject result = new JsonObject();

            ConnectionJSObject connJsObj = ParamsParser.ParseConn(joConn);
            FeatureClassJSObject fcObj = ParamsParser.ParseFeatrureClass(joFC);
            FilterWhereJSObject filterObj = ParamsParser.ParseFilterWhere(joFilter);

            List<FeatureObject> features;
            string outMsg = string.Empty;
            FeaturesHelper helper = new FeaturesHelper();
            features = helper.GetFeatures(connJsObj, fcObj, filterObj, out outMsg);
            List<JsonObject> featureList = new List<JsonObject>();
            foreach (FeatureObject fo in features)
            {
                JsonObject jo = new JsonObject();
                jo.AddArray("attributes", fo.attributes.ToArray());
                jo.AddJsonObject("geometry", Conversion.ToJsonObject(fo.geometry));
                featureList.Add(jo);
            }
            result.AddArray("features", featureList.ToArray());
            result.AddString("outMsg", outMsg);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

    }
}
