using Comun;
using Modelo;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class RespuestaNE
    {
        public static Boolean esProcesado = false;
        private Bitacora _bitacora = null;
        private Util _util = null;
        private RespuestaRE _respuestaRE = null;
        private String _carpetaRemota = String.Empty, _carpetaDescargado = String.Empty, _carpetaProcesado = String.Empty, _carpetaRespaldo = String.Empty;
        private ConexionSapNE _conexionSapNE = null;

        public RespuestaNE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _util = _util ?? new Util();
            _respuestaRE = _respuestaRE ?? new RespuestaRE();
            RespuestaNE.esProcesado = true;
            _carpetaRemota = ConfigurationManager.AppSettings[Constante.SERVIDOR_SFTP_CARPETA_OUT] ?? String.Empty;
            _carpetaDescargado = ConfigurationManager.AppSettings[Constante.CARPETA_DESCARGADO] ?? String.Empty;
            _carpetaProcesado = ConfigurationManager.AppSettings[Constante.CARPETA_PROCESADO] ?? String.Empty;
            _carpetaRespaldo = ConfigurationManager.AppSettings[Constante.CARPETA_RESPALDO] ?? String.Empty;
            _conexionSapNE = _conexionSapNE ?? new ConexionSapNE();
        }

        public async Task<RespuestaMO> ProcesarRespuesta(CancellationToken cancelToken)
        {
            RespuestaMO respuestaMO = new RespuestaMO();
            Int32 contador = 0;
            Dictionary<String, String> listaNombreArchivos = null;
            String arguments = String.Empty, error = String.Empty, message = String.Empty;
            Boolean puedeContinuar = false;
            try
            {
                String servidor = ConfigurationManager.AppSettings[Constante.SERVIDOR_SFTP_IP] ?? String.Empty;
                Int32 puerto = ConfigurationManager.AppSettings[Constante.SERVIDOR_SFTP_PUERTO] == String.Empty ? 0 : Convert.ToInt32(ConfigurationManager.AppSettings[Constante.SERVIDOR_SFTP_PUERTO]);
                String usuario = ConfigurationManager.AppSettings[Constante.SERVIDOR_SFTP_USUARIO] ?? String.Empty;
                String clave = ConfigurationManager.AppSettings[Constante.SERVIDOR_SFTP_CLAVE] ?? String.Empty;
                
                PasswordAuthenticationMethod authentication = new PasswordAuthenticationMethod(usuario, clave);
                ConnectionInfo connection = new ConnectionInfo(servidor, puerto, usuario, authentication);

                using (SftpClient sftpClient = new SftpClient(connection))
                {
                    sftpClient.Connect();
                    Boolean isConnected = sftpClient.IsConnected;

                    if (isConnected)
                    {
                        IEnumerable<SftpFile> listaArchivos = sftpClient.ListDirectory(_carpetaRemota);

                        if (listaArchivos == null || !listaArchivos.Any())
                        {
                            String MENSAJE_CARPETA_ORIGEN_VACIA = String.Format("{0} | {1}", Constante.MENSAJE_CARPETA_ORIGEN_VACIA, _carpetaRemota);
                            await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_RESPUESTA_NE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, MENSAJE_CARPETA_ORIGEN_VACIA);
                            respuestaMO.Codigo = Constante.CODIGO_OK;
                            respuestaMO.Mensaje = MENSAJE_CARPETA_ORIGEN_VACIA;
                        }
                        else
                        {
                            puedeContinuar = true;
                            listaNombreArchivos = new Dictionary<String, String>();

                            foreach (SftpFile archivo in listaArchivos)
                            {
                                if (archivo.Name != Constante.SINGLE_DOT && archivo.Name != Constante.DOUBLE_DOT)
                                {
                                    if (archivo.IsRegularFile)
                                    {
                                        String rutaRemota = String.Format("{0}{1}", _carpetaRemota, archivo.Name);
                                        String rutaArchivo = String.Format("{0}{1}", _carpetaDescargado, archivo.Name);

                                        using (FileStream fileStream = File.Create(rutaArchivo))
                                        {
                                            sftpClient.DownloadFile(rutaRemota, fileStream);
                                        }

                                        Boolean esDescargado = File.Exists(rutaArchivo);

                                        if (esDescargado)
                                        {
                                            sftpClient.DeleteFile(rutaRemota);
                                            String nombreArchivo = archivo.Name.Replace(Constante.EXTENSION_PGP, Constante.EXTENSION_TXT);
                                            listaNombreArchivos.Add(nombreArchivo, rutaArchivo);
                                            contador++;
                                        }

                                        String mensaje = esDescargado == true ? Constante.MENSAJE_DESCARGAR_ARCHIVOS_ASYNC_OK : Constante.MENSAJE_DESCARGAR_ARCHIVOS_ASYNC_NO_OK;
                                        mensaje = String.Format("{0} | {1}", mensaje, archivo.Name);
                                        await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_RESPUESTA_NE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, mensaje);
                                    }
                                }
                            }

                            if (contador != listaArchivos.Count() - Constante._2)
                            {
                                String MENSAJE_CARPETA_ORIGEN_VACIA = String.Format("{0} | {1}", Constante.MENSAJE_CARPETA_ORIGEN_VACIA, _carpetaRemota);
                                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_RESPUESTA_NE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, MENSAJE_CARPETA_ORIGEN_VACIA);
                                respuestaMO.Codigo = Constante.CODIGO_OK;
                                respuestaMO.Mensaje = MENSAJE_CARPETA_ORIGEN_VACIA;
                            }
                        }
                    }

                    sftpClient.Disconnect();
                    sftpClient.Dispose();
                }

                if (puedeContinuar == false)
                {
                    RespuestaNE.esProcesado = true;
                }
                else if (puedeContinuar == true && listaNombreArchivos != null && listaNombreArchivos.Count == 0)
                {
                    RespuestaNE.esProcesado = true;
                }
                else if (puedeContinuar == true && listaNombreArchivos != null && listaNombreArchivos.Count > 0)
                {
                    Boolean esConforme = false, esDesencriptado = false, esAlmacenado = false;
                    contador = 0;
                    String carpetaProcesado = _carpetaProcesado.Substring(Constante._0, _carpetaProcesado.LastIndexOf(Constante.BACK_SLASH));

                    foreach (var item in listaNombreArchivos)
                    {
                        String nombreArchivo = item.Key;
                        String rutaArchivo = item.Value;
                        String homeDirectory = string.Format("\"{0}\"", Constante.PGP_DIRECTORY);
                        arguments = String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", Constante.PGP_VERBOSE, Constante.PGP_HOME_DIRECTORY, homeDirectory, Constante.PGP_DECRYPT, rutaArchivo, Constante.PGP_PASSPHRASE_DEV, Constante.PGP_OUTPUT, carpetaProcesado, Constante.PGP_OVERWRITE);
                        Process process = new Process();
                        process.StartInfo.FileName = Constante.PGP_EXE;
                        process.StartInfo.Arguments = arguments;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.Start();
                        message = process.StandardOutput.ReadToEnd();
                        error = process.StandardError.ReadToEnd();
                        process.WaitForExit(Timeout.Infinite);

                        rutaArchivo = String.Format("{0}{1}", _carpetaProcesado, nombreArchivo.Replace(Constante.EXTENSION_TXT, String.Empty));

                        if (File.Exists(rutaArchivo))
                        {
                            esDesencriptado = true;
                            StringBuilder stringBuilder = await _util.ConvertirCadenaAXml(cancelToken, rutaArchivo);

                            if (stringBuilder.ToString() != String.Empty)
                            {
                                respuestaMO = await _respuestaRE.ProcesarRespuestaAsync(cancelToken, nombreArchivo, stringBuilder.ToString());

                                if (respuestaMO != null && respuestaMO.Codigo == Constante.CODIGO_OK)
                                {
                                    String rutaRespaldo = String.Format("{0}{1}", _carpetaRespaldo, nombreArchivo);
                                    File.Move(rutaArchivo, rutaRespaldo);
                                    esAlmacenado = true;
                                    esConforme = true;
                                    await _conexionSapNE.EnviarEstadoProcesoHostToHostAsync(cancelToken, respuestaMO.IdSociedad, respuestaMO.Anio, respuestaMO.MomentoOrden, respuestaMO.IdEstadoOrden, respuestaMO.IdSap, respuestaMO.Usuario);
                                }

                                String mensajeDesencriptado = esDesencriptado ? Constante.MENSAJE_DESENCRIPTAR_ARCHIVO_ASYNC_OK : Constante.MENSAJE_DESENCRIPTAR_ARCHIVO_ASYNC_NO_OK;
                                String mensajeAlmacenado = esAlmacenado ? Constante.MENSAJE_ALMACENAR_ARCHIVO_ASYNC_OK : Constante.MENSAJE_ALMACENAR_ARCHIVO_ASYNC_NO_OK;
                                String mensaje = esConforme ? String.Format("{0} {1}", Constante.MENSAJE_DESENCRIPTAR_ARCHIVO_ASYNC_OK, Constante.MENSAJE_ALMACENAR_ARCHIVO_ASYNC_OK) : String.Format("{0} | {1} | {2} | {3} | {4}", mensajeDesencriptado, mensajeAlmacenado, arguments, message, error);
                                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_NEGOCIO, Constante.CLASE_RESPUESTA_NE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, mensaje);
                            }
                        }

                        contador++;
                    }

                    if (contador == listaNombreArchivos.Count())
                    {
                        RespuestaNE.esProcesado = true;
                    }
                }
            }
            catch (Exception e)
            {
                RespuestaNE.esProcesado = true;
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_NEGOCIO, Constante.CLASE_RESPUESTA_NE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, Constante.MENSAJE_PROCESAR_RESPUESTA_ASYNC_NO_OK, e.Message);
                respuestaMO.Codigo = Constante.CODIGO_ERROR;
                respuestaMO.Mensaje = e.Message;
            }
            return respuestaMO;
        }
    }
}