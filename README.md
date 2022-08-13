# isoxml-dotnet
An ISOXML Implementation in DotNet


## Classes

###  ISOXML 

This is the main Class. Create ISOXMLs from here with new ISOXML("C:/") or load existing ones ISOXML.Load("C:/Existing");


### ISO11783_TaskDataFile

This is the RootElement of any TaskData

### IsoGrid
This is an Application, Seeding or Prescription Map

Each Grid can be of Type1 or Type2

#### Type1

Type 1 is an array of bytes; each value in the grid redirects to the corresponding TreatmentZone and its proposed Value

Format: datat1[height,width]

### Type2

Type 2 can include multiple layers and 

Format: datat1[layer,height,width]



### WSM
An analysis class to Analyse WorkingSetMasterNames




# License note on Logos

Please be aware that the logo assigned to this project is only covered under Apache2 when used with this project. 
You may not use this logo for any other purpose whatsoever without consent by https://www.dev4Agriculture.de
