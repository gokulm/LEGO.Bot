## LEGO Bot

Note: The code in this repository may not work as it is. 

Here is a YouTube video - https://www.youtube.com/watch?v=YUXAIZXGfQk


Requirements:
-------------
- [LEGO Mindstorms NXT 2.0](https://www.amazon.com/LEGO-Mindstorms-NXT-Discontinued-manufacturer/dp/B001V7RF9U)
- [Microsoft Robotics Developer Studio](https://www.microsoft.com/en-us/download/details.aspx?id=29081) (the link points to the latest version, please refer the code to the appropriate version)
- Network/IP Camera (I used my iPhone with WiFi Camera app installed on it)
- WPF Dashboard Controls

How to set it up?
-----------------
Create a MRDS Service and add the partners - NxtDrive and NxtBattery.  Adding the partners will automatically declare and instantiate an object for BatteryOperations and an object for DriveOperations. Please add one more for DriveOperations as the code mentioned below.

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
    
Add a new class library project to the solution, and add an interface to it. This project needs to be referenced in the MRDS Service and the WPF UI projects (will explain the UI project later).

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

Change the MRDS Service in such a way that it implements this interface. Values for all the properties but the first property (GearPower) will be set in the service, and they will be retrieved and used in the UI layer.

Now, add a new WPF project. VS will choose x86 as the targeted platform by default, change it to "Any CPU". Declare a property named Service of type IMyLegoCarService, and add a constructor, something like this.

    public Dashboard(IMyLegoCarService service) : this()
    {
        Service = service;
        Service.GearPower = 0;
        brsr_ipcamera.Navigate(new Uri("http://ipoftheiphoneorthewebserver/iphonecamera/index.htm"));

        UpdateInitialOdometer();

        uiTimer = new DispatcherTimer();
        uiTimer.Interval = TimeSpan.FromSeconds(1);
        uiTimer.Tick += uiTimer_Tick;
        uiTimer.Start();
    }

This constructor should be called from the Service’s constructor, so that the service and the UI will be on the same thread. Here “brsr_ipcamera” is the web browser control to display the ip camera’s image/video (in my case my iphone). I added an html page to my webserver displaying only the video from the camera. Add a timer control to display the information retrieved from the service periodically. Here I’ve used WPF Dashboard Controls’ dial controls as speedometers (for left motor front, left motor reverse, right motor front and right motor reverse), odometer control as odometer and progress bar control as fuel gauge.  Left/Right Power Current properties were used to initialize the speedometers. Left/Right Encoder properties were used to initialize the odometer, these properties basically give us the degrees that the servo motors rotated. Using the formula: **distance = Convert.ToInt32(Math.Abs(currentEncoderCurrent) / 360 * 2 * 3.14 * 0.75**, we can calculate the distance covered. Here, pi = 3.14 and 0.75 is the radius of the wheels.

![Screenshot](https://github.com/gokulm/LEGO.Bot/blob/master/CropperCapture%5B5%5D_thumb.jpg)

Coming back to the service. Declare and/or instantiate the following classes.

    wpf.WpfServicePort _wpfServicePort;
    drive.SetDriveRequest _nxtSetDriveRequest = new drive.SetDriveRequest();
    battery.BatteryState _nxtBatteryState;

WpfServicePort is used toinvoke the WPF UI, SetDriveRequest to rotate the motors and the BatteryState to get the battery information.

Add a port named “TimerTick” to the service types similar to the automatically created ports “Get”, “Subscribe” etc. Now your serviceoperations class declaration will be something like this -

    [ServicePort]
    public class MyLegoCarServiceOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Subscribe, TimerTick>
    {
    }
    public class TimerTick : Update<TimerTickRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
        public TimerTick()
            : base(new TimerTickRequest())
        {
        }
    }
 
    [DataContract]
    public class TimerTickRequest
    {
    }

Modify the service’s start method something like this -

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

Timerport is used to retrieve the battery information periodically.

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

DriveEncoderUpdate to retrieve the information from the servo motors.

    private void DriveEncoderHandler(drive.DriveEncodersUpdate statistics)
    {
        LeftEncoderCurrent = statistics.Body.LeftEncoderCurrent;
        RightEncoderCurrent = statistics.Body.RightEncoderCurrent;
        LeftPowerCurrent = statistics.Body.LeftPowerCurrent;
        RightPowerCurrent = statistics.Body.RightPowerCurrent;

    }

Create an enum named “DriveAction” in the common class library project. This is to handle the keyboard or the click events from the UI Layer.
    
    public enum DriveAction
    {
        Front,
        Back,
        Left,
        Right,
        Stop
    }

Implement the Drive method in the service.

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

I suppose I’ve explained most of the important parts in the service. Please let me know if you have any questions.
