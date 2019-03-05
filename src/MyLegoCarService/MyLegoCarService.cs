using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using brick = Microsoft.Robotics.Services.Sample.Lego.Nxt.Brick.Proxy;
using battery = Microsoft.Robotics.Services.Sample.Lego.Nxt.Battery.Proxy;
using drive = Microsoft.Robotics.Services.Sample.Lego.Nxt.Drive.Proxy;
using motor = Microsoft.Robotics.Services.Sample.Lego.Nxt.Motor.Proxy;
using wpf = Microsoft.Ccr.Adapters.Wpf;
using MyLegoCarWpfUI;
using System.Windows;
using Microsoft.Robotics.Services.Sample.Lego.Nxt.Common;
using MyLegoCarCommon;
using System.Windows.Threading;

namespace MyLegoCarService
{
    [Contract(Contract.Identifier)]
    [DisplayName("MyLegoCarService")]
    [Description("MyLegoCarService service (no description provided)")]
    class MyLegoCarService : DsspServiceBase, IMyLegoCarService
    {
        #region Global Variables

        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState]
        MyLegoCarServiceState _state = new MyLegoCarServiceState();

        /// <summary>
        /// Main service port
        /// </summary>
        [ServicePort("/MyLegoCarService", AllowMultipleInstances = true)]
        MyLegoCarServiceOperations _mainPort = new MyLegoCarServiceOperations();

        [SubscriptionManagerPartner]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        /// <summary>
        /// NxtBattery partner
        /// </summary>
        [Partner("NxtBattery", Contract = battery.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        battery.BatteryOperations _nxtBatteryPort = new battery.BatteryOperations();

        /// <summary>
        /// NxtDrive partner
        /// </summary>
        [Partner("NxtDrive", Contract = drive.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        drive.DriveOperations _nxtDrivePort = new drive.DriveOperations();
        drive.DriveOperations _nxtDriveNotify = new drive.DriveOperations();
                
        wpf.WpfServicePort _wpfServicePort;
        drive.SetDriveRequest _nxtSetDriveRequest = new drive.SetDriveRequest();
        battery.BatteryState _nxtBatteryState;
        Port<DateTime> _timerPort = new Port<DateTime>();
        
        [Partner("brick", Contract = brick.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        brick.NxtBrickOperations _nxtBrickPort = new brick.NxtBrickOperations();


       #endregion

        #region Properties

        public double GearPower { get; set; }
        public long LeftEncoderCurrent { get; set; }
        public long RightEncoderCurrent { get; set; }

        public double LeftPowerCurrent { get; set; }
        public double RightPowerCurrent { get; set; }

        public double MotorDegree { get; set; }
        public double BatteryPower { get; set; }
                                
        #endregion

        #region Constructors

        /// <summary>
        /// Service constructor
        /// </summary>
        public MyLegoCarService(DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }


        #endregion

        #region Miscellaneous

        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
            SpawnIterator(DoStart);    
        }

        private IEnumerator<ITask> DoStart()
        {
            DispatcherQueue queue = new DispatcherQueue();

            this._wpfServicePort = wpf.WpfAdapter.Create(queue);

            // invoke the UI
            var runWindow = this._wpfServicePort.RunWindow(() => (Window)new Dashboard(this));
            yield return (Choice)runWindow;

            var exception = (Exception)runWindow;
            if (exception != null)
            {
                LogError(exception);
                StartFailed();
                yield break;
            }    

            // Subscribe to partners  
            var subscribe1 = this._nxtDrivePort.Subscribe(_nxtDriveNotify);
            yield return (Choice)subscribe1;

            _timerPort.Post(DateTime.Now);
            
            // Activate independent tasks
            Activate<ITask>(
                Arbiter.Receive<drive.DriveEncodersUpdate>(true, _nxtDriveNotify, DriveEncoderHandler),
                Arbiter.Receive(true, _timerPort, TimerHandler)
            );

            // Start operation handlers and insert into directory service.
            StartHandlers();          
        }

        private void StartHandlers()
        {
            // Activate message handlers for this service and insert into the directory.
            base.Start();

        }

        public void StopEngine()
        {
            _nxtDrivePort.AllStop(MotorStopState.Coast);
            base.Stop();
            base.Shutdown();
        }
        
        #endregion
        
        #region Handlers

        /// <summary>
        /// Handles Subscribe messages
        /// </summary>
        /// <param name="subscribe">the subscribe request</param>
        [ServiceHandler]
        public void SubscribeHandler(Subscribe subscribe)
        {
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> TimerTickHandler(TimerTick incrementTick)
        {
            incrementTick.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            battery.Get batteryGet;
            yield return _nxtBatteryPort.Get(GetRequestType.Instance, out batteryGet);
            _nxtBatteryState = batteryGet.ResponsePort;
            if (_nxtBatteryState != null)
            {
                BatteryPower = _nxtBatteryState.PercentBatteryPower;              
            }        

            yield break;
        }

        private void DriveEncoderHandler(drive.DriveEncodersUpdate statistics)
        {
            LeftEncoderCurrent = statistics.Body.LeftEncoderCurrent;
            RightEncoderCurrent = statistics.Body.RightEncoderCurrent;
            LeftPowerCurrent = statistics.Body.LeftPowerCurrent;
            RightPowerCurrent = statistics.Body.RightPowerCurrent;

        }

        void TimerHandler(DateTime signal)
        {
            _mainPort.Post(new TimerTick());
            Activate(
                Arbiter.Receive(false, TimeoutPort(3000),
                    delegate(DateTime time)
                    {
                        _timerPort.Post(time);
                    }
                )
            );
        }

        #endregion

        #region DriveActions
        
        public void Drive(DriveAction driveAction)
        {
            switch (driveAction)
            {
                case DriveAction.Front:
                    _nxtSetDriveRequest.LeftPower = -GearPower;
                    _nxtSetDriveRequest.RightPower = -GearPower;
                    _nxtDrivePort.DriveDistance(_nxtSetDriveRequest);

                    break;
                case DriveAction.Back:
                    _nxtSetDriveRequest.LeftPower = GearPower;
                    _nxtSetDriveRequest.RightPower = GearPower;
                    _nxtDrivePort.DriveDistance(_nxtSetDriveRequest);
                    break;
                case DriveAction.Left:
                    _nxtSetDriveRequest.LeftPower = -.4;
                    _nxtSetDriveRequest.RightPower = .4;
                    _nxtDrivePort.DriveDistance(_nxtSetDriveRequest);

                    break;
                case DriveAction.Right:
                      _nxtSetDriveRequest.LeftPower = .4;
                    _nxtSetDriveRequest.RightPower = -.4;
                    _nxtDrivePort.DriveDistance(_nxtSetDriveRequest);
                    break;
                case DriveAction.Stop:
                    _nxtDrivePort.AllStop(MotorStopState.Coast);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}


