//! https://github.com/SN1P3S101

using System;
using System.Globalization;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Oxide.Core;

namespace Carbon.Plugins {

    [Info("rec_database", "SN1P3S_", "0.0.1")]
    [Description("Everything related to the database of the server.")]
    public class rec_database : CarbonPlugin {



        //! variables
        private Dictionary<string, Dictionary<string, object>> config;
        private Dictionary<string, Dictionary<string, object>> schema;
        private MySqlConnection database;

        public rec_database() {

            config = [];
            schema = [];
            database = new MySqlConnection();

        }



        //! hooks
        private void OnServerInitialized() {

            config = (Dictionary<string, Dictionary<string, object>>)Interface.Call("rec_config_get", "rec_main");

            log("DEBUG", (string)config["rec_main"]["server_name"]);

        }



        //! external hooks
        [HookMethod("rec_database_get")]
        private MySqlConnection rec_database_get() {

            return database;

        }



        //! functions
        // build schema
        private (string status, string message) schema_build() {

            return (
                status: "DEV",
                message: "Function not yet implemented."
            );

        }

        // connect to database
        private (string status, string message) database_connect() {

            return (
                status: "DEV",
                message: "Function not yet implemented."
            );

        }

        // exit connection
        private (string status, string message) database_disconnect() {

            return (
                status: "DEV",
                message: "Function not yet implemented."
            );

        }

        // check if connection is available and working
        private (string status, string message) database_available() {

            return (
                status: "DEV",
                message: "Function not yet implemented."
            );

        }

        // check if database is in line with schema and update if needed
        private (string status, string message) database_verify() {

            return (
                status: "DEV",
                message: "Function not yet implemented."
            );

        }



        //! rec_functions
        private void log(string level, string message) {

            if (string.Equals(level, "DEBUG", StringComparison.OrdinalIgnoreCase)) {

                Puts("[DEBUG] " + message);

            } else if (string.Equals(level, "INFO", StringComparison.OrdinalIgnoreCase)) {

                Puts("[INFO] " + message);

            } else if (string.Equals(level, "WARN", StringComparison.OrdinalIgnoreCase)) {

                Puts("[WARN] " + message);

            } else if (string.Equals(level, "FAIL", StringComparison.OrdinalIgnoreCase)) {

                Puts("[FAIL] " + message);

            } else if (string.Equals(level, "ERROR", StringComparison.OrdinalIgnoreCase)) {

                Puts("[ERROR] " + message);

            } else {

                Puts("[UNKNOWN] " + message);

            }

        }

        

    }

}