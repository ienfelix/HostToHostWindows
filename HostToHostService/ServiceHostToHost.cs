using Comun;
using Modelo;
using Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace HostToHostService
{
    public partial class ServiceHostToHost : ServiceBase
    {
        private Timer _timerTrama = null;
        private Bitacora _bitacora = null;
        private TramaNE _tramaNE = null;

        public ServiceHostToHost()
        {
            InitializeComponent();
            _timerTrama = _timerTrama ?? new Timer();
            _bitacora = _bitacora ?? new Bitacora();
            _tramaNE = _tramaNE ?? new TramaNE();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _bitacora = _bitacora ?? new Bitacora();
                _tramaNE = _tramaNE ?? new TramaNE();
                _timerTrama.Elapsed += new ElapsedEventHandler(this.OnTimerTrama);
                _timerTrama.Interval = Constante.TIEMPO_UN_MINUTO;
                _timerTrama.Start();
            }
            catch (Exception e)
            {
                _bitacora.RegistrarEvento(Constante.BITACORA_ERROR, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_START, Constante.MENSAJE_ON_TIMER_TRAMA_NO_OK, e.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {

            }
            catch (Exception e)
            {
                _bitacora.RegistrarEvento(Constante.BITACORA_ERROR, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_STOP, Constante.MENSAJE_ON_TIMER_TRAMA_NO_OK, e.Message);
            }
        }

        public void OnTimerTrama(object sender, ElapsedEventArgs args)
        {
            try
            {
                if (TramaNE.esProcesado)
                {
                    TramaNE.esProcesado = false;
                    RespuestaMO respuestaMO = _tramaNE.ProcesarTrama();
                    _bitacora.RegistrarEvento(Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_TIMER_TRAMA, respuestaMO.Mensaje);
                    _bitacora.RegistrarEvento(Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_TIMER_TRAMA, Constante.ASTERIK);
                }
            }
            catch (Exception e)
            {
                _bitacora.RegistrarEvento(Constante.BITACORA_ERROR, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_TIMER_TRAMA, Constante.MENSAJE_ON_TIMER_TRAMA_NO_OK, e.Message);
            }

        }
    }
}
