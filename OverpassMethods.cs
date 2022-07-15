using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace OverpassLibrary
{
    /// <summary>
    /// Статические методы для работы с OSM
    /// </summary>
    public static class OverpassMethods
    {
        /// <summary>
        /// Максимальное количество ID на один запрос почтовых индексов. 
        /// Nominatim API имеет ограничение в 50 ID-шников на один запрос
        /// </summary>
        const int MAX_IDS_PER_POSTCODE_REQUEST = 50;

        /// <summary>
        /// Возвращает информацию о городах из Nominatim API
        /// </summary>
        /// <param name="cityNameInRussian">Название города на русском</param>
        /// <returns>Список найденных городов, либо null, если город не был найден или для города 
        /// нет объекта типа relation</returns>
        /// <remarks>Поскольку возможны города с одинаковыми именами, метод возвращает список найденных городов, 
        /// а не отдельный город. Чтобы отличать такие города, следует использовать свойство <code>State</code>, 
        /// которое хранит в себе область города</remarks>
        public static List<OsmClass> GetCityInfo(string cityNameInRussian)
        {
            WebRequest reqForCity = WebRequest.Create($"https://nominatim.openstreetmap.org/search" +
                $"?city={cityNameInRussian}" +
                $"&format=json" +
                $"&addressdetails=1" +
                $"&accept-language=ru-RU");
            reqForCity.Headers.Add(HttpRequestHeader.UserAgent, "dotnet");

            List<OsmClass> foundCities = new List<OsmClass>();
            dynamic cityJArray;
            using (StreamReader reader = new StreamReader(reqForCity.GetResponse().GetResponseStream()))
            {
                cityJArray = JArray.Parse(reader.ReadToEnd());
            }
            if (cityJArray.Count == 0)
                return null;

            foreach (dynamic city in cityJArray)
            {
                if (city.osm_type != "relation")
                    continue;
                OsmClass newCity = new OsmClass();
                newCity.City = city.address.city ?? city.address.town ?? city.address.village;
                newCity.State = city.address.state;
                dynamic cityBoundingBox = city.boundingbox;
                newCity.CityNorthEast = new System.Drawing.PointF(
                    float.Parse(cityBoundingBox[1].Value),
                    float.Parse(cityBoundingBox[3].Value));
                newCity.CitySouthWest = new System.Drawing.PointF(
                    float.Parse(cityBoundingBox[0].Value),
                    float.Parse(cityBoundingBox[2].Value));
                foundCities.Add(newCity);
            }

            return foundCities;
        }
        /// <summary>
        /// Найти все OSM места в прямоугольнике из базы данных openstreetmap.ru
        /// </summary>
        /// <param name="northEastPoint">Северо-восточный угол прямоугольника</param>
        /// <param name="southWestPoint">Юго-западный угол прямоугольника</param>
        /// <param name="placeTypes">Типы мест, которые необходимо найти</param>
        /// <returns>Список объектов класса OsmClass, представляющий места, найденные в прямоугольнике, 
        /// либо null, если места не найдены</returns>
        /// <exception cref="JsonReaderException">Ошибка при чтении из ответа на запрос</exception>
        public static List<OsmClass> GetAllPlacesInBox(PointF northEastPoint, PointF southWestPoint,
            params string[] placeTypes)
        {
            List<OsmClass> places = new List<OsmClass>();
            StringBuilder urlBuilder = new StringBuilder($"https://openstreetmap.ru/api/poi" +
                $"?action=getpoibbox" +
                $"&t={northEastPoint.X}&r={northEastPoint.Y}" +
                $"&b={southWestPoint.X}&l={southWestPoint.Y}" +
                $"&nclass=");

            urlBuilder.Append(string.Join(",", placeTypes));

            WebRequest placesReq = WebRequest.Create(urlBuilder.ToString());
            dynamic placesJArray;
            using (StreamReader reader = new StreamReader(placesReq.GetResponse().GetResponseStream()))
            {
                try
                {
                    JObject parsedResponse = JObject.Parse(reader.ReadToEnd());
                    if (!parsedResponse.ContainsKey("data"))
                        return places;
                    placesJArray = parsedResponse["data"];
                }
                catch (JsonReaderException ex)
                {
                    throw new ArgumentException("Во время обработки запроса о точках произошла ошибка: " + ex.Message);
                }
            }
            if (placesJArray.Count == 0)
                return null;

            foreach (dynamic place in placesJArray)
            {
                if (string.IsNullOrEmpty(place.addr_street.Value) ||
                    string.IsNullOrEmpty(place.addr_house.Value) ||
                    string.IsNullOrEmpty(place.name_ru.Value))
                    continue;
                OsmClass newObj = new OsmClass()
                {
                    OsmId = ((string)place.osm_id).ToUpper(), // N, W или R в начале id должны быть заглавными для
                                                              // дальнейшей работы с Nominatim API
                    Name = place.name_ru,
                    City = place.addr_city ?? place.addr_village,
                    State = place.addr_region,
                    Latitude = place.lat,
                    Longitude = place.lon,
                    Street = place.addr_street,
                    HouseNum = place.addr_house
                };
                places.Add(newObj);
            }

            List<OsmClass> objectsWithoutPostcodes = places.Where(x => string.IsNullOrEmpty(x.PostCode)).ToList();
            if (objectsWithoutPostcodes.Count != 0)
                SetPostCodes(ref objectsWithoutPostcodes);
            places.RemoveAll(x => !x.IsAddressFull); // убираем места с неполными адресами

            return places;
        }
        /// <summary>
        /// Установить почтовые индексы, если они имеются, для мест из Nominatim API
        /// </summary>
        /// <param name="placesToLookUp">Список мест, для которых необходимо найти и установить почтовые индексы</param>
        public static void SetPostCodes(ref List<OsmClass> placesToLookUp)
        {
            for (int i = 0; i <= placesToLookUp.Count / MAX_IDS_PER_POSTCODE_REQUEST; i++)
            {
                StringBuilder requestUrlBuilder = new StringBuilder($"https://nominatim.openstreetmap.org/lookup" +
                    $"?format=json" +
                    $"&accept-language=ru-RU" +
                    $"&osm_ids=");
                IEnumerable<string> idsOfCurrentObjsRange =
                    placesToLookUp
                    .Skip(i * MAX_IDS_PER_POSTCODE_REQUEST)
                    .Take(MAX_IDS_PER_POSTCODE_REQUEST)
                    .Select(x => x.OsmId);
                requestUrlBuilder.Append(string.Join(",", idsOfCurrentObjsRange));

                Thread.Sleep(1000); // снижение нагрузки на nominatim сервер для избежания 429
                // если нужно рискнуть - комментируем строку или ставим таймаут меньше
                WebRequest request = WebRequest.Create(requestUrlBuilder.ToString());
                request.Headers.Add(HttpRequestHeader.UserAgent, "dotnet");

                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    JArray parsedObjects = JArray.Parse(reader.ReadToEnd());
                    foreach (dynamic obj in parsedObjects)
                    {
                        if (obj?.address?.postcode is null)
                            continue;
                        placesToLookUp
                            .First(x => x.OsmId == ((string)obj.osm_type).ToUpper()[0] + (string)obj.osm_id)
                                                    // Nominatim API не возвращает ID вместе с литерой типа,
                                                    // поэтому приходится совмещать их вручную
                            .PostCode = obj.address.postcode;
                    }
                }
            }
        }
    }
}
