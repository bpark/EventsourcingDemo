using System;
using System.Net;
using System.Text;
using EventsourcingDemoClient.model;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EventsourcingDemoClient
{
    public class Program
    {
        private IEventStoreConnection Connection { get; } = Connect();
        
        private static IEventStoreConnection Connect()
        {
            var c =
                EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse("192.168.99.103"), 32001));

            c.ConnectAsync().Wait();

            return c;
        }

        private void WriteEvent(MyEventData myEventData, MyEventMeta myEventMeta)
        {
            var myEvent = new EventData(Guid.NewGuid(), "testEvent", true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(myEventData)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(myEventMeta)));

            Connection.AppendToStreamAsync("test-stream", ExpectedVersion.Any, myEvent).Wait();    
        }

        private void ReadEvents()
        {
            var streamEvents =
                //connection.ReadStreamEventsForwardAsync("test-stream", 0, 3, false).Result;
                Connection.ReadStreamEventsBackwardAsync("stream/events", StreamPosition.End, 3, false).Result;

            foreach (var evt in streamEvents.Events)
            {
                Console.WriteLine("Read event with data: {0}, metadata: {1}",
                    Encoding.UTF8.GetString(evt.Event.Data),
                    Encoding.UTF8.GetString(evt.Event.Metadata));
            }
        }
        
        private static void Main(string[] args)
        {

            var eventData = new MyEventData()
            {
                Description = "Test",
                Country = "US",
                Name = "Doe"
            };

            var eventMeta = new MyEventMeta()
            {
                Timestamp = DateTime.Now
            };
            
            var program = new Program();
            program.WriteEvent(eventData, eventMeta);
            program.ReadEvents();
            
        }
    }
}