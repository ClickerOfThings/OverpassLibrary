using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace OverpassLibrary
{
    public class OsmClass : IEquatable<OsmClass>, IComparable<OsmClass>
    {
        public string OsmId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public PointF CitySouthWest { get; set; }
        public PointF CityNorthEast { get; set; }
        public string Street { get; set; }
        public string HouseNum { get; set; }
        public string FullAddressString =>
            (string.IsNullOrEmpty(PostCode) ? "" : PostCode + ", ") 
            + State + ", " + City + " г, " +
            Street + ", " + HouseNum;
        public string PostCode { get; set; }
        public bool IsAddressFull =>
            !string.IsNullOrEmpty(State) &&
            !string.IsNullOrEmpty(City) &&
            !string.IsNullOrEmpty(Street) &&
            !string.IsNullOrEmpty(HouseNum);

        public int Compare(OsmClass x, OsmClass y)
        {
            long xID = long.Parse(x.OsmId.Substring(1));
            long yID = long.Parse(y.OsmId.Substring(1));
            if (xID > yID)
                return 1;
            if (xID < yID)
                return -1;
            else
                return 0;

        }

        public bool Equals([AllowNull] OsmClass other)
        {
            if (other is null)
                return false;
            if (this.OsmId == other.OsmId)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return OsmId.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OsmClass);
        }

        public int CompareTo([AllowNull] OsmClass other)
        {
            char xNodeTypeChar = this.OsmId[0];
            char yNodeTypeChar = other.OsmId[0];
            if (xNodeTypeChar != yNodeTypeChar)
                return xNodeTypeChar.CompareTo(yNodeTypeChar);
            long xID = long.Parse(this.OsmId.Substring(1));
            long yID = long.Parse(other.OsmId.Substring(1));
            if (xID > yID)
                return 1;
            if (xID < yID)
                return -1;
            else
                return 0;
        }
    }
}
