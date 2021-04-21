using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;
using RestSharp;
using RestSharp.Authenticators;
using AgentInterface;
using System.Diagnostics;


namespace Console981
{   


    class Program
    {
        public static int VR_TYPE = 0;
        public static string url;

        static void Main(string[] args)
        {
            Updater updater = new Updater();
            updater.Init();

            if (args.Length < 2)
            {
                Console.WriteLine("insufficient arguments <VR Type:args[0]> <url:args[1]>");
                Console.ReadLine();
                return;
            }


            bool b = int.TryParse(args[0], out VR_TYPE);
            if (b == false)
            {
                Console.WriteLine("wrong type argument <VR Type:args[0]:number>");
                Console.ReadLine();
                return;
            }

            switch (VR_TYPE)
            {
                case 2:
                    Console.WriteLine("VR E/1");
                    break;

                case 3:
                    Console.WriteLine("VR E/2");
                    break;

                case 4:
                    Console.WriteLine("VR X/1");
                    break;

                case 5:
                    Console.WriteLine("VR X/2");
                    break;

                default:
                    Console.WriteLine("not defined type " + VR_TYPE);
                    Console.ReadLine();
                    return;
            }

            url = args[1];
            Console.WriteLine("URL: " + url);
            Console.WriteLine("ex) " + url + VR_TYPE + "/end");

            while (true)
            {
                updater.Update();
            }

            
        }
    }



    public class Updater
    {
        static public T DeepCopy<T>(T obj)
        {
            BinaryFormatter s = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                s.Serialize(ms, obj);
                ms.Position = 0;
                T t = (T)s.Deserialize(ms);

                return t;
            }
        }

        AppHTTPListener listener;
        AgentInterface.MMFWrapper wrapper;
        AgentInterface.InterfaceStruct bkStruct;
        AgentInterface.InterfaceStruct readStruct;

        public void Init()
        {
            listener = new AppHTTPListener();
            listener.Init();

            wrapper = new AgentInterface.MMFWrapper();
            bkStruct = new AgentInterface.InterfaceStruct();
        }

        public void Update()
        {
            wrapper.ReadMMF(ref readStruct);

            if (bkStruct.state != readStruct.state)
            {
                Process_state((AppState)bkStruct.state, (AppState)readStruct.state);
            }


            bkStruct = DeepCopy<AgentInterface.InterfaceStruct>(readStruct);
            Thread.Sleep(100);
        }


        private void Process_state(AppState last, AppState curr)
        {
            if (curr == AppState.Run)
            {
                _Start();
            }

            if (curr != AppState.Run)
            {
                _End();
            }
        }


        private void _Start()
        {
            //var client = new RestClient("http://dev-apis.981park.net:80/lab981/admin/game/device/" + Program.VR_TYPE + "/start");
            string uri = Program.url + Program.VR_TYPE + "/start";
            var client = new RestClient(uri);
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-Authorization-Client-Id", "981park-lab981-external");
            request.AddHeader("X-Authorization-Client-Secret", "161f94ca-0bb5-4231-a6f7-11b14403af1d");
            request.AddParameter("application/json", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }


        private void _End()
        {
            //var client = new RestClient("http://dev-apis.981park.net:80/lab981/admin/game/device/"+Program.VR_TYPE+"/end");
            string uri = Program.url + Program.VR_TYPE + "/end";
            var client = new RestClient(uri);
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-Authorization-Client-Id", "981park-lab981-external");
            request.AddHeader("X-Authorization-Client-Secret", "161f94ca-0bb5-4231-a6f7-11b14403af1d");
            request.AddParameter("application/json", "", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
    }
}
