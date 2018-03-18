using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terrakoton.Helpers;
using Terrakoton.Helpers.Mt;
using Terrakoton.WebSokets;
using Terrakoton.WebSokets.Protocols.Bitfinex;

namespace ConsoleTests
{
    class Program
    {
        public const string BitfinexPublicKey = "S4tLToz7dXfOjvDgRZuihq8FvvKqPBcqXtnQDktAO4w";
        public const string BitfinexSecretKey = "dKnyxnmBWxWQ5hc4oSk3FZ8rhsqEJgYj0ExcvpPcCRF";
        static void Main(string[] args)
        {
            OrderCreateTest(new MtOrderArgs(
                MtOrderDirections.SELL,
                MtOrderTypes.LIMIT,
                "BTCUSD",
                2m,
                400m));
            Console.ReadLine();
        }

        private static void OrderCreateTest(MtOrderArgs orderArgs)
        {
            var protocol = new BitfinexProtocol(BitfinexPublicKey, BitfinexSecretKey, false);

            using (var manager = new WebSocketManager(protocol))
            {
                bool onWaitCall = false;
                bool result = false;
                manager.OpenSslConnection(
                    WebSocketManager.UrlDictionary[SourceTypes.BITFINEX]
                );
                protocol.OnAuthEvent += (sender, args) =>
                {
                    protocol.CreateOrder(orderArgs,
                        manager,
                        (id, executed, remaining, isCanceled) =>
                        {
                            Console
                                .WriteLine($"[OrderCreateTest] [OnSuccsess] External id:{id}");
                            result = true;
                        },
                        (l, s) =>
                        {
                            Console.WriteLine($"[OrderCreateTest] [OnError] {s}");
                            if (s.Contains("not enough tradable balance"))
                            {
                                result = true;
                            }
                        },
                        l =>
                        {
                            Console.WriteLine($"[OrderCreateTest] [OnWait] External id:{l}");
                            onWaitCall = true;
                        }
                    );
                    Console.WriteLine("[OrderCreateTest] Send message");
                };
                Thread.Sleep(5 * 1000);
            }
        }
    }
}
