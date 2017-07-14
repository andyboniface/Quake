using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake
{
    public class TimeScheduler
    {
        static private TimeScheduler scheduler_;
        private IDictionary<string, TimeSchedulerTask> currentTasks;

        private TimeScheduler()
        {
            currentTasks = new Dictionary<string, TimeSchedulerTask>();

            logMessage("Starting scheduler thread");

            var task3 = new Task(() => TimeSchedulerRunner(),
                    TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);

            task3.Start();
        }

        //
        // This is the main method in our thread - it looks to see which timers 
        // should be fired waiting a short while between each one....
        /// 
        /// </summary>
        private void TimeSchedulerRunner()
        {
            logMessage("Scheduler thread is running");
            bool workDone = false;
            while (true)
            {
                try
                {
                    if (workDone == false)
                    {
                        Task.Delay(TimeSpan.FromSeconds(2)).Wait();             // We wait two seconds....
                    }
                    else
                    {
                        workDone = false;
                    }
                    long nowInSeconds = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                    TimeSchedulerTask task = GetNextTask();
                    if (task != null)
                    {
                        logMessage("Checking time on task " + task.TaskName + " taskStartTime=" + task.TaskStartTime + " now=" + nowInSeconds);
                        if (task.TaskStartTime <= nowInSeconds)
                        {
                            workDone = true;

                            logMessage("About to execute task " + task.TaskName);
                            currentTasks.Remove(task.TaskName);             // Remove task from list - we are about to execute it...

                            // Ok....we should be running this task now...
                            Task<TimeSpan> func = task.InvokeFunction.Invoke();
                            if (func != null)
                            {
                                func.Wait();

                                if (func.IsFaulted)
                                {
                                    logMessage("Task " + task.TaskName + " faulted=" + func.Exception.Message + " stack=" + func.Exception.StackTrace);
                                }

                                //
                                // Now we need to re-schedule it....
                                //
                                if (func.Result != null)
                                {
                                    task.SetNextStartTime(func.Result);
                                    logMessage("Rescheduling task " + task.TaskName + " for " + task.TaskStartTime);
                                    currentTasks.Add(task.TaskName, task);      // Put back onto queue for next time....
                                }
                            }
                            else
                            {
                                logMessage("task " + task.TaskName + " returned null - will not be re-scheduled");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logMessage("Runner has errored - " + e.Message);
                }
            }
        }

        private void logMessage(string msg)
        {
            System.Diagnostics.Debug.WriteLine("RMMobile: " + msg);
        }

        private TimeSchedulerTask GetNextTask()
        {
            if (currentTasks.Count == 0)
            {
                return null;
            }
            return currentTasks.Select(x => x.Value).OrderBy(x => x.TaskStartTime).First();
        }

        public void AddTask(string taskName, TimeSpan initialInterval, Func<Task<TimeSpan>> tick)
        {
            TimeSchedulerTask task = new TimeSchedulerTask(taskName, initialInterval, tick);
            if (currentTasks.ContainsKey(task.TaskName))
            {
                currentTasks.Remove(task.TaskName);
            }
            currentTasks.Add(task.TaskName, task);
        }

        public void RemoveTask(string taskName)
        {
            if (currentTasks.ContainsKey(taskName))
            {
                currentTasks.Remove(taskName);
            }
        }

        static public TimeScheduler GetTimeScheduler()
        {
            if (scheduler_ == null)
            {
                scheduler_ = new TimeScheduler();
            }

            return scheduler_;
        }
    }

    public class TimeSchedulerTask
    {
        public TimeSchedulerTask(string name, TimeSpan initialInterval, Func<Task<TimeSpan>> tick)
        {
            TaskName = name;
            SetNextStartTime(initialInterval);
            InvokeFunction = tick;
        }

        public void SetNextStartTime(TimeSpan when)
        {
            long startTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
            startTime += (long)when.TotalSeconds;
            TaskStartTime = startTime;
        }
        public Func<Task<TimeSpan>> InvokeFunction { get; private set; }

        public string TaskName { get; private set; }
        public long TaskStartTime { get; private set; }
    }

}
