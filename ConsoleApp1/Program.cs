using System;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using NW = Newtonsoft.Json;
using MS = System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//Obtenemos las estaciones
//https://www.euskalmet.euskadi.eus/vamet/stations/stationList/stationList.json
//Obtenemos los datos de las estaciones
//https://www.euskalmet.euskadi.eus/vamet/stations/readings/C00B/2022/01/22/readingsData.json

var requestUrl = $"https://www.euskalmet.euskadi.eus/vamet/stations/stationList/stationList.json";

Debug.WriteLine(requestUrl);
var client = new HttpClient { BaseAddress = new Uri(requestUrl) };
var responseMessage = await client.GetAsync("", HttpCompletionOption.ResponseContentRead);
var resultData = await responseMessage.Content.ReadAsStringAsync();
dynamic stationTypeJson = JsonConvert.DeserializeObject(resultData);
foreach(var items in stationTypeJson)
{
    Console.WriteLine(items.id);
    var cliente = new HttpClient { BaseAddress = new Uri($"https://www.euskalmet.euskadi.eus/vamet/stations/readings/{items.id}/2022/01/22/readingsData.json")};
    var responseMessagee = await cliente.GetAsync("", HttpCompletionOption.ResponseContentRead);
    var resultDatae = await responseMessagee.Content.ReadAsStringAsync();
    dynamic stationReadingsJson = JsonConvert.DeserializeObject(resultDatae);
    foreach (var items2 in stationReadingsJson)
    {
        foreach(JObject item in items2)
        {
            String dataType = item["name"].ToString();
            JObject preDataJson = JObject.Parse(item["data"].ToString());
            IList<string> keys = preDataJson.Properties().Select(p => p.Name).ToList();
            JObject dataJson = JObject.Parse(preDataJson[keys[0]].ToString());
            switch (dataType)
            {
                case "temperature":
                    Console.WriteLine(dataJson.ToString());
                    List<string> dataJsonTimeList = dataJson.Properties().Select(p => p.Name).ToList();
                    dataJsonTimeList.Sort();
                    //Console.WriteLine(dataJsonTimeList.Last());
                    break;
            }
        }
        
    }
}

/*
class Program
{
    static void Main(string[] args)
    {
        
    }

}

*/