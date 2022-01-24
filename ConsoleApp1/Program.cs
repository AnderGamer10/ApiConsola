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
foreach(var Estaciones in stationTypeJson)
{
    try
    {
        DateTime fecha = DateTime.Now;
        var dia = "";
        var mes = "";
        if (fecha.Day < 10)
        {
            dia = 0 + "" + fecha.Day;
        }else
        {
            dia = Convert.ToString(fecha.Day);
        }

        if (fecha.Month < 10)
        {
            mes = 0 + "" + fecha.Month;
        }else
        {
            dia = Convert.ToString(fecha.Month);
        }

        Console.WriteLine(Estaciones.id);
        var cliente = new HttpClient { BaseAddress = new Uri($"https://www.euskalmet.euskadi.eus/vamet/stations/readings/{Estaciones.id}/{fecha.Year}/{mes}/{dia}/readingsData.json")};
        var responseMessagee = await cliente.GetAsync("", HttpCompletionOption.ResponseContentRead);
        var resultDatae = await responseMessagee.Content.ReadAsStringAsync();
        dynamic stationReadingsJson = JsonConvert.DeserializeObject(resultDatae);
        foreach (var DatosEstaciones in stationReadingsJson)
        {
            foreach (JObject item in DatosEstaciones)
            {
                try
                {
                    String dataType = item["name"].ToString();
                    JObject preDataJson = JObject.Parse(item["data"].ToString());
                    IList<string> keys = preDataJson.Properties().Select(p => p.Name).ToList();
                    JObject dataJson = JObject.Parse(preDataJson[keys[0]].ToString());
                    switch (dataType)
                    {
                        case "temperature":
                            Console.WriteLine("Temperature");
                            List<string> dataJsonTimeList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonTimeList.Sort();
                            Console.WriteLine(dataJson[dataJsonTimeList.Last()]);
                            break;
                        case "precipitation":
                            Console.WriteLine("Precipitation");
                            List<string> dataJsonPreciList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonPreciList.Sort();
                            Console.WriteLine(dataJson[dataJsonPreciList.Last()]);
                            break;
                        case "humidity":
                            Console.WriteLine("Humidity");
                            List<string> dataJsonHumiList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonHumiList.Sort();
                            Console.WriteLine(dataJson[dataJsonHumiList.Last()]);
                            break;
                        case "mean_speed":
                            Console.WriteLine("Wind speed");
                            List<string> dataJsonWindList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonWindList.Sort();
                            Console.WriteLine(dataJson[dataJsonWindList.Last()]);
                            break;
                    }
                }catch(Exception e)
                {
                
                }
            }
        
        }
    }catch(Exception e)
    {

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