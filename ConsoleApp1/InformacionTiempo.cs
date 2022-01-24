using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class InformacionTiempo
{
    [Key]
    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Datos_ultima_hora { get; set; }
    public string Temperatura { get; set; }
    public string Humedad { get; set; }
    public string Velocidad_Viento { get; set; }
    public string Precipitacion_Acumulada { get; set; }
    public string GpxX { get; set; }
    public string GpxY { get; set; }    
}

