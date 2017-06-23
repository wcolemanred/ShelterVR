# Shelter Project by Salesforce

**Disclaimer:** Salesforce employees created this project as a demonstration environment. It is provided as-is without official support from Salesforce.

## Overview
The goal of the Shelter project is to learn about the shelter needs of displaced people. The participants will take the role of a business that up-cycles shipping containers to provide shelters to aid agencies around the world.
 
The project involves a Salesforce Lightning application and an interactive Virtual Reality (VR) application.
 
The Lightning application allows creating a ‘Brief’ based on requirements (budget, location, aid agency…). Participants can create a configuration (bill of materials) for a brief. A configuration contains the list of material (furniture, power sources…) that can be utilized in the container shelter.
 
Once a bill of materials is assembled, participants use the Shelter VR application to place the configuration’s material inside the shelter in 3D.

## Requirements
In order to build and run the project you will need:
- a good internet connection (several GB of download)
- some development skills 

You will also need the following:

**Hardware**
- A [Google Daydream View](https://vr.google.com/daydream/) VR headset
- A [Daydream compatible mobile phone](https://vr.google.com/intl/en_uk/daydream/smartphonevr/phones/) (we used a Google Pixel XL)

*Note:* if you do now own this hardware, you can still run the project on your computer with the [Dadyream controller emulator](https://developers.google.com/vr/daydream/controller-emulator) (requires an Android phone with a gyroscope).

**Software** (installation instruction follow)
- Android SDK tools
- Unity 5.6.1
- A free Salesforce Developer Edition (DE) organization

## Installation
Follow these installation steps in this precise order.

### Android SDK tools
Install [Android SDK tools](https://developer.android.com/studio/index.html#downloads). You can either just install the CLI or the full Android Studio.

Once the tools are installed, make sure that the `adb` (Android Debug Bridge) command is added to your path.
To check that, open a terminal and type `adb version` this should report the adb version:
```
> adb version
Android Debug Bridge version 1.0.39
Revision 5943271ace17-android
```

### Enable Developer Mode on your phone
Find the instructions to enable the developer mode on your phone (instructions vary depending on the phone model).

Once you have enabled developer mode, enable USB debugging.
Plug in your phone to your computer via USB and set the USB connection mode to 'charge'.

Open a terminal and type `adb devices` this should report something like this:
```
> adb devices
List of devices attached
* daemon not running. starting it now at tcp:5037 *
* daemon started successfully *
06157df6c8c8c834	device
```

You want to make sure that the status is `device`.

### Clone or download project from git
Start cloning or downloading the project while you continue to setup.

### Install Unity
Install [Unity 5.6.1](https://unity3d.com/).

### Sign up for a Salesforce DE Org
Sign up for a free [Salesforce Developer Edition](https://developer.salesforce.com/signup) (DE) organization.

#### Create a Connected App
1. Log in to your DE org.
2. At the top right of the page, select the gear icon and then click **Setup**.
3. From Setup, enter "App Manager" in the Quick Find and select **App Manager**.
4. Click **New Connected App**.
5. Enter "SalesforceVR" as the **Connected App Name**
6. Enter your **Contact Email**.
7. Under **API (Enable OAuth Settings)**, check the **Enable OAuth Settings** checkbox.
8. Enter "http://localhost:8080/" as the **Callback UR**.
9. Under **Selected OAuth Scope**, move **Access and manage your data (API)** to the Selected OAuth Scopes list.
10. Click **Save**.
11. From this screen, copy the connect app’s **Consumer Key** and **Consumer Secret** someplace temporarily. You’ll enter this information in Unity after we’re done setting up here.

#### Setup My Domain
1. [Enable My Domain](https://help.salesforce.com/articleView?id=domain_name_setup.htm)
2. Deploy it to your users:
   1. In Setup, enter "My Domain" in the Quick Find box and select **My Domain**.
   2. Click **Deploy to Users**.
   3. Click **OK**.

#### Deploy the Shelter Project package
**TODO: finish writing instructions**

#### Open the ShelterVR Unity project
1. Open Unity
2. In the project list click **Open** and select the directory of the git project (ShelterVR by default).
3. Let Unity load the project (it takes longer the first time).

After loading the project, you will get a couple of warnings and errors (these will vary accross versions and envirnoments).

Here are some of the errors you may get:
- Unable to parse YAML file
- Unable to parse *<FILE>*.meta
- Compilation errors related to GvrController
- ImportFBX Errors

Here are some the warning you may get:
- Use of UNITY_MATRIX_MV is detected
- File 'Character_woman' has animation import warnings.

You should be able to ignore those for now as we will correct them by swithing the target platform to Android in the next step.

#### Switch to an Android build
1. Open the **File > Build Settings** menu
2. Select **Android** as the Platform
3. Select **ETC2 (GLES 3.0)** as the Texture Compression
4. Click on **Switch Platform** and wait a couple of minutes for the project to convert (this can take more than 10 minutes of low-end machines).

Once converterted into an Android project, the compilation errors should be gone.

#### Setup the oAuth connection
1. In the **Project** window, navigate the tree to **Assets > scripts**
2. Double-click on **SalesforceAuthConfig** in the list of resources at the right of the tree. This will open the code editor (MS Visual Studio or MonoDevelop)
3. Fill in the constant values with the oAuth settings you saved when you created the Connected App in Salesforce.
4. Optionnaly fill in your username and password to save some time for your tests
5. Close the code editor

#### Run a test in Unity
**TODO: finish writing instructions**

#### Build and run on Android
**TODO: finish writing instructions**

## Licenses
This project is licensed under under Apache License 2.0 (see LICENSE.txt for more details).

This project uses the following dependencies:
- [Google VR SDK for Unity](https://github.com/googlevr/gvr-unity-sdk) licensed under Apache License 2.0
- [Salesforce Unity SDK](https://github.com/pozil/salesforce-unity-sdk) licensed under BSD-3-Clause
  - JSONObject Copyright (C) 2012 Boomlagoon Ltd.
