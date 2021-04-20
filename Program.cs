using System;
using System.Configuration;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AudreysCloud.Community.SharpHomeAssistant;
using AudreysCloud.Community.SharpHomeAssistant.Exceptions;
using AudreysCloud.Community.SharpHomeAssistant.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;

namespace AudreysCloud.Community.HomeAssistantWatcher
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Home Assistant Watcher is licensed under the MIT License.");
			Console.WriteLine("See https://opensource.org/licenses/MIT for the full license.");
			Console.WriteLine();

			ConfigurationBuilder builder = new ConfigurationBuilder();
			builder.AddCommandLine(args);
			IConfigurationRoot config = builder.Build();
			GetConnectionSettings(config, out Uri path, out string token);

			SharpHomeAssistantConnection connection = new SharpHomeAssistantConnection() { AccessToken = token };

			CancellationTokenSource asyncCancelTokenSource = new CancellationTokenSource();

			try
			{
				await connection.ConnectAsync(path, asyncCancelTokenSource.Token);
			}
			catch (ConnectFailedException ex)
			{
				Console.WriteLine("Connection attempt to the remote server failed.");
				Console.WriteLine(ex);
				System.Environment.Exit(-1);
			}
			catch (SharpHomeAssistantProtocolException ex)
			{
				Console.WriteLine("Protocol error was encountered while trying to negotiate with the remote server.");
				Console.WriteLine(ex);
				System.Environment.Exit(-1);
			}


			SubscribeEventsMessage subscribeEventsMessage = new SubscribeEventsMessage()
			{
				CommandId = 1,
				EventName = "*"
			};

			if (!await connection.SendMessageAsync(subscribeEventsMessage, asyncCancelTokenSource.Token))
			{
				Console.WriteLine("Internal error while trying to send subscribe all events message.");
				await CloseConnection(connection, asyncCancelTokenSource.Token);

				System.Environment.Exit(-1);
			}


			Task pingLoopAsync = PingLoopAsync(connection, asyncCancelTokenSource.Token);

			for (IncomingMessageBase message = await connection.ReceiveMessageAsync(asyncCancelTokenSource.Token);
				message != null;
				message = await connection.ReceiveMessageAsync(asyncCancelTokenSource.Token))
			{

				if (ResultMessage.TryConvert(message, out ResultMessage resultMessage))
				{
					if (resultMessage.CommandId == 1)
					{
						if (!resultMessage.Success)
						{
							Console.WriteLine("Could not subscribe to events.");
							Console.WriteLine(resultMessage.ErrorDetails.Message);

							await CloseConnection(connection, asyncCancelTokenSource.Token);

							System.Environment.Exit(-1);
						}

						Console.WriteLine("Subscribed to all events.");
						Console.WriteLine();
						Console.WriteLine();
						break;
					}
				}
			}


			for (IncomingMessageBase message = await connection.ReceiveMessageAsync(asyncCancelTokenSource.Token);
				message != null;
				message = await connection.ReceiveMessageAsync(asyncCancelTokenSource.Token))
			{
				if (EventMessage.TryConvert(message, out EventMessage eventMessage))
				{
					if (eventMessage.CommandId == 1)
					{

						JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
						string json = JsonSerializer.Serialize(eventMessage.Event, eventMessage.Event.GetType(), options);

						Console.WriteLine();
						Console.WriteLine(json);
						Console.WriteLine();
					}
				}
			}


			Console.WriteLine("Closing connection");
			await CloseConnection(connection, asyncCancelTokenSource.Token);
			Console.WriteLine("Connection closed.");

			System.Environment.Exit(0);
		}


		static async Task PingLoopAsync(SharpHomeAssistantConnection connection, CancellationToken token)
		{
			for (int i = 2; await connection.SendMessageAsync(new PingMessage() { CommandId = i }, token); i++)
			{
				await Task.Delay(10000, token);
			}

			Console.WriteLine("Sending ping failed");
		}
		static async Task CloseConnection(SharpHomeAssistantConnection connection, CancellationToken token)
		{
			try
			{
				await connection.CloseAsync(token);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occured while trying to close the remote connection.");
				Console.WriteLine(ex.Message);
			}
		}

		static void GetConnectionSettings(IConfigurationRoot root, out Uri path, out string token)
		{
			try
			{
				path = new Uri(root["uri"]);
				token = root["accessToken"];

			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed loading config file.");
				Console.WriteLine(ex.Message);
				System.Environment.Exit(-1);

				throw new Exception();
			}

		}
	}
}
