﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using IDEA_common.operations.risk;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using WinRTXamlToolkit.IO.Serialization;

namespace PanoramicDataWin8.controller.data
{
    public abstract class OperationJob
    {
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private bool _isRunning;
        private int _executionId = -1;

        protected OperationJob(OperationModel operationModel)
        {
            OperationModel = operationModel;
            _executionId = operationModel.ResultCauserClone.ExecutionId;
        }

        public OperationModel OperationModel { get; set; }
        public OperationParameters OperationParameters { get; set; }
        public OperationReference OperationReference { get; set; }

        public event EventHandler<JobEventArgs> JobUpdate;
        public event EventHandler<JobEventArgs> JobCompleted;

        public void Start()
        {
            _stopWatch.Start();
            lock (_lock)
            {
                _isRunning = true;
            }
            Task.Run(() => run());
        }

        private async void run()
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var response = await IDEAGateway.Request(JsonConvert.SerializeObject(OperationParameters, IDEAGateway.JsonSerializerSettings), "operation");
                OperationReference = JsonConvert.DeserializeObject<OperationReference>(response, IDEAGateway.JsonSerializerSettings);
                await Task.Delay(100);

                var resultCommand = new ResultCommand();
                bool first = true;
                // starting looping for updates
                while (_isRunning)
                {
                    var resultParams = OperationModel.ResultParameters;
                    FireExecutionStateChanged(ExecutionState.Running);
                    resultParams.OperationReference = OperationReference;
                    var result = await resultCommand.GetResult(resultParams);
                    if (result != null)
                    {
                        if (first && OperationParameters is HistogramOperationParameters)
                        {
                            Debug.WriteLine("first update in " + sw.ElapsedMilliseconds + " " + result.Progress);
                            first = false;
                        }
                        FireJobUpdated(new JobEventArgs {Result = result, ResultExecutionId = _executionId });

                        if (result.Progress >= 1.0)
                        {
                            FireExecutionStateChanged(ExecutionState.Stopped);
                            _isRunning = false;
                            FireJobCompleted(new JobEventArgs {Result = result, ResultExecutionId = _executionId });
                            if (OperationParameters is HistogramOperationParameters)
                            {
                                Debug.WriteLine("job completed in " + sw.ElapsedMilliseconds);
                            }
                        }
                    }
                    await Task.Delay(150);
                }
            }
            catch (Exception exc)
            {
                ErrorHandler.HandleError(exc.Message);
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                IDEAGateway.Request(JsonConvert.SerializeObject(OperationReference, IDEAGateway.JsonSerializerSettings), "pause");
                _isRunning = false;
                FireExecutionStateChanged(ExecutionState.Stopped);
            }
        }

        protected async void FireExecutionStateChanged(ExecutionState state)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OperationModel.ExecutionState = state;
            });
        }

        protected async void FireJobUpdated(JobEventArgs jobEventArgs)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                JobUpdate?.Invoke(this, jobEventArgs);
            });
        }

        protected async void FireJobCompleted(JobEventArgs jobEventArgs)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                JobCompleted?.Invoke(this, jobEventArgs);
            });
        }
    }
}