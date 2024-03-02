using Azure.Messaging.ServiceBus;
using SOAP_APIs.IVUZedasVehicles;
using Microsoft.ApplicationInsights;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using Shared.Utils;

namespace FromZedasServiceIntervals
{
    /// <summary>
    /// Summary description for FromZedasServiceIntervals
    /// </summary>
    [WebService(Namespace = "http://www.ivu.de/mb/intf/vehicle/remote/")]
    //[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ZedasServiceIntervals : System.Web.Services.WebService//, VehicleWebFacade
    {
        private readonly TelemetryClient telemetryClient = new TelemetryClient();

        [WebMethod]
        [SoapDocumentMethod("http://www.ivu.de/mb/intf/vehicle/remote", OneWay = false, Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]

        //public importVehiclesResponse importVehicles(importVehicles request)
        public importVehiclesResponse importVehicles([XmlElement(ElementName = "vehicleRequest", Namespace = "http://www.ivu.de/mb/intf/vehicle/remote")] VehicleRequest request)
        {
            try
            {
                telemetryClient.TrackTrace("importVehicles was called");
                string xmlSoapRequest = SerializeFromObject(request);
                // Validate topic and connectionString 
                string topicName, connectionString;
                ValidateAppSettings(out topicName, out connectionString);
                SendtoServiceBus(xmlSoapRequest, connectionString, topicName);
                telemetryClient.TrackTrace("importVehicle Process Completed Successfully");
                return new importVehiclesResponse() { vehicleResponse = new VehicleResponse() };
            }
            catch (Exception ex)
            {
                telemetryClient.TrackTrace("An error occurred in importVehicles: " + ex.Message);
                return new importVehiclesResponse() { vehicleResponse = new VehicleResponse() };                
            }
        }
        public static string SerializeFromObject<T>(T obj)
        {
            /* using (MemoryStream stream = new MemoryStream())
             using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
             {
                 XmlSerializer serializer = new XmlSerializer(typeof(T));
                 serializer.Serialize(writer, obj);
                 return System.Text.Encoding.UTF8.GetString(stream.ToArray());
             }*/

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            // Use XmlWriter to control the XML declaration
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                serializer.Serialize(writer, obj);
            }
            return sb.ToString();
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
            topicName = Utils.VerifyAppSettingString("ServiceIntervalsTopicName");
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
}