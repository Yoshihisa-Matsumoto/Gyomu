using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GyomuTaskExecutor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("In case you execute Task");
                Console.WriteLine("[App ID] [Task ID] [Parameter:optional] [[From Date: optional , yyyyMMdd] [From Date: optional , yyyyMMdd]]");
                Console.WriteLine("In case you register Task");
                Console.WriteLine("Register [Assembly]");
                System.Environment.Exit(-1);
            }
            try
            {

                if (args[0].Equals("Register"))
                {
                    string assemblyName = args[1];
                    System.Reflection.Assembly selectedAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(t => t.FullName.Equals(assemblyName)).FirstOrDefault();
                    if (selectedAssembly == null)
                    {
                        selectedAssembly = AppDomain.CurrentDomain.Load(assemblyName);
                    }
                    if (selectedAssembly == null)
                    {
                        Console.WriteLine(assemblyName + " Not Found");
                        Console.ReadLine();
                        foreach (System.Reflection.Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
                            Console.WriteLine(ass.FullName);
                        return;
                    }

                    Console.WriteLine(selectedAssembly.GetName().Name);
                    List<Gyomu.Models.TaskInfo> taskInfoList= Gyomu.Common.GyomuDataAccess.SelectTaskInfoByAssembly(selectedAssembly.GetName().Name);
                    List<Gyomu.Models.TaskInfo> addedTaskInfo = new List<Gyomu.Models.TaskInfo>();
                    foreach (Type assemblyType in selectedAssembly.GetTypes().Where(t => t.IsClass && IsTaskBaseClass(t)))
                    {
                        if (taskInfoList.Where(taskInfo => taskInfo.class_name.Equals(assemblyType.FullName)).FirstOrDefault() != null)
                            continue;
                        Gyomu.Common.Tasks.AbstractBaseTask taskObject = (Gyomu.Common.Tasks.AbstractBaseTask)selectedAssembly.CreateInstance(assemblyType.FullName);
                        try
                        {
                            addedTaskInfo.Add(new Gyomu.Models.TaskInfo()
                            {
                                application_id = taskObject.ApplicationID,
                                task_id = taskObject.TaskInfoID,
                                description = GetDescription(assemblyType),
                                assembly_name = selectedAssembly.GetName().Name,
                                class_name = assemblyType.FullName,
                                restartable = true
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error on " + assemblyType.FullName);
                            Console.WriteLine(ex.ToString());
                            return;
                        }
                    }

                    int i = 0;
                    short app_id = 0;
                    bool isChanged = false;
                    foreach (Gyomu.Models.TaskInfo taskInfo in taskInfoList)
                    {
                        if (i == 0)
                        {
                            Console.WriteLine("App ID: " + taskInfo.application_id);
                            app_id = taskInfo.application_id;
                        }
                        else if (taskInfo.application_id != app_id)
                            Console.WriteLine("App ID is different : " + taskInfo.application_id);
                        i++;
                        
                        Console.WriteLine(taskInfo.task_id + " : " + taskInfo.description );
                    }
                    foreach (Gyomu.Models.TaskInfo taskInfo in addedTaskInfo)
                    {
                        if (i == 0)
                        {
                            Console.WriteLine("App ID: " + taskInfo.application_id);
                            app_id = taskInfo.application_id;
                        }
                        else if (taskInfo.application_id != app_id)
                            Console.WriteLine("App ID is different : " + taskInfo.application_id);
                        i++;

                        Console.WriteLine(taskInfo.task_id + " : " + taskInfo.description + ":New");
                    }
                    string strResponse = null;
                    if (isChanged)
                    {
                        Console.WriteLine("Do you want to register? [Y/N]");
                        strResponse = Console.ReadLine();
                        if (strResponse.Equals("Y") == false)
                            return;

                        foreach(Gyomu.Models.TaskInfo taskInfo in addedTaskInfo)
                        {
                            Gyomu.Common.GyomuDataAccess.InsertTaskInfo(taskInfo);
                        }
                        Console.WriteLine("Done");
                    }

                }
                else
                {
                    short app_id = Int16.Parse(args[0]);
                    short task_id = Int16.Parse(args[1]);
                    string param = null;
                    if (args.Length > 3)
                    {
                        DateTime startDate = DateTime.ParseExact(args[2], "yyyyMMdd", null);
                        DateTime endDate = DateTime.ParseExact(args[3], "yyyyMMdd", null);
                        Gyomu.Access.MarketDateAccess market = new Gyomu.Access.MarketDateAccess(Gyomu.Access.MarketDateAccess.SupportMarket.Japan);

                        DateTime targetDate = startDate;
                        while (targetDate <= endDate)
                        {
                            Console.WriteLine("Param:" + targetDate.ToString("yyyyMMdd"));
                            Gyomu.Common.Tasks.AbstractBaseTask t = Gyomu.Access.TaskAccess.CreateNewTask(app_id, task_id);

                            t.PropertyChanged += T_PropertyChanged;
                            Gyomu.StatusCode retVal = t.Start(targetDate.ToString("yyyyMMdd"), null);
                            bool isFinish = t.WaitForCompletion(7200, 5);
                            t.FinishStatus(out retVal);
                            if (retVal.IsSucceeded && isFinish)
                                targetDate = market.GetBusinessDay(targetDate, 1);
                            else
                            {
                                System.Environment.Exit(-1);
                            }
                        }
                    }
                    else
                    {
                        if (args.Length > 2)
                        {
                            param = args[2];
                        }

                        Gyomu.Common.Tasks.AbstractBaseTask t = Gyomu.Access.TaskAccess.CreateNewTask(app_id, task_id);
                        t.PropertyChanged += T_PropertyChanged;
                        Gyomu.StatusCode retVal = t.Start(param, null);
                        bool isFinish = t.WaitForCompletion(7200, 5);
                        t.FinishStatus(out retVal);
                        Console.WriteLine(retVal.ToString());
                        if (retVal.IsSucceeded)
                            System.Environment.Exit(0);
                        else
                        {
                            System.Threading.Thread.Sleep(5000);
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                            Environment.FailFast("error happens." + retVal.ToString());
                            System.Environment.Exit(4);
                        }
                    }
                }
            }
            catch (Exception )
            {
 //               Gyomu.StatusCode retVal = new Gyomu.StatusCode(TashBatchStatusCode.BATCH_TASK_UNKNOWN_ERROR, ex, Gyomu.Common.BaseConfigurator.GetInstance());
 //               Console.Error.WriteLine(retVal.ToString());
                System.Environment.Exit(4);
            }
        }
        private static void T_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ":" + e.PropertyName);
        }
        private static bool IsTaskBaseClass(Type t)
        {
            while (t.BaseType != null)
            {
                if (t == typeof(Gyomu.Common.Tasks.AbstractBaseTask))
                    return true;
                t = t.BaseType;
            }
            return false;

        }
        private static string GetDescription(Type t)
        {
            DescriptionAttribute[] attrs = (DescriptionAttribute[])t.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs != null && attrs.Length > 0)
                return attrs[0].Description;
            return t.ToString();
        }
    }
}
