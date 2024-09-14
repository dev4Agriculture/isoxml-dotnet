# Abstract
ISOXML is an agricultural data format to create Task Descriptions and record machine data in agricultural machines.
ISOXML is standardized in ISO11783-10.

# Developers and Maintainers
![Logo](https://raw.githubusercontent.com/dev4Agriculture/isoxml-dotnet/main/.github/workflows/badge.svg)

Dev4Agriculture is specialized in agricultural data analysis. We love to make farming software work.

- [Website](https://www.dev4agriculture.de)
- [Get in Contact](https://www.dev4agriculture.de/en/company/#contactus)

# What are all the sub projects

This repository consists of multiple sub-projects. They all belong to isoxml.net but onliy Dev4Agriculture.ISO11783.ISOXML is the one that represents the library itself

## Dev4Agriculture.ISO11783.ISOXML.Generation

This subproject is used to autogenerate the classes from XSD Schema Files. Those are adjusted versions of https://isobus.net (adjusted to fit V3 and V4 in parallel)

## Dev4Agriculture.ISO11783.ISOXML.Test
Guess what, those are the tests for the ISOXML.net library. You may find interesting examples and also a few test data here.

## Dev4Agriculture.ISO11783.ISOXML.Examples
Another sub project that provides Examples


## Dev4Agriculture.ISO11783.ISOXML

This is the library itself that will be described in the following chapters

--- 
# The Library

## Nuget Package
isoxml.net is published as a nuget package here: https://www.nuget.org/packages/Dev4Agriculture.ISO11783.ISOXML

## Classes

![DataStructures in the ISOXML.net Library](https://raw.githubusercontent.com/dev4Agriculture/isoxml-dotnet/main/docs/drawings/DataStructures.png)

### ISOXML

The Datastructures in the ISOXML reflect an ISOXML TaskDataSet that was loaded or created:
- Messages: A list of Warnings and errors that accoured during loading
- Data: The TaskData Structure including Tasks, Devices, etc. 
- Grids: The List of Grids (Prescription maps) used in the TaskSet
- TimeLogs: The List of MachineData-Packages provided with the TaskSet
- IdTable: The IdTable collects all IDs from within the TaskSet (e.g. CTR1, TSK-1, etc.)
- LinkList: In case of a Version 4 TaskSet, this is the List of Links between TaskData-Internal Ids (e.g. CTR1) and IDs from the FMIS (e.g. UUIDs).
 

This is the main Class. It consists of functions to create, load and save ISOXML. 

### static Load or LoadAsync
The static Load-Functions are used to Load a TaskDataSet from a storage folder. 
Its result consists of a Type ISOXML.
There are also functions to read ISOXML From a Zip File.

### static Create
Creates an Empty TaskDataSet

### Save or SaveAsync
Stores the ISOXML to the given FolderPath




## ISO11783_TaskDataFile

This is the RootElement of any TaskData. It includes all the Coding Data Elements of ISOXML such as Customers, Farmers, Partfields etc.

All these subElements have names beginning with ISO. E.g. ISOPartfield, ISOCustomer, ISOTask

The Tasks include links to TimeLogs as well as a grid if those data are available in the TaskSet.


## ISOGridFile
This is an Application, Seeding or Prescription Map

>**Remark**: 
>Each Grid can be of Type1 or Type2
>- *Type1*: Type 1 is an array of bytes; each value in the grid redirects to the corresponding TreatmentZone and its proposed Value
>- *Type2*: Type 2 is an array of 32bit integers. It directly includes values rather than a link to the treatmentZone.


**Important**: Both Grids should be acessed through the Functions GetValue/SetValue only!
 

## ClientName
An analysis class to Analyse WorkingSetMasterNames/ClientNames

The WorkingSetMasterName, also known as ClientName is the Unique Identifier for a machine in the ISOXML world.(Actually it's "nearly unique" only :-/)

It can be decoded to read things like
- Type of machine
- Name of Manufacturer (via the ID; see https://www.isobus.net/isobus/manufacturerCode)

### ToString()

This outputs the Hex-encoded name.

### ToArray()
This is used to export a WorkingSetMasterName to an ISOTaskDataFile.


## TaskController Emulator
The TaskController Emulator can be used to generate machine data just like a TaskController would do it. 

### Generate

Use the Generate-Function to create a new TaskController Emulator. You can set Version, and some info on the TaskController Manufacturer

### Localization

To ensure that all your devices receive a valid DeviceDescription, Use the SetLocalization-Function to define Language and Unit Systems


### GenerateDevice

This returns a DeviceGenerator. When calling "GenerateDevice", the generated, yet empty device is automatically added to the ISOXML TaskSet.

Use the Functions of the DeviceGenerator to Simplify machine Generation:

- AddDeviceElement: Use this if you need more than the main DeviceElement. The MainElement is directly generated with the Device
- AddDeviceProcessData/AddDeviceProperty: Create a DPD or DPT-Element, directly link it inside the DeviceElement and - if provided - add the Value Presentation
- NextDevice...Id: These functions can be used to generate IDs for DPD, DPT, DET, DVP. They are already called internally in the AddDeviceProcessData/AddDeviceProperty, so you don't need to call them. But you can if you want to influence the numbers

### BUS Simulation: Connect and Disconnect

In a normal TaskController. A DeviceDescription, a TimeLog and a DeviceAllocation is always added when a machine is connected.
To do this in our emulator, use **ConnectDevice** and **DisconnectDevice**.

### ISOXML Manipulation

To be able to add Customers, Product, etc. and to be able to save the ISOXML, you can get the Whole ISOXML-Object from GetTaskDataSet();

### TaskController Actions

- If you want to collect machine data, you need to Start a task using StartTask().
- Once you call StartTask, any other Started Task is paused
- If you call FinishTask, the Task is set to "Final" and should not be started anymore

### Data Recording

The data Recording consists of 2 types of Functions:

#### Time and Position Record

We see Time and Position as leading, which means, that before any machine data is recorded, we need to record a Time and Position (position might as well be of status "UNKNOWN")

#### Recording Machine Data

There is in total 2 Functions to record Data, each comes in 2 forms.


- **AddValue vs. UpdateValue**: UpdateValue can be used to set a new value for DDI+DET; not caring about the old value. Add(Raw)ValueTo... can used to sum up values over time. E.g. You might not want to keep track of the total driven distance, you might just add the value driven since the last record was done.
- **Update vs. UpdateRaw***: ISOBUS works with Standardized Units, defined for each DDI at https://isobus.net. To track Data in this Unit, use the UpdateRaw function. 
 You might however have a ValuePresentation defined and prefer to document data in your unit of choice. If so, use the Update... or Add...-Function. It will automatically convert the value based on your ValuePresentation




- 
# License note on Logos

Please be aware that the logo assigned to this project is only covered under Apache2 when used with this project. 
You may not use this logo for any other purpose whatsoever without consent by https://www.dev4Agriculture.de
