using System;
using System.Collections.Generic;
using System.Text;

namespace OverpassLibrary
{
    /// <summary>
    /// Константы библиотеки OverpassLibrary
    /// </summary>
    public static class OverpassConsts
    {
        /// <summary>
        /// Все типы мест, используемые в openstreetmap.ru
        /// </summary>
        public readonly static string[] ALL_PLACES = new string[]
        {
            "root","democracy","administrative","town_hall","government","register_office","migration",
            "tax_inspection","police","pension_fund","embassy","prosecutor","bailiff","social_facility",
            "courthouse","customs","prison","food","bar","cafe","biergarten","pub","restaurant","food_court",
            "fast_food","health","pharmacy","hospital","veterinary","doctors","nursing_home","mortuary","clinic",
            "ambulance_station","dentist","infrastructure","water_supply","water_works","water_tower","water_well",
            "wastewater_plant","surveillance","landfill","communications","telephone_exchange","communication_tower",
            "construction","transport_constructions","bridge","tunnel","power","power_generator","substation",
            "transformer","power_plant","culture","library","zoo","cinema","clubs","club_automobile","club_astronomy",
            "club_charity","club_veterans","club_game","club_history","club_cinema","club_art","club_computer",
            "club_motorcycle","club_music","club_board_games","club_nature","club_shooting","club_hunting",
            "club_linux","club_fishing","club_sport","club_theatre","club_tourism","club_fan","club_photography",
            "club_chess","club_ethnic","museum","community_centre","dance","theatre","circus","arts_centre","shop",
            "hifi","car_parts","car","outdoor","anime","antiques","charity","boutique","chemist","bicycle",
            "variety_store","gas","interior_decoration","baby_goods","window_blind","newsagent","pet","toys",
            "doityourself","stationery","atv","kiosk","bookshop","carpet","computer","cosmetics","paint","copyshop",
            "kitchen","tableware","pawnbroker","bed","furniture","motorcycle","musical_instrument","organic","shoes",
            "clothes","glaziery_shop","optician_shop","trade","hunting","gift","bedding","art","ticket","video",
            "shop_food","bakery","alcohol","deli","confectionery","convenience","seafood","ice_cream","butcher",
            "beverages","greengrocer","supermarket","mall","farm","vacuum_cleaner","radiotechnics","frame",
            "fishing_shop","marketplace","mobile_phone","bathroom_furnishing","second_hand","erotic","garden_centre",
            "hardware","hearing_aids","dive","sports","bag","supermarket","tobacco","fabric","mall","herbalist",
            "department_store","houseware","florist","sewing","curtain","electronics","jewelry","ship_chandler",
            "education","driving_school","library","kindergarten","university","college","school","office",
            "estate_agent","administrative","town_hall","architect","employment_agency","bookmaker","accountant",
            "it","research","educational_institution","ngo","newspaper","advertising_agency","insurance","studio",
            "telecommunication","travel_agent","company","lawyer","notary","natural","bay","waterfall","volcano",
            "cave_entrance","peak","cape","island","islet","beach","strait","spring","saddle","religion","cemetery",
            "place_of_worship","place_of_worship_muslim","place_of_worship_jewish","place_of_worship_christian",
            "craft","tailor","boatbuilder","scaffolder","pottery","upholsterer","tinsmith","stand_builder","key_cutter",
            "sun_protection","window_construction","optician_craft","stonemason","caterer","roofer","blacksmith",
            "painter","metal_construction","hvac","shoemaker","rigger","parquet_layer","beekeeper","bookbinder",
            "brewery","sawmill","basket_maker","tiler","saddler","sailmaker","dressmaker","craft_computer",
            "agricultural_engines","handicraft","gardener","plumber","sculptor","locksmith","glaziery_craft",
            "carpenter","sweep","carpet_layer","insulation","photographer","photographic_laboratory","clockmaker",
            "watchmaker","plasterer","electrician","jeweller","sport_and_entertainment","water_park","sauna",
            "swimming_pool","running_track","bicycle_track","raceway","playground","horse_track","ice_rink","fishing",
            "miniature_golf","nightclub","park","theme_park","dog_park","beach_resort","golf_course","marina",
            "bicycle_rental","slipway","pitch","club_sport","sports_centre","stadium","dance","transport",
            "bicycle_transport","bicycle_parking","bicycle_rental","water_transport","ferry_terminal","slipway",
            "ship_navigation","lateral_buoy","lighthouse","leading_beacon","air_transport","airport","helipad",
            "personal_transport","car_wash","car_repair","car_repair_tyres","parking_entrance","speed_camera",
            "fuel","ev_charging","parking","vehicle_ramp","public_transport","bus_station","subway_entrance",
            "railway_halt","railway_station","bus_stop","tram_stop","subway_station","taxi","funicular",
            "road_obstacles","ford","gate","speed_camera","bump","hump","cushion","mountain_pass","toll_booth",
            "lift_gate","transport_constructions","bridge","tunnel","tourism","viewpoint","sights","artwork",
            "viewpoint","city_gate","attraction","castle","zoo","ruins","lighthouse","boundary_stone","memorial",
            "wreck","monument","museum","theme_park","battlefield","archaeological_site","rune_stone","ship",
            "fountain","fort","information","camp_site","picnic_site","drinking_water","lodging","guest_house",
            "hotel","motel","hostel","spring","travel_agent","club_tourism","shelter","funicular","clock","service",
            "driving_school","tailor","sauna","ev_charging","internet_cafe","nails","massage","currencyexchange",
            "hairdresser","recycling","post_office","post_box","laundry","car_rental","bicycle_rental",
            "craft_computer","funeral_directors","beauty_salon","tattoo","telephone","toilets","dry_cleaning",
            "finance","bank","atm","accountant","pawnbroker","currencyexchange","money_lender","emergency","police",
            "fire_station","fire_hydrant","ambulance_station","emergency_phone"
        };

        /// <summary>
        /// Основные типы мест, используемые в openstreetmap.ru
        /// </summary>
        public readonly static string[] MAIN_PLACES = new string[]
        {
            "democracy","administrative","town_hall","government","register_office","migration",
            "tax_inspection","police","pension_fund","embassy","prosecutor","bailiff","social_facility",
            "courthouse","customs","prison","food","bar","cafe","biergarten","pub","restaurant","food_court",
            "fast_food","hospital","clinic","cinema","kiosk","organic","shop_food","bakery","alcohol","deli",
            "confectionery","convenience","seafood","ice_cream","butcher","beverages","greengrocer","supermarket",
            "mall","farm","supermarket","mall","department_store","education","driving_school","library",
            "kindergarten","university","college","school","place_of_worship","place_of_worship_muslim",
            "place_of_worship_jewish","place_of_worship_christian"
        };
    }
}
