using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OverpassLibrary
{
    public enum OsmObjTypes
    {
        Node,
        Way,
        Relation
    }
    public class osmClass
    {
        public static Dictionary<OsmObjTypes, char> ObjTypeToChar = new Dictionary<OsmObjTypes, char>()
        {
            {OsmObjTypes.Node, 'N' },
            {OsmObjTypes.Way, 'W' },
            {OsmObjTypes.Relation, 'R' }
        };
        public static Dictionary<char, OsmObjTypes> CharToObjType = new Dictionary<char, OsmObjTypes>()
        {
            {'N', OsmObjTypes.Node },
            {'W', OsmObjTypes.Way },
            {'R', OsmObjTypes.Relation }
        };

        public long Id { get; set; } = -1;
        public OsmObjTypes ObjType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PlaceType { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string CityId { get; set; }
        public PointF CitySouthWest { get; set; }
        public PointF CityNorthEast { get; set; }
        public string Street { get; set; }
        public string HouseNum { get; set; }
        public string FullAddress =>
            (string.IsNullOrEmpty(PostCode) ? "" : PostCode + ", ") 
            + State + ", " + City + " г, " +
            Street + ", " + HouseNum;
        public string PostCode { get; set; }

        public bool IsAddressFull(bool checkPostCode) =>
            (!checkPostCode || !string.IsNullOrEmpty(PostCode)) &&
            !string.IsNullOrEmpty(State) &&
            !string.IsNullOrEmpty(City) &&
            !string.IsNullOrEmpty(Street) &&
            !string.IsNullOrEmpty(HouseNum);
    }
}
