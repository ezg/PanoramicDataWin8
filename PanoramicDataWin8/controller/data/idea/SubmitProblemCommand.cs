﻿using System.Linq;
using System.Threading.Tasks;
using IDEA_common.catalog;
using IDEA_common.operations;
using IDEA_common.operations.ml.optimizer;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class SubmitProblemCommand
    {
        public async Task SumbitResult(PredictorOperationModel model)
        {
            var submitParam = new SubmitProblemParameters();
            submitParam.Id = (model.Result as OptimizerResult).TopKSolutions.First().SolutionId;

            var ser = JsonConvert.SerializeObject(submitParam, IDEAGateway.JsonSerializerSettings);
            var response = await IDEAGateway.Request(ser, "submitProblem");
        }
    }

    public class SpecifyProblemCommand
    {
        public async Task SpecifyProblem(PredictorOperationModel model, string userComment)
        {
            var specifyProblemParam = new SpecifyProblemParameters();
            specifyProblemParam.Id = (model.Result as OptimizerResult).TopKSolutions.First().SolutionId;
            specifyProblemParam.UserComment = userComment;

            var ser = JsonConvert.SerializeObject(specifyProblemParam, IDEAGateway.JsonSerializerSettings);
            var response = await IDEAGateway.Request(ser, "specifyProblem");
        }
    }
}