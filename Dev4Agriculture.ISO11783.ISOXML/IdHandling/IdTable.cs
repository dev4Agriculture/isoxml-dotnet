using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.IdHandling
{
    public class IdTable
    {
        public Dictionary<System.Type, IdList> IdLists;

        public IdTable()
        {
            IdLists = new Dictionary<System.Type, IdList>();

            AddList("BSN", typeof(ISOBaseStation));
            AddList("CCT", typeof(ISOCodedComment));
            AddList("CCG", typeof(ISOCodedCommentGroup));
            AddList("CTP", typeof(ISOCropType));
            AddList("CPC", typeof(ISOCulturalPractice));
            AddList("CTR", typeof(ISOCustomer));
            AddList("DVC", typeof(ISODevice));
            AddList("FRM", typeof(ISOFarm));
            AddList("OTQ", typeof(ISOOperationTechnique));
            AddList("PFD", typeof(ISOPartfield));
            AddList("PDT", typeof(ISOProduct));
            AddList("PGP", typeof(ISOProductGroup));
            AddList("TSK", typeof(ISOTask));
            AddList("VPN", typeof(ISOValuePresentation));
            AddList("PNT", typeof(ISOPoint));
            AddList("PLN", typeof(ISOPolygon));
            AddList("LSG", typeof(ISOLineString));
            AddList("WKR", typeof(ISOWorker));
        }



        private void AddList(string name, System.Type type)
        {
            IdLists.Add(type, new IdList(name));

        }

        public string AddObjectAndAssignIdIfNone(object obj)
        {
            var result = IdLists.TryGetValue(obj.GetType(), out var idList);
            if (idList != null)
            {
                return idList.AddObjectAndAssignIdIfNone(obj);
            }
            return "";
        }

        /// <summary>
        /// Add an Object to the corresponding IDList WITHOUT generating an ID if none is given.
        /// CleanListFromTempEntries() should be called once reading all objects is done!
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>The ID read from the Object</returns>
        public string ReadObject(object obj)
        {
            var result = IdLists.TryGetValue(obj.GetType(), out var idList);
            if (idList != null)
            {
                return idList.ReadObject(obj);
            }
            return "";
        }

        /// <summary>
        /// Add an Object to the corresponding ID list with the given ID.
        /// ATTENTION: Generating IDs yourself can cause problems, if this is not absolutely required,
        ///            please use AddObject, which will generate an ID for you (if non is yet given in the object).
        /// </summary>
        /// <param name="obj">The Object to Add</param>
        /// <param name="id">The ID to assign</param>
        public void AddObjectWithOwnId(ref object obj, string id)
        {
            var result = IdLists.TryGetValue(obj.GetType(), out var idList);
            if (idList != null)
            {
                idList.AddId(id, ref obj);
            }
        }

        /// <summary>
        /// Find the Object corresponding for this ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object FindById(string id)
        {
            foreach (var list in IdLists)
            {
                if (list.Key.Equals(id.Substring(0, 3)))
                {
                    return list.Value.FindObject(id);
                }
            }
            return null;
        }


        public List<ResultMessage> CleanListFromTempEntries()
        {
            var result = new List<ResultMessage>();

            foreach (var entry in IdLists)
            {
                result.AddRange(entry.Value.CleanListFromTempEntries());
            }
            return result;
        }

    }

}
