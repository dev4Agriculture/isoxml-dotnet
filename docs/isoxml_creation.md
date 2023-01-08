# ISOXML Creation


## Create a new ISOXML Dataset

Use ISOXML.Create to create a new Dataset. Make sure to set a path where the Dataset shell be stored:

```cs
    var isoxml = ISOXML.Create("C://test");
    isoxml.Save();  
```
*This saves an empty taskset*

See [ISOXML Creation](./documentation/isoxmlcreation.md) to fill such structure


## Load an ISOXML DataSet Load or LoadAsync
The static Load-Functions are used to Load a TaskDataSet from a storage folder. 
Its result consists of a Type ISOXML.

```cs
    var isoxml = ISOXML.Load("C://test")
    Console.WriteLine("Messages: "+ isoxml.Messages.Count)
    Console.WriteLine("     Errors: "+ isoxml.Messages.CountErrors())
    Console.WriteLine("     Warnings: " + isoxml.Messages.CountWarnings())
    Console.WriteLine("Name of first customer": isoxml.Data.Customer[0].CustomerLastName)
```



## Save an ISOXML Dataset with Save or SaveAsync
Stores the ISOXML to the given FolderPath
```cs
    var isoxml = ISOXML.Create("C://test");
    isoxml.Save();  
```
*This saves an empty taskset*

## Read parts of an ISOXML File with  ParseFromXMLString
This function was build for the usecase you might only have a Device Description or a Partfield. Calling ParseFromXMLString creates an ISOXML Object with all relevant elements. The parsed Element is added to an Empty Data-Object. If you parsed a DeviceDescription, The Data-Object will include one Device but no Tasks, Partfields, etc....



## Other elements
The Datastructures in the ISOXML reflect an ISOXML TaskDataSet that was loaded or created:
- Messages: A list of Warnings and errors that accoured during loading
- Data: The TaskData Structure including Tasks, Devices, etc. 
- Grids: The List of Grids (Prescription maps) used in the TaskSet
- TimeLogs: The List of MachineData-Packages provided with the TaskSet
- IdTable: The IdTable collects all IDs from within the TaskSet (e.g. CTR1, TSK-1, etc.)
- LinkList: In case of a Version 4 TaskSet, this is the List of Links between TaskData-Internal Ids (e.g. CTR1) and IDs from the FMIS (e.g. UUIDs).