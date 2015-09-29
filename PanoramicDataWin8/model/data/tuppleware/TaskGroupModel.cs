using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.utils;
namespace PanoramicDataWin8.model.data.tuppleware
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
