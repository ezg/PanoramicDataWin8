using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.view.tilemenu;

namespace PanoramicDataWin8.model.data.tuppleware
{

    public class TuppleWareGateway
    {
        public static void PopulateSchema(TuppleWareOriginModel tuppleWareOriginModel)
        {
            SchemaCommand schemaCommand = new SchemaCommand();
            schemaCommand.PopulateSchema(tuppleWareOriginModel);
        }

        public static async Task<JToken> Request(string endPoint, JObject data)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage = await httpClient.PostAsync(endPoint, new StringContent(
                data.ToString(),
                Encoding.UTF8,
                "application/json"));
            var stringContent = await httpResponseMessage.Content.ReadAsStringAsync();
            JToken jToken = JToken.Parse(stringContent);

            Debug.WriteLine(stringContent);

            return jToken;
        }
    }

    public class SchemaCommand
    {
        public async void PopulateSchema(TuppleWareOriginModel tuppleWareOriginModel)
        {
            JObject data = new JObject(
                new JProperty("command", "schema"),
                new JProperty("filename", "mimic2"));
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
                        TuppleWareInputModel inputModel = new TuppleWareInputModel(token[0].ToString(), "float", token[1].ToString().ToLower() == "true" ? "numeric" : "enum");
                        inputModel.OriginModel = tuppleWareOriginModel;
                        if (parentGroupModel != null)
                        {
                            parentGroupModel.InputModels.Add(inputModel);
                        }
                        else
                        {
                            tuppleWareOriginModel.InputModels.Add(inputModel);
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
