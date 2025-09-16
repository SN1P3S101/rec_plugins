//! https://github.com/SN1P3S101

using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

using Oxide.Core;
using Oxide.Core.Plugins;

namespace Carbon.Plugins {

    [Info("rec_database", "SN1P3S_", "0.0.1")]
    [Description("Everything related to the database of the server.")]
    public class rec_database : CarbonPlugin {



        //! variables
        private Dictionary<string, Dictionary<string, object>> config;
        private Dictionary<string, (Dictionary<string, string> columns, List<string> fks)> schema;
        private MySqlConnection? database;
        private Plugin rec_config;

        public rec_database() {

            config = [];
            schema = [];
            database = new MySqlConnection();
            rec_config = new Plugin();

        }



        //! hooks
        private void OnServerInitialized() {

            var build = schema_build();

            if (build.status != "OK") {

                log("ERROR", "Schema build failed: " + build.message);

                return;

            }

            rec_config = plugins.Find("rec_config");

            if (rec_config == null) {

                log("ERROR", "Could not find rec_config plugin, database plugin cannot function without it.");

                return;

            }

            config = (Dictionary<string, Dictionary<string, object>>)Interface.Call("rec_config_get", "rec_database");

            if (config == null || !config.ContainsKey("rec_database")) {

                log("ERROR", "Missing 'rec_database' config section; cannot continue.");

                return;

            }

            var available = database_available();

            if (available.status != "OK") {

                var (status, message) = database_connect();

                if (status != "OK") {

                    log("ERROR", "DB connect failed: " + message);

                    return;

                }

            }

            var verify = database_verify();

            if (verify.status == "FIXED") {

                log("INFO", "DB updated to match schema: " + verify.message);

            } else if (verify.status != "OK") {

                log("ERROR", "DB verify failed: " + verify.message);

                return;

            }

            available = database_available();

            if (available.status != "OK") {

                var (status, message) = database_connect();

                if (status != "OK") {

                    log("ERROR", "DB unavailable after verify: " + message);

                    return;

                }

            }

            log("INFO", "Database connection established and ready for use.");

        }



        //! external hooks
        [HookMethod("rec_database_get")]
        private MySqlConnection? rec_database_get() {

            var available = database_available();

            if (available.status != "OK") {

                var (status, message) = database_connect();

                if (status != "OK") {

                    log("ERROR", "DB connect failed: " + message);

                    return null;

                }

            }

            var verify = database_verify();

            if (verify.status == "FIXED") {

                log("INFO", "DB updated to match schema: " + verify.message);

            } else if (verify.status != "OK") {

                log("ERROR", "DB verify failed: " + verify.message);

                return null;

            }

            available = database_available();

            if (available.status != "OK") {

                var (status, message) = database_connect();

                if (status != "OK") {

                    log("ERROR", "DB unavailable after verify: " + message);

                    return null;

                }

            }

            log("DEBUG", "Made connection available for use.");

            return database;

        }



        //! functions
        private (string status, string message) schema_build() {

            schema = new Dictionary<string, (Dictionary<string, string>, List<string>)>(StringComparer.OrdinalIgnoreCase) {

                ["player"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"steam_id", "VARCHAR(17) NOT NULL"},
                    {"username", "VARCHAR(32) NOT NULL"},
                    {"known_usernames", "TEXT"},
                    {"discord_id", "VARCHAR(20)"},
                    {"ip", "VARCHAR(45) NOT NULL"},
                    {"known_ips", "TEXT"},
                    {"last_login", "DATETIME NOT NULL"},
                    {"last_logout", "DATETIME"},
                    {"last_logout_reason", "VARCHAR(255)"}
                }, [
                    "UNIQUE KEY `uq_player_steam` (`steam_id`)",
                    "KEY `ix_player_username` (`username`)"
                ]),

