using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using ApiAppPay.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.HttpResults;
using ApiAppPay.Models.Responses;
using Microsoft.AspNetCore.Mvc;

public class AppService
{
    private readonly string _connectionString;
    private readonly IMemoryCache _cache;


    public AppService(IConfiguration configuration, IMemoryCache memoryCache)
    {
        _connectionString = configuration.GetConnectionString("sdms");
        _cache = memoryCache; // Inject memory cache
    }

    public async Task<Application> GetAppByIdAsync(long applicationId)
    {
        Application? application = null;

        try
        {
            // Check if the data is in cache first
            if (_cache.TryGetValue(applicationId, out application))
            {
                // Return from cache
                return application;
            }

            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();

                string applicationQuery = @"
                    SELECT 
                            T_APPLICATION.VID,
                            T_APPLICATION.ID,
                            T_APPLICATION.ID_APPLICATION_TYPE,
                            T_APPLICATION.ID_CURRENT_STATE,
                            T_APPLICATION.USE_CASE,
                            T_APPLICATION.LICENSE_NUMBER, 
                            T_APPLICATION.LICENSE_NUMBER_LATIN, 
                            T_APPLICATION.GOVERNORATE, 
                            T_APPLICATION.USAGE, 
                            T_APPLICATION.SEATS1, 
                            T_APPLICATION.VEHICLE_CATEGORY, 
                            T_APPLICATION.CYLINDERS, 
                            T_APPLICATION.AXIS, 
                            T_APPLICATION.CABIN_TYPE, 
                            T_APPLICATION.LOAD_WEIGHT
                        FROM 
                            SDMS_IRQDLVR.T_APPLICATION
                        WHERE 
                            T_APPLICATION.CREATED > :CREATED_DATE
                        AND 
                            T_APPLICATION.APPLICATION_ID = :APPLIC_ID
                         ";
                string nameQuery = @"SELECT
                                            T_APPLICANT.GIVEN_NAME, 
                                            T_APPLICANT.FATHER_NAME, 
                                            T_APPLICANT.GRANDFATHER_NAME, 
                                            T_APPLICANT.MOTHER_NAME, 
                                            T_APPLICANT.MOTHER_FATHERNAME, 
                                            T_APPLICANT.TPID
                                        FROM
                                            SDMS_IRQDLVR.T_APPLICATION_APPLICANT
                                        JOIN
                                            SDMS_IRQDLVR.T_APPLICANT
                                            ON T_APPLICATION_APPLICANT.ID_APPLICANT = T_APPLICANT.ID
                                        WHERE
                                            T_APPLICATION_APPLICANT.ID_APPLICATION = :APPLIC_ID";
                     
                string vcQuery = @"
                    SELECT 
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_ISSUE,
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_EXPIRY
                    FROM 
                        SDMS_IRQDLVR.T_DOCUMENT
                    INNER JOIN 
                        SDMS_IRQDLVR.T_APPLICATION
                        ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_DOCUMENT.ID_APPLICATION
                    INNER JOIN 
                        SDMS_IRQDLVR.T_APPLICATION_APPLICANT
                        ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICATION
                    INNER JOIN 
                        SDMS_IRQDLVR.T_APPLICANT
                        ON SDMS_IRQDLVR.T_APPLICANT.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICANT
                    WHERE 
                        SDMS_IRQDLVR.T_APPLICATION.VID = :V_ID
                    AND 
                        SDMS_IRQDLVR.T_APPLICATION.ID_APPLICATION_TYPE = SDMS_IRQDLVR.T_DOCUMENT.ID_DOCUMENT_TYPE
                    ORDER BY 
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_ISSUE DESC ";

                string dlQuery = @"
                    SELECT
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_ISSUE, 
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_EXPIRY
                    FROM
                        SDMS_IRQDLVR.T_DOCUMENT
                    INNER JOIN
                        SDMS_IRQDLVR.T_APPLICATION
                        ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_DOCUMENT.ID_APPLICATION
                    INNER JOIN
                        SDMS_IRQDLVR.T_APPLICATION_APPLICANT
                        ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICATION
                    INNER JOIN
                        SDMS_IRQDLVR.T_APPLICANT
                        ON SDMS_IRQDLVR.T_APPLICANT.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICANT
                    WHERE
                        SDMS_IRQDLVR.T_APPLICANT.TPID = :TP_ID
                    AND 
                        SDMS_IRQDLVR.T_APPLICATION.ID_APPLICATION_TYPE = SDMS_IRQDLVR.T_DOCUMENT.ID_DOCUMENT_TYPE
                    ORDER BY
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_ISSUE DESC ";

                string catgoryQuery = @"
                        SELECT SDMS_IRQDLVR.T_APPLICATION.ID,
                            SDMS_IRQDLVR.T_DL_CLASS.APPLICATION_ID,
                            SDMS_IRQDLVR.T_DL_CLASS.CATEGORY
                            FROM SDMS_IRQDLVR.T_APPLICATION
                            INNER JOIN SDMS_IRQDLVR.T_DL_CLASS
                            ON SDMS_IRQDLVR.T_APPLICATION.ID    = SDMS_IRQDLVR.T_DL_CLASS.APPLICATION_ID
                            WHERE SDMS_IRQDLVR.T_APPLICATION.ID = :APPLIC_ID ";

                // Main query for Application
                using (var command = new OracleCommand(applicationQuery, connection))
                {
                    command.Parameters.Add(":CREATED_DATE", OracleDbType.Date).Value = DateTime.Now.AddYears(-1);
                    command.Parameters.Add(":APPLIC_ID", OracleDbType.Char).Value = applicationId;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {

                            application = new Application
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("ID")) ? null : reader.GetInt64(reader.GetOrdinal("ID")),
                                UseCase = reader.IsDBNull(reader.GetOrdinal("USE_CASE")) ? null : reader.GetString(reader.GetOrdinal("USE_CASE")),
                                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER")),
                                LicenseNumberLatin = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER_LATIN")) ? null : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER_LATIN")),
                                Governorate = reader.IsDBNull(reader.GetOrdinal("GOVERNORATE")) ? null : reader.GetString(reader.GetOrdinal("GOVERNORATE")),
                                Usage = reader.IsDBNull(reader.GetOrdinal("USAGE")) ? null : reader.GetString(reader.GetOrdinal("USAGE")),
                                Passengers = reader.IsDBNull(reader.GetOrdinal("SEATS1")) ? null : reader.GetString(reader.GetOrdinal("SEATS1")),
                                VehicleCategory = reader.IsDBNull(reader.GetOrdinal("VEHICLE_CATEGORY")) ? null : reader.GetString(reader.GetOrdinal("VEHICLE_CATEGORY")),
                                Cylinders = reader.IsDBNull(reader.GetOrdinal("CYLINDERS")) ? null : reader.GetString(reader.GetOrdinal("CYLINDERS")),
                                Axis = reader.IsDBNull(reader.GetOrdinal("AXIS")) ? null : reader.GetString(reader.GetOrdinal("AXIS")),
                                CabinType = reader.IsDBNull(reader.GetOrdinal("CABIN_TYPE")) ? null : reader.GetString(reader.GetOrdinal("CABIN_TYPE")),
                                LoadWeight = reader.IsDBNull(reader.GetOrdinal("LOAD_WEIGHT")) ? null : reader.GetString(reader.GetOrdinal("LOAD_WEIGHT")),
                               
                                VichleId = reader.IsDBNull(reader.GetOrdinal("VID")) ? null : reader.GetString(reader.GetOrdinal("VID")),
                                ApplicationType = reader.IsDBNull(reader.GetOrdinal("ID_APPLICATION_TYPE")) ? null : reader.GetInt16(reader.GetOrdinal("ID_APPLICATION_TYPE")),
                                IdCurrentState = reader.IsDBNull(reader.GetOrdinal("ID_CURRENT_STATE")) ? null : reader.GetInt16(reader.GetOrdinal("ID_CURRENT_STATE")),

                            };
                        }
                    }
                    if (application == null)
                        return null;
                }
                var nameTask = Task.Run(async () =>
                {
                        using (var nameCommand = new OracleCommand(nameQuery, connection))
                        {
                        nameCommand.Parameters.Add(":APPLIC_ID", OracleDbType.Int64).Value = application.Id;

                            using (var nameReader = await nameCommand.ExecuteReaderAsync())
                            {
                                if (await nameReader.ReadAsync())
                                {
                           
                                application.Tpid = nameReader.IsDBNull(nameReader.GetOrdinal("TPID")) ? null : nameReader.GetString(nameReader.GetOrdinal("TPID"));
                                    application.GivenName = nameReader.IsDBNull(nameReader.GetOrdinal("GIVEN_NAME")) ? null : nameReader.GetString(nameReader.GetOrdinal("GIVEN_NAME"));
                                    application.FatherName = nameReader.IsDBNull(nameReader.GetOrdinal("FATHER_NAME")) ? null : nameReader.GetString(nameReader.GetOrdinal("FATHER_NAME"));
                                    application.GrandfatherName = nameReader.IsDBNull(nameReader.GetOrdinal("GRANDFATHER_NAME")) ? null : nameReader.GetString(nameReader.GetOrdinal("GRANDFATHER_NAME"));
                                    application.MotherName = nameReader.IsDBNull(nameReader.GetOrdinal("MOTHER_NAME")) ? null : nameReader.GetString(nameReader.GetOrdinal("MOTHER_NAME"));
                                application.MotherFatherName = nameReader.IsDBNull(nameReader.GetOrdinal("MOTHER_FATHERNAME")) ? null : nameReader.GetString(nameReader.GetOrdinal("MOTHER_FATHERNAME"));

                            }
                            }
                        }

                });
                // Fetch Driver License details in parallel if required
                var vcTask = Task.Run(async () =>
                {
                    if (application.ApplicationType == 2)
                    {
                        using (var command = new OracleCommand(vcQuery, connection))
                        {
                            command.Parameters.Add(":V_ID", OracleDbType.Char).Value = application.VichleId;

                            using (var vrReader = await command.ExecuteReaderAsync())
                            {
                                if (await vrReader.ReadAsync())
                                {
                                    application.DateOfIssue = vrReader.IsDBNull(vrReader.GetOrdinal("DATE_OF_ISSUE")) ? null : vrReader.GetDateTime(vrReader.GetOrdinal("DATE_OF_ISSUE"));
                                    application.DateOfExpiry = vrReader.IsDBNull(vrReader.GetOrdinal("DATE_OF_EXPIRY")) ? null : vrReader.GetDateTime(vrReader.GetOrdinal("DATE_OF_EXPIRY"));
                                }
                            }
                        }
                    }

                });

                var dlTask = Task.Run(async () =>
                {
                    if (application.ApplicationType == 1)
                    {
                        using (var command = new OracleCommand(dlQuery, connection))
                        {
                            command.Parameters.Add(":TP_ID", OracleDbType.Char).Value = application.Tpid;

                            using (var dlReader = await command.ExecuteReaderAsync())
                            {
                                if (await dlReader.ReadAsync())
                                {
                                    application.DateOfIssue = dlReader.IsDBNull(dlReader.GetOrdinal("DATE_OF_ISSUE")) ? null : dlReader.GetDateTime(dlReader.GetOrdinal("DATE_OF_ISSUE"));
                                    application.DateOfExpiry = dlReader.IsDBNull(dlReader.GetOrdinal("DATE_OF_EXPIRY")) ? null : dlReader.GetDateTime(dlReader.GetOrdinal("DATE_OF_EXPIRY"));
                                }
                            }
                        }
                    }
                });
                var CatgoryTask = Task.Run(async () =>
                {
                    if (application.ApplicationType == 1)
                    {
                        using (var command = new OracleCommand(catgoryQuery, connection))
                        {
                            command.Parameters.Add(":APPLIC_ID", OracleDbType.Int64).Value = application.Id;

                            using (var catReader = await command.ExecuteReaderAsync())
                            {
                                if (await catReader.ReadAsync())
                                {
                                    application.DlCategory = catReader.IsDBNull(catReader.GetOrdinal("CATEGORY")) ? null : catReader.GetString(catReader.GetOrdinal("CATEGORY"));
                                }
                            }
                        }
                    }

                });

                // Wait for all parallel tasks to finish
                await Task.WhenAll(nameTask,vcTask, dlTask, CatgoryTask);

                //Cache the result for a specified time(e.g., 5 minutes)

                _cache.Set(applicationId, application, TimeSpan.FromMinutes(5));
            }
        }
        catch (OracleException ex)
        {
            throw new Exception("Database operation failed", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while fetching user data.", ex);
        }

        return application;
    }
}
