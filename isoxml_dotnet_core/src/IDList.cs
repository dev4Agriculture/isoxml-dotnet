using Dev4ag.Exceptions;
using Dev4ag.ISO11783.LinkListFile;
using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4ag
{
    public class IDList
    {
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


        public static ISO11783TaskDataFileDataTransferOrigin dataOrign = ISO11783TaskDataFileDataTransferOrigin.FMIS;
        string name;
        Dictionary<int,object> ids;
        int next;
        public IDList(string name)
        {
            this.name = name;
            this.next = 1;
            this.ids = new Dictionary<int,object>();
        }

        public string AddObject(object obj)
        {
            string id = FindId(obj);
            if (id == null || id.Equals(""))
            {
                id = name + (dataOrign == ISO11783TaskDataFileDataTransferOrigin.FMIS ?
                    next.ToString() :
                    "-" + next.ToString()
                    );
                ids.Add(next, obj);
                next++;
            } else
            {
                int nr = int.Parse(id.Substring(3));
                if (ids.ContainsKey(nr))
                {
                    throw new DuplicatedISOObjectException(id);
                }
                if(nr >= next)
                {
                    next = nr + 1;
                }
                ids.Add(nr, obj);
            }
            return id;
        }

        public void Add(string id, object obj)
        {
            int nr = int.Parse(id.Substring(3));
            if (ids.ContainsKey(nr))
            {
                throw new DuplicatedISOObjectException(id);
            } 
            ids.Add(nr,obj);
        }

        public object FindObject(string idString)
        {
            int id = int.Parse(idString.Substring(3));  
            return ids[id];
        }


    }
    public class IDTable
    {
        public Dictionary<System.Type, IDList> idLists;

        public IDTable()
        {
            idLists = new Dictionary<System.Type, IDList>();

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
            AddList("LGP", typeof(LinkGroup));
            AddList("LNK", typeof(Link));
            

        }



        private void AddList(string name, System.Type type)
        {
            idLists.Add(type, new IDList(name));

        }

        public string AddObject(object obj)
        {
            IDList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
            if (idList != null)
            {
                return idList.AddObject(obj);
            }
            return "";
        }

        public void AddId(object obj, string id)
        {
            IDList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
            if (idList != null)
            {
                idList.Add(id,obj);
            }
        }


        public object FindById(string id)
        {
            foreach(var list in idLists)
            {
                if (list.Key.Equals(id.Substring(0, 3)))
                {
                    return list.Value.FindObject(id);
                } 
            }
            return null;
        }


    }
}
