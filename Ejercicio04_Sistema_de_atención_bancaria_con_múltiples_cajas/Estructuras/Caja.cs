using Ejercicio04_Sistema_de_atención_bancaria_con_múltiples_cajas.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio04_Sistema_de_atención_bancaria_con_múltiples_cajas.Estructuras
{
    public class Caja
    {
        public int Numero { get; private set; }
        public EstadoCaja Estado { get; set; }
        public Queue<Cliente> ColaEspera { get; private set; }
        public Cliente ClienteActual { get; set; }

        public int ClientesAtendidos { get; private set; }
        public int OperacionesRealizadas { get; private set; }
        public int SumaTiempoEsperaMinutos { get; private set; }

        public bool EstaHabilitada => Estado == EstadoCaja.Habilitada;

        public Caja(int numero)
        {
            Numero = numero;
            Estado = EstadoCaja.Habilitada;
            ColaEspera = new Queue<Cliente>();
        }

        public int TiempoPendienteTotalMinutos()
        {
            int enCola = ColaEspera.Sum(c => c.TiempoRestanteMinutos);
            int actual = (ClienteActual != null && !ClienteActual.HaFinalizado)
                ? ClienteActual.TiempoRestanteMinutos
                : 0;
            return enCola + actual;
        }

        public bool TienePreferencialesEnEspera()
        {
            return ColaEspera.Any(c => c.Tipo == TipoAtencion.Preferencial);
        }

        public void EncolarCliente(Cliente cliente)
        {
            ColaEspera.Enqueue(cliente);
        }

        public Cliente VerSiguienteCliente()
        {
            return ColaEspera.Count > 0 ? ColaEspera.Peek() : null;
        }

        public void IniciarSiguienteSiCorresponde(int minutoActual)
        {
            if (ClienteActual == null && Estado != EstadoCaja.Cerrada && ColaEspera.Count > 0)
            {
                Cliente siguiente = ColaEspera.Dequeue();
                siguiente.MinutoInicio = minutoActual;
                siguiente.HoraInicioAtencion = DateTime.Now;
                ClienteActual = siguiente;
            }
        }

        public void AvanzarMinuto()
        {
            if (ClienteActual != null && ClienteActual.TiempoRestanteMinutos > 0)
            {
                ClienteActual.TiempoRestanteMinutos -= 1;
            }
        }

        public Cliente RetirarSiFinalizo(int minutoActual)
        {
            if (ClienteActual != null && ClienteActual.HaFinalizado)
            {
                Cliente finalizado = ClienteActual;
                finalizado.MinutoFin = minutoActual;
                finalizado.HoraFinAtencion = DateTime.Now;

                ClientesAtendidos += 1;
                OperacionesRealizadas += finalizado.CantidadOperaciones;
                SumaTiempoEsperaMinutos += finalizado.TiempoEsperaMinutos;

                ClienteActual = null;

                if (Estado == EstadoCaja.Cerrando)
                {
                    Estado = EstadoCaja.Cerrada;
                }

                return finalizado;
            }

            if (ClienteActual == null && Estado == EstadoCaja.Cerrando)
            {
                Estado = EstadoCaja.Cerrada;
            }

            return null;
        }

        public double TiempoPromedioEspera()
        {
            return ClientesAtendidos > 0
                ? (double)SumaTiempoEsperaMinutos / ClientesAtendidos
                : 0.0;
        }
    }
}
