# Abstract
ISOXML is an agricultural data format to create Task Descriptions and record machine data in agricultural machines.
ISOXML is standardized in ISO11783-10.

# Developers and Maintainers
![Logo](/assets/dev4Agriculture.svg)

Dev4Agriculture is specialized in agricultural data analysis. We love to make farming software work.


## Classes

###  ISOXML 

This is the main Class. Create ISOXMLs from here with new ISOXML.Create("C:/") or load existing ones ISOXML.Load("C:/Existing");


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
