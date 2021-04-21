using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using System.ServiceModel;
using System.Runtime.CompilerServices;
using AgentInterface;

using Newtonsoft.Json;

namespace Console981
{
    [System.Serializable]
    public class ClsResponse
    {
        public string code;
    }

    class AppHTTPListener
    {

        private static ClsResponse responseClass = new ClsResponse();

        public static IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }


        public HttpListener _listener;

        private const int _apiPortNumber = 8080;
        public const string BasePath = "";
        public static EndpointAddress MyEndPoint => new EndpointAddress(
            new UriBuilder(
                    Uri.UriSchemeHttp,
                    //"localhost",//only allow connections on localhost (no remote access)
                    LocalIPAddress().ToString(),//only allow connections on localhost (no remote access)
                    _apiPortNumber,
                    BasePath)
                .Uri);


        private delegate void RequestHandler(Match match, HttpListenerResponse response);
        private Dictionary<Regex, RequestHandler> _requestHandlers = new Dictionary<Regex, RequestHandler>();


        public void Init()
        {
            Console.WriteLine("Prefix: " + MyEndPoint.Uri.ToString());

            _listener = new HttpListener();
            _listener.Prefixes.Add(MyEndPoint.Uri.ToString());
            //_listener.AuthenticationSchemes = AuthenticationSchemes.None;

            _listener.Start();
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);

            _requestHandlers[new Regex(@"^/start")] = HandleRegister_start;
            _requestHandlers[new Regex(@"^/stop")] = HandleRegister_stop;
            //_requestHandlers[new Regex(@"^/stoicregister/(.*)/MACADDR:(.*)$")] = HandleRegister;
            //_requestHandlers[new Regex(@"^/stoicregister_/(.*)/MACADDR:(.*)$")] = HandleRegister_NotEncrypted;
            //_requestHandlers[new Regex(@"^/makeserial/(\d+)/(.*)$")] = HandleMakeSerial;
            //_requestHandlers[new Regex(@"^/stoicopengenerator/(.*)$")] = HandleGeneratorFirst;
            //_requestHandlers[new Regex(@"^/stoicgenerator/(.*)$")] = HandleGenerator;
            //_requestHandlers[new Regex(@"^/stoicgamelog/(.*)/(.*)/(.*)$")] = HandleGameLog;
        }

        private static void HandleRegister_start(Match match, HttpListenerResponse response)
        {
            //string param1 = match.Groups[1].Value;
            //string param2 = match.Groups[2].Value;            

            responseClass.code = "EC200";
            string responseString = JsonConvert.SerializeObject(responseClass);

            Console.WriteLine("[HandleRegister_start] " + responseString);

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();

            MMFWrapper.sCommander.SetCommand(Command.Run);
        }

        private static void HandleRegister_stop(Match match, HttpListenerResponse response)
        {
            //string param1 = match.Groups[1].Value;
            //string param2 = match.Groups[2].Value;            

            responseClass.code = "EC200";
            string responseString = JsonConvert.SerializeObject(responseClass);

            Console.WriteLine("[HandleRegister_stop] " + responseString);

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();

            MMFWrapper.sCommander.SetCommand(Command.Stop);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            Console.WriteLine("[HTTPListenerBase] ListenerCallback ");

            HttpListener listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;


            Console.WriteLine("[HTTPListenerBase] ListenerCallback " + request.Url.AbsolutePath);


            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
            //*/
            foreach (Regex r in _requestHandlers.Keys)
            {
                Match m = r.Match(request.Url.AbsolutePath);
                if (m.Success)
                {
                    (_requestHandlers[r])(m, response);
                    Console.WriteLine("[HTTPListenerBase] ListenerCallback leave " + request.Url.AbsolutePath);
                    return;
                }
            }
            //*/

            Console.WriteLine("[HTTPListenerBase] ListenerCallback " + request.Url.AbsolutePath + " failed return 404");
            response.StatusCode = 404;
            response.Close();
        }        
    }
}
