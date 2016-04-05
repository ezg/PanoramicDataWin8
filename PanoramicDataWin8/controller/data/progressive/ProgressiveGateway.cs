using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class ProgressiveGateway
    {
        public event EventHandler<string> Response;

        private MessageWebSocket _webSocket = null;

        public async void PostRequest(string endpoint, string data)
        {
            _webSocket = new MessageWebSocket();
            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.MessageReceived += _webSocket_MessageReceived;
            _webSocket.Closed += _webSocket_Closed;
            await _webSocket.ConnectAsync(new Uri(endpoint));

            DataWriter messageWriter = new DataWriter(_webSocket.OutputStream);
            messageWriter.WriteString(data);
            await messageWriter.StoreAsync();
        }

        private void _webSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
        }

        private void _webSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                fireResult(message);
            }
        }

        private async void fireResult(string message)
        {
            if (Response != null)
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Response(this, message);
                });
                _webSocket.Close(1000, "");
            }
        }
    }
}
