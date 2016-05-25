using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySignalRServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.



            //var option = new StartOptions();
            //option.Urls.Add("http://localhost:80");
            //option.Urls.Add("https://localhost:443");

            //using (WebApp.Start<Startup>(option))
            //{
            //    Console.WriteLine("Server running on {0}", option);
            //    Console.ReadLine();
            //}



            //using (WebApp.Start<Startup>("http://localhost:80"))
            //{
            //    Console.WriteLine("Server running on {0}", "http://localhost:80");
            //    Console.ReadLine();
            //}



            using (WebApp.Start<Startup>("https://*:8080"))
            {
                Console.WriteLine("Server running on {0}", "https://*:8080");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    public class MyHub : Hub
    {
        public void Send(string name, string message)
        {
            Console.WriteLine($"Send name : {name} message :{message}");
            Clients.All.addMessage(name, message);
        }

        public void StartTimer(int count)
        {
            Console.WriteLine($"StartTimer count : {count}");

            Task.Run(async () => 
            {
                var msg = $"타이머 시작됨...";
                Console.WriteLine(msg);
                Clients.Caller.showMsg(msg);

                for (int i = 0; i < count; i++)
                {
                    await Task.Delay(1000);
                    msg = $"타이머 카운트 {i}/{count}...";
                    Console.WriteLine(msg);
                    Clients.Caller.showMsg(msg);
                }

                msg = $"타이머 종료됨...";
                Console.WriteLine(msg);
                Clients.Caller.showMsg(msg);
            });
        }

        public override Task OnConnected()
        {
            Console.WriteLine("OnConnected");
            _PrintContext();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine("OnDisconnected");
            _PrintContext();
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Console.WriteLine("OnReconnected");
            _PrintContext();
            return base.OnReconnected();
        }

        private void _PrintContext()
        {
            Console.WriteLine($"this.Context.ConnectionId : {this.Context.ConnectionId}");
            Console.WriteLine($"this.Context.Request.Url : {this.Context.Request.Url}");
            Console.WriteLine($"this.Context.Headers : {JsonConvert.SerializeObject(this.Context.Headers)}");
        }
    }
}
