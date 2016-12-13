using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data
{
    public abstract class OperationJob
    {
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private readonly TimeSpan _throttle;
        private bool _isRunning;

        protected OperationJob(OperationModel operationModel, TimeSpan throttle)
        {
            OperationModel = operationModel;
            _throttle = throttle;
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
                var response = await IDEAGateway.Request(JsonConvert.SerializeObject(OperationParameters, IDEAGateway.JsonSerializerSettings), "operation");
                OperationReference = JsonConvert.DeserializeObject<OperationReference>(response, IDEAGateway.JsonSerializerSettings);

                var resultCommand = new ResultCommand();

                // starting looping for updates
                while (_isRunning)
                {
                    var result = await resultCommand.GetResult(OperationReference);
                    if (result != null)
                    {
                        FireJobUpdated(new JobEventArgs {Result = result});

                        if (result.Progress >= 1.0)
                        {
                            Stop();
                            FireJobCompleted(new JobEventArgs {Result = result});
                        }
                    }
                    await Task.Delay(_throttle);
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
            }
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