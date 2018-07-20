using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ConsoleAppslmoc
{
    public class UserStatus
    {
        public string UserId { get; set; }
        public string Status { get; set; }
    }
    public class ResultMessage
    {
        public string result { get; set; }
        public string message { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            MqttClient client = new MqttClient("innodev.vnetcloud.com");

            byte code = client.Connect(Guid.NewGuid().ToString(), "vsnbroker", "Password1!");
            //byte code  = client.Connect(Guid.NewGuid().ToString(),
            //                       "vsnbroker",
            //                       "Password1!",
            //                       true, // will retain flag
            //                       MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // will QoS
            //                       true, // will flag
            //                       "/UserOffline", // will topic
            //                       "User Laptop", // will message
            //                       true,
            //                       20);

            if (client.IsConnected)
            {
                Console.WriteLine("Mqtt connection success");
            }
            else
            {
                Console.WriteLine("Mqtt connection failed");
            }


            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            ushort msgId = client.Subscribe(new string[] { "webOnlineTracking" },
                 new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Console.WriteLine("Connect");



            //Console.ReadLine();

            //client.Disconnect();
        }

        private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //update status DB
            //Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
            Console.WriteLine("Received Message");
            Console.WriteLine("Topic: " + e.Topic);
            //Console.WriteLine("Payload: " + e.Payload);
            string getTopic = Encoding.UTF8.GetString(e.Message);
            string UserId = getTopic.Split(',').First();
            string Status = getTopic.Split(',').Last();
            Console.WriteLine("UserId: " + UserId + " - " + Status);
            try
            {

                RunAsync(UserId, Status).Wait();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        static async Task<ResultMessage> UpdateUserStatusAsync(UserStatus UserStatus)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://lmocs.vnetcloud.com/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //Console.WriteLine("testtt");
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"api/Transaction?updateStatusUser", UserStatus);
            Console.WriteLine("User Status Updated Successfully.");

            // Deserialize the updated product from the response body.
            ResultMessage result = new ResultMessage();
            result = await response.Content.ReadAsAsync<ResultMessage>();
            return result;
        }
        static async Task RunAsync(string UserId, string Status)
        {
            // Update port # in the following line.
            UserStatus UserStatus = new UserStatus
            {
                UserId = UserId,
                Status = Status
            };

            Console.WriteLine("Updating status user...");
            await UpdateUserStatusAsync(UserStatus);

        }
    }
}

