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

        //Console.WriteLine(Estaciones.id);
        var cliente = new HttpClient { BaseAddress = new Uri($"https://www.euskalmet.euskadi.eus/vamet/stations/readings/{Estaciones.id}/{fecha.Year}/{mes}/{dia}/readingsData.json")};
        var responseMessagee = await cliente.GetAsync("", HttpCompletionOption.ResponseContentRead);
        var resultDatae = await responseMessagee.Content.ReadAsStringAsync();
        dynamic stationReadingsJson = JsonConvert.DeserializeObject(resultDatae);
        var temperatura = "No hay datos";
        var precipitacion = "No hay datos";
        var humedad = "No hay datos";
        var viento = "No hay datos";
        
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
                            //Console.WriteLine("Temperature");
                            List<string> dataJsonTimeList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonTimeList.Sort();
                            temperatura = Convert.ToString(dataJson[dataJsonTimeList.Last()]);
                            //Console.WriteLine(temperatura);
                            break;
                        case "precipitation":
                            //Console.WriteLine("Precipitation");
                            List<string> dataJsonPreciList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonPreciList.Sort();
                            precipitacion = Convert.ToString(dataJson[dataJsonPreciList.Last()]);
                            //Console.WriteLine(precipitacion);
                            break;
                        case "humidity":
                            //Console.WriteLine("Humidity");
                            List<string> dataJsonHumiList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonHumiList.Sort();
                            humedad = Convert.ToString(dataJson[dataJsonHumiList.Last()]);
                            //Console.WriteLine(humedad);
                            break;
                        case "mean_speed":
                            //Console.WriteLine("Wind speed");
                            List<string> dataJsonWindList = dataJson.Properties().Select(p => p.Name).ToList();
                            dataJsonWindList.Sort();
                            viento = Convert.ToString(dataJson[dataJsonWindList.Last()]);
                            //Console.WriteLine(viento);
                            break;
                    } 
                }
                catch(Exception e)
                {
                    Console.WriteLine("No va");
                }
            }

            /* ------------------------------ No se guarda*/
            /*if (temperatura == null && humedad == null && precipitacion == null && viento == null)
            {
                Console.WriteLine("No hay datos");
            }
            else
            {
                
            }*/
            /*--------------------------------------*/
        }
        using (var db = new TiempoContext())
        {
            try
            {
                string id = Estaciones.id;
                //Metiendo nuevos datos
                /*if (temperatura == "No hay datos" && humedad == "No hay datos" && precipitacion == "No hay datos" && viento == "No hay datos")
                {
                    Console.WriteLine("No hay datos");
                }
                else
                {
                    Console.WriteLine("Metiendo los datos");
                    var ao1 = new InformacionTiempo
                    {
                        Id = Estaciones.id,
                        Nombre = Estaciones.name,
                        Temperatura = temperatura,
                        Humedad = humedad,
                        Velocidad_Viento = viento,
                        Precipitacion_Acumulada = precipitacion,
                        GpxX = Estaciones.x,
                        GpxY = Estaciones.y
                    };
                    db.InformacionTiempo.Add(ao1);
                };*/
                var row = db.InformacionTiempo.Where(a => a.Id == id).Single();
                /*Zona de actualizacion de datos*/
                Console.WriteLine("Actualizando los datos");
                row.Temperatura = temperatura;
                row.Humedad = humedad;
                row.Velocidad_Viento = viento;
                row.Precipitacion_Acumulada = precipitacion;


                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("No se ha podido guardar" + e);
            }
        }

    }
    catch (Exception e)
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