using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware
{

    public class TuppleWareGateway
    {
        public async static Task<JArray> Classify(TuppleWareOriginModel tuppleWareOriginModel, List<InputFieldModel> features, List<InputFieldModel> labels, JobType jobType)
        {
            ClassifyCommand classifyCommand = new ClassifyCommand();
            return await  classifyCommand.Classify(tuppleWareOriginModel, features, labels, jobType);
        }

        public static void PopulateSchema(TuppleWareOriginModel tuppleWareOriginModel)
        {
            SchemaCommand schemaCommand = new SchemaCommand();
            schemaCommand.PopulateSchema(tuppleWareOriginModel);
        }

        public async static Task<JArray> GetData(TuppleWareOriginModel tuppleWareOriginModel, List<InputFieldModel> inputModels, string select, int page, int samples)
        {
            DataCommand dataCommand = new DataCommand();
            return await dataCommand.GetData(tuppleWareOriginModel, inputModels, select, page, samples);
        }

        public static async Task<JToken> Request(string endPoint, JObject data)
        {
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine(data.ToString());
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage = await httpClient.PostAsync(endPoint, new StringContent(
                data.ToString(),
                Encoding.UTF8,
                "application/json"));
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine("TuppleWare Roundtrip Time: " + sw.ElapsedMilliseconds);
            }
            sw.Restart();

            var stringContent = await httpResponseMessage.Content.ReadAsStringAsync();
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine("TuppleWare Read Content Time: " + sw.ElapsedMilliseconds);
            }
            sw.Restart();
          
            JToken jToken = JToken.Parse(stringContent);
            if (MainViewController.Instance.MainModel.Verbose)
            {
                Debug.WriteLine("TuppleWare Json Parsing Time: " + sw.ElapsedMilliseconds);
            }
           

            return jToken;
        }
    }

    public class ClassifyCommand
    {
        public async Task<JArray> Classify(TuppleWareOriginModel tuppleWareOriginModel, List<InputFieldModel> features, List<InputFieldModel> labels, JobType jobType)
        {
            JObject data = new JObject(
                new JProperty("command", "classify"),
                new JProperty("classifier", jobType.ToString()),
                new JProperty("project", string.Join(" ", features.Concat(labels).Select(im => im.Name))),
                new JProperty("labels", string.Join(" ", labels.Select(im => im.Name))),
                new JProperty("filename", tuppleWareOriginModel.Name));
            JToken response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);
            if (response is JObject)
            {
                JArray arr = new JArray(response);
                return arr;
            }
            else
            {
                return response as JArray;
            }
        }
    }


    public class DataCommand
    {
        public async Task<JArray> GetData(TuppleWareOriginModel tuppleWareOriginModel, List<InputFieldModel> inputModels, string select, int page, int samples)
        {
            JObject data = new JObject(
                new JProperty("command", "data"),
                new JProperty("project", string.Join(" ", inputModels.Select(im => im.Name).Distinct())),
                new JProperty("limit", samples),
                new JProperty("page", page),
                new JProperty("filename", tuppleWareOriginModel.Name));
            if (!string.IsNullOrEmpty(select))
            {
                data.Add(new JProperty("select", select));
            }
            JToken response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);

            return response as JArray;
        }
    }


    public class SchemaCommand
    {
        public async void PopulateSchema(TuppleWareOriginModel tuppleWareOriginModel)
        {
            JObject data = new JObject(
                new JProperty("command", "schema"),
                new JProperty("filename", tuppleWareOriginModel.Name));
            JToken response = await TuppleWareGateway.Request(tuppleWareOriginModel.DatasetConfiguration.EndPoint, data);

            foreach (var child in response)
            {
                recursiveCreateAttributeModels(child, null, tuppleWareOriginModel);
            }
        }

        private static void recursiveCreateAttributeModels(JToken token, TuppleWareInputGroupModel parentGroupModel, TuppleWareOriginModel tuppleWareOriginModel)
        {
            if (token is JArray)
            {
                if (token[0] is JValue)
                {
                    if (token[1] is JValue)
                    {
                        TuppleWareFieldInputModel fieldInputModel = new TuppleWareFieldInputModel(token[0].ToString(), "float", token[1].ToString().ToLower() == "true" ? "numeric" : "enum");
                        fieldInputModel.OriginModel = tuppleWareOriginModel;
                        if (parentGroupModel != null)
                        {
                            parentGroupModel.InputModels.Add(fieldInputModel);
                        }
                        else
                        {
                            tuppleWareOriginModel.InputModels.Add(fieldInputModel);
                        }
                    }
                    else
                    {
                        TuppleWareInputGroupModel groupModel = new TuppleWareInputGroupModel(token[0].ToString());
                        groupModel.OriginModel = tuppleWareOriginModel;
                        if (parentGroupModel != null)
                        {
                            parentGroupModel.InputModels.Add(groupModel);
                        }
                        else
                        {
                            tuppleWareOriginModel.InputModels.Add(groupModel);
                        }
                        foreach (var child in token[1])
                        {
                            recursiveCreateAttributeModels(child, groupModel, tuppleWareOriginModel);
                        }
                    }
                }
            }
        }
    }
}
