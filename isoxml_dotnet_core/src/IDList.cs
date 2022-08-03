using Dev4ag.ISO11783.TaskFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4ag
{
    public class IDList
    {
        public static ISO11783TaskDataFileDataTransferOrigin dataOrign = ISO11783TaskDataFileDataTransferOrigin.FMIS;
        string name;
        List<int> values;
        int next;
        public IDList(string name)
        {
            this.name = name;
            this.next = 1;
            this.values = new List<int>();
        }

        public string Generate()
        {
            string id = name + ( dataOrign==ISO11783TaskDataFileDataTransferOrigin.FMIS ? 
                next.ToString() : 
                "-" +  next.ToString()
                );
            values.Add(next);
            next++;
            return id;
        }

        public void Add(string id)
        {
            if( dataOrign ==ISO11783TaskDataFileDataTransferOrigin.FMIS)
            {
                int nr = int.Parse(id.Substring(4));
                values.Add(nr);

            } else
            {
                int nr = int.Parse(id.Substring(5));
                values.Add(nr);
            }

        }

    }
    public class IDTable
    {
        public Dictionary<System.Type, IDList> idLists;

        public IDTable()
        {
            idLists = new Dictionary<System.Type, IDList>();

            addList("BSN",typeof(BaseStation));
            addList("CCT", typeof(CodedComment));
            addList("CCG", typeof(CodedCommentGroup));
            addList("CTP", typeof(CropType));
            addList("CPC", typeof(CulturalPractice));
            addList("CTR", typeof(Customer));
            addList("DVC", typeof(Device));
            addList("FRM", typeof(Farm));
            addList("OTQ", typeof(OperationTechnique));
            addList("PFD", typeof(Partfield));
            addList("PDT", typeof(Product));
            addList("PGP", typeof(ProductGroup));
            addList("TSK", typeof(Task));
            addList("VPN", typeof(ValuePresentation));
            addList("PNT", typeof(Point));
            addList("PLN", typeof(Polygon));
            addList("LSG", typeof(LineString));
            addList("WKR", typeof(Worker));

        }



        private void addList(string name, System.Type type)
        {
            idLists.Add(type, new IDList(name));

        }

        public string getNewId(object obj)
        {
            IDList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
            if (idList != null)
            {
                return idList.Generate();
            }
            return "";
        }

        public void addId(object obj, string id)
        {
            IDList idList = null;
            var result = idLists.TryGetValue(obj.GetType(), out idList);
            if (idList != null)
            {
                idList.Add(id);
            }
        }


    }
}
