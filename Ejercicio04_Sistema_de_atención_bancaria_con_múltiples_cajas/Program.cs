using Ejercicio04_Sistema_de_atención_bancaria_con_múltiples_cajas.Entidades;
using Ejercicio04_Sistema_de_atención_bancaria_con_múltiples_cajas.Estructuras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio04_Sistema_de_atención_bancaria_con_múltiples_cajas
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            List<Caja> cajas = new List<Caja>
            {
                new Caja(1),
                new Caja(2),
                new Caja(3)
            };

            int minutoActual = 0;
            int correlativoTicket = 1;
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE ATENCIÓN BANCARIA (3 CAJAS) ===");
                Console.WriteLine($"Minuto de simulación: {minutoActual}");
                Console.WriteLine("a. Registrar cliente y asignar caja");
                Console.WriteLine("b. Mostrar clientes pendientes en cada caja");
                Console.WriteLine("c. Mostrar siguiente cliente en cada caja (sin retirar)");
                Console.WriteLine("d. Simular avance de 1 minuto en las tres cajas");
                Console.WriteLine("e. Retirar automáticamente clientes finalizados");
                Console.WriteLine("f. Cerrar temporalmente una caja y redistribuir clientes");
                Console.WriteLine("g. Habilitar nuevamente una caja");
                Console.WriteLine("h. Mostrar estadísticas por caja (atendidos, espera, operaciones)");
                Console.WriteLine("i. Determinar caja con mayor cantidad de operaciones");
                Console.WriteLine("0. Salir");
                Console.Write("\nSeleccione una opción: ");

                string opcion = Console.ReadLine()?.Trim().ToLower();

                switch (opcion)
                {
                    case "a":
                        RegistrarCliente(cajas, minutoActual, ref correlativoTicket);
                        break;
                    case "b":
                        MostrarClientesPendientes(cajas, minutoActual);
                        break;
                    case "c":
                        MostrarSiguienteCliente(cajas, minutoActual);
                        break;
                    case "d":
                        AvanzarUnMinuto(cajas, ref minutoActual, retirarAutomaticamente: false);
                        break;
                    case "e":
                        AvanzarUnMinuto(cajas, ref minutoActual, retirarAutomaticamente: true);
                        break;
                    case "f":
                        CerrarTemporalmenteCaja(cajas, minutoActual);
                        break;
                    case "g":
                        HabilitarCaja(cajas, minutoActual);
                        break;
                    case "h":
                        MostrarEstadisticas(cajas);
                        break;
                    case "i":
                        MostrarCajaMayorOperaciones(cajas);
                        break;
                    case "0":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }

                if (!salir)
                {
                    Console.WriteLine("\nPresione una tecla para continuar...");
                    Console.ReadKey(true);
                }
            }
        }

        // a. Registrar un cliente y asignarlo automáticamente
        static void RegistrarCliente(List<Caja> cajas, int minutoActual, ref int correlativoTicket)
        {
            Console.Write("Nombre del cliente: ");
            string nombre = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Cliente";

            Console.Write("Cantidad de operaciones (2 min c/u): ");
            if (!int.TryParse(Console.ReadLine(), out int ops) || ops <= 0)
            {
                Console.WriteLine("Cantidad de operaciones inválida.");
                return;
            }

            Console.Write("Tipo de atención (1 = Regular, 2 = Preferencial): ");
            TipoAtencion tipo = Console.ReadLine()?.Trim() == "2"
                ? TipoAtencion.Preferencial
                : TipoAtencion.Regular;

            string ticket = $"TK-{correlativoTicket:D3}";
            correlativoTicket++;

            Cliente cliente = new Cliente(ticket, nombre, ops, tipo, minutoActual);
            Caja destino = SeleccionarCaja(cajas, tipo);

            if (destino == null)
            {
                Console.WriteLine($"No hay cajas habilitadas disponibles para {cliente.NumeroTicket}.");
                return;
            }

            destino.EncolarCliente(cliente);
            destino.IniciarSiguienteSiCorresponde(minutoActual);

            Console.WriteLine($"\nRegistrado {cliente.NumeroTicket} ({cliente.Nombre}, {cliente.Tipo}, {cliente.TiempoEstimadoMinutos} min) -> Caja {destino.Numero}.");
        }

        static Caja SeleccionarCaja(List<Caja> cajas, TipoAtencion tipo)
        {
            Caja caja1 = cajas[0];
            Caja caja2 = cajas[1];
            Caja caja3 = cajas[2];

            if (tipo == TipoAtencion.Preferencial)
            {
                if (caja3.EstaHabilitada)
                    return caja3;

                return cajas
                    .Where(c => c.EstaHabilitada)
                    .OrderBy(c => c.TiempoPendienteTotalMinutos())
                    .ThenBy(c => c.Numero)
                    .FirstOrDefault();
            }

            // Regular: Cajas 1 y 2; Caja 3 si está habilitada y no tiene preferenciales pendientes
            List<Caja> candidatas = new List<Caja>();
            if (caja1.EstaHabilitada) candidatas.Add(caja1);
            if (caja2.EstaHabilitada) candidatas.Add(caja2);
            if (caja3.EstaHabilitada && !caja3.TienePreferencialesEnEspera()) candidatas.Add(caja3);

            if (candidatas.Count == 0)
            {
                candidatas = cajas.Where(c => c.EstaHabilitada).ToList();
            }

            return candidatas
                .OrderBy(c => c.TiempoPendienteTotalMinutos())
                .ThenBy(c => c.Numero)
                .FirstOrDefault();
        }

        // b. Mostrar clientes pendientes en cada caja
        static void MostrarClientesPendientes(List<Caja> cajas, int minutoActual)
        {
            Console.WriteLine($"\n--- CLIENTES PENDIENTES (Minuto {minutoActual}) ---");
            foreach (Caja caja in cajas)
            {
                Console.WriteLine($"\nCaja {caja.Numero} [{caja.Estado}] | Pendiente total: {caja.TiempoPendienteTotalMinutos()} min");
                Console.WriteLine("  Atendiendo: " + (caja.ClienteActual != null ? caja.ClienteActual.ToString() : "Ninguno"));

                if (caja.ColaEspera.Count == 0)
                {
                    Console.WriteLine("  En cola: (vacía)");
                }
                else
                {
                    int pos = 1;
                    foreach (Cliente cli in caja.ColaEspera)
                    {
                        Console.WriteLine($"  {pos}. {cli}");
                        pos++;
                    }
                }
            }
        }

        // c. Mostrar siguiente cliente sin retirarlo de la cola
        static void MostrarSiguienteCliente(List<Caja> cajas, int minutoActual)
        {
            Console.WriteLine($"\n--- SIGUIENTE CLIENTE EN COLA (Minuto {minutoActual}) ---");
            foreach (Caja caja in cajas)
            {
                Cliente siguiente = caja.VerSiguienteCliente();
                if (siguiente != null)
                {
                    Console.WriteLine($"Caja {caja.Numero} [{caja.Estado}]: {siguiente.NumeroTicket} - {siguiente.Nombre} ({siguiente.Tipo}, {siguiente.TiempoRestanteMinutos} min restantes)");
                }
                else
                {
                    Console.WriteLine($"Caja {caja.Numero} [{caja.Estado}]: Sin clientes en cola.");
                }
            }
        }

        // d. y e. Simular avance de 1 minuto y retirar finalizados
        static void AvanzarUnMinuto(List<Caja> cajas, ref int minutoActual, bool retirarAutomaticamente)
        {
            minutoActual += 1;
            Console.WriteLine($"\n--- AVANCE A MINUTO {minutoActual} ---");

            foreach (Caja caja in cajas)
            {
                caja.IniciarSiguienteSiCorresponde(minutoActual);
                caja.AvanzarMinuto();

                if (retirarAutomaticamente || (caja.ClienteActual != null && caja.ClienteActual.HaFinalizado))
                {
                    Cliente retirado = caja.RetirarSiFinalizo(minutoActual);
                    if (retirado != null)
                    {
                        Console.WriteLine($"[RETIRADO] {retirado.NumeroTicket} ({retirado.Nombre}) finalizó en Caja {caja.Numero}.");
                        caja.IniciarSiguienteSiCorresponde(minutoActual);
                    }
                }
            }
        }

        // f. Cerrar temporalmente una caja y redistribuir clientes en espera
        static void CerrarTemporalmenteCaja(List<Caja> cajas, int minutoActual)
        {
            Console.Write("Número de caja a cerrar temporalmente (1-3): ");
            if (!int.TryParse(Console.ReadLine(), out int numCaja))
            {
                Console.WriteLine("Número inválido.");
                return;
            }

            Caja caja = cajas.FirstOrDefault(c => c.Numero == numCaja);
            if (caja == null)
            {
                Console.WriteLine("La caja no existe.");
                return;
            }

            if (caja.Estado == EstadoCaja.Cerrada)
            {
                Console.WriteLine($"La caja {numCaja} ya está cerrada.");
                return;
            }

            if (caja.ClienteActual != null && !caja.ClienteActual.HaFinalizado)
            {
                caja.Estado = EstadoCaja.Cerrando;
                Console.WriteLine($"Caja {numCaja} en estado CERRANDO: {caja.ClienteActual.NumeroTicket} ({caja.ClienteActual.Nombre}) terminará sus {caja.ClienteActual.TiempoRestanteMinutos} min restantes antes del cierre.");
            }
            else
            {
                caja.Estado = EstadoCaja.Cerrada;
                Console.WriteLine($"Caja {numCaja} cerrada temporalmente.");
            }

            List<Cliente> enEspera = new List<Cliente>();
            while (caja.ColaEspera.Count > 0)
            {
                enEspera.Add(caja.ColaEspera.Dequeue());
            }

            List<Cliente> redistribuir = enEspera
                .OrderBy(c => c.Tipo == TipoAtencion.Preferencial ? 0 : 1)
                .ToList();

            foreach (Cliente cli in redistribuir)
            {
                Caja destino = SeleccionarCaja(cajas, cli.Tipo);
                if (destino != null)
                {
                    destino.EncolarCliente(cli);
                    destino.IniciarSiguienteSiCorresponde(minutoActual);
                    Console.WriteLine($"  -> {cli.NumeroTicket} ({cli.Nombre}, {cli.Tipo}) redistribuido a Caja {destino.Numero}.");
                }
                else
                {
                    caja.EncolarCliente(cli);
                    Console.WriteLine($"  -> Sin cajas habilitadas para reasignar a {cli.NumeroTicket}.");
                }
            }
        }

        // g. Habilitar nuevamente una caja
        static void HabilitarCaja(List<Caja> cajas, int minutoActual)
        {
            Console.Write("Número de caja a habilitar (1-3): ");
            if (!int.TryParse(Console.ReadLine(), out int numCaja))
            {
                Console.WriteLine("Número inválido.");
                return;
            }

            Caja caja = cajas.FirstOrDefault(c => c.Numero == numCaja);
            if (caja == null)
            {
                Console.WriteLine("La caja no existe.");
                return;
            }

            caja.Estado = EstadoCaja.Habilitada;
            caja.IniciarSiguienteSiCorresponde(minutoActual);
            Console.WriteLine($"Caja {numCaja} habilitada nuevamente.");
        }

        // h. Estadísticas por caja
        static void MostrarEstadisticas(List<Caja> cajas)
        {
            Console.WriteLine("\n--- ESTADÍSTICAS POR CAJA ---");
            foreach (Caja caja in cajas)
            {
                Console.WriteLine(
                    $"Caja {caja.Numero} [{caja.Estado}] | Atendidos: {caja.ClientesAtendidos} | " +
                    $"Tiempo prom. espera: {caja.TiempoPromedioEspera():F2} min | Operaciones: {caja.OperacionesRealizadas}"
                );
            }
        }

        // i. Caja con mayor cantidad de operaciones
        static void MostrarCajaMayorOperaciones(List<Caja> cajas)
        {
            Console.WriteLine("\n--- CAJA CON MAYOR CANTIDAD DE OPERACIONES ---");
            int maxOps = cajas.Max(c => c.OperacionesRealizadas);

            if (maxOps == 0)
            {
                Console.WriteLine("Aún no se han realizado operaciones.");
                return;
            }

            List<Caja> lideres = cajas.Where(c => c.OperacionesRealizadas == maxOps).ToList();
            if (lideres.Count == 1)
            {
                Console.WriteLine($"Caja {lideres[0].Numero} con {lideres[0].OperacionesRealizadas} operaciones realizadas.");
            }
            else
            {
                string nombres = string.Join(", ", lideres.Select(c => $"Caja {c.Numero}"));
                Console.WriteLine($"Empate con {maxOps} operaciones entre: {nombres}.");
            }
        }
    }
}
