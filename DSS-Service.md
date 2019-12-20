When I wanted to create my first robotics service, I had many difficulties and found very few resources over the internet for LEGO NXT 2.0 using Microsoft Robotics, so just thought of blogging about this.

Requirements
------------
Microsoft Robotics Studio – Click here to download. Click here to know more about the studio.
LEGO NXT 2.0

Installation
------------
Microsoft Robotics Studio can be used as a stand-alone development environment or it can be used with any of the Visual Studio 2008 or 2010 Editions, including the Express Editions.

How to create DSS Service?
--------------------------
1. After Installing Microsoft Robotics Studio, open Visual Studio IDE (I’m using 2010).
2. Click File/New Project, you should be able to see Microsoft Robotics under C# like this -

![Screenshot](https://github.com/gokulm/LEGO.Bot/blob/master/screenshot1.jpg)

3. Create the project under Microsoft Robotics Studio installation folder (it will be mostly in c:\user\username\microsoft robotics dev studio 2008 r3\), so that the common robotics dlls will be referenced properly.
4. Click OK and you will be allowed to enter your service name, namespace and other details. The most important thing is – you will be allowed to choose your partners from here -

![Screenshot](https://github.com/gokulm/LEGO.Bot/blob/master/screenshot2.jpg)

5. Choose the partners one by one from the list and click “Add as partner”. Check the “Add notification port” checkbox and leave the Creation Policy as it is. Lego NXT Brick (v2) should be selected as one of the partners, and the other partners can be selected based on the sensors that you will be using. In case if you want to edit or delete any of the added partners, you can choose the partner from the partner dropdown and perform your actions. Once all the partners are selected, click OK.
6. You should be able to see the auto-generated code now. Build your solution.

How to create manifest using DSS Manifest Editor?
1. Open DSS Manifest Editor.
2. Choose your service from the left, drag and drop it on the editor. You should be able to see this -

![Screenshot](https://github.com/gokulm/LEGO.Bot/blob/master/screenshot3.jpg)

3. Search for your partners on the left, drag and drop on “Use service’s partner definition (UseExistingOrCreate)” boxes appropriately.
4. As the sensors in turn use the nxtbrick as their partner, another set of “Use service’s partner definition (UseExistingOrCreate)” boxes will be created for the brick.
5. You need to select nxtbrick service again and drop it on the boxes. If you see a pop like the one below, choose the already created service and NOT “Add a new instance”.

![Screenshot](https://github.com/gokulm/LEGO.Bot/blob/master/screenshot4.jpg)

6. To configure the service, click it and then press “Create Initial State” button on the properties window. For the NxtBrick, you need to set the serial port number used for bluetooth connection. For the sensors, you need to set the ports appropriately.
7. Replace this manifest with the manifest created by the service.
8. To start the service execute **dsshost /port:50000 /manifest:"C:\Users\username\Microsoft Robotics Dev Studio 2008 R3\MyDemoService\MyDemoService.manifest.xml”** in DSS Command Prompt.
9. You can also start the service from the DSS Control Panel. In order to view the control panel, start DSS Node and then go to http://localhost:50000 (default port is 50000, if you’ve changed it use it accordingly).
10. Switch ON your LEGO NXT 2.0 and then start the service. If everything is configured properly, you should be able to hear connecting sound from the NXT Brick.

This is it for now, in the next blog post, I’ll be explaining about adding handlers to the sensors and using the motors.
