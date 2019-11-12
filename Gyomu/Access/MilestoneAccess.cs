using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Access
{
    public class MilestoneAccess
    {
        public static bool Exists(string milestoneId, DateTime targetDate, bool isMonthly = false)
        {
            return Common.GyomuDataAccess.SelectMilestoneDaily(targetDate, milestoneId, isMonthly) != null;
            
        }
        public static void Register(string milestoneId, DateTime targetDate, bool isMonthly = false)
        {
            if (Exists(milestoneId, targetDate, isMonthly))
                return;
            Common.GyomuDataAccess.InsertMilestoneDaily(targetDate, milestoneId, isMonthly);
        }
        public static bool Wait(string milestoneId, DateTime targetDate, int timeoutMinute)
        {
            DateTime timeout = DateTime.Now.AddMinutes(timeoutMinute);
            while (true)
            {
                if (Exists(milestoneId, targetDate))
                    return true;

                if (timeout.Subtract(DateTime.Now).TotalSeconds < 0)
                    return false;
                System.Threading.Thread.Sleep(5000);
            }
        }
    }
}
