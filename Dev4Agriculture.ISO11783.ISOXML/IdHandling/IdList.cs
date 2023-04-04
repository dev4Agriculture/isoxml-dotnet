using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.IdHandling
{
    public class IdList
    {
        internal static int NextTmpBase = 1000000;
        internal static ISO11783TaskDataFileDataTransferOrigin DataOrign = ISO11783TaskDataFileDataTransferOrigin.FMIS;

        /// <summary>
        /// Find the ID in the Object. Object must be a valid ISO11783-10 Element with an ID attribute.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string FindId(object obj)
        {
            switch (obj)
            {
                case ISOBaseStation baseStation:
                    return baseStation.BaseStationId;
                case ISOCodedComment codedComment:
                    return codedComment.CodedCommentId;
                case ISOCodedCommentGroup codedCommentGroup:
                    return codedCommentGroup.CodedCommentGroupId;
                case ISOCodedCommentListValue codedCommentListValue:
                    return codedCommentListValue.CodedCommentListValueId;
                case ISOColourLegend colourLegend:
                    return colourLegend.ColourLegendId;
                case ISOCropType cropType:
                    return cropType.CropTypeId;
                case ISOCropVariety cropVariety:
                    return cropVariety.CropVarietyId;
                case ISOCulturalPractice culturalPractice:
                    return culturalPractice.CulturalPracticeId;
                case ISOCustomer customer:
                    return customer.CustomerId;
                case ISODevice device:
                    return device.DeviceId;
                case ISODeviceElement deviceElement:
                    return deviceElement.DeviceElementId;
                case ISOFarm farm:
                    return farm.FarmId;
                case ISOGuidanceGroup guidanceGroup:
                    return guidanceGroup.GuidanceGroupId;
                case ISOLineString lineString:
                    return lineString.LineStringId;
                case ISOOperationTechnique operationTechnique:
                    return operationTechnique.OperationTechniqueId;
                case ISOPartfield partfield:
                    return partfield.PartfieldId;
                case ISOProduct product:
                    return product.ProductId;
                case ISOProductGroup productGroup:
                    return productGroup.ProductGroupId;
                case ISOPoint point:
                    return point.PointId;
                case ISOPolygon polygon:
                    return polygon.PolygonId;
                case ISOTask task:
                    return task.TaskId;
                case ISOValuePresentation valuePresentation:
                    return valuePresentation.ValuePresentationId;
                case ISOWorker worker:
                    return worker.WorkerId;
                case ISOLinkGroup iSOLinkGroup:
                    return iSOLinkGroup.LinkGroupId;
                default:
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
            switch (obj)
            {
                case ISOBaseStation baseStation:
                    baseStation.BaseStationId = id;
                    break;
                case ISOCodedComment codedComment:
                    codedComment.CodedCommentId = id;
                    break;
                case ISOCodedCommentGroup codedCommentGroup:
                    codedCommentGroup.CodedCommentGroupId = id;
                    break;
                case ISOCodedCommentListValue codedCommentListValue:
                    codedCommentListValue.CodedCommentListValueId = id;
                    break;
                case ISOColourLegend colourLegend:
                    colourLegend.ColourLegendId = id;
                    break;
                case ISOCropType cropType:
                    cropType.CropTypeId = id;
                    break;
                case ISOCropVariety cropVariety:
                    cropVariety.CropVarietyId = id;
                    break;
                case ISOCulturalPractice culturalPractice:
                    culturalPractice.CulturalPracticeId = id;
                    break;
                case ISOCustomer customer:
                    customer.CustomerId = id;
                    break;
                case ISODevice device:
                    device.DeviceId = id;
                    break;
                case ISODeviceElement deviceElement:
                    deviceElement.DeviceElementId = id;
                    break;
                case ISOFarm farm:
                    farm.FarmId = id;
                    break;
                case ISOGuidanceGroup guidanceGroup:
                    guidanceGroup.GuidanceGroupId = id;
                    break;
                case ISOLineString lineString:
                    lineString.LineStringId = id;
                    break;
                case ISOOperationTechnique operationTechnique:
                    operationTechnique.OperationTechniqueId = id;
                    break;
                case ISOPartfield partfield:
                    partfield.PartfieldId = id;
                    break;
                case ISOProduct product:
                    product.ProductId = id;
                    break;
                case ISOProductGroup productGroup:
                    productGroup.ProductGroupId = id;
                    break;
                case ISOPoint point:
                    point.PointId = id;
                    break;
                case ISOPolygon polygon:
                    polygon.PolygonId = id;
                    break;
                case ISOTask task:
                    task.TaskId = id;
                    break;
                case ISOValuePresentation valuePresentation:
                    valuePresentation.ValuePresentationId = id;
                    break;
                case ISOWorker worker:
                    worker.WorkerId = id;
                    break;
                case ISOLinkGroup linkGroup:
                    linkGroup.LinkGroupId = id;
                    break;
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
            return name + (DataOrign == ISO11783TaskDataFileDataTransferOrigin.FMIS ?
                    index.ToString() :
                    "-" + index.ToString()
                    );
        }


        public string Name { get; private set; }
        private readonly Dictionary<int, object> _ids;
        private int _nextId;
        private int _nextTmpId = NextTmpBase;
        public IdList(string name)
        {
            Name = name;
            _nextId = 1;
            _ids = new Dictionary<int, object>();
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
                id = BuildID(Name, _nextId);
                _ids.Add(_nextId, obj);
                SetId(obj, id);
                _nextId++;
            }
            else
            {
                var nr = ToIntId(id);
                if (_ids.ContainsKey(nr))
                {
                    throw new DuplicatedISOObjectException(id);
                }
                if (nr >= _nextId)
                {
                    _nextId = nr + 1;
                }
                _ids.Add(nr, obj);
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
                _ids.Add(_nextTmpId, obj);
                _nextTmpId++;
            }
            else
            {
                var nr = ToIntId(id);
                if (_ids.ContainsKey(nr))
                {
                    throw new DuplicatedISOObjectException(id);
                }
                if (nr >= _nextId)
                {
                    _nextId = nr + 1;
                }
                _ids.Add(nr, obj);
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
            var nr = ToIntId(id);
            if (_ids.ContainsKey(nr))
            {
                throw new DuplicatedISOObjectException(id);
            }
            SetId(obj, id);
            _ids.Add(nr, obj);
        }

        public object FindObject(string idString)
        {
            var id = ToIntId(idString);
            try
            {
                return _ids[id];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This function iterates through the ID List and replaces all Elements without a proper ID with one
        /// that has a new, unique and valid Id assigned to itself; linked by this new Id as a key
        /// </summary>
        /// <returns>A list of Messages for each element that needed a new ID assigned</returns>
        public ResultMessageList CleanListFromTempEntries()
        {
            var tempItems = new Dictionary<int, object>();


            //First find all elements that currently are TEMP and generate an object with a proper id
            var result = new ResultMessageList();
            foreach (var entry in _ids)
            {
                if (entry.Key >= NextTmpBase)
                {
                    var id = BuildID(Name, _nextId);
                    SetId(entry.Value, id);
                    tempItems.Add(_nextId, entry.Value);
                    result.AddWarning(ResultMessageCode.MissingId,
                        ResultDetail.FromString(Name),
                        ResultDetail.FromId(id)
                        );
                }
            }


            //Now delete the Temp Elements
            for (var entry = NextTmpBase; entry < _nextTmpId; entry++)
            {
                _ids.Remove(entry);
            }

            //And add the new Elements
            foreach (var entry in tempItems)
            {
                _ids.Add(entry.Key, entry.Value);
            }
            return result;

        }

        /// <summary>
        /// Get the Integer representation of the ISOXML ID; e.g. -3 for DET-3
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int ToIntId(string id)
        {
            return int.Parse(id.Substring(3));
        }

    }
}
