using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{
    public class TasksCommand
    {
        public async Task<List<TaskModel>> GetTasks(string url)
        {
            JObject data = new JObject(
                new JProperty("type", "tasks"));
            string response = await TuppleWareGateway.Request(data);

            JToken jToken = JToken.Parse(response);

            List<TaskModel> tasks = new List<TaskModel>();
            TaskGroupModel parent = new TaskGroupModel();
            foreach (var child in jToken as JArray)
            {
                recursiveCreateAttributeModels(child, parent);
            }
            return parent.TaskModels.ToList();
        }

        private void recursiveCreateAttributeModels(JToken token, TaskGroupModel parent)
        {
            if (token is JArray)
            {
                if (token[0] is JValue)
                {
                    TaskGroupModel groupModel = new TaskGroupModel() {Name = token[0].ToString()};
                    parent.TaskModels.Add(groupModel);
                    foreach (var child in token[1])
                    {
                        recursiveCreateAttributeModels(child, groupModel);
                    }
                }
            }
            if (token is JValue)
            {
                TaskModel taskModel = new TaskModel() { Name = token.ToString() };
                parent.TaskModels.Add(taskModel);
            }
        }
    }
}
