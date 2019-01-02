using Comun;
using Modelo;
using Negocio;
using System;
using System.ServiceProcess;
using System.Threading;

namespace HostToHostWindows
{
    public partial class ServiceHostToHost : ServiceBase
    {
        private Timer _timerTrama = null;
        private Bitacora _bitacora = null;
        private TramaNE _tramaNE = null;
        private CancellationToken _cancelToken;

        public ServiceHostToHost()
        {
            InitializeComponent();
        }

        protected async override void OnStart(string[] args)
        {
            try
            {
                _bitacora = _bitacora ?? new Bitacora();
                _tramaNE = _tramaNE ?? new TramaNE();
                _cancelToken = new CancellationToken(false);
                _timerTrama = new Timer(OnTimerTramaAsync, null, Constante._0, Constante.TIEMPO_UN_MINUTO);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(_cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_START, Constante.MENSAJE_ON_TIMER_TRAMA_NO_OK, e.Message);
            }
        }

        protected async override void OnStop()
        {
            try
            {

            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(_cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_STOP, Constante.MENSAJE_ON_TIMER_TRAMA_NO_OK, e.Message);
            }
        }

        private async void OnTimerTramaAsync(object sender)
        {
            try
            {
                if (TramaNE.esProcesado)
                {
                    TramaNE.esProcesado = false;
                    RespuestaMO respuestaMO = await _tramaNE.ProcesarTrama(_cancelToken);
                    await _bitacora.RegistrarEventoAsync(_cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_TIMER_TRAMA, respuestaMO.Mensaje);
                    await _bitacora.RegistrarEventoAsync(_cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_TIMER_TRAMA, Constante.ASTERIK);
                }
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(_cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_SERVICE, Constante.CLASE_SERVICE_HOST_TO_HOST, Constante.METODO_ON_TIMER_TRAMA, Constante.MENSAJE_ON_TIMER_TRAMA_NO_OK, e.Message);
            }
        }
    }
}
