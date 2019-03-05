using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyLegoCarCommon;

namespace MyLegoCarService
{
    public class UiManager
    {
        public IDashboard DashboardHandler { get; set; }

        public void UpdateBattery(double percentage)
        {
            DashboardHandler.UpdateBatteryPower(percentage);
        }

    }
}
