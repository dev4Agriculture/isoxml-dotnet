# TaskController Emulation


The TaskController Emulation consists of the DeviceGenerator and the TaskController Emulator.

## DeviceGenerator
To simplify work with the TaskController Emulator, we have added several functions to generate Devices:

The Constructor is used to generate an initial device with a Main deviceElement


### SetLocalization
Set the Localization of your TaskController Emulator

### AddDeviceElement
Adds other DeviceElements and links them in the DeviceTree. **Not required for the Main Element**

### AddDeviceProcessData
Links an ISODeviceProcessData to the DeviceTree and links it; provides IDs if necessary, etc.. ValuePresentations linked in the DPD are automatically added to the DeviceTree.

### AddDeviceProperties
Links an ISODeviceProperty to the DeviceTree and links it; provides IDs if necessary, etc.. ValuePresentations linked in the DPT are automatically added to the DeviceTree.

### NextDevice...Id: 

These functions can be used to generate IDs for DPD, DPT, DET, DVP. They are already called internally in the AddDeviceProcessData/AddDeviceProperty, so you don't need to call them. But you can if you want to influence the numbers

## TCEmulator

The TaskController Emulator can be used to generate ISOXML like a TaskController.

You can do the following with it:

### Generate

Use the Generate-Function to create a new TaskController Emulator. You can set Version, and some info on the TaskController Manufacturer


### ImportISOXML
The import function is meant to emulate an import of CodingData - or even already-started ISOXML - into your TC Emulator

### Export ISOXML
The export is the equivalent to your USB Export on the terminal.

**Important**: Even though there is a "GetTaskData"-Function; for export you should use the export function as it ensures your taskdata is properly finished; e.g. regarding the Device Allocations in the Task

### ConnectDevice
This is your equivalent to plugging the ISOBUS Breakaway Connector to the back of your tractor. It tells the Emulator that the machine is now available and can be used in Tasks.

### DisconnectDevice
You're able to cut ties with a machine by disconnecting it. Afterwards you're not able to add further machine data for this machine

### StartTask
You can start a Task, either from the existing TaskSet or by adding a new TaskSet by name.

**Remark**: If you intent to start a specific Task from your TaskSet, you can use the *GetTaskDataSet()* function to read it. You can then just add the Task as parameter; the emulator will automatically recognize that it already knows this Task


### PauseTask
Have a break. Stop the Task and - if Autologging is not activated - nothing is running.

### AddTimeAndPosition
Before adding any machine data, this is the function you need to call to initialize the DataLine for your TimeLog

### Update(Raw)MachineValue
This function is used to change the value of a "Current" or "LifeTime" value. It overwrites the currently existing value. UpdateMachineValue respects the Unit of a potentially linked DeviceValuePresentation while UpdateRawMachineValue will understand the value just as the standardized Value (so, the value that would normally be sent via the BUS).

### Add(Raw)ValueToMachineValue
This function is used to update a TotalValue, applying the next difference; e.g. the lately processed Area. 
AddValueToMachineValue will respect the DeviceValuePresentation while AddRawValueToMachineValue doesn't do any conversion before applying the value.




### GetDevice
This function is used by end of the DeviceGeneration to load the Device and add it to the ISOXML.