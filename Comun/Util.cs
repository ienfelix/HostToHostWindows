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

        private Boolean CrearCarpetasLocales(CancellationToken cancelToken)
        {
            Boolean isCreated = false;
            try
            {
                String carpetaCorrecto = ConfigurationManager.AppSettings[Constante.CARPETA_CORRECTO] ?? String.Empty;
                String carpetaIncorrecto = ConfigurationManager.AppSettings[Constante.CARPETA_INCORRECTO] ?? String.Empty;
                String carpetaEncriptado = ConfigurationManager.AppSettings[Constante.CARPETA_ENCRIPTADO] ?? String.Empty;
                String carpetaDescargado = ConfigurationManager.AppSettings[Constante.CARPETA_DESCARGADO] ?? String.Empty;
                String carpetaProcesado = ConfigurationManager.AppSettings[Constante.CARPETA_PROCESADO] ?? String.Empty;

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
                if (!Directory.Exists(carpetaDescargado))
                {
                    Directory.CreateDirectory(carpetaDescargado);
                }
                if (!Directory.Exists(carpetaProcesado))
                {
                    Directory.CreateDirectory(carpetaProcesado);
                }

                isCreated = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            return isCreated;
        }

        public String ValidarNombreArchivo(String rutaArchivo, out String nombreArchivo)
        {
            String mensaje = String.Empty;
            try
            {
                String nombreArchivoConExtension = rutaArchivo.Substring(rutaArchivo.LastIndexOf(Constante.BACK_SLASH) + 1);
                String nombreArchivoSinExtension = nombreArchivoConExtension.Split(Constante.DOT)[Constante._0];
                String parametros = nombreArchivoSinExtension.Substring(nombreArchivoSinExtension.IndexOf(Constante.AMPERSON) + 1);
                String extension = String.Format("{0}{1}", Constante.DOT, rutaArchivo.Split(Constante.DOT)[Constante._1]);
                nombreArchivoConExtension = String.Format("{0}{1}", nombreArchivoSinExtension.Split(Constante.AMPERSON)[Constante._0], Constante.EXTENSION_TXT);
                nombreArchivo = nombreArchivoConExtension;
                var arregloParametros = parametros.Split(Constante.AMPERSON);

                if (nombreArchivoConExtension.Length != Constante._15)
                {
                    mensaje += String.Format("{0}{1}", Constante.MENSAJE_LONGITUD_CARACTERES_ARCHIVO_INVALIDO, Environment.NewLine);
                }
                if (extension.ToLower() != Constante.EXTENSION_TXT)
                {
                    mensaje += String.Format("{0}{1}", Constante.MENSAJE_EXTENSION_ARCHIVO_INVALIDO, Environment.NewLine);
                }
                if (parametros == String.Empty)
                {
                    mensaje += String.Format("{0}{1}", Constante.MENSAJE_PARAMETROS_NO_PRESENTES, Environment.NewLine);
                }
                if (arregloParametros.Length != Constante._8)
                {
                    mensaje += String.Format("{0}{1}", Constante.MENSAJE_CANTIDAD_PARAMETROS_INVALIDO, Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return mensaje;
        }

        public async Task<String> ConvertirCadenaHaciaXml(CancellationToken cancelToken, String rutaArchivo, String nombreArchivo)
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

                String mensaje = stringBuilder.ToString() != String.Empty ? Constante.MENSAJE_CONVERTIR_CADENA_HACIA_XML_OK : Constante.MENSAJE_CONVERTIR_CADENA_HACIA_XML_NO_OK;
                mensaje = String.Format("{0} | {1}", mensaje, rutaArchivo);
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_CONVERTIR_CADENA_HACIA_XML, nombreArchivo, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_CONVERTIR_CADENA_HACIA_XML, nombreArchivo, Constante.MENSAJE_CONVERTIR_CADENA_HACIA_XML_NO_OK, e.Message);
                throw e;
            }
            return stringBuilder.ToString();
        }

        public async Task<Boolean> MoverArchivos(CancellationToken cancelToken, String rutaOrigen, String carpetaDestino, String nombreArchivo)
        {
            Boolean isMoved = false;
            try
            {
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
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_MOVER_ARCHIVOS, nombreArchivo, mensaje);
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_COMUN, Constante.CLASE_UTIL, Constante.METODO_MOVER_ARCHIVOS, nombreArchivo, Constante.MENSAJE_MOVER_ARCHIVOS_NO_OK, e.Message);
                throw e;
            }
            return isMoved;
        }
    }
}
