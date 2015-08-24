using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class TasksCommand
    {
        public async Task<List<string>> GetTasks(string url)
        {
            JObject data = new JObject(
                new JProperty("type", "tasks"));
            string response = await TuppleWareGateway.Request(url, data);

            JToken jToken = JToken.Parse(response);

            List<string> tasks = new List<string>();
            foreach (var child in jToken as JArray)
            {
                tasks.Add(child.Value<string>());
            }
            return tasks;
        }
    }
}
