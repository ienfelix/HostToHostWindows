using Comun;
using Modelo;
using Repositorio;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;

namespace Negocio
{
    public class TramaNE
    {
        public static Boolean esProcesado = false;
        private Bitacora _bitacora = null;
        private Util _util = null;
        private TramaRE _tramaRE = null;

        public TramaNE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _util = _util ?? new Util();
            _tramaRE = _tramaRE ?? new TramaRE();
            esProcesado = true;
        }

        public RespuestaMO ProcesarTrama()
        {
            RespuestaMO respuestaMO = new RespuestaMO();
            Int32 contador = 0;
            String rutaOrigen = String.Empty;
            String nombreArchivo = String.Empty;
            try
            {
                String carpetaOrigen = ConfigurationManager.AppSettings[Constante.CARPETA_ORIGEN] ?? String.Empty;
                String[] listaArchivosPendientes = Directory.GetFiles(carpetaOrigen, Constante.PATRON_TXT, SearchOption.TopDirectoryOnly);
                if (listaArchivosPendientes.Length == Constante._0)
                {
                    String MENSAJE_CARPETA_ORIGEN_VACIA = String.Format("{0} | {1}", Constante.MENSAJE_CARPETA_ORIGEN_VACIA, carpetaOrigen);
                    _bitacora.RegistrarEvento(Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA, MENSAJE_CARPETA_ORIGEN_VACIA);
                    respuestaMO.Codigo = Constante.CODIGO_OK;
                    respuestaMO.Mensaje = MENSAJE_CARPETA_ORIGEN_VACIA;
                    esProcesado = true;
                }
                else
                {
                    foreach (String archivo in listaArchivosPendientes)
                    {
                        rutaOrigen = archivo;
                        String nombreArchivoConExtension = archivo.Substring(archivo.LastIndexOf(Constante.BACK_SLASH) + 1);
                        String nombreArchivoSinExtension = nombreArchivoConExtension.Split(Constante.DOT)[Constante._0];
                        String parametros = nombreArchivoSinExtension.Substring(nombreArchivoSinExtension.IndexOf(Constante.AMPERSON) + 1);
                        TramaMO tramaMO = MapearCadenaAModelo(nombreArchivoConExtension, archivo, Constante.CARPETA_CORRECTO, parametros);
                        nombreArchivo = tramaMO.NombreArchivo;
                        StringBuilder stringBuilder = _util.ConvertirCadenaAXml(archivo);

                        if (stringBuilder.ToString() != String.Empty)
                        {
                            respuestaMO = _tramaRE.ProcesarTrama(tramaMO, stringBuilder.ToString());
                            Boolean esMovido = false;

                            if (respuestaMO != null && respuestaMO.Codigo == Constante.CODIGO_OK)
                            {
                                esMovido = _util.MoverArchivos(archivo, Constante.CARPETA_CORRECTO, tramaMO.NombreArchivo);
                            }

                            String mensaje = esMovido == true ? Constante.MENSAJE_PROCESAR_TRAMA_OK : Constante.MENSAJE_PROCESAR_TRAMA_NO_OK;
                            mensaje = String.Format("{0} | {1}", mensaje, tramaMO.NombreArchivo);
                            _bitacora.RegistrarEvento(Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA, mensaje);
                        }

                        contador++;
                    }

                    if (contador == listaArchivosPendientes.Length)
                    {
                        esProcesado = true;
                    }
                }
            }
            catch (Exception e)
            {
                esProcesado = true;
                Boolean esMovido = _util.MoverArchivos(rutaOrigen, Constante.CARPETA_INCORRECTO, nombreArchivo);
                String mensaje = e.Message;
                mensaje = String.Format("{0} | {1}", mensaje, nombreArchivo);
                _bitacora.RegistrarEvento(Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_PROCESAR_TRAMA, Constante.MENSAJE_PROCESAR_TRAMA_NO_OK, mensaje);
                respuestaMO.Codigo = Constante.CODIGO_ERROR;
                respuestaMO.Mensaje = mensaje;
                throw e;
            }
            return respuestaMO;
        }

        private TramaMO MapearCadenaAModelo(String nombreArchivo, String rutaArchivo, String carpeta, String cadenaParametros)
        {
            TramaMO tramaMO = new TramaMO();
            try
            {
                String carpetaDestino = ConfigurationManager.AppSettings[carpeta];
                String[] parametros = cadenaParametros.Split(Constante.AMPERSON);
                tramaMO.IdBanco = parametros[Constante._0];
                tramaMO.Usuario = parametros[Constante._1];
                tramaMO.TipoOrden = parametros[Constante._2];
                tramaMO.IdSociedad = parametros[Constante._3];
                tramaMO.IdSap = parametros[Constante._4];
                tramaMO.Anio = parametros[Constante._5];
                tramaMO.MomentoOrden = parametros[Constante._6];
                tramaMO.NombreArchivo = String.Format("{0}{1}", nombreArchivo.Split(Constante.AMPERSON)[Constante._0], Constante.EXTENSION_TXT);
                tramaMO.RutaArchivo = String.Format("{0}{1}", carpetaDestino, tramaMO.NombreArchivo);
                tramaMO.Parametros = cadenaParametros;

                String mensaje = cadenaParametros != String.Empty ? Constante.MENSAJE_MAPEAR_CADENA_A_MODELO_OK : Constante.MENSAJE_MAPEAR_CADENA_A_MODELO_NO_OK;
                mensaje = String.Format("{0} | {1}", mensaje, cadenaParametros);
                _bitacora.RegistrarEvento(Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_MAPEAR_CADENA_A_MODELO, mensaje);
            }
            catch (Exception e)
            {
                _bitacora.RegistrarEvento(Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_TRAMA_NE, Constante.METODO_MAPEAR_CADENA_A_MODELO, Constante.MENSAJE_MAPEAR_CADENA_A_MODELO_NO_OK, e.Message);
                throw e;
            }
            return tramaMO;
        }
    }
}