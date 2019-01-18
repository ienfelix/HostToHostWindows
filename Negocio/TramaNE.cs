using Comun;
using Modelo;
using Repositorio;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class TramaNE
    {
        public static Boolean esProcesado = false;
        private Bitacora _bitacora = null;
        private Util _util = null;
        private TramaRE _tramaRE = null;
        private String _carpetaOrigen = String.Empty, _carpetaCorrecto = String.Empty, _carpetaIncorrecto = String.Empty;

        public TramaNE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _util = _util ?? new Util();
            _tramaRE = _tramaRE ?? new TramaRE();
            TramaNE.esProcesado = true;
            _carpetaOrigen = ConfigurationManager.AppSettings[Constante.CARPETA_ORIGEN] ?? String.Empty;
            _carpetaCorrecto = ConfigurationManager.AppSettings[Constante.CARPETA_CORRECTO] ?? String.Empty;
            _carpetaIncorrecto = ConfigurationManager.AppSettings[Constante.CARPETA_INCORRECTO] ?? String.Empty;
        }

        public async Task<RespuestaMO> ProcesarTrama(CancellationToken cancelToken)
        {
            RespuestaMO respuestaMO = new RespuestaMO();
            Int32 contador = 0;
            String rutaOrigen = String.Empty, nombreArchivo = String.Empty;
            try
            {
                String[] listaArchivosPendientes = Directory.GetFiles(_carpetaOrigen, Constante.PATRON_TXT, SearchOption.TopDirectoryOnly);
                if (listaArchivosPendientes.Length == Constante._0)
                {
                    String mensaje = String.Format("{0} | {1}", Constante.MENSAJE_CARPETA_ORIGEN_VACIA, _carpetaOrigen);
                    await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA_ASYNC, String.Empty, mensaje);
                    respuestaMO.Codigo = Constante.CODIGO_OK;
                    respuestaMO.Mensaje = mensaje;
                    TramaNE.esProcesado = true;
                }
                else
                {
                    foreach (String archivo in listaArchivosPendientes)
                    {
                        rutaOrigen = archivo;
                        String mensajeValidacion = _util.ValidarNombreArchivo(archivo, out nombreArchivo);
                        respuestaMO.NombreArchivo = nombreArchivo;

                        if (mensajeValidacion != String.Empty)
                        {
                            await _util.MoverArchivos(cancelToken, archivo, _carpetaIncorrecto, nombreArchivo);
                            await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA_ASYNC, nombreArchivo, mensajeValidacion);
                        }
                        else
                        {
                            String nombreArchivoConExtension = archivo.Substring(archivo.LastIndexOf(Constante.BACK_SLASH) + 1);
                            String nombreArchivoSinExtension = nombreArchivoConExtension.Split(Constante.DOT)[Constante._0];
                            String parametros = nombreArchivoSinExtension.Substring(nombreArchivoSinExtension.IndexOf(Constante.AMPERSON) + 1);
                            nombreArchivoConExtension = String.Format("{0}{1}", nombreArchivoSinExtension.Split(Constante.AMPERSON)[Constante._0], Constante.EXTENSION_TXT);
                            TramaMO tramaMO = MapearCadenaHaciaModelo(nombreArchivoConExtension, archivo, parametros);
                            String cadenaXml = await _util.ConvertirCadenaHaciaXml(cancelToken, archivo, nombreArchivo);

                            if (cadenaXml != String.Empty)
                            {
                                RespuestaMO respuestaMO2 = await _tramaRE.ProcesarTramaAsync(cancelToken, tramaMO, cadenaXml, nombreArchivo);
                                Boolean esMovido = false;

                                if (respuestaMO2 != null && respuestaMO2.Codigo == Constante.CODIGO_OK)
                                {
                                    esMovido = await _util.MoverArchivos(cancelToken, archivo, _carpetaCorrecto, nombreArchivo);
                                }
                                else
                                {
                                    respuestaMO.Mensaje = respuestaMO2.Mensaje;
                                }

                                String mensaje = esMovido == true ? Constante.MENSAJE_PROCESAR_TRAMA_ASYNC_OK : Constante.MENSAJE_PROCESAR_TRAMA_ASYNC_NO_OK;
                                mensaje = String.Format("{0} | {1}", mensaje, tramaMO.NombreArchivo);
                                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA_ASYNC, nombreArchivo, mensaje);
                            }
                        }

                        contador++;
                    }

                    if (contador == listaArchivosPendientes.Length)
                    {
                        TramaNE.esProcesado = true;
                    }
                }
            }
            catch (Exception e)
            {
                TramaNE.esProcesado = true;
                await _util.MoverArchivos(cancelToken, rutaOrigen, _carpetaIncorrecto, nombreArchivo);
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA_ASYNC, nombreArchivo, Constante.MENSAJE_PROCESAR_TRAMA_ASYNC_NO_OK, e.Message);
                respuestaMO.Codigo = Constante.CODIGO_ERROR;
                respuestaMO.Mensaje = String.Format("{0} | {1}", nombreArchivo, e.Message);
            }
            return respuestaMO;
        }

        private TramaMO MapearCadenaHaciaModelo(String nombreArchivo, String rutaArchivo, String cadenaParametros)
        {
            TramaMO tramaMO = null;
            try
            {
                tramaMO = new TramaMO();
                String carpetaDestino = ConfigurationManager.AppSettings[Constante.CARPETA_CORRECTO];
                String[] parametros = cadenaParametros.Split(Constante.AMPERSON);
                tramaMO.IdBanco = parametros[Constante._0];
                tramaMO.Usuario = parametros[Constante._1];
                tramaMO.TipoOrden = parametros[Constante._2];
                tramaMO.IdSociedad = parametros[Constante._3];
                tramaMO.IdSap = parametros[Constante._4];
                tramaMO.Anio = parametros[Constante._5];
                tramaMO.MomentoOrden = parametros[Constante._6];
                tramaMO.Propietario = parametros[Constante._7];
                tramaMO.NombreArchivo = nombreArchivo;
                tramaMO.RutaArchivo = String.Format("{0}{1}", carpetaDestino, tramaMO.NombreArchivo);
                tramaMO.Parametros = cadenaParametros;
            }
            catch (Exception e)
            {
                throw e;
            }
            return tramaMO;
        }
    }
}