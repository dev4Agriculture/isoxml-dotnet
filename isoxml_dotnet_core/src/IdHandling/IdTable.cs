using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4ag
{
    public class IdTable
    {
        public Dictionary<System.Type, IdList> idLists;

        public IdTable()
        {
            idLists = new Dictionary<System.Type, IdList>();

            AddList("BSN", typeof(BaseStation));
            AddList("CCT", typeof(CodedComment));
            AddList("CCG", typeof(CodedCommentGroup));
            AddList("CTP", typeof(CropType));
            AddList("CPC", typeof(CulturalPractice));
            AddList("CTR", typeof(Customer));
            AddList("DVC", typeof(Device));
            AddList("FRM", typeof(Farm));
            AddList("OTQ", typeof(OperationTechnique));
            AddList("PFD", typeof(Partfield));
            AddList("PDT", typeof(Product));
            AddList("PGP", typeof(ProductGroup));
            AddList("TSK", typeof(Task));
            AddList("VPN", typeof(ValuePresentation));
            AddList("PNT", typeof(Point));
            AddList("PLN", typeof(Polygon));
            AddList("LSG", typeof(LineString));
            AddList("WKR", typeof(Worker));
        }



        private void AddList(string name, System.Type type)
        {
            idLists.Add(type, new IdList(name));

        }

        public string AddObjectAndAssignIdIfNone(object obj)
        {
            IdList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
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
            IdList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
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
            IdList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
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
            foreach (var list in idLists)
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

            foreach (var entry in this.idLists)
            {
                result.AddRange(entry.Value.CleanListFromTempEntries());
            }
            return result;
        }

    }

}
