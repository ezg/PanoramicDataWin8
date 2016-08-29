using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data
{
    public class TaskGroupModel : TaskModel
    {
        private ObservableCollection<TaskModel> _taskModels = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> TaskModels
        {
            get
            {
                return _taskModels;
            }
        }
    }
}
