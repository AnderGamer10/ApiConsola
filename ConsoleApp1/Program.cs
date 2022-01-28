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
class Program
{
    static async Task Main(string[] args)
    {
        TimeSpan interval = new TimeSpan(0, 0, 300);
        while (true)
        {
            await ActualizandoDatos();
            Thread.Sleep(interval);
        }
    }
    static async Task ActualizandoDatos()
    {
        Console.WriteLine("Actualizando");

        DateTime fecha = DateTime.Now;
        var dia = "";
        var mes = "";
        if (fecha.Day < 10)
        {
            dia = 0 + "" + fecha.Day;
        }
        else
        {
            dia = Convert.ToString(fecha.Day);
        }

        if (fecha.Month < 10)
        {
            mes = 0 + "" + fecha.Month;
        }
        else
        {
            dia = Convert.ToString(fecha.Month);
        }

        Console.WriteLine("****************************" + fecha.TimeOfDay + "****************************");
        var requestUrl = $"https://www.euskalmet.euskadi.eus/vamet/stations/stationList/stationList.json";
        var client = new HttpClient { BaseAddress = new Uri(requestUrl) };
        var responseMessage = await client.GetAsync("", HttpCompletionOption.ResponseContentRead);
        var resultData = await responseMessage.Content.ReadAsStringAsync();
        dynamic stationTypeJson = JsonConvert.DeserializeObject(resultData);
        foreach (var Estaciones in stationTypeJson)
        {
            try
            {
                var cliente = new HttpClient { BaseAddress = new Uri($"https://www.euskalmet.euskadi.eus/vamet/stations/readings/{Estaciones.id}/{fecha.Year}/{mes}/{dia}/readingsData.json") };
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
                                    List<string> dataJsonTimeList = dataJson.Properties().Select(p => p.Name).ToList();
                                    dataJsonTimeList.Sort();
                                    temperatura = Convert.ToString(dataJson[dataJsonTimeList.Last()]);
                                    break;
                                case "precipitation":
                                    List<string> dataJsonPreciList = dataJson.Properties().Select(p => p.Name).ToList();
                                    dataJsonPreciList.Sort();
                                    precipitacion = Convert.ToString(dataJson[dataJsonPreciList.Last()]);
                                    break;
                                case "humidity":
                                    List<string> dataJsonHumiList = dataJson.Properties().Select(p => p.Name).ToList();
                                    dataJsonHumiList.Sort();
                                    humedad = Convert.ToString(dataJson[dataJsonHumiList.Last()]);
                                    break;
                                case "mean_speed":
                                    List<string> dataJsonWindList = dataJson.Properties().Select(p => p.Name).ToList();
                                    dataJsonWindList.Sort();
                                    viento = Convert.ToString(dataJson[dataJsonWindList.Last()]);
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Sin obtener datos 'Error'");
                        }
                    }
                }
                using (var db = new TiempoContext())
                {
                    try
                    {
                        if (temperatura == "No hay datos" && humedad == "No hay datos" && precipitacion == "No hay datos" && viento == "No hay datos")
                        {
                            Console.WriteLine("Sin datos");
                        }
                        else
                        {
                            //Zona de actualizacion de datos
                            string id = Estaciones.id;
                            try
                            {
                                var infoNueva = db.InformacionTiempo.Where(a => a.Id == id).Single();
                                Console.WriteLine(Estaciones.id + ": Actualizando los datos");
                                infoNueva.Temperatura = temperatura;
                                infoNueva.Humedad = humedad;
                                infoNueva.VelocidadViento = viento;
                                infoNueva.PrecipitacionAcumulada = precipitacion;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Metiendo datos nuevos");
                                var ao1 = new InformacionTiempo
                                {
                                    Id = Estaciones.id,
                                    Nombre = Estaciones.name,
                                    Municipio = Estaciones.municipality,
                                    Temperatura = temperatura,
                                    Humedad = humedad,
                                    VelocidadViento = viento,
                                    PrecipitacionAcumulada = precipitacion,
                                    GpxX = Estaciones.x,
                                    GpxY = Estaciones.y
                                };
                                db.InformacionTiempo.Add(ao1);
                            }
                        };
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
    }
}
