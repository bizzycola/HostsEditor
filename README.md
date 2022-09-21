# HostsEditor
Simple .net core 6 WPF app for editing the Windows hosts file.

![screenshot](https://user-images.githubusercontent.com/845115/191452507-bd0b33a6-14dd-4d6a-a698-6822a66275ce.png)

## How it works
One think you may notice when you open the app is it doesn't display any existing host records.
The program works by adding its own section to the hosts file within which it adds and removes records created with the UI.

It does not touch anything outside the section it creates, but any changes created whilst the app is running outside that section will be overwritten when it saves.
This is something I may consider changing at some point, but it hasn't been an issue for me in daily use.

## Running
**Note:** This app requires administrator privileges to run as they are required to modify the hosts file.

You can download the app from Releases page on Github. 

There are 3 available versions for download:

* Framework Dependent: Requires you to install the .net core 6 framework from [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to run it. Useful if you already have .net core 6 installed.
* Framework Independent: Includes .net core bundled within the app. Larger filesize but useful if you don't want to install additional dependencies.
* Click Once: An installer that will automatically download, install and update the program and dependencies. 