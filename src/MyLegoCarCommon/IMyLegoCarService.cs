using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyLegoCarCommon
{
    public interface IMyLegoCarService
    {
        double GearPower { get; set; }
        
        long LeftEncoderCurrent { get; set; }
        long RightEncoderCurrent { get; set; }

        double LeftPowerCurrent { get; set; }
        double RightPowerCurrent { get; set; }

        double BatteryPower { get; set; }

        void Drive(DriveAction driveDirection);
        void StopEngine();
    }
}
