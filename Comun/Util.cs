using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Comun
{
    public class Util
    {
        private Bitacora _bitacora = null;

        public Util()
        {
            _bitacora = _bitacora ?? new Bitacora();
            var isCreated = CrearCarpetasLocales(new CancellationToken(false));
        }

        private async Task<Boolean> CrearCarpetasLocales(CancellationToken cancelToken)
        {
            Boolean isCreated = false;
            try
            {
                String carpetaCorrecto = ConfigurationManager.AppSettings[Constante.CARPETA_CORRECTO] ?? String.Empty;
                String carpetaIncorrecto = ConfigurationManager.AppSettings[Constante.CARPETA_INCORRECTO] ?? String.Empty;
                String carpetaEncriptado = ConfigurationManager.AppSettings[Constante.CARPETA_ENCRIPTADO] ?? String.Empty;
                String carpetaDesencriptado = ConfigurationManager.AppSettings[Constante.CARPETA_DESENCRIPTADO] ?? String.Empty;

                if (!Directory.Exists(carpetaCorrecto))
                {
                    Directory.CreateDirectory(carpetaCorrecto);
                }
                if (!Directory.Exists(carpetaIncorrecto))
                {
                    Directory.CreateDirectory(carpetaIncorrecto);
                }
                if (!Directory.Exists(carpetaEncriptado))
                {
                    Directory.CreateDirectory(carpetaEncriptado);
                }
                if (!Directory.Exists(carpetaDesencriptado))
                {
                    Directory.CreateDirectory(carpetaDesencriptado);
                }

                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_CREAR_CARPETAS_LOCALES, Constante.MENSAJE_CREAR_CARPETAS_LOCALES_OK);
                isCreated = true;
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_CREAR_CARPETAS_LOCALES, Constante.MENSAJE_CREAR_CARPETAS_LOCALES_NO_OK, e.Message);
                throw e;
            }
            return isCreated;
        }

        public async Task<StringBuilder> ConvertirCadenaAXml(CancellationToken cancelToken, String rutaArchivo)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
                {
                    String[] lineas = File.ReadAllLines(rutaArchivo);
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement(Constante.TRAMAS);

                    for (int i = 0; i < lineas.Length - 1; i++)
                    {
                        Int32 id = i + 1;
                        xmlWriter.WriteStartElement(Constante.TRAMA);
                        xmlWriter.WriteElementString(Constante.ID, id.ToString());
                        xmlWriter.WriteElementString(Constante.CADENA, lineas[i]);
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                }

                String mensaje = stringBuilder.ToString() != String.Empty ? Constante.MENSAJE_CONVERTIR_CADENA_A_XML_OK : Constante.MENSAJE_CONVERTIR_CADENA_A_XML_NO_OK;
                mensaje = String.Format("{0} | {1}", mensaje, rutaArchivo);
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_CONVERTIR_CADENA_A_XML, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_CONVERTIR_CADENA_A_XML, Constante.MENSAJE_CONVERTIR_CADENA_A_XML_NO_OK, e.Message);
                throw e;
            }
            return stringBuilder;
        }

        public async Task<Boolean> MoverArchivos(CancellationToken cancelToken, String rutaOrigen, String carpeta, String nombreArchivo)
        {
            Boolean isMoved = false;
            try
            {
                String carpetaDestino = ConfigurationManager.AppSettings[carpeta];
                String rutaDestino = String.Format("{0}{1}", carpetaDestino, nombreArchivo);
                
                if (File.Exists(rutaOrigen))
                {
                    if (File.Exists(rutaDestino))
                    {
                        File.Delete(rutaDestino);
                    }

                    File.Move(rutaOrigen, rutaDestino);
                    isMoved = true;
                }

                String mensaje = isMoved == true ? Constante.MENSAJE_MOVER_ARCHIVOS_OK : Constante.MENSAJE_MOVER_ARCHIVOS_NO_OK;
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_MOVER_ARCHIVOS, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_MOVER_ARCHIVOS, Constante.MENSAJE_MOVER_ARCHIVOS_NO_OK, e.Message);
                throw e;
            }
            return isMoved;
        }
    }
}
