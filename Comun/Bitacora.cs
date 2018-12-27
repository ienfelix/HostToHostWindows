using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Comun
{
    public class Bitacora
    {
        public void RegistrarEvento(String tipo, String proyecto, String clase, String metodo, String mensaje)
        {
            String rutaArchivo = String.Empty;
            try
            {
                String carpetaBitacora = ConfigurationManager.AppSettings[Constante.CARPETA_BITACORA] ?? String.Empty;
                String fecha = DateTime.Now.ToString("yyyyMMdd");
                String nombreArchivo = String.Format("{0}_{1}{2}", Constante.NOMBRE_BITACORA, fecha, Constante.EXTENSION_TXT);
                rutaArchivo = String.Format("{0}{1}", carpetaBitacora, nombreArchivo);
                Boolean existeArchivo = ValidarExistenciaArchivo(rutaArchivo, nombreArchivo);

                if (!existeArchivo)
                {
                    if (!Directory.Exists(carpetaBitacora))
                    {
                        Directory.CreateDirectory(carpetaBitacora);
                    }

                    File.WriteAllText(rutaArchivo, Environment.NewLine);
                }

                fecha = String.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                String texto = String.Format("{0} {1} > {2} - {3} - {4} - {5}{6}", tipo, fecha, proyecto, clase, metodo, mensaje, Environment.NewLine);
                File.AppendAllText(rutaArchivo, texto);
            }
            catch (Exception ex)
            {
                File.WriteAllText(rutaArchivo, ex.Message);
                throw ex;
            }
        }

        public void RegistrarEvento(String tipo, String proyecto, String clase, String metodo, String mensaje, String excepcion)
        {
            String rutaArchivo = String.Empty;
            try
            {
                String carpetaBitacora = ConfigurationManager.AppSettings[Constante.CARPETA_BITACORA] ?? String.Empty;
                String fecha = DateTime.Now.ToString("yyyyMMdd");
                String nombreArchivo = String.Format("{0}_{1}{2}", Constante.NOMBRE_BITACORA, fecha, Constante.EXTENSION_TXT);
                rutaArchivo = String.Format("{0}{1}", carpetaBitacora, nombreArchivo);
                Boolean existeArchivo = ValidarExistenciaArchivo(rutaArchivo, nombreArchivo);

                if (!existeArchivo)
                {
                    if (!Directory.Exists(carpetaBitacora))
                    {
                        Directory.CreateDirectory(carpetaBitacora);
                    }

                    File.WriteAllText(rutaArchivo, Environment.NewLine);
                }

                fecha = String.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                String texto = String.Format("{0} {1} > {2} - {3} - {4} - {5} | {6}{7}", tipo, fecha, proyecto, clase, metodo, mensaje, excepcion, Environment.NewLine);
                File.AppendAllText(rutaArchivo, texto);
            }
            catch (Exception ex)
            {
                File.WriteAllText(rutaArchivo, ex.Message);
                throw ex;
            }
        }

        private Boolean ValidarExistenciaArchivo(String rutaBitacora, String nombreArchivo)
        {
            Boolean existeArchivo = false;
            try
            {
                String carpetaBitacora = ConfigurationManager.AppSettings[Constante.CARPETA_BITACORA] ?? String.Empty;

                if (Directory.Exists(carpetaBitacora))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(carpetaBitacora);
                    FileInfo[] fileInfo = directoryInfo.GetFiles(Constante.PATRON_TXT, SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime).ToArray();
                    String nombreArchivoActual = fileInfo[0].Name;
                    existeArchivo = nombreArchivo.Trim().ToLower() == nombreArchivoActual.Trim().ToLower() ? true : false;
                    fileInfo = null;
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(rutaBitacora, ex.Message);
                throw ex;
            }
            return existeArchivo;
        }
    }
}
