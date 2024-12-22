using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;  // Add caching
using System.Threading.Tasks;
using ApiAppPay.Models;
using System;
using System.Collections.Generic;

public class AppService
{
    private readonly string _connectionString;
    private readonly IMemoryCache _cache;  // MemoryCache dependency

    public AppService(IConfiguration configuration, IMemoryCache memoryCache)
    {
        // ConnetionString from appsettings.json
        _connectionString = configuration.GetConnectionString("sdms");
        _cache = memoryCache;  // Inject memory cache
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
                                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER")),
                                LicenseNumberLatin = reader.IsDBNull(reader.GetOrdinal("LICENSE_NUMBER_LATIN")) ? null : reader.GetString(reader.GetOrdinal("LICENSE_NUMBER_LATIN")),
                                Governorate = reader.IsDBNull(reader.GetOrdinal("GOVERNORATE")) ? null : reader.GetString(reader.GetOrdinal("GOVERNORATE")),
                                Usage = reader.IsDBNull(reader.GetOrdinal("USAGE")) ? null : reader.GetString(reader.GetOrdinal("USAGE")),
                                Passengers = reader.IsDBNull(reader.GetOrdinal("SEATS1")) ? null : reader.GetString(reader.GetOrdinal("SEATS1")),
                                VehicleCategory = reader.IsDBNull(reader.GetOrdinal("VEHICLE_CATEGORY")) ? null : reader.GetString(reader.GetOrdinal("VEHICLE_CATEGORY")),
                                Cylinders = reader.IsDBNull(reader.GetOrdinal("CYLINDERS")) ? null : reader.GetString(reader.GetOrdinal("CYLINDERS")),
                                Axis = reader.IsDBNull(reader.GetOrdinal("AXIS")) ? null : reader.GetString(reader.GetOrdinal("AXIS")),
                                CabinType = reader.IsDBNull(reader.GetOrdinal("CABIN_TYPE")) ? null : reader.GetString(reader.GetOrdinal("CABIN_TYPE")),
                                LoadWeight = reader.IsDBNull(reader.GetOrdinal("LOAD_WEIGHT")) ? null : reader.GetString(reader.GetOrdinal("LOAD_WEIGHT"))
                            };

                            // Cache the result for a specified time (e.g., 5 minutes)
                            _cache.Set(applicationId, application, TimeSpan.FromMinutes(5));
                        }
                    }
                }
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

