using SOAP_APIs.IVUVehicleConsist;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using Shared.Utils;

namespace IVUVehicleConsist
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://www.ivu.de/intf.jobs.impl/IntfJobsPushWebService/IntfJobsSendExport")]
    public class IVUVehicle : System.Web.Services.WebService
    {
        private TelemetryClient _telemetryClient;
        private TelemetryConfiguration configuration;

        [WebMethod]
        [SoapDocumentMethod("http://www.ivu.de/intf.jobs.impl/IntfJobsPushWebService/IntfJobsSendExport", OneWay = false, Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        public void sendIntfJobsExport([XmlElement(Namespace = "http://www.ivu.de/integration/vehicle/vehiclegroup/standard")] VehicleGroupExport vehicleGroupExport)
        {            
            try
            {
                _telemetryClient = TelemetryFactory.GetTelemetryClient();
                _telemetryClient.TrackTrace("IVUVehicleConsist: sendIntfJobsExport was called");
                string xmlSoapRequest = SerializeFromObject(vehicleGroupExport);
                //Validate topic and connectionString
                ValidateAppSettings(out string topicName, out string connectionString);
                SendtoServiceBus(xmlSoapRequest, connectionString, topicName);
                _telemetryClient.TrackTrace("IVUVehicleConsist: sendIntfJobsExport Process Completed Successfully");
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
            }
            finally
            {
                _telemetryClient.Flush();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (configuration != null)
                configuration.Dispose();
        }
        public static string SerializeFromObject<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, obj);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        private static void extractXml(out XmlDocument xmlSoapRequest)
        {
            // Initialize soap request XML
            xmlSoapRequest = new XmlDocument();
            // Get raw request body
            Stream receiveStream = HttpContext.Current.Request.InputStream;
            // Move to beginning of input stream and read
            receiveStream.Position = 0;
            using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
            {
                // Load into XML document
                xmlSoapRequest.Load(readStream);
            }
        }
        private static void ValidateAppSettings(out string topicName, out string connectionString)
        {
            topicName = Utils.VerifyAppSettingString("IVUVehicleConsistTopicName");
            connectionString = Utils.VerifyAppSettingString("ServiceBusConnectionString");
        }
        private static void SendtoServiceBus(string xmlSoapRequest, string connectionString, string topicName)
        {
            // Send to service bus 
            // Create a ServiceBusClient
            ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
            // Create a sender for the specified topic
            ServiceBusSender sender = serviceBusClient.CreateSender(topicName);
            // Create a message with the SOAP request XML
            ServiceBusMessage message = new ServiceBusMessage(xmlSoapRequest);
            // Send the message to the Service Bus
            sender.SendMessageAsync(message).Wait();
            sender.CloseAsync();
            serviceBusClient.DisposeAsync();
        }
    }
    public static class TelemetryFactory
    {
        private static TelemetryClient _telemetryClient;

        public static TelemetryClient GetTelemetryClient()
        {
            if (_telemetryClient == null)
            {
                TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();
                telemetryConfiguration.ConnectionString = Utils.VerifyAppSettingString("APPLICATIONINSIGHTS_CONNECTION_STRING");
                Console.WriteLine(telemetryConfiguration.ConnectionString);
                _telemetryClient = new TelemetryClient(telemetryConfiguration);
            }
            return _telemetryClient;
        }
    }
}