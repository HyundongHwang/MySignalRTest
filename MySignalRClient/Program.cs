using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySignalRClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var hubConnection = new HubConnection("https://localhost:8080/");
            var certFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "my.cer");
            var embedCert = X509Certificate.CreateFromCertFile(certFilePath);
            hubConnection.AddClientCertificate(embedCert);
            var myHubProxy = hubConnection.CreateHubProxy("MyHub");

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var result = embedCert.GetCertHashString() == certificate.GetCertHashString();

                if (!result)
                {
                    Console.WriteLine($@"
certificate is modified !
embedCert.GetCertHashString() : {embedCert.GetCertHashString()}
certificate.GetCertHashString() : {certificate.GetCertHashString()}
                    ");
                }

                return result;
            };

            myHubProxy.On<string, string>("addMessage", (name, message) =>
            {
                Console.WriteLine($"RECV addMessage : {name} : {message}");
            });

            myHubProxy.On<string>("showMsg", (msg) =>
            {
                Console.WriteLine($"RECV showMsg : {msg}");
            });

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

            Console.WriteLine("ended!!!");
            Console.ReadLine();
        }
    }
}
