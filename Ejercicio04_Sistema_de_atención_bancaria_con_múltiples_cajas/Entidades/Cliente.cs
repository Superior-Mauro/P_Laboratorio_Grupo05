using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio04_Sistema_de_atención_bancaria_con_múltiples_cajas.Entidades
{
    public class Cliente
    {
        public string NumeroTicket { get; set; }
        public string Nombre { get; set; }
        public int CantidadOperaciones { get; set; }
        public TipoAtencion Tipo { get; set; }

        // Cada operación requiere 2 minutos
        public int TiempoEstimadoMinutos => CantidadOperaciones * 2;
        public int TiempoRestanteMinutos { get; set; }

        // Marcas de tiempo (minuto de simulación + hora de reloj)
        public int MinutoLlegada { get; set; }
        public int? MinutoInicio { get; set; }
        public int? MinutoFin { get; set; }

        public DateTime HoraLlegada { get; set; }
        public DateTime? HoraInicioAtencion { get; set; }
        public DateTime? HoraFinAtencion { get; set; }

        public bool HaFinalizado => TiempoRestanteMinutos <= 0;

        public int TiempoEsperaMinutos =>
            MinutoInicio.HasValue
                ? Math.Max(0, MinutoInicio.Value - MinutoLlegada)
                : 0;

        public Cliente(string numeroTicket, string nombre, int cantidadOperaciones, TipoAtencion tipo, int minutoLlegada)
        {
            NumeroTicket = numeroTicket;
            Nombre = nombre;
            CantidadOperaciones = cantidadOperaciones;
            Tipo = tipo;
            TiempoRestanteMinutos = TiempoEstimadoMinutos;

            MinutoLlegada = minutoLlegada;
            HoraLlegada = DateTime.Now;
        }

        public override string ToString()
        {
            string inicio = HoraInicioAtencion.HasValue ? HoraInicioAtencion.Value.ToString("HH:mm:ss") : "Pendiente";
            string fin = HoraFinAtencion.HasValue ? HoraFinAtencion.Value.ToString("HH:mm:ss") : "Pendiente";

            return $"[{NumeroTicket}] {Nombre} | {Tipo} | Ops: {CantidadOperaciones} | " +
                   $"Restante: {TiempoRestanteMinutos}/{TiempoEstimadoMinutos} min | " +
                   $"Llegada: {HoraLlegada:HH:mm:ss} | Inicio: {inicio} | Fin: {fin}";
        }
    }
}
