using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.data.sim;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.controller.data
{
    public class Logger
    {
        private static Logger _instance;
        private Stream _stream = null;
        private Stream _mouseStream = null;


        private Logger()
        {

        }

        public static async Task<Logger> CreateInstance(MainModel mainModel)
        {
            _instance?._stream.Flush();
            _instance?._mouseStream.Flush();

            var root = (mainModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel;
            

            _instance = new Logger();
            var fileName = mainModel.Participant + "_LOG_" + DateTime.Now.Ticks.ToString() + "#" + root.DatasetConfiguration.Name;
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            _instance._stream = await file.OpenStreamForWriteAsync();

            fileName = mainModel.Participant + "_MOUSE_" + DateTime.Now.Ticks.ToString() + "#" + root.DatasetConfiguration.Name;
            file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            _instance._mouseStream = await file.OpenStreamForWriteAsync();

            return _instance;
        }

        public static Logger Instance
        {
            get
            {
                return _instance;
            }
        }

        private async void log(string msg, Stream stream)
        {
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes((msg + "\n").ToCharArray());
            stream?.WriteAsync(fileBytes, 0, fileBytes.Length);
        }

        private string queryModelToString(QueryModel queryModel)
        {
            if (queryModel != null)
            {
                queryModel.Id.ToString();
            }
            return "";
        }

        public void LogQueryResult(string evt, QueryModel qm, ResultModel resultModel, string brushQuery, string filterQuery)
        {
            JObject data = new JObject(
                new JProperty("timestamp", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")),
                new JProperty("evt", evt),
                new JProperty("visualization", queryModelToString(qm)),
                new JProperty("filterQuery", filterQuery),
                new JProperty("brushQuery", brushQuery),
                new JProperty("payload", JsonConvert.DeserializeObject(JsonConvert.SerializeObject(resultModel, Formatting.Indented, new KeysJsonConverter(typeof(InputOperationModel))))));

            log(data.ToString(), _stream);
        }

        public void Log(string evt)
        {
            JObject data = new JObject(
                new JProperty("timestamp", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")),
                new JProperty("evt", evt));

            log(data.ToString(), _stream);
        }

        public void Log(string evt, params JProperty[] properties)
        {
            JObject data = new JObject(
                new JProperty("timestamp", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")),
                new JProperty("evt", evt));

            foreach (var jProperty in properties)
            {
                data.Add(jProperty);
            }

            log(data.ToString(), _stream);
        }

        public void Log(string evt, QueryModel qm, params JProperty[] properties)
        {
            var p = properties.ToList();
            p.Insert(0, new JProperty("visualization", queryModelToString(qm)));
            Log(evt, p.ToArray());
        }

        public void LogMouse(string evt, double x, double y, bool rightButton, string deviceType)
        {
            JObject data = new JObject(
                new JProperty("timestamp", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")),
                new JProperty("evt", evt),
                new JProperty("x", x),
                new JProperty("y", y),
                new JProperty("rightButton", rightButton),
                new JProperty("deviceType", deviceType));

            log(data.ToString(), _mouseStream);
        }

        public void Flush()
        {
            _mouseStream?.FlushAsync();
            _stream?.FlushAsync();
        }
    }

    public class KeysJsonConverter : JsonConverter
    {
        private readonly Type[] _types;

        public KeysJsonConverter(params Type[] types)
        {
            _types = types;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                if (value is InputOperationModel)
                {
                    var obj = new JObject(
                        new JProperty("attribute", ((SimInputFieldModel)((InputOperationModel)value).InputModel).Name),
                        new JProperty("aggregation", ((InputOperationModel)value).AggregateFunction.ToString()));
                    obj.WriteTo(writer);
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }
    }
}
