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

# The Dataformat

ISOXML is an agricultural data format to exchange information between a FarmManagement System and ISOBUS compatible TaskControllers. 

An ISOXML File consists of one major and multiple optional files:

## TASKDATA.XML

The TASKDATA.XML is the major file. It includes Coding Data such as
- **Farm***
- **Partfield** (With geometric form, size and name)
- **Customer**
- **Product** (What to apply or get from the field)
- **CropType** ( What is grown on the field?)
- **Device** ( A machine on the field; either a Tractor, a carried/pulled/pushed implement or a selfpropelled machine like a combine)
- **Worker** ( Who should work on the field)
- **Task** (The actual work on the field. The Task comb)

This list is not complete.

### What is a task?

A task is the central object within the TASKDATA.XML file. It combines a customer, farm, partfield, CropType, Devices etc. to all the data of one operation (e.g. Harvesting Corn on Partfield 1 by Worker Joe in his Combine)

## Grids

A grid includes geolocated setpoints for a field. It might e.g. say that at 52.1234&deg;N and 1.23345&deg;E, there shall be 40kg/ha of seed applied. 52.1235&deg;N and 1.23345&deg;E, 20kg/ha shall be seeded.

A grid is like a 2 or 3 dimensional array with dimension 1 being the latitude, dimension 2 being the longitude and potential dimension 3 (0 in the real array) being the layer. There might be multiple layers in a
grid, e.g. one for seeding and a second for fertilizing.

Details on grids can be found in the [grids documentation](./docs/grids.md)

## TimeLogs

Timelogs are used to record the data while the task is performed. In Timelogs you can find:

- Timestamps
- Geopositions
- A list of machine data; e.g. 'fuel consumtion'' or 'actual moisture of the crop'.

Details on TimeLogs can be found in [the timelogs documentation](./docs/timelogs.md).

## LinkList

LinkLists were introduced in Version 4 of the standard (anno 2015). These additional files allow to link the TaskSet-internal ids ('TSK1', 'CTR2') to other Ids, e.g. the UUIDs a farm management system might use to identify elements of a TaskSet.

Details can be found in [the LinkList documentation](./docs/linklist.md).

## More info

If you need more info on ISOXML in general, feel free to contact us at https://www.dev4Agriculture.de

--- 
# The Library

## ISOXML

ISOXML is the main class of the library. It is used to load, generate and write ISOXML Folders.  It uses Static Functions to generate ISOXML Files.

### Structure of the ISOXML Object
The following graph shows the structure of the ISOXML Object and the interconnection of the objects

![DataStructures in the ISOXML.net Library](https://raw.githubusercontent.com/dev4Agriculture/isoxml-dotnet/main/docs/drawings/DataStructures.png)

#### The Data Object
The Data Object includes all such information that is read from the TASKDATA.XML. Here you can find all customers, Tasks, Partfields, etc. etc. Within the Task-Object you can find links to Grids and TimeLogs

#### The Message List
The message List includes a list of messages generated during loading an ISOXML TaskSet. 


### Handling of errors and warnings

The library was built with a focus on error tolerance. Nothing is more frustrating for a customer than a software that rejects 90% of the files he wants to load. Therefore the library tries to load the data as good as possible and provide errors and warnings wherever necessary without canceling the loading. 

Each Message has an Error Code and an Error Type.

The error types are as follows:
- **Error**: Such messages reflect a serious issue with the TaskDataSet. It will still include data but some tasks might be broken
- **Warning**: Such messages reflect an uncertain dialect in the data. Such warnings mean that the dataset is not broken but there might be some things to take care of.
- **Info**: Such messages reflect an uncommon content of the dataset. Those infos are purely information and don't mark any potential problem.

Here is a full list of [error codes](./Dev4Agriculture.ISO11783.ISOXML/Messaging/ResultMessageCode.cs) and [what they mean](./Dev4Agriculture.ISO11783.ISOXML/Messaging/ResultMessage.cs). This list will be extended over time.

## ISO11783_TaskDataFile

This is the RootElement of any TASKDATA.XML. It includes all the Coding Data Elements of ISOXML such as Customers, Farmers, Partfields etc.

All these subElements have names beginning with ISO. E.g. ISOPartfield, ISOCustomer, ISOTask

The Tasks include links to TimeLogs as well as a grid if those data are available in the TaskSet.
 

## WSM
An analysis class to Analyse WorkingSetMasterNames

The WorkingSetMasterName, also known as ClientName is the Unique Identifier for a machine in the ISOXML world.(Actually it's "nearly unique" only :-/)

It can be decoded to read things like
- Type of machine
- Name of Manufacturer (via the ID; see https://www.isobus.net/isobus/manufacturerCode)

### ToString()

This outputs the Hex-encoded name.

### ToArray()
This is used to export a WorkingSetMasterName to an ISOTaskDataFile.


# Nuget Package
isoxml.net is published as a nuget package here: https://www.nuget.org/packages/Dev4Agriculture.ISO11783.ISOXML


# License note on Logos

Please be aware that the logo assigned to this project is only covered under Apache2 when used with this project. 
You may not use this logo for any other purpose whatsoever without consent by https://www.dev4Agriculture.de
