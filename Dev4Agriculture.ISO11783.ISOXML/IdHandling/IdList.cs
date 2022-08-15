using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System.Collections.Generic;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class IdList
    {
        internal static int nextTmpBase = 1000000;
        internal static ISO11783TaskDataFileDataTransferOrigin dataOrign = ISO11783TaskDataFileDataTransferOrigin.FMIS;

        /// <summary>
        /// Find the ID in the Object. Object must be a valid ISO11783-10 Element with an ID attribute.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string FindId(object obj)
        {
            if (obj.GetType().Equals(typeof(ISOBaseStation)))
            {
                return ((ISOBaseStation)obj).BaseStationId;
            }
            else if (obj.GetType().Equals(typeof(ISOCodedComment)))
            {
                return ((ISOCodedComment)obj).CodedCommentId;
            }
            else if (obj.GetType().Equals(typeof(ISOCodedCommentGroup)))
            {
                return ((ISOCodedCommentGroup)obj).CodedCommentGroupId;
            }
            else if (obj.GetType().Equals(typeof(ISOCropType)))
            {
                return ((ISOCropType)obj).CropTypeId;
            }
            else if (obj.GetType().Equals(typeof(ISOCulturalPractice)))
            {
                return ((ISOCulturalPractice)obj).CulturalPracticeId;
            }
            else if (obj.GetType().Equals(typeof(ISODevice)))
            {
                return ((ISODevice)obj).DeviceId;
            }
            else if (obj.GetType().Equals(typeof(ISOFarm)))
            {
                return ((ISOFarm)obj).FarmId;
            }
            else if (obj.GetType().Equals(typeof(ISOOperationTechnique)))
            {
                return ((ISOOperationTechnique)obj).OperationTechniqueId;
            }
            else if (obj.GetType().Equals(typeof(ISOPartfield)))
            {
                return ((ISOPartfield)obj).PartfieldId;
            }
            else if (obj.GetType().Equals(typeof(ISOProduct)))
            {
                return ((ISOProduct)obj).ProductId;
            }
            else if (obj.GetType().Equals(typeof(ISOTask)))
            {
                return ((ISOTask)obj).TaskId;
            }
            else if (obj.GetType().Equals(typeof(ISOValuePresentation)))
            {
                return ((ISOValuePresentation)obj).ValuePresentationId;
            }
            else if (obj.GetType().Equals(typeof(ISOWorker)))
            {
                return ((ISOWorker)obj).WorkerId;
            }
            else if (obj.GetType().Equals(typeof(ISOLinkGroup)))
            {
                return ((ISOLinkGroup)obj).LinkGroupId;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Assigns the given ID to the correct ID-Attribute within the Object
        /// </summary>
        /// <param name="obj">The Object must be an ISO11783-10 Element that has an ID-Attribute</param>
        /// <param name="id">The ID to set</param>
        public static void SetId(object obj, string id)
        {
            if (obj.GetType().Equals(typeof(ISOBaseStation)))
            {
                ((ISOBaseStation)obj).BaseStationId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOCodedComment)))
            {
                ((ISOCodedComment)obj).CodedCommentId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOCodedCommentGroup)))
            {
                ((ISOCodedCommentGroup)obj).CodedCommentGroupId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOCropType)))
            {
                ((ISOCropType)obj).CropTypeId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOCulturalPractice)))
            {
                ((ISOCulturalPractice)obj).CulturalPracticeId = id;
            }
            else if (obj.GetType().Equals(typeof(ISODevice)))
            {
                ((ISODevice)obj).DeviceId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOFarm)))
            {
                ((ISOFarm)obj).FarmId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOOperationTechnique)))
            {
                ((ISOOperationTechnique)obj).OperationTechniqueId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOPartfield)))
            {
                ((ISOPartfield)obj).PartfieldId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOProduct)))
            {
                ((ISOProduct)obj).ProductId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOTask)))
            {
                ((ISOTask)obj).TaskId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOValuePresentation)))
            {
                ((ISOValuePresentation)obj).ValuePresentationId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOWorker)))
            {
                ((ISOWorker)obj).WorkerId = id;
            }
            else if (obj.GetType().Equals(typeof(ISOLinkGroup)))
            {
                ((ISOLinkGroup)obj).LinkGroupId = id;
            }
            else
            {
                //TODO if it's another Object we need an error handling
            }
        }

        /// <summary>
        /// Generate a proper ID as described in ISO11783-10 for xs:id-Fields
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string BuildID(string name, int index)
        {
            return name + (dataOrign == ISO11783TaskDataFileDataTransferOrigin.FMIS ?
                    index.ToString() :
                    "-" + index.ToString()
                    );
        }


        private readonly string Name;
        readonly Dictionary<int, object> Ids;
        private int NextId;
        private int NextTmpId = nextTmpBase;
        public IdList(string name)
        {
            Name = name;
            NextId = 1;
            Ids = new Dictionary<int, object>();
        }

        /// <summary>
        /// Adds the given Object to the List of IDs and generates an ID if not existent
        /// </summary>
        /// <param name="obj">The Object to add to the List. The ID within this Object might be changed!</param>
        /// <returns>The ID of the Object</returns>
        /// <exception cref="DuplicatedISOObjectException"></exception>
        public string AddObjectAndAssignIdIfNone(object obj)
        {
            var id = FindId(obj);
            if (id == null || id.Equals(""))
            {
                id = BuildID(Name, NextId);
                Ids.Add(NextId, obj);
                IdList.SetId(obj, id);
                NextId++;
            }
            else
            {
                var nr = int.Parse(id.Substring(3));
                if (Ids.ContainsKey(nr))
                {
                    throw new DuplicatedISOObjectException(id);
                }
                if (nr >= NextId)
                {
                    NextId = nr + 1;
                }
                Ids.Add(nr, obj);
            }
            return id;
        }


        /// <summary>
        /// Adds an object to the List, assigning a TEMP ID if no ID was given.
        /// The object is NOT changed 
        /// </summary>
        /// <param name="obj">The object to add to the List. The ID within the Object will NOT be changed</param>
        /// <returns>The ID if any exists</returns>
        /// <exception cref="DuplicatedISOObjectException"></exception>
        public string ReadObject(object obj)
        {
            var id = FindId(obj);
            if (id == null || id.Equals(""))
            {
                Ids.Add(NextTmpId, obj);
                NextTmpId++;
            }
            else
            {
                var nr = int.Parse(id.Substring(3));
                if (Ids.ContainsKey(nr))
                {
                    throw new DuplicatedISOObjectException(id);
                }
                if (nr >= NextId)
                {
                    NextId = nr + 1;
                }
                Ids.Add(nr, obj);
            }
            return id;
        }

        /// <summary>
        /// Add a new Entry 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <exception cref="DuplicatedISOObjectException"></exception>
        public void AddId(string id, ref object obj)
        {
            var nr = int.Parse(id.Substring(3));
            if (Ids.ContainsKey(nr))
            {
                throw new DuplicatedISOObjectException(id);
            }
            IdList.SetId(obj, id);
            Ids.Add(nr, obj);
        }

        public object FindObject(string idString)
        {
            var id = int.Parse(idString.Substring(3));
            return Ids[id];
        }

        /// <summary>
        /// This function iterates through the ID List and replaces all Elements without a proper ID with one
        /// that has a new, unique and valid Id assigned to itself; linked by this new Id as a key
        /// </summary>
        /// <returns>A list of Messages for each element that needed a new ID assigned</returns>
        public List<ResultMessage> CleanListFromTempEntries()
        {
            var tempItems = new Dictionary<int, object>();


            //First find all elements that currently are TEMP and generate an object with a proper id
            var result = new List<ResultMessage>();
            foreach (var entry in Ids)
            {
                if (entry.Key >= nextTmpBase)
                {
                    var id = BuildID(Name, NextId);
                    SetId(entry.Value, id);
                    tempItems.Add(NextId, entry.Value);
                    result.Add(new ResultMessage(ResultMessageType.Warning, "Object of Type " + Name + " without ID found. Assigning " + id));
                }
            }


            //Now delete the Temp Elements
            for (var entry = nextTmpBase; entry < NextTmpId; entry++)
            {
                Ids.Remove(entry);
            }

            //And add the new Elements
            foreach (var entry in tempItems)
            {
                Ids.Add(entry.Key, entry.Value);
            }
            return result;

        }

    }
}
