using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System;
using System.Collections.Generic;
using System.Text;

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
            if (obj.GetType().Equals(typeof(BaseStation))){
                return ((BaseStation)obj).BaseStationId;
            }
            else if(obj.GetType().Equals(typeof(CodedComment))){
                return ((CodedComment)obj).CodedCommentId;
            }
            else if (obj.GetType().Equals(typeof(CodedCommentGroup))){
                return ((CodedCommentGroup)obj).CodedCommentGroupId;
            }
            else if (obj.GetType().Equals(typeof(CropType))){
                return ((CropType)obj).CropTypeId;
            }
            else if (obj.GetType().Equals(typeof(CulturalPractice))){
                return ((CulturalPractice)obj).CulturalPracticeId;
            }
            else if (obj.GetType().Equals(typeof(Device))){
                return ((Device)obj).DeviceId;
            }
            else if (obj.GetType().Equals(typeof(Farm))){
                return ((Farm)obj).FarmId;
            }
            else if (obj.GetType().Equals(typeof(OperationTechnique))){
                return ((OperationTechnique)obj).OperationTechniqueId;
            }
            else if (obj.GetType().Equals(typeof(Partfield))){
                return ((Partfield)obj).PartfieldId;
            }
            else if (obj.GetType().Equals(typeof(Product))){
                return ((Product)obj).ProductId;
            }
            else if (obj.GetType().Equals(typeof(Task))){
                return ((Task)obj).TaskId;
            }
            else if (obj.GetType().Equals(typeof(ValuePresentation))){
                return ((ValuePresentation)obj).ValuePresentationId;
            }
            else if (obj.GetType().Equals(typeof(Worker))){
                return ((Worker)obj).WorkerId;
            }
            else if (obj.GetType().Equals(typeof(LinkGroup)))
            {
                return ((LinkGroup)obj).LinkGroupId;
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
            if (obj.GetType().Equals(typeof(BaseStation)))
            {
                ((BaseStation)obj).BaseStationId = id;
            }
            else if (obj.GetType().Equals(typeof(CodedComment)))
            {
                ((CodedComment)obj).CodedCommentId = id;
            }
            else if (obj.GetType().Equals(typeof(CodedCommentGroup)))
            {
                ((CodedCommentGroup)obj).CodedCommentGroupId = id;
            }
            else if (obj.GetType().Equals(typeof(CropType)))
            {
                ((CropType)obj).CropTypeId = id;
            }
            else if (obj.GetType().Equals(typeof(CulturalPractice)))
            {
                ((CulturalPractice)obj).CulturalPracticeId = id;
            }
            else if (obj.GetType().Equals(typeof(Device)))
            {
                ((Device)obj).DeviceId = id;
            }
            else if (obj.GetType().Equals(typeof(Farm)))
            {
                ((Farm)obj).FarmId = id;
            }
            else if (obj.GetType().Equals(typeof(OperationTechnique)))
            {
                ((OperationTechnique)obj).OperationTechniqueId = id;
            }
            else if (obj.GetType().Equals(typeof(Partfield)))
            {
                ((Partfield)obj).PartfieldId = id;
            }
            else if (obj.GetType().Equals(typeof(Product)))
            {
                ((Product)obj).ProductId = id;
            }
            else if (obj.GetType().Equals(typeof(Task)))
            {
                ((Task)obj).TaskId = id;
            }
            else if (obj.GetType().Equals(typeof(ValuePresentation)))
            {
                ((ValuePresentation)obj).ValuePresentationId = id;
            }
            else if (obj.GetType().Equals(typeof(Worker)))
            {
                ((Worker)obj).WorkerId = id;
            }
            else if (obj.GetType().Equals(typeof(LinkGroup)))
            {
                ((LinkGroup)obj).LinkGroupId  = id; 
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


        private string Name;
        Dictionary<int,object> Ids;
        private int NextId;
        private int NextTmpId = nextTmpBase;
        public IdList(string name)
        {
            this.Name = name;
            this.NextId = 1;
            this.Ids = new Dictionary<int,object>();
        }

        /// <summary>
        /// Adds the given Object to the List of IDs and generates an ID if not existent
        /// </summary>
        /// <param name="obj">The Object to add to the List. The ID within this Object might be changed!</param>
        /// <returns>The ID of the Object</returns>
        /// <exception cref="DuplicatedISOObjectException"></exception>
        public string AddObjectAndAssignIdIfNone(object obj)
        {
            string id = FindId(obj);
            if (id == null || id.Equals(""))
            {
                id = BuildID(Name, NextId);
                Ids.Add(NextId, obj);
                IdList.SetId(obj,id);
                NextId++;
            } else
            {
                int nr = int.Parse(id.Substring(3));
                if (Ids.ContainsKey(nr))
                {
                    throw new DuplicatedISOObjectException(id);
                }
                if(nr >= NextId)
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
            string id = FindId(obj);
            if (id == null || id.Equals(""))
            {
                Ids.Add(NextTmpId, obj);
                NextTmpId++;
            }
            else
            {
                int nr = int.Parse(id.Substring(3));
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
            int nr = int.Parse(id.Substring(3));
            if (Ids.ContainsKey(nr))
            {
                throw new DuplicatedISOObjectException(id);
            }
            IdList.SetId(obj, id);
            Ids.Add(nr,obj);
        }

        public object FindObject(string idString)
        {
            int id = int.Parse(idString.Substring(3));  
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
            foreach( var entry in Ids)
            {
                if( entry.Key >= nextTmpBase)
                {
                    var id = BuildID(Name, NextId);
                    SetId(entry.Value, id );
                    tempItems.Add(NextId, entry.Value);
                    result.Add(new ResultMessage(ResultMessageType.Warning, "Object of Type " + Name + " without ID found. Assigning " + id));
                }
            }


            //Now delete the Temp Elements
            for(var entry = nextTmpBase; entry < NextTmpId; entry++)
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