                ["rank"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"name", "VARCHAR(32) NOT NULL"},
                    {"display_name", "VARCHAR(32) NOT NULL"},
                    {"color", "VARCHAR(7) NOT NULL"},
                    {"priority", "INT NOT NULL"}
                }, [
                    "UNIQUE KEY `uq_rank_priority` (`priority`)",
                    "UNIQUE KEY `uq_rank_name` (`name`)"
                ]),

                ["tag"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"name", "VARCHAR(32) NOT NULL"},
                    {"display_name", "VARCHAR(32) NOT NULL"},
                    {"color", "VARCHAR(7) NOT NULL"},
                    {"priority", "INT NOT NULL"}
                }, [
                    "UNIQUE KEY `uq_tag_priority` (`priority`)",
                    "UNIQUE KEY `uq_tag_name` (`name`)"
                ]),

                ["player_rank"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"rank_id", "INT UNSIGNED NOT NULL"},
                    {"assigned_by", "VARCHAR(17) NOT NULL"},
                    {"reason", "VARCHAR(255)"},
                    {"created", "DATETIME NOT NULL"},
                    {"expires", "DATETIME"}
                }, [
                    "KEY `ix_player_rank_player` (`player_id`)",
                    "KEY `ix_player_rank_rank` (`rank_id`)",
                    "CONSTRAINT `fk_player_rank_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE",
                    "CONSTRAINT `fk_player_rank_rank` FOREIGN KEY (`rank_id`) REFERENCES `rank`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

                ["player_tag"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"tag_id", "INT UNSIGNED NOT NULL"},
                    {"assigned_by", "VARCHAR(17) NOT NULL"},
                    {"assigned_at", "DATETIME NOT NULL"},
                    {"assigned_reason", "VARCHAR(255)"},
                    {"expires", "DATETIME"}
                }, [
                    "KEY `ix_player_tag_player` (`player_id`)",
                    "KEY `ix_player_tag_tag` (`tag_id`)",
                    "CONSTRAINT `fk_player_tag_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE",
                    "CONSTRAINT `fk_player_tag_tag` FOREIGN KEY (`tag_id`) REFERENCES `tag`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

                ["player_ban"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"banned_by", "VARCHAR(17) NOT NULL"},
                    {"reason", "VARCHAR(255)"},
                    {"created", "DATETIME NOT NULL"},
                    {"expires", "DATETIME"}
                }, [
                    "KEY `ix_player_ban_player` (`player_id`)",
                    "CONSTRAINT `fk_player_ban_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

                ["player_warning"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"warned_by", "VARCHAR(17) NOT NULL"},
                    {"reason", "VARCHAR(255)"},
                    {"created", "DATETIME NOT NULL"},
                    {"expires", "DATETIME"}
                }, [
                    "KEY `ix_player_warning_player` (`player_id`)",
                    "CONSTRAINT `fk_player_warning_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

                ["player_report"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"reported_by", "VARCHAR(17) NOT NULL"},
                    {"reason", "VARCHAR(255)"},
                    {"handled_by", "VARCHAR(17)"},
                    {"handled_at", "DATETIME"},
                    {"handled_reason", "VARCHAR(255)"},
                    {"status", "VARCHAR(10) NOT NULL"},
                    {"created", "DATETIME NOT NULL"}
                }, [
                    "KEY `ix_player_report_player` (`player_id`)",
                    "KEY `ix_player_report_status` (`status`)",
                    "CONSTRAINT `fk_player_report_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

                ["economy"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"balance", "DECIMAL(19,4) NOT NULL"},
                    {"transactions", "TEXT"},
                    {"last_updated", "DATETIME NOT NULL"}
                }, [
                    "UNIQUE KEY `ux_economy_player` (`player_id`)",
                    "CONSTRAINT `fk_economy_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

                ["economy_transaction"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},
                    {"type", "VARCHAR(10) NOT NULL"},
                    {"target", "VARCHAR(255)"},
                    {"amount", "DECIMAL(19,4) NOT NULL"},
                    {"reason", "VARCHAR(255)"},
                    {"created", "DATETIME NOT NULL"}
                }, [
                    "KEY `ix_economy_tx_player` (`player_id`)",
                    "KEY `ix_economy_tx_type` (`type`)",
                    "CONSTRAINT `fk_economy_tx_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),
                
                ["player_stat"] = (new Dictionary<string, string> {
                    {"id", "INT UNSIGNED AUTO_INCREMENT PRIMARY KEY"},
                    {"player_id", "INT UNSIGNED NOT NULL"},

                    {"playtime_seconds", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"crafts_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"deploys_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"builds_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"upgrades_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"repairs_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"deconstructions_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"airdrops_called", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"airdrops_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hackable_crates_started", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hackable_crates_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"crates_barrel_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"crates_normal_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"crates_toolbox_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"crates_military_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"crates_elite_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"crates_underwater_looted", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"kills_players", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"deaths", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"suicides", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"headshots", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"animals_killed_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_bear_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_polar_bear_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_boar_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_wolf_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_stag_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_horse_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_chicken_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"animals_shark_killed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"hits_taken_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_head", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_chest", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_stomach", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_left_arm", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_right_arm", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_left_leg", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_taken_right_leg", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"hits_given_player_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_head", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_chest", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_stomach", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_left_arm", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_right_arm", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_left_leg", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_player_right_leg", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"hits_given_npc_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_head", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_chest", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_stomach", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_left_arm", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_right_arm", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_left_leg", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"hits_given_npc_right_leg", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"ammo_556", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_556_hv", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_556_explosive", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_556_incendiary", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_pistol", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_pistol_hv", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_12g_buckshot", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_12g_slug", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_12g_incendiary", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_handmade_shell", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_rocket_basic", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_rocket_hv", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"ammo_rocket_incendiary", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"arrow_wooden", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"arrow_hv", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"arrow_fire", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"arrow_bone", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"bradley_kills", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"heli_kills", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"nodes_stone_count", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"nodes_stone_amount", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"nodes_metal_count", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"nodes_metal_amount", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"nodes_sulfur_count", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"nodes_sulfur_amount", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trees_chopped", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"wood_gathered", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"barrels_destroyed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"used_c4", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"used_satchel", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"used_beancan_grenade", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"used_f1_grenade", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"used_rocket_basic", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"used_rocket_hv", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"used_rocket_incendiary", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"traps_placed_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"traps_destroyed_total", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_landmine_placed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_landmine_destroyed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_shotgun_placed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_shotgun_destroyed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_flame_turret_placed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_flame_turret_destroyed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_snap_trap_placed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},
                    {"trap_snap_trap_destroyed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"tcs_destroyed", "BIGINT UNSIGNED NOT NULL DEFAULT 0"},

                    {"last_updated", "DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"}
                }, [
                    "UNIQUE KEY `ux_player_stat_player` (`player_id`)",
                    "KEY `ix_playtime` (`playtime_seconds`)",
                    "KEY `ix_kills` (`kills_players`)",
                    "KEY `ix_deaths` (`deaths`)",
                    "CONSTRAINT `fk_player_stat_player` FOREIGN KEY (`player_id`) REFERENCES `player`(`id`) ON DELETE CASCADE ON UPDATE CASCADE"
                ]),

            };

            log("DEBUG", "Built schema definition with " + schema.Count + " tables.");

            return (
                status: "OK",
                message: "Built schema definition with " + schema.Count + " tables."
            );

        }

        private (string status, string message) database_connect() {

            if (database != null && database.State == ConnectionState.Open) {

                if (database.Ping()) {

                    log("DEBUG", "Reusing existing database connection.");

                    return (
                        status: "OK",
                        message: "Reused existing database connection. (pooling)"
                    );

                }

            }

            try {

                if (database != null) {

                    try {

                        database.Close();

                    } catch { }

                    try {

                        database.Dispose();

                    } catch { }

                }

            } catch { }

            // connection is not open/working/closed so we create a new one!
            database = null;

            var hostname = (string)config["rec_database"]["hostname"];
            var port = (int)config["rec_database"]["port"];
            var database_name = (string)config["rec_database"]["database"];
            var username = (string)config["rec_database"]["username"];
            var password = (string)config["rec_database"]["password"];
            var connection = "Server=" + hostname + ";Port=" + port + ";Database=" + database_name + ";User ID=" + username + ";Password=" + password + ";Pooling=true;SslMode=none;";

            var conn = new MySqlConnection(connection);

            try {

                conn.Open();

                if (conn.Ping()) {

                    database = conn;

                    log("DEBUG", "Opened new database connection.");

                    return (
                        status: "OK",
                        message: "Successfully connected to the database."
                    );

                }

                try {

                    conn.Close();

                } catch { }

                try {

                    conn.Dispose();

                } catch { }

                log("ERROR", "Opened connection but Ping() failed.");

                return (
                    status: "FAIL",
                    message: "Opened connection but Ping() failed."
                );

            } catch (Exception e) {

                try {

                    conn.Dispose();

                } catch { }

                log("ERROR", "Could not connect to the database. Exception: " + e.Message);

                return (
                    status: "ERROR",
                    message: "Could not connect to the database. Exception: " + e.Message
                );

            }

        }

        private (string status, string message) database_disconnect() {

            try {

                if (database == null) {

                    log("DEBUG", "No active connection to close.");

                    return (
                        status: "OK",
                        message: "No active connection to close."
                    );

                }

                try {

                    database.Close();

                } catch { }

                try {

                    database.Dispose();

                } catch { }

                database = null;

                log("DEBUG", "Disconnected and disposed.");

                return (
                    status: "OK",
                    message: "Disconnected and disposed."
                );

            } catch (Exception e) {

                database = null;

                log("ERROR", "Disconnect exception: " + e.Message);

                return (
                    status: "ERROR",
                    message: "Disconnect exception: " + e.Message
                );

            }

        }

        private (string status, string message) database_available() {

            if (database == null) {

                return (
                    status: "EMPTY",
                    message: "No connection instance."
                );

            }

            if (database.State != ConnectionState.Open) {

                return (
                    status: "CLOSED",
                    message: "Connection state: " + database.State
                );

            }

            try {

                if (database.Ping()) {

                    return (
                        status: "OK",
                        message: "Ping OK."
                    );

                }

                return (
                    status: "FAIL",
                    message: "Ping returned false."
                );

            } catch (Exception e) {

                log("ERROR", "Ping exception: " + e.Message);

                return (
                    status: "ERROR",
                    message: "Ping exception: " + e.Message
                );

            }

        }

        private (string status, string message) database_verify() {

            try {

                bool changed = false;
                var notes = new List<string>();
                var pending_fks = new List<(string table, string fk)>();

                foreach (var kv in schema) {

                    var table = kv.Key;
                    var cols = kv.Value.columns;
                    var keys = kv.Value.fks;
                    bool exists;

                    using (var cmd = database!.CreateCommand()) {

                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @db AND TABLE_NAME = @tab";
                        cmd.Parameters.AddWithValue("@db", database.Database);
                        cmd.Parameters.AddWithValue("@tab", table);

                        exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

                    }

                    if (!exists) {

                        var parts = new List<string>();

                        foreach (var c in cols) {

                            parts.Add("`" + c.Key + "` " + c.Value);

                        }

                        if (keys != null) {

                            foreach (var k in keys) {

                                if (is_fk(k)) {

                                    pending_fks.Add((table, k));

                                }

                                else {

                                    parts.Add(k);

                                }

                            }

                        }

                        var sql = "CREATE TABLE `" + table + "` (" + string.Join(", ", parts.ToArray()) + ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

                        using (var cmd = database.CreateCommand()) {

                            cmd.CommandText = sql; cmd.ExecuteNonQuery();

                        }

                        log("INFO", "created table \"" + table + "\".");

                        changed = true;

                        notes.Add("created table \"" + table + "\"");

                        continue;

                    }

                    var existing_cols = new Dictionary<string, (string column_type, string is_nullable, string column_key, string extra, string column_default)>(StringComparer.OrdinalIgnoreCase);

                    using (var cmd = database.CreateCommand()) {

                        cmd.CommandText =
                            "SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_KEY, EXTRA, " +
                            "COALESCE(CAST(COLUMN_DEFAULT AS CHAR), '') AS COLUMN_DEFAULT " +
                            "FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @db AND TABLE_NAME = @tab";
                        cmd.Parameters.AddWithValue("@db", database.Database);
                        cmd.Parameters.AddWithValue("@tab", table);

                        using (var r = cmd.ExecuteReader()) {

                            while (r.Read()) {

                                var n = r.GetString(0);
                                var t = r.GetString(1);
                                var nul = r.GetString(2);
                                var key = r.IsDBNull(3) ? "" : r.GetString(3);
                                var ex = r.IsDBNull(4) ? "" : r.GetString(4);
                                var defv = r.IsDBNull(5) ? "" : r.GetString(5);

                                existing_cols[n] = (t, nul, key, ex, defv);

                            }

                        }

                    }

                    foreach (var c in cols) {

                        var col = c.Key;
                        var def = c.Value;

                        if (!existing_cols.ContainsKey(col)) {

                            using (var cmd = database.CreateCommand()) {

                                cmd.CommandText = "ALTER TABLE `" + table + "` ADD COLUMN `" + col + "` " + def + ";";
                                cmd.ExecuteNonQuery();

                            }

                            log("INFO", "added column \"" + table + "\".\"" + col + "\".");

                            changed = true;

                            notes.Add("added column \"" + table + "\".\"" + col + "\"");

                        } else {

                            var ex = existing_cols[col];

                            if (needs_modify(ex, def)) {

                                using (var cmd = database.CreateCommand()) {

                                    cmd.CommandText = "ALTER TABLE `" + table + "` MODIFY COLUMN `" + col + "` " + def + ";";

                                    try {

                                        cmd.ExecuteNonQuery();

                                        log("INFO", "modified column \"" + table + "\".\"" + col + "\".");

                                        changed = true;

                                        notes.Add("modified column \"" + table + "\".\"" + col + "\"");

                                    } catch { }

                                }

                            }

                        }
                        
                    }

                    if (keys != null) {

                        foreach (var raw in keys) {

                            if (is_fk(raw)) {

                                pending_fks.Add((table, raw));

                                continue;

                            }

                            using (var cmd = database.CreateCommand()) {

                                cmd.CommandText = "ALTER TABLE `" + table + "` ADD " + raw + ";";

                                try {

                                    cmd.ExecuteNonQuery();

                                    log("INFO", "added key on \"" + table + "\": " + raw + ".");

                                    changed = true;

                                    notes.Add("added key on \"" + table + "\": " + raw);

                                } catch { }

                            }

                        }

                    }

                }

                foreach (var entry in pending_fks) {

                    using (var cmd = database!.CreateCommand()) {

                        cmd.CommandText = "ALTER TABLE `" + entry.table + "` ADD " + entry.fk + ";";

                        try {

                            cmd.ExecuteNonQuery();

                            log("INFO", "added FK on \"" + entry.table + "\": " + entry.fk + ".");

                            changed = true;

                            notes.Add("added FK on \"" + entry.table + "\": " + entry.fk);

                        } catch { }

                    }

                }

                if (changed) {

                    return (
                        status: "FIXED",
                        message: string.Join("; ", notes.ToArray())
                    );

                }

                return (
                    status: "OK",
                    message: "Database is up-to-date."
                );

            }

            catch (Exception e) {

                log("ERROR", "Verify exception: " + e.Message);

                return (status: "ERROR", message: "Verify exception: " + e.Message);

            }

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



        //! utilities
        private bool is_fk(string k) {

            var s = (k ?? "").TrimStart();

            return s.StartsWith("CONSTRAINT ", StringComparison.OrdinalIgnoreCase) || s.StartsWith("FOREIGN KEY ", StringComparison.OrdinalIgnoreCase);

        }

        private bool needs_modify((string column_type, string is_nullable, string column_key, string extra, string column_default) ex, string def ) {

            var type = (ex.column_type ?? "").ToLowerInvariant();
            var is_nullable = string.Equals(ex.is_nullable ?? "", "YES", StringComparison.OrdinalIgnoreCase);
            var extra = (ex.extra ?? "").ToLowerInvariant();
            var col_default = (ex.column_default ?? "").ToLowerInvariant();
            var d = (def ?? "").ToLowerInvariant();

            if (d.StartsWith("bigint") && !type.StartsWith("bigint")) {

                return true;

            }

            if (d.StartsWith("int") && !type.StartsWith("int")) {

                return true;

            }

            if (d.StartsWith("varchar(")) {

                var i1 = d.IndexOf("varchar(") + 8; var i2 = d.IndexOf(')', i1);

                if (i1 > 7 && i2 > i1) {

                    var want = d.Substring(i1, i2 - i1); 

                    if (!type.StartsWith("varchar(" + want)) {

                        return true;

                    }

                }

            }

            if (d.StartsWith("decimal(")) {

                var i1 = d.IndexOf("decimal(") + 8; var i2 = d.IndexOf(')', i1);

                if (i1 > 7 && i2 > i1) {

                    var want = d.Substring(i1, i2 - i1);

                    if (!type.StartsWith("decimal(" + want)) {

                        return true;

                    }

                }

            }

            if (d.Contains(" unsigned") && !type.Contains(" unsigned")) {

                return true;

            }

            if (d.Contains(" not null") && is_nullable) {

                return true;

            }

            if (d.Contains(" auto_increment") && !extra.Contains("auto_increment")) {

                return true;

            }

            if (d.Contains(" default current_timestamp")) {

                if (!col_default.Contains("current_timestamp")) {

                    return true;

                }

            } else if (d.Contains(" default 0")) {

                if (col_default != "0") {

                    return true;

                }

            }

            if (d.Contains(" on update current_timestamp") && !extra.Contains("on update current_timestamp")) {

                return true;

            }

            return false;
            
        }

        

    }

}