using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace OverpassLibrary
{
    public static class OverpassMethods
    {
        /// <returns>Поскольку могут быть города с одинаковыми названиями, 
        /// метод возвращает массив объектов городов.</returns>
        public static List<osmClass> GetCityInfo(string cityNameInRussian)
        {
            WebRequest reqForCity = WebRequest.Create($"https://nominatim.openstreetmap.org/search" +
                $"?city={cityNameInRussian}" +
                $"&format=json" +
                $"&addressdetails=1" +
                $"&accept-language=ru-RU");
            reqForCity.Headers.Add(HttpRequestHeader.UserAgent, "c# script");
            List<osmClass> returnObjects = new List<osmClass>();
            dynamic mainObj;
            using (StreamReader reader = new StreamReader(reqForCity.GetResponse().GetResponseStream()))
            {
                mainObj = JArray.Parse(reader.ReadToEnd());
            }
            if (mainObj.Count == 0)
                throw new ArgumentException("Города не существует");
            foreach (dynamic city in mainObj)
            {
                if (city.osm_type != "relation")
                    continue;
                osmClass newObj = new osmClass();
                newObj.City = city.address.city ?? city.address.town ?? city.address.village;
                newObj.CityId = city.osm_id;
                newObj.State = city.address.state;
                dynamic boxArray = city.boundingbox;
                newObj.CityNorthEast = new System.Drawing.PointF(float.Parse(boxArray[1].Value), float.Parse(boxArray[3].Value));
                newObj.CitySouthWest = new System.Drawing.PointF(float.Parse(boxArray[0].Value), float.Parse(boxArray[2].Value));
                returnObjects.Add(newObj);
            }
            if (returnObjects.Count == 0)
                throw new ArgumentException("Ошибка поиска города. Города не существует, либо для города нет типа relation " +
                    "в базе данных OSM");
            return returnObjects;
        }

        public static List<osmClass> GetAllPlacesInBox(PointF northEast, PointF southWest, params string[] placeTypes)
        {
            return GetAllPlacesInCity(new osmClass
            {
                CityNorthEast = northEast,
                CitySouthWest = southWest
            }, placeTypes);
        }
        public static List<osmClass> GetAllPlacesInCity(osmClass cityInfo, params string[] placeTypes)
        {
            List<osmClass> places = new List<osmClass>();
            StringBuilder urlBuilder = new StringBuilder($"https://openstreetmap.ru/api/poi" +
                $"?action=getpoibbox" +
                $"&t={cityInfo.CityNorthEast.X}&r={cityInfo.CityNorthEast.Y}" +
                $"&b={cityInfo.CitySouthWest.X}&l={cityInfo.CitySouthWest.Y}" +
                $"&nclass=");
            for (int i = 0; i < placeTypes.Length; i++)
            {
                urlBuilder.Append(placeTypes[i]);
                if (i < placeTypes.Length - 1)
                    urlBuilder.Append(",");
            }

            WebRequest request = WebRequest.Create(urlBuilder.ToString());
            dynamic mainObj;
            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                try
                {
                    mainObj = JObject.Parse(reader.ReadToEnd());
                }
                catch(JsonReaderException ex)
                {
                    throw new ArgumentException("Города не существует, либо не найдено точек с введёнными типами");
                }
            }
            if (mainObj.data.Count == 0)
                throw new ArgumentException("Объектов с введёнными типами не существует");
            dynamic placesObjs = mainObj.data;
            foreach (dynamic placeObj in placesObjs)
            {
                if (string.IsNullOrEmpty(placeObj.addr_street.Value) ||
                    string.IsNullOrEmpty(placeObj.addr_house.Value) ||
                    string.IsNullOrEmpty(placeObj.name_ru.Value))
                    continue;
                osmClass newObj = new osmClass()
                {
                    Id = long.Parse(placeObj.osm_id.Value.Substring(1)),
                    ObjType = osmClass.CharToObjType[placeObj.osm_id.Value.ToUpper()[0]],
                    PlaceType = placeObj["class"],
                    Name = placeObj.name_ru,
                    City = placeObj.addr_city.Value ?? placeObj.addr_village,
                    State = placeObj.addr_region,
                    Latitude = placeObj.lat,
                    Longitude = placeObj.lon,
                    Street = placeObj.addr_street,
                    HouseNum = placeObj.addr_house
                };
                places.Add(newObj);
            }
            SetPostCodes(ref places);
            places.RemoveAll(x => !x.IsAddressFull(false)); // убираем места с неполными адресами
            return places;
        }

        public static string LookUpPostCode(osmClass objToLookUp)
        {
            if (objToLookUp.Id < 0)
                throw new ArgumentException("Пустой объект класса osmClass в методе LookUpPostCode");
            WebRequest request = WebRequest.Create($"https://nominatim.openstreetmap.org/lookup" +
                $"?osm_ids={osmClass.ObjTypeToChar[objToLookUp.ObjType] + objToLookUp.Id.ToString()}" +
                $"&format=json" +
                $"&accept-language=ru-RU");
            request.Headers.Add(HttpRequestHeader.UserAgent, "c# script");
            dynamic lookUpObj;
            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                lookUpObj = JArray.Parse(reader.ReadToEnd());
            }
            if (lookUpObj.Count == 0)
                throw new ArgumentException("Пустой ответ от API. Вероятнее всего, неправильный ID или " +
                    "тип объекта");
            return lookUpObj[0].address.postcode;
        }

        public static void SetPostCodes(ref List<osmClass> objectsToLookUp)
        {
            int objectCounter = 0;
            while (objectCounter < objectsToLookUp.Count)
            {
                StringBuilder requestUrlBuilder = new StringBuilder($"https://nominatim.openstreetmap.org/lookup" +
                    $"?format=json" +
                    $"&accept-language=ru-RU" +
                    $"&osm_ids=");
                int previousObjectCounter = objectCounter;
                int currentMax = Math.Clamp(objectCounter + 50, int.MinValue, objectsToLookUp.Count);
                for (; objectCounter < currentMax; objectCounter++)
                {
                    if (objectsToLookUp[objectCounter].Id < 0)
                        throw new ArgumentException("Пустой объект класса osmClass в методе LookUpPostCode: " +
                            objectsToLookUp[objectCounter].FullAddress);
                    requestUrlBuilder.Append(
                        osmClass.ObjTypeToChar[objectsToLookUp[objectCounter].ObjType] + objectsToLookUp[objectCounter].Id.ToString());
                    if (objectCounter < currentMax - 1)
                        requestUrlBuilder.Append(',');
                }
                Thread.Sleep(1000); // снижение нагрузки на nominatim сервер для избежания 429
                // если нужно рискнуть - комментируем строку или ставим таймаут меньше :)
                WebRequest request = WebRequest.Create(requestUrlBuilder.ToString());
                request.Headers.Add(HttpRequestHeader.UserAgent, "c# script");
                dynamic lookUpObj;
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    lookUpObj = JArray.Parse(reader.ReadToEnd());
                }
                if (lookUpObj.Count == 0)
                    throw new ArgumentException("Пустой ответ от API. Вероятнее всего, неправильный ID или " +
                        "тип объекта");
                for (int i = 0; i < lookUpObj.Count; previousObjectCounter++, i++)
                {
                    if (lookUpObj[i].osm_id != objectsToLookUp[previousObjectCounter].Id)
                    {
                        i--;
                        continue;
                    }
                    objectsToLookUp[previousObjectCounter].PostCode = lookUpObj[i].address.postcode;
                }
            }
        }
    }
}
