using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyLegoCarCommon;
using System.Windows.Threading;

namespace MyLegoCarWpfUI
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        #region Global Variables

        private DispatcherTimer uiTimer;
        private Int64 _oldEncoderTarget;
        private int _odometerReading = 0;

        #endregion

        #region Properties

        public IMyLegoCarService Service { get; set; }
                
        #endregion

        #region Constructors

        public Dashboard()
        {
            InitializeComponent();
        }

        public Dashboard(IMyLegoCarService service) : this()
        {
            Service = service;
            Service.GearPower = 0;
            brsr_ipcamera.Navigate(new Uri("http://iphoneip/iphonecamera/index.htm"));

            UpdateInitialOdometer();

            uiTimer = new DispatcherTimer();
            uiTimer.Interval = TimeSpan.FromSeconds(1);
            uiTimer.Tick += uiTimer_Tick;
            uiTimer.Start();
        }

        #endregion

        #region DriveActions

        private void btn_front_Click(object sender, RoutedEventArgs e)
        {
            Service.Drive(DriveAction.Front);
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            Service.Drive(DriveAction.Stop);
            sldr_gear.Value = 0;
        }

        private void btn_right_Click(object sender, RoutedEventArgs e)
        {
            Service.Drive(DriveAction.Right);
        }

        private void btn_left_Click(object sender, RoutedEventArgs e)
        {
            Service.Drive(DriveAction.Left);
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            Service.Drive(DriveAction.Back);
        }

        private void sldr_gear_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Service.GearPower = sldr_gear.Value / 5;
        } 

        #endregion

        #region Miscellaneous
        
        private void uiTimer_Tick(object sender, EventArgs e)
        {
            UpdateFuel();
            UpdateSpeedometer();
            UpdateOdometer();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    Service.Drive(DriveAction.Stop);
                    break;
                case Key.Down:
                    Service.Drive(DriveAction.Back);
                    break;
                case Key.Left:
                    Service.Drive(DriveAction.Left);
                    break;
                case Key.Right:
                    Service.Drive(DriveAction.Right);
                    break;
                case Key.Up:
                    Service.Drive(DriveAction.Front);
                    break;
                case Key.PageUp:
                    sldr_gear.Value = sldr_gear.Value + 1 > 5 ? 5 : sldr_gear.Value + 1;
                    break;
                case Key.PageDown:
                    sldr_gear.Value = sldr_gear.Value - 1 < 1 ? 1 : sldr_gear.Value - 1;
                    break;
                default:
                    Service.Drive(DriveAction.Stop);
                    break;
            }
        }
      
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Service.StopEngine();
        }
            
        private void UpdateSpeedometer()
        {
            dial_bottomleft.Value = 0.0;
            dial_topleft.Value = 0.0;
            dial_bottomright.Value = 0.0;
            dial_topright.Value = 0.0;

            if (Service.LeftPowerCurrent > 0)
                dial_bottomleft.Value = Math.Abs(Service.LeftPowerCurrent) * 100;
            else
                dial_topleft.Value = Math.Abs(Service.LeftPowerCurrent) * 100;

            if (Service.RightPowerCurrent > 0)
                dial_bottomright.Value = Math.Abs(Service.RightPowerCurrent) * 100;
            else
                dial_topright.Value = Math.Abs(Service.RightPowerCurrent) * 100;

        }

        private void UpdateInitialOdometer()
        {
            Int64 currentEncoderCurrent;
            int distance = 0;

            currentEncoderCurrent = Math.Abs(Service.LeftEncoderCurrent);
            _oldEncoderTarget = currentEncoderCurrent;

            distance = Convert.ToInt32(Math.Abs(currentEncoderCurrent) / 360 * 2 * 3.14 * 0.75);
            _odometerReading += distance;
            odo_distance.FinalValue = _odometerReading;
        }

        private void UpdateOdometer()
        {
            Int64 differenceEncoderCurrent, currentEncoderCurrent;
            int distance = 0;

            currentEncoderCurrent = Math.Abs(Service.LeftEncoderCurrent);
            differenceEncoderCurrent = Math.Abs(_oldEncoderTarget - currentEncoderCurrent);
            _oldEncoderTarget = currentEncoderCurrent;

            distance = Convert.ToInt32(Math.Abs(differenceEncoderCurrent) / 360 * 2 * 3.14 * 0.75);
            _odometerReading += distance;
            odo_distance.FinalValue = _odometerReading;
        }
        
        #endregion

        #region BatteryActions

        public void UpdateFuel()
        {
            double batteryPercentage = Math.Round(Service.BatteryPower, 2) * 100;
            prgs_battery.Value = batteryPercentage;
            lbl_battery.Content = batteryPercentage;

        }   

        #endregion


    }
}
