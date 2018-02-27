using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;

namespace AlarmServiceBusConsoleApp
{
    class WebServerConnector
    {
        private string _webServerUrl;

        public WebServerConnector(string webServerUrl)
        {
            this._webServerUrl = webServerUrl;
        }

        public string PostTelemetryAlarm(AlarmMessage alarmMessage)
        {
            try
            {
                string postData = "ioTHubDeviceID=" + alarmMessage.ioTHubDeviceID + "&" +
                                    "alarmType=" + alarmMessage.alarmType + "&" +
                                    "reading=" + alarmMessage.reading + "&" +
                                    "threshold=" + alarmMessage.threshold + "&" +
                                    "createdAt=" + alarmMessage.createdAt;

                var data = Encoding.UTF8.GetBytes(postData);

                var request = (HttpWebRequest)WebRequest.Create(_webServerUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
