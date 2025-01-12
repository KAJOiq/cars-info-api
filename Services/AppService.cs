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

                string query = @"
                    SELECT 
                        T_APPLICANT.GIVEN_NAME, 
                        T_APPLICANT.FATHER_NAME, 
                        T_APPLICANT.GRANDFATHER_NAME, 
                        T_APPLICANT.MOTHER_NAME, 
                        T_APPLICANT.MOTHER_FATHERNAME,
                        T_APPLICANT.TPID,
                        T_APPLICATION.VID,
                        T_APPLICATION.DTYPE,
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
                    JOIN 
                        SDMS_IRQDLVR.T_APPLICATION_APPLICANT 
                        ON T_APPLICATION.ID = T_APPLICATION_APPLICANT.ID_APPLICATION 
                    JOIN 
                        SDMS_IRQDLVR.T_APPLICANT 
                        ON T_APPLICATION_APPLICANT.ID_APPLICANT = T_APPLICANT.ID
                    WHERE 
                        T_APPLICATION.APPLICATION_ID = :APPLIC_ID";

                string vcQuery = @"SELECT 
                                    SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_ISSUE,
                                    SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_EXPIRY,
                                    SDMS_IRQDLVR.T_REGISTERED_UNIT.CREATED  
                                FROM 
                                    SDMS_IRQDLVR.T_DOCUMENT
                                JOIN 
                                    SDMS_IRQDLVR.T_APPLICATION
                                    ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_DOCUMENT.ID_APPLICATION
                                JOIN 
                                    SDMS_IRQDLVR.T_APPLICATION_APPLICANT
                                    ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICATION
                                JOIN 
                                    SDMS_IRQDLVR.T_APPLICANT
                                    ON SDMS_IRQDLVR.T_APPLICANT.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICANT
                                JOIN 
                                    SDMS_IRQDLVR.T_REGISTERED_UNIT
                                    ON SDMS_IRQDLVR.T_REGISTERED_UNIT.VID = SDMS_IRQDLVR.T_APPLICATION.VID
                                WHERE 
                                    SDMS_IRQDLVR.T_APPLICANT.TPID = :TP_ID
                                    AND SDMS_IRQDLVR.T_DOCUMENT.DTYPE = :DOC_TYPE
                                    AND SDMS_IRQDLVR.T_APPLICATION.VID = :V_ID
                                ORDER BY 
                                    SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_EXPIRY DESC";

                string dlQuery = @"
                    SELECT  
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_ISSUE, 
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_EXPIRY,
                        SDMS_IRQDLVR.T_DL_CLASS.CATEGORY
                    FROM  
                        SDMS_IRQDLVR.T_DOCUMENT
                    JOIN  
                        SDMS_IRQDLVR.T_APPLICATION
                        ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_DOCUMENT.ID_APPLICATION
                    JOIN  
                        SDMS_IRQDLVR.T_APPLICATION_APPLICANT
                        ON SDMS_IRQDLVR.T_APPLICATION.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICATION
                    JOIN  
                        SDMS_IRQDLVR.T_APPLICANT
                        ON SDMS_IRQDLVR.T_APPLICANT.ID = SDMS_IRQDLVR.T_APPLICATION_APPLICANT.ID_APPLICANT
                    LEFT JOIN  
                        SDMS_IRQDLVR.T_DL_CLASS 
                        ON SDMS_IRQDLVR.T_DL_CLASS.DOCUMENT_ID = SDMS_IRQDLVR.T_DOCUMENT.ID
                    WHERE  
                        SDMS_IRQDLVR.T_APPLICANT.TPID = :TP_ID
                        AND SDMS_IRQDLVR.T_DOCUMENT.DTYPE = :DOC_TYPE
                    ORDER BY  
                        SDMS_IRQDLVR.T_DOCUMENT.DATE_OF_EXPIRY DESC";

                // Main query for Application
                using (var command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":APPLIC_ID", OracleDbType.Char).Value = applicationId;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            application = new Application
                            {
                                GivenName = reader.IsDBNull(reader.GetOrdinal("GIVEN_NAME")) ? null : reader.GetString(reader.GetOrdinal("GIVEN_NAME")),
                                FatherName = reader.IsDBNull(reader.GetOrdinal("FATHER_NAME")) ? null : reader.GetString(reader.GetOrdinal("FATHER_NAME")),
                                GrandfatherName = reader.IsDBNull(reader.GetOrdinal("GRANDFATHER_NAME")) ? null : reader.GetString(reader.GetOrdinal("GRANDFATHER_NAME")),
                                MotherName = reader.IsDBNull(reader.GetOrdinal("MOTHER_NAME")) ? null : reader.GetString(reader.GetOrdinal("MOTHER_NAME")),
                                MotherFatherName = reader.IsDBNull(reader.GetOrdinal("MOTHER_FATHERNAME")) ? null : reader.GetString(reader.GetOrdinal("MOTHER_FATHERNAME")),
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
                                 Tpid = reader.IsDBNull(reader.GetOrdinal("TPID")) ? null : reader.GetString(reader.GetOrdinal("TPID")),
                                VichleId = reader.IsDBNull(reader.GetOrdinal("VID")) ? null : reader.GetString(reader.GetOrdinal("VID")),
                                ApplicationType = reader.IsDBNull(reader.GetOrdinal("DTYPE")) ? null : reader.GetString(reader.GetOrdinal("DTYPE")),
                                IdCurrentState = reader.IsDBNull(reader.GetOrdinal("ID_CURRENT_STATE")) ? null : reader.GetInt16(reader.GetOrdinal("ID_CURRENT_STATE")),

                            };

                            
                        }
                    }
                    if (application == null)
                        return null;
                }

                // Fetch Vehicle Category and Driver License details in parallel if required
                var vcTask = Task.Run(async () =>
                {
                    if (application.ApplicationType == "irq_vr")
                    {
                        using (var command = new OracleCommand(vcQuery, connection))
                        {
                            command.Parameters.Add(":TP_ID", OracleDbType.Char).Value = application.Tpid;
                            command.Parameters.Add(":DOC_TYPE", OracleDbType.Char).Value = "irq_doc_vc";
                            command.Parameters.Add(":V_ID", OracleDbType.Char).Value = application.VichleId;

                            using (var vrReader = await command.ExecuteReaderAsync())
                            {
                                if (await vrReader.ReadAsync())
                                {
                                    application.DateOfIssue = vrReader.IsDBNull(vrReader.GetOrdinal("DATE_OF_ISSUE")) ? null : vrReader.GetDateTime(vrReader.GetOrdinal("DATE_OF_ISSUE"));
                                    application.DateOfExpiry = vrReader.IsDBNull(vrReader.GetOrdinal("DATE_OF_EXPIRY")) ? null : vrReader.GetDateTime(vrReader.GetOrdinal("DATE_OF_EXPIRY"));
                                    application.CustomsApplyDate = vrReader.IsDBNull(vrReader.GetOrdinal("CREATED")) ? null : vrReader.GetDateTime(vrReader.GetOrdinal("CREATED"));
                                }
                            }
                        }
                    }
                });

                var dlTask = Task.Run(async () =>
                {
                    if (application.ApplicationType == "irq_dl")
                    {
                        using (var command = new OracleCommand(dlQuery, connection))
                        {
                            command.Parameters.Add(":TP_ID", OracleDbType.Char).Value = application.Tpid;
                            command.Parameters.Add(":DOC_TYPE", OracleDbType.Char).Value = "irq_doc_dl";

                            using (var dlReader = await command.ExecuteReaderAsync())
                            {
                                if (await dlReader.ReadAsync())
                                {
                                    application.DateOfIssue = dlReader.IsDBNull(dlReader.GetOrdinal("DATE_OF_ISSUE")) ? null : dlReader.GetDateTime(dlReader.GetOrdinal("DATE_OF_ISSUE"));
                                    application.DateOfExpiry = dlReader.IsDBNull(dlReader.GetOrdinal("DATE_OF_EXPIRY")) ? null : dlReader.GetDateTime(dlReader.GetOrdinal("DATE_OF_EXPIRY"));
                                    application.DlCategory = dlReader.IsDBNull(dlReader.GetOrdinal("CATEGORY")) ? null : dlReader.GetString(dlReader.GetOrdinal("CATEGORY"));
                                }
                            }
                        }
                    }
                });

                // Wait for all parallel tasks to finish
                await Task.WhenAll(vcTask, dlTask);

                // Cache the result for a specified time (e.g., 5 minutes)
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
