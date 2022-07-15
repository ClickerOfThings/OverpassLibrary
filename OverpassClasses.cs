using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace OverpassLibrary
{
    /// <summary>
    /// Объект OSM
    /// </summary>
    public class OsmClass : IEquatable<OsmClass>, IComparable<OsmClass>
    {
        /// <summary>
        /// ID объекта с литерой типа ([N]ode, [W]ay, [R]elation) в начале
        /// </summary>
        public string OsmId { get; set; }
        /// <summary>
        /// Широта объекта
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Долгота объекта
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Название объекта
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Область, в котором находится объект
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Город, деревня, село или посёлок, в котором находится объект
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Северо-восточная точка прямоугольника границы города (если объектом является город)
        /// </summary>
        public PointF CitySouthWest { get; set; }
        /// <summary>
        /// Юго-западная точка прямоугольника границы города (если объектом является город)
        /// </summary>
        public PointF CityNorthEast { get; set; }
        /// <summary>
        /// Улица, на которой находится объект
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Номер дома, на котором находится объект
        /// </summary>
        public string HouseNum { get; set; }
        /// <summary>
        /// Почтовый индекс объекта (может отсутствовать)
        /// </summary>
        public string PostCode { get; set; }
        /// <summary>
        /// Полный адрес объекта формата: почтовый индекс, область, город/деревня/село/посёлок, улица, дом
        /// </summary>
        public string FullAddressString =>
            (string.IsNullOrEmpty(PostCode) ? "" : PostCode + ", ") 
            + State + ", " + City + " г, " +
            Street + ", " + HouseNum;
        /// <summary>
        /// Имеет ли объект все данные о своём адресе (область, город/деревня/село/посёлок, улица, дом)
        /// </summary>
        public bool IsAddressFull =>
            !string.IsNullOrEmpty(State) &&
            !string.IsNullOrEmpty(City) &&
            !string.IsNullOrEmpty(Street) &&
            !string.IsNullOrEmpty(HouseNum);
        /// <summary>
        /// Проверяет, равен ли OsmId текущего объекта OsmId проверяемого объекта
        /// </summary>
        /// <param name="other">Проверяемый объект</param>
        /// <returns>true если OsmId у обоих объектов равны, false если не равны</returns>
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
        /// <summary>
        /// Проверяет, равен ли OsmId текущего объекта OsmId проверяемого объекта
        /// </summary>
        /// <param name="obj">Проверяемый объект</param>
        /// <returns>true если OsmId у обоих объектов равны, false если не равны</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as OsmClass);
        }
        /// <summary>
        /// Сравнение проверяемого OSM объекта с текущим по OsmId, включая литеру в начале
        /// </summary>
        /// <param name="other">Проверяемый объект</param>
        /// <returns>-1 если OsmId текущего объекта меньше OsmId проверяемого объекта,
        /// 0 если OsmId текущего объекта равен OsmId проверяемого объекта,
        /// 1 если OsmId текущего объекта больше OsmId проверяемого объекта</returns>
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
