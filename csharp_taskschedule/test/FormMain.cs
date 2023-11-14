using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace test
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            string path = Path.GetDirectoryName(Application.ExecutablePath);

            TaskService.Instance.AddTask("Teste1xx", QuickTriggerType.Logon, path + "\\" + AppDomain.CurrentDomain.FriendlyName);

            using (TaskService taskService = new TaskService())
            {
                Regex regex = new Regex(String.Format(@"{0}", "Teste1xx"));
                Microsoft.Win32.TaskScheduler.Task[] allTasksCollection = taskService.FindAllTasks(regex, true);
                foreach (Microsoft.Win32.TaskScheduler.Task task in allTasksCollection)
                {
                    TaskDefinition td = taskService.NewTask();
                    td = task.Definition;
                    td.RegistrationInfo.Description = "New teste1";
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    taskService.RootFolder.RegisterTaskDefinition("Teste1", td);
                }
            };
        }
    }
}
