using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySignalRClient
{
    class Program
    {
        private const bool _USE_SSL = false;

        private static string _embedCertHash;

        

        static void Main(string[] args)
        {
            HubConnection hubConnection = null;

            if (_USE_SSL)
            {
                // init hub connection with url ...
                hubConnection = new HubConnection("https://localhost:8080/");



                // init cert from cer resource
                using (var myCerStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MySignalRClient.my.cer"))
                using (var ms = new MemoryStream())
                {
                    myCerStream.CopyTo(ms);
                    var buf = ms.ToArray();
                    var embedCert = new X509Certificate(buf);
                    _embedCertHash = embedCert.GetCertHashString();
                    hubConnection.AddClientCertificate(embedCert);
                }
            }
            else
            {
                // init hub connection with url ...
                hubConnection = new HubConnection("http://localhost:8079/");
            }
            
            var myHubProxy = hubConnection.CreateHubProxy("MyHub");

            // validate certificate from MIM attack
            ServicePointManager.ServerCertificateValidationCallback = _ValidateCert;

            // attach event handler from server sent message.
            myHubProxy.On<string, string>("addMessage", _OnAddMessage);
            myHubProxy.On<string>("showMsg", _OnShowMsg);

            // retry connection every 3 seconds ...
            while (hubConnection.State != ConnectionState.Connected)
            {
                try
                {
                    hubConnection.Start().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"
connection failed !
sleep 3 sec ...
try reconnect ...
                    ");

                    Task.Delay(3000).Wait();
                }
            }



            // run actions (Send, StartTimer)
            _RunServerLoop(myHubProxy);



            // exit program
            Console.WriteLine("ended!!!");
            Console.ReadLine();
        }



        private static bool _ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var result = _embedCertHash == certificate.GetCertHashString();

            if (!result)
            {
                Console.WriteLine($@"
certificate is modified !
_certHashStr : {_embedCertHash}
certificate.GetCertHashString() : {certificate.GetCertHashString()}
                    ");
            }

            return result;
        }

        

        private static void _OnShowMsg(string msg)
        {
            Console.WriteLine($"RECV showMsg : {msg}");
        }



        private static void _OnAddMessage(string name, string message)
        {
            Console.WriteLine($"RECV addMessage : {name} : {message}");
        }



        private static void _RunServerLoop(IHubProxy myHubProxy)
        {
            while (true)
            {
                Console.WriteLine(@"
commands
------------------------------------
Q : quit
S : Send(""william"", ""hello"")
T : StartTimer(10)
                ");

                var cmd = Console.ReadKey();

                if (cmd.Key == ConsoleKey.Q)
                {
                    break;
                }
                else if (cmd.Key == ConsoleKey.S)
                {
                    myHubProxy.Invoke("Send", new object[] { "william", "hello" });
                }
                else if (cmd.Key == ConsoleKey.T)
                {
                    myHubProxy.Invoke("StartTimer", new object[] { 10 });
                }
            }
        }
    }
}
