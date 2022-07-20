# Overpass Library
Overpass Library является библиотекой для извлечения данных о городах и местах в конкретном городе, используя API сайта [openstreetmap.ru](https://openstreetmap.ru/) и [Nominatim API](https://nominatim.org/).

## Зависимости
Overpass Library использует фреймворк .NET Core и библиотеку Json.NET.

## Использование
Найти список городов по названию:
```c#
OverpassMethods.GetCityInfo("Москва");
```
Метод найдёт все города с названием "Москва" и информацию о них при помощи Nominatim API.

Найти все места в прямоугольнике:
```c#
// Либо задать точки прямоугольника вручную
PointF northEastPoint = new PointF(55.9577717f, 37.9674277f);
PointF southWestPoint = new PointF(55.4913076f, 37.290502f);

// Либо объединить с вызовом GetCityInfo
OsmClass foundCity = OverpassMethods.GetCityInfo("Москва").First();
PointF northEastPoint = foundCity.CityNorthEast;
PointF southWestPoint = foundCity.CitySouthWest;

// Либо вручную указать типы мест
string[] placesTypes = new string[] {
"school", "shop", "kiosk"
};

// Либо воспользоваться одной из констант библиотеки
string[] placesTypes = OverpassConsts.ALL_PLACES; // Все виды мест
string[] placesTypes = OverpassConsts.MAIN_PLACES; // Основные виды мест

OverpassMethods.GetAllPlacesInBox(northEastPoint, southWestPoint, placesTypes);
```
Метод найдёт все места в прямоугольнике с определённым типом при помощи API сайта openstreetmap.ru.

По техническим причинам, API сайта openstreetmap.ru не может вернуть более 1000 точек за один запрос.