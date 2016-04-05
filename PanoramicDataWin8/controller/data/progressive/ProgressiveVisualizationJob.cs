using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.progressive;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ProgressiveVisualizationJob : Job
    {
        private bool _isRunning = false;
        private Object _lock = new Object();
        private MessageWebSocket _webSocket = null;
        private int _sampleSize = 0;
        private Stopwatch _stopWatch = new Stopwatch();
        private JObject _query = null;

        private TimeSpan _throttle = TimeSpan.FromMilliseconds(0);
        public QueryModel QueryModel { get; set; }
        public QueryModel QueryModelClone { get; set; }

        public ProgressiveVisualizationJob(QueryModel queryModel, QueryModel queryModelClone, TimeSpan throttle, int sampleSize)
        {
            QueryModel = queryModel;
            QueryModelClone = queryModelClone;
            _sampleSize = sampleSize;
            _throttle = throttle;
            var psm = (queryModelClone.SchemaModel as ProgressiveSchemaModel);
            string filter = "";
            List<string> aggregateFunctions = new List<string>();
            List<string> aggregateDimensions = new List<string>();

            _query = new JObject(
                new JProperty("type", "execute"),
                new JProperty("dataset", psm.RootOriginModel.DatasetConfiguration.Name),
                new JProperty("task",
                    new JObject(
                        new JProperty("filter", filter),
                        new JProperty("aggregateFunctions", aggregateFunctions),
                        new JProperty("type", "visualization"),
                        new JProperty("chunkSize", sampleSize),
                        new JProperty("aggregateDimensions", aggregateDimensions),
                    ))
                );

            /*
            job = {
              "type": "execute",
              "dataset": "cars_small.csv",
              "task": {
                "filter": "model_year > 70",
                "aggregateFunctions": [
                  "Count"
                ],
                "type": "visualization",
                "chunkSize": 1000,
                "aggregateDimensions": [
                  "cylinders"
                ],
                "nrOfBins": [
                  10
                ],
                "brushes": [
                  "cylinders == 8",
                  "mpg > 1"
                ],
                "dimensionAggregateFunctions": [
                  "None"
                ],
                "dimensions": [
                  "mpg"
                ]
              }
            }
            */

        }
        public override void Start()
        {
            _stopWatch.Start();
            Task.Run(() => run());
        }

        private async void run()
        {
            lock (_lock)
            {
                _isRunning = true;
            }

            var data = _query.ToString();
            _webSocket = new MessageWebSocket();
            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.MessageReceived += webSocket_MessageReceived;
            _webSocket.Closed += webSocket_Closed;
            await _webSocket.ConnectAsync(new Uri(MainViewController.Instance.MainModel.Ip));

            DataWriter messageWriter = new DataWriter(_webSocket.OutputStream);
            messageWriter.WriteString(data);
            await messageWriter.StoreAsync();
            
        }

        void webSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            lock (_lock)
            {
                _isRunning = false;
            }
        }

        async void webSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string message = reader.ReadString(reader.UnconsumedBufferLength);

                //RegexQueryResult regexQueryResult = JsonConvert.DeserializeObject<RegexQueryResult>(message);
                //await fireUpdated(message);
            }
        }


        public override void Stop()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    _isRunning = false;
                    _webSocket.Close(1000, "");
                }
            }
        }
    }
}
