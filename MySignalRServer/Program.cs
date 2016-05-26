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
        private const bool _USE_SSL = false;

        static void Main(string[] args)
        {
            // server host url
            var url = "";

            if (_USE_SSL)
            {
                url = "https://*:8080";
            }
            else
            {
                url = "http://*:8079";
            }

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine($"Server running on url : {url}");
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
        // server side method #1 : Send
        // echo name and message
        public void Send(string name, string message)
        {
            Console.WriteLine($"Send name : {name} message :{message}");
            Clients.All.addMessage(name, message);
        }

        // server side method #2 : StartTimer
        // send msgs every seconds until count variable ...
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

        /// <summary>
        /// print context object
        /// we can know about additional information (like account, auth, ...) in this.Context.
        /// </summary>
        private void _PrintContext()
        {
            Console.WriteLine($"this.Context.ConnectionId : {this.Context.ConnectionId}");
            Console.WriteLine($"this.Context.Request.Url : {this.Context.Request.Url}");
            Console.WriteLine($"this.Context.Headers : {JsonConvert.SerializeObject(this.Context.Headers)}");
        }
    }
}
