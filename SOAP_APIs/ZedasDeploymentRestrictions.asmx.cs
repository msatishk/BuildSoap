using Azure.Messaging.ServiceBus;
using Shared.Utils;
using System;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using SOAP_APIs.IVUZedas;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using System.Web.Services.Protocols;

public class Global : HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        // Fetch the Instrumentation Key from the environment variable
        string instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
        // Initialize Application Insights if the key is not null or empty
        if (!string.IsNullOrEmpty(instrumentationKey))
        {
            TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.ConnectionString = instrumentationKey;
        }
    }
}

namespace FromZedasDeploymentRestrictions
{
    [WebService(Namespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0")]

    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ZedasDeploymentRestrictions : System.Web.Services.WebService
    //, OnxDeploymentRestrictionServicePortType
    {
        private readonly TelemetryClient telemetryClient = new TelemetryClient();

        [WebMethod]
        [SoapDocumentMethod("http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0/createDeploymentRestriction",
            RequestNamespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0",
            ResponseNamespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0",
            Use = System.Web.Services.Description.SoapBindingUse.Literal,
            ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]

        public createDeploymentRestrictionResponse createDeploymentRestriction([XmlElement(ElementName = "createDeploymentRestriction")] createDeploymentRestrictionRequest request)
        {
            try
            {
                telemetryClient.TrackTrace("createDeploymentRestriction was called" + request);
                string xmlSoapRequest = SerializeFromObject(request);
                // Validate topic and connectionString 
                string topicName, connectionString;
                ValidateAppSettings(out topicName, out connectionString);
                SendtoServiceBus(xmlSoapRequest, connectionString, topicName);
                telemetryClient.TrackTrace("createDeploymentRestriction Process Completed Successfully");
                return new createDeploymentRestrictionResponse(0, "Success");
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                return new createDeploymentRestrictionResponse(1, "Error");
            }
            finally
            {
                telemetryClient.Flush();
            }
        }

        [WebMethod]
        [SoapDocumentMethod("http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0/modifyDeploymentRestriction",
            RequestNamespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0",
            ResponseNamespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0",
            Use = System.Web.Services.Description.SoapBindingUse.Literal,
            ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]

        public modifyDeploymentRestrictionResponse modifyDeploymentRestriction([XmlElement(ElementName = "modifyDeploymentRestriction")] modifyDeploymentRestrictionRequest request)
        {
            try
            {
                telemetryClient.TrackTrace("modifyDeploymentRestriction was called");
                string xmlSoapRequest = SerializeFromObject(request);
                // Validate topic and connectionString 
                string topicName, connectionString;
                ValidateAppSettings(out topicName, out connectionString);
                SendtoServiceBus(xmlSoapRequest, connectionString, topicName);
                telemetryClient.TrackTrace(" modifyDeploymentRestriction Process Completed Succesfully");
                return new modifyDeploymentRestrictionResponse(0, "Success");
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                return new modifyDeploymentRestrictionResponse(1, "Error");
            }
            finally
            {
                telemetryClient.Flush();
            }
        }

        [WebMethod]
        [SoapDocumentMethod("http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0/deleteDeploymentRestriction",
            RequestNamespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0",
            ResponseNamespace = "http://www.ivu.de/MICROBUS/DeploymentRestrictionService/1.0",
            Use = System.Web.Services.Description.SoapBindingUse.Literal,
            ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        public deleteDeploymentRestrictionResponse deleteDeploymentRestriction([XmlElement(ElementName = "deleteDeploymentRestriction")] deleteDeploymentRestrictionRequest request)
        {
            try
            {
                telemetryClient.TrackTrace("deleteDeploymentRestriction was called");
                string xmlSoapRequest = SerializeFromObject(request);
                // Validate topic and connectionString 
                string topicName, connectionString;
                ValidateAppSettings(out topicName, out connectionString);
                SendtoServiceBus(xmlSoapRequest, connectionString, topicName);
                telemetryClient.TrackTrace("deleteDeploymentRestriction Process Completed Succesfully");
                return new deleteDeploymentRestrictionResponse(0, "Success");
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                return new deleteDeploymentRestrictionResponse(1, "Error");
            }
            finally
            {
                telemetryClient.Flush();
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

        private static void extractXml(string xmlSoapRequest)
        {
            // Initialize soap request XML
            //xmlSoapRequest = new XmlDocument();
            // Get raw request body
            //Stream receiveStream = HttpContext.Current.Request.InputStream;
            // Move to beginning of input stream and read
            //receiveStream.Position = 0;
            //using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
            //{
            // Load into XML document
            //  xmlSoapRequest.Load(readStream);
            //}
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlSoapRequest);
        }

        private static void ValidateAppSettings(out string topicName, out string connectionString)
        {
            topicName = Utils.VerifyAppSettingString("DeploymentRestrictionsTopicName");
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