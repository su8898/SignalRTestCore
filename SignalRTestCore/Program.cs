﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRTestCore
{
    class Program
    {
        static void Main(string[] args)
        {
            RunTaksAsync().GetAwaiter().GetResult();
        }

        private static async Task RunTaksAsync()
        {
            List<Task> TaskList = new List<Task>();
            for (int userIndex = 0; userIndex < 50; userIndex++)
            {
                var LastTask = Main2Async(userIndex);

                TaskList.Add(LastTask);
            }
            await Task.WhenAll(TaskList.ToArray());

            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }

        private static async Task Main2Async(int userIndex)
        {
            HubConnection connection;

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8085/signalr").Build();

            #region snippet_ClosedRestart
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            #endregion

            #region snippet_ConnectionOn
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"Received:{message}");
            });
            #endregion

            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}: {ex.InnerException.Message}");
            }
            if (connection.State != HubConnectionState.Connected)
            {
                return;
            }
            // Call "Cancel" on this CancellationTokenSource to send a cancellation message to
            // the server, which will trigger the corresponding token in the hub method.
            var cancellationTokenSource = new CancellationTokenSource();
            var channel = await connection.StreamAsChannelAsync<object>(
                "GetTestProducts", cancellationTokenSource.Token);

            // Wait asynchronously for data to become available
            while (await channel.WaitToReadAsync())
            {
                // Read all currently available data synchronously, before waiting for more data
                while (channel.TryRead(out var response))
                {
                    Console.WriteLine($"User:{userIndex} {response.ToString().Length}");
                }
            }
            try
            {
                await connection.SendAsync("GetTestProducts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private static async Task MainAsync()
        {
            HubConnection connection;

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7192/chatHub").Build();

            #region snippet_ClosedRestart
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            #endregion

            #region snippet_ConnectionOn
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"Received:{message}");
            });
            #endregion

            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}: {ex.InnerException.Message}");
            }
            if (connection.State != HubConnectionState.Connected) { return; }
            try
            {
                await connection.InvokeAsync("SendMessage", "Hello", "Hello1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            Console.ReadLine();
        }
    }
}
