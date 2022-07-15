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
    public static class OverpassMethods
    {
        /// <summary>
        /// Возвращает информацию о городах из Nominatim API
        /// </summary>
        /// <param name="cityNameInRussian">Название города на русском</param>
        /// <returns>Объект класса OsmClass с заполненной информацией о названии, области и границах прямоугольника</returns>
        /// <exception cref="ArgumentException">Город не может быть найден в базе данных OSM, 
        /// либо найденный город не имеет необходимого OSM типа relation</exception>
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
                //throw new ArgumentException("Города не существует");
                return foundCities;

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
            /*if (foundCities.Count == 0)
                throw new ArgumentException("Ошибка поиска города. Города не существует, либо для города нет типа relation " +
                    "в базе данных OSM");*/
            return foundCities;
        }
        /// <summary>
        /// Найти все OSM точки в прямоугольнике из базы данных openstreetmap.ru
        /// </summary>
        /// <param name="northEastPoint">Северо-восточный угол прямоугольника</param>
        /// <param name="southWestPoint">Юго-западный угол прямоугольника</param>
        /// <param name="placeTypes">Типы точек, которые необходимо найти</param>
        /// <returns>Список объектов класса OsmClass, представляющий точки, найденные в прямоугольнике</returns>
        /// <exception cref="ArgumentException">Не найдены точки (пустой запрос от сервера)</exception>
        public static List<OsmClass> GetAllPlacesInBox(PointF northEastPoint, PointF southWestPoint, params string[] placeTypes)
        {
            List<OsmClass> places = new List<OsmClass>();
            StringBuilder urlBuilder = new StringBuilder($"https://openstreetmap.ru/api/poi" +
                $"?action=getpoibbox" +
                $"&t={northEastPoint.X}&r={northEastPoint.Y}" +
                $"&b={southWestPoint.X}&l={southWestPoint.Y}" +
                $"&nclass=");

            string lastPlaceInArray = placeTypes.Last();
            foreach (string placeType in placeTypes)
            {
                urlBuilder.Append(placeType);
                if (placeType != lastPlaceInArray)
                    urlBuilder.Append(",");
            }

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
                return places;

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
        const int MAX_IDS_PER_POSTCODE_REQUEST = 50;
        /// <summary>
        /// Установить почтовые индексы для точек, если имеются, из Nominatim API
        /// </summary>
        /// <param name="objectsToLookUp">Список точек, для которых необходимо найти почтовые индексы</param>
        /// <exception cref="ArgumentException"></exception>
        public static void SetPostCodes(ref List<OsmClass> objectsToLookUp)
        {
            int currentEndIndex = 0;
            for (int currentIndex = 0; currentIndex < objectsToLookUp.Count;)
            {
                StringBuilder requestUrlBuilder = new StringBuilder($"https://nominatim.openstreetmap.org/lookup" +
                    $"?format=json" +
                    $"&accept-language=ru-RU" +
                    $"&osm_ids=");
                currentEndIndex = Math.Clamp(currentIndex + MAX_IDS_PER_POSTCODE_REQUEST, 0, objectsToLookUp.Count);
                for (; currentIndex < currentEndIndex; currentIndex++)
                {
                    requestUrlBuilder.Append(objectsToLookUp[currentIndex].OsmId);
                    if (objectsToLookUp[currentIndex] != objectsToLookUp[currentEndIndex - 1])
                        requestUrlBuilder.Append(',');
                }
                Thread.Sleep(1000); // снижение нагрузки на nominatim сервер для избежания 429
                // если нужно рискнуть - комментируем строку или ставим таймаут меньше :)
                WebRequest request = WebRequest.Create(requestUrlBuilder.ToString());
                request.Headers.Add(HttpRequestHeader.UserAgent, "dotnet");
                List<OsmClass> parsedObjsWithPostcodesFound = new List<OsmClass>(50);
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    JArray parsedObjects = JArray.Parse(reader.ReadToEnd());
                    foreach (dynamic obj in parsedObjects)
                    {
                        if (obj?.address?.postcode is null)
                            continue;
                        string newOsmId = ((string)obj.osm_type).ToUpper()[0] + (string)obj.osm_id;
                        parsedObjsWithPostcodesFound.Add(new OsmClass
                        {
                            OsmId = newOsmId,
                            PostCode = obj.address.postcode
                        });
                    }
                }
                if (parsedObjsWithPostcodesFound.Count == 0)
                    /*throw new ArgumentException("Пустой ответ от API. Вероятнее всего, неправильный ID или " +
                        "тип объекта");*/ // наверное, довольно радикально на каждый пустой запрос возвращать исключение
                    continue;
                List<OsmClass> lookUpObjsWithPostcodesFound =
                    objectsToLookUp.Intersect(parsedObjsWithPostcodesFound).ToList();
                parsedObjsWithPostcodesFound.Sort(); // для двоичного поиска
                for (int i = 0; i < lookUpObjsWithPostcodesFound.Count; i++)
                {
                    OsmClass parsedObjWithSameId = parsedObjsWithPostcodesFound[
                        parsedObjsWithPostcodesFound.BinarySearch(new OsmClass
                        {
                            OsmId = lookUpObjsWithPostcodesFound[i].OsmId
                        })];
                    lookUpObjsWithPostcodesFound[i].PostCode =
                        parsedObjWithSameId.PostCode;
                }
            }
        }
    }
}
