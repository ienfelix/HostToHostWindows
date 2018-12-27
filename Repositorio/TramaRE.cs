using Comun;
using Modelo;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Repositorio
{
    public class TramaRE
    {
        private Bitacora _bitacora = null;
        private String _conexion = String.Empty;
        private SqlConnection _con = null;
        private SqlCommand _cmd = null;
        private SqlDataReader _reader = null;

        public TramaRE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _conexion = ConfigurationManager.ConnectionStrings[Constante.CONEXION_DESARROLLO].ConnectionString;
        }

        public async Task<RespuestaMO> ProcesarTrama(CancellationToken cancelToken, TramaMO tramaMO, String tramaDetalle)
        {
            RespuestaMO respuestaMO = new RespuestaMO();
            try
            {
                using (_con = new SqlConnection(_conexion))
                {
                    using (_cmd = new SqlCommand(Constante.SPS_HTH_PROCESAR_TRAMA, _con))
                    {
                        _cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter par1 = _cmd.Parameters.Add(Constante.ID_BANCO, System.Data.SqlDbType.NChar, Constante._5);
                        par1.Direction = System.Data.ParameterDirection.Input;
                        par1.Value = tramaMO.IdBanco;

                        SqlParameter par2 = _cmd.Parameters.Add(Constante.USUARIO, System.Data.SqlDbType.NVarChar, Constante._20);
                        par2.Direction = System.Data.ParameterDirection.Input;
                        par2.Value = tramaMO.Usuario;

                        SqlParameter par3 = _cmd.Parameters.Add(Constante.TIPO_ORDEN, System.Data.SqlDbType.NChar, Constante._3);
                        par3.Direction = System.Data.ParameterDirection.Input;
                        par3.Value = tramaMO.TipoOrden;

                        SqlParameter par4 = _cmd.Parameters.Add(Constante.ID_SOCIEDAD, System.Data.SqlDbType.NChar, Constante._4);
                        par4.Direction = System.Data.ParameterDirection.Input;
                        par4.Value = tramaMO.IdSociedad;

                        SqlParameter par5 = _cmd.Parameters.Add(Constante.ID_SAP, System.Data.SqlDbType.NVarChar, Constante._10);
                        par5.Direction = System.Data.ParameterDirection.Input;
                        par5.Value = tramaMO.IdSap;

                        SqlParameter par6 = _cmd.Parameters.Add(Constante.ANIO, System.Data.SqlDbType.NChar, Constante._4);
                        par6.Direction = System.Data.ParameterDirection.Input;
                        par6.Value = tramaMO.Anio;

                        SqlParameter par7 = _cmd.Parameters.Add(Constante.MOMENTO_ORDEN, System.Data.SqlDbType.NChar, Constante._8);
                        par7.Direction = System.Data.ParameterDirection.Input;
                        par7.Value = tramaMO.MomentoOrden;

                        SqlParameter par8 = _cmd.Parameters.Add(Constante.NOMBRE_ARCHIVO, System.Data.SqlDbType.NVarChar, Constante._100);
                        par8.Direction = System.Data.ParameterDirection.Input;
                        par8.Value = tramaMO.NombreArchivo;

                        SqlParameter par9 = _cmd.Parameters.Add(Constante.RUTA_ARCHIVO, System.Data.SqlDbType.NVarChar, Constante._200);
                        par9.Direction = System.Data.ParameterDirection.Input;
                        par9.Value = tramaMO.RutaArchivo;

                        SqlParameter par10 = _cmd.Parameters.Add(Constante.PARAMETROS, System.Data.SqlDbType.NVarChar, Constante._100);
                        par10.Direction = System.Data.ParameterDirection.Input;
                        par10.Value = tramaMO.Parametros;

                        SqlParameter par11 = _cmd.Parameters.Add(Constante.TRAMA_DETALLE, System.Data.SqlDbType.Xml);
                        par11.Direction = System.Data.ParameterDirection.Input;
                        par11.Value = tramaDetalle;

                        _con.Open();
                        _reader = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                        if (_reader != null && _reader.HasRows)
                        {
                            if (_reader.Read())
                            {
                                respuestaMO.Codigo = _reader.IsDBNull(Constante._0) ? 0 : _reader.GetInt32(Constante._0);
                                respuestaMO.Mensaje = _reader.IsDBNull(Constante._1) ? String.Empty : _reader.GetString(Constante._1);
                            }
                        }

                        _reader.Close();
                        _con.Close();
                        String mensaje = respuestaMO.Codigo == Constante.CODIGO_OK ? Constante.MENSAJE_PROCESAR_TRAMA_OK : Constante.MENSAJE_PROCESAR_TRAMA_NO_OK;
                        await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_REPOSITORIO, Constante.CLASE_TRAMA_RE, Constante.METODO_PROCESAR_TRAMA, mensaje);
                    }
                }
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_REPOSITORIO, Constante.CLASE_TRAMA_RE, Constante.METODO_PROCESAR_TRAMA, Constante.MENSAJE_PROCESAR_TRAMA_NO_OK, e.Message);
                throw e;
            }
            return respuestaMO;
        }
    }
}
