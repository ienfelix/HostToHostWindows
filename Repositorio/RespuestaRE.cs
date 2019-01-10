﻿using Comun;
using Modelo;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Repositorio
{
    public class RespuestaRE
    {
        private Bitacora _bitacora = null;
        private String _conexion = String.Empty;
        private SqlConnection _con = null;
        private SqlCommand _cmd = null;
        private SqlDataReader _reader = null;

        public RespuestaRE()
        {
            _bitacora = _bitacora ?? new Bitacora();
            _conexion = ConfigurationManager.ConnectionStrings[Constante.CONEXION_DESARROLLO].ConnectionString;
        }

        public async Task<RespuestaMO> ProcesarRespuestaAsync(CancellationToken cancelToken, String nombreArchivo, String tramaRespuesta)
        {
            RespuestaMO respuestaMO = new RespuestaMO();
            try
            {
                using (_con = new SqlConnection(_conexion))
                {
                    using (_cmd = new SqlCommand(Constante.SPS_HTH_PROCESAR_RESPUESTA, _con))
                    {
                        _cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter par1 = _cmd.Parameters.Add(Constante.NOMBRE_ARCHIVO, System.Data.SqlDbType.NVarChar, Constante._100);
                        par1.Direction = System.Data.ParameterDirection.Input;
                        par1.Value = nombreArchivo;

                        SqlParameter par2 = _cmd.Parameters.Add(Constante.TRAMA_RESPUESTA, System.Data.SqlDbType.Xml);
                        par2.Direction = System.Data.ParameterDirection.Input;
                        par2.Value = tramaRespuesta;

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
                        String mensaje = respuestaMO.Codigo == Constante.CODIGO_OK ? Constante.MENSAJE_PROCESAR_RESPUESTA_ASYNC_OK : Constante.MENSAJE_PROCESAR_RESPUESTA_ASYNC_NO_OK;
                        await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_NOTIFICACION, Constante.PROYECTO_REPOSITORIO, Constante.CLASE_RESPUESTA_RE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, mensaje);
                    }
                }
            }
            catch (Exception e)
            {
                await _bitacora.RegistrarEventoAsync(cancelToken, Constante.BITACORA_ERROR, Constante.PROYECTO_REPOSITORIO, Constante.CLASE_RESPUESTA_RE, Constante.METODO_PROCESAR_RESPUESTA_ASYNC, Constante.MENSAJE_PROCESAR_RESPUESTA_ASYNC_NO_OK, e.Message);
                throw e;
            }
            return respuestaMO;
        }
    }
}