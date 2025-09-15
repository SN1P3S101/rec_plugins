//! https://github.com/SN1P3S101

using System;
using System.Globalization;
using System.Collections.Generic;

namespace Carbon.Plugins {

    [Info("rec_config", "SN1P3S_", "0.1.0")]
    [Description("Everything related to the configuration of the server.")]
    public class rec_config : CarbonPlugin {



        //! variables
        private Dictionary<string, Dictionary<string, object>> config;
        private Dictionary<string, Dictionary<string, object>> schema;

        public rec_config() {

            config = [];
            schema = [];

        }



        //! hooks
        private void OnServerInitialized() {

            log("INFO", "Loading plugin...");

            var (sb_status, sb_message) = schema_build();

            if (sb_status != "OK") {

                log("ERROR", "Building config schema failed: " + sb_message);

                return;

            }

            var (cl_status, cl_message) = config_load();

            if (cl_status != "OK") {

                log("ERROR", "Loading config failed: " + cl_message);

                return;

            }

            var (cv_status, cv_message) = config_verify();

            if (cv_status != "OK") {

                if (cv_status == "FIXED") {

                    log("DEBUG", "Config auto-healed: " + cv_message);

                } else if (cv_status == "FAIL") {

                    log("FAIL", "Verifying config failed: " + cv_message);

                } else {

                    log("ERROR", "Verifying config failed: " + cv_message);

                }

                var (cs_status, cs_message) = config_save();

                if (cs_status != "OK") {

                    log("ERROR", "Saving config failed: " + cs_message);

                    return;

                }

            }

            log("INFO", "Plugin loaded.");

        }



        //! external hooks
        [HookMethod("rec_config_get")]
        public object get(string? sections_csv = null) {

            schema ??= new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            if (schema.Count == 0) {

                _ = schema_build();

            }

            _ = config_load();

            config ??= new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            var (status, message) = config_verify();

            if (status == "FIXED") {

                _ = config_save();

            }

            if (config.Count == 0) {

                _ = config_build();

                config ??= new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            }

            var export = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sections_csv)) {

                foreach (var kv in config) {

                    var section_copy = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    if (kv.Value != null) {

                        foreach (var item in kv.Value) {

                            section_copy[item.Key] = item.Value;

                        }

                    }

                    export[kv.Key] = section_copy;

                }

                return export;

            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parts = (sections_csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++) {

                var name = (parts[i] ?? string.Empty).Trim();

                if (name.Length == 0 || !seen.Add(name)) {

                    continue;

                }

                var section_copy = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                if (config.TryGetValue(name, out var src) && src != null) {

                    foreach (var item in src) {

                        section_copy[item.Key] = item.Value;

                    }

                } else if (schema.TryGetValue(name, out var sdef) && sdef != null) {

                    foreach (var item in sdef) {

                        if (item.Value is schema_item s_item) {

                            section_copy[item.Key] = s_item.default_value;

                        }

                    }

                }

                export[name] = section_copy;

            }

            log("DEBUG", "Exported config sections: " + string.Join(", ", seen));

            return export;

        }



        //! functions
        private (string status, string message) schema_build() {

            schema = new Dictionary<string, Dictionary<string, object>> {

                {"rec_core", new Dictionary<string, object> {

                    {"server_id",
                    new schema_item(
                        "myserver",
                        true,
                        "Important! Unique server identifier.",
                        "This should be a short string without spaces or special characters. This ID is used to uniquely identify your server in the database and must be unique in your network of servers. \nChanging this value after initial setup will cause your server to be treated as a new server, losing access to existing data.\n\nThis value is required and will be linked to all of your players data and all stats connected to it. Including ranks, tags, playtime, bans and more. Choose wisely.\n\nIf you are running a single server, you can leave this as the default value.\n\nIf you are running multiple servers, ensure each has a unique ID.\nOfcource you can keep the default value if you have multiple servers AND if you have multiple databases. We made it this way to give this eco system a more dynamic, flexible, modular and most importantly, a more CONNECTED approach.\n\nIf you are a server owner or developer you can use this data to make websites, leaderboards, stat trackers and more. The possibilities are endless.\n\nEach plugin will use this value to link data to your server. For example; if you have two servers running with the same server_id, they will share the same player data (this can be used to have one big leaderbord that also shows people from yout other servers). If you have two servers with different server_id values, they will have separate player data (this can be used to have completely separate servers with their own stats, leaderboards, etc). But this will most times be the default because most providers will give you a new database access for each server.\n\nSo beware when changing this value after initial setup. It will cause your server to be treated as a new server, losing access to existing data!",
                        typeof(string)
                    )},

                    {"link_website",
                    new schema_item(
                        "https://recaris.eu",
                        false,
                        "Website URL",
                        "This is the URL to your server's website. It will be used in various places, such as in chat commands or web interfaces, to provide players with a link to your website.\n\nMake sure to include the full URL, including the 'http://' or 'https://' prefix.",
                        typeof(string)
                    )},

                    {"link_discord",
                    new schema_item(
                        "https://discord.gg/fDAjtATuma",
                        false,
                        "Discord Invite Link",
                        "This is the invite link to your server's Discord. It will be used in various places, such as in chat commands or web interfaces, to provide players with a link to your Discord server.\n\nMake sure to use a valid invite link.",
                        typeof(string)
                    )},

                    {"link_shop",
                    new schema_item(
                        "https://recaris.eu",
                        false,
                        "Shop URL",
                        "This is the URL to your server's shop. It will be used in various places, such as in chat commands or web interfaces, to provide players with a link to your shop.\n\nMake sure to include the full URL, including the 'http://' or 'https://' prefix.",
                        typeof(string)
                    )},

                    {"server_name",
                    new schema_item(
                        "Recaris Gaming",
                        true,
                        "Server Name",
                        "The name of your server. This will be used in various places, such as in chat commands or web interfaces, to identify your server.",
                        typeof(string)
                    )},

                    {"server_tag",
                    new schema_item(
                        "[REC]",
                        false,
                        "Server Tag for chat",
                        "This tag will be prefixed to all chat messages sent by the plugin. It should be short and easily recognizable, as it will help players identify messages from the server in the chat.\n\nExample: [REC] Welcome to the server!",
                        typeof(string)
                    )},
                        

                    {"style_primary_color",
                    new schema_item(
                        "#ff952a",
                        true,
                        "Primary Style Color",
                        "The primary color used in different kinds of elements. This should be a valid hex color code (e.g., #ff952a).",
                        typeof(string)
                    )},

                    {"style_secondary_color",
                    new schema_item(
                        "#72ffba",
                        true,
                        "Secondary Style Color",
                        "The secondary color used in different kinds of elements. This should be a valid hex color code (e.g., #72ffba).",
                        typeof(string)
                    )},

                    {"steam_id",
                    new schema_item(
                        "76561199855661809",
                        false,
                        "Steam ID for icon in chat",
                        "This is the Steam ID of the account whose avatar will be used as the icon in chat messages sent by the plugin. It should be a valid Steam ID (e.g., 76561199855661809).\n\nThis is optional, but recommended to give your server a more personalized touch. Also default system messages will use this icon and your server_tag as the prefix. Example: [image] [<server_tag>] This is a system message.",
                        typeof(string)
                    )}

                }},

                {"rec_database", new Dictionary<string, object> {

                    // hostname, port, database, username, password

                    {"hostname",
                    new schema_item(
                        "localhost",
                        true,
                        "Database Hostname",
                        "The hostname or IP address of your MySQL database server.",
                        typeof(string)
                    )},

                    {"port",
                    new schema_item(
                        3306,
                        true,
                        "Database Port",
                        "The port number on which your MySQL database server is listening.",
                        typeof(int)
                    )},

                    {"database",
                    new schema_item(
                        "recaris_rust",
                        true,
                        "Database Name",
                        "The name of the MySQL database to use for storing server data.",
                        typeof(string)
                    )},

                    {"username",
                    new schema_item(
                        "recaris_rust",
                        true,
                        "Database Username",
                        "The username to use when connecting to your MySQL database.",
                        typeof(string)
                    )},

                    {"password",
                    new schema_item(
                        "<changeme>",
                        true,
                        "Database Password",
                        "The password to use when connecting to your MySQL database.",
                        typeof(string)
                    )}

                }},

            };

            log("DEBUG", "Schema built.");

            return (
                status: "OK",
                message: "Schema built successfully."
            );

        }

        private (string status, string message) config_load() {

            try {

                if (!Config.Exists()) {

                    log("DEBUG", "Config file does not exist, creating new one...");

                    config_build();

                }

                var loaded = Config.ReadObject<Dictionary<string, Dictionary<string, object>>>();

                if (loaded == null) {

                    log("DEBUG", "Config file is empty or corrupted, rebuilding...");

                    config_build();

                } else {

                    config = loaded;

                    log("DEBUG", "Config file loaded.");

                }

                return (
                    status: "OK",
                    message: "Config loaded successfully."
                );

            } catch (Exception error) {

                return (
                    status: "ERROR",
                    message: "Exception while loading config: " + error.Message
                );

            }

        }

        private (string status, string message) config_verify() {

            try {

                bool changed = false;
                var issues = new List<string>();

                // check to see if all sections from schema exist
                foreach (var section_kv in schema) {

                    var section_name = section_kv.Key;
                    var section_schema = section_kv.Value;

                    if (!config.TryGetValue(section_name, out var section_cfg) || section_cfg == null) {

                        section_cfg = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        config[section_name] = section_cfg;
                        changed = true;

                        issues.Add("created section '" + section_name + "'");

                    }

                    // validate keys within the section
                    foreach (var item_kv in section_schema) {

                        var key = item_kv.Key;
                        var s_item = (schema_item)item_kv.Value;

                        if (!section_cfg.TryGetValue(key, out var current_value)) {

                            section_cfg[key] = s_item.default_value;
                            changed = true;

                            issues.Add("added missing key '" + section_name + "." + key + "'");

                            continue;

                        }

                        // type check and cast if needed
                        if (!value_matches_type(current_value, s_item.type)) {

                            if (try_convert_value(current_value, s_item.type, out var converted)) {

                                section_cfg[key] = converted;
                                changed = true;

                                issues.Add("type-casted '" + section_name + "." + key + "'");

                            } else {

                                section_cfg[key] = s_item.default_value;
                                changed = true;

                                issues.Add("replaced invalid type at '" + section_name + "." + key + "' with default");

                            }

                        }

                        // required check
                        if (s_item.required) {

                            var val = section_cfg[key];

                            if (is_null_or_empty(val)) {

                                if (!is_null_or_empty(s_item.default_value)) {

                                    section_cfg[key] = s_item.default_value;
                                    changed = true;

                                    issues.Add("filled required '" + section_name + "." + key + "' from default");

                                } else {

                                    issues.Add("required key empty without default: '" + section_name + "." + key + "'");

                                    return (
                                        status: "ERROR",
                                        message: string.Join("; ", issues.ToArray())
                                    );

                                }

                            }

                        }

                    }

                }

                if (changed) {

                    return (
                        status: "FIXED",
                        message: issues.Count == 0 ? "Adjusted by schema." : string.Join("; ", [.. issues])
                    );

                }

                return (
                    status: "OK",
                    message: "Config verified (no changes)."
                );

            } catch (Exception e) {

                return (
                    status: "ERROR",
                    message: "Exception while verifying: " + e.Message
                );

            }

        }

        private (string status, string message) config_build() {

            config = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            foreach (var section in schema) {

                config[section.Key] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var item in section.Value) {

                    var s_item = (schema_item)item.Value;

                    config[section.Key][item.Key] = s_item.default_value;

                }

            }

            log("INFO", "Config file built from schema.");

            config_save();

            return (
                status: "OK",
                message: "Config built successfully."
            );

        }

        private (string status, string message) config_save() {

            try {

                Config.WriteObject(config, true);

                log("DEBUG", "Config saved successfully.");

                return (
                    status: "OK",
                    message: "Config saved successfully."
                );

            } catch (Exception e) {

                return (
                    status: "ERROR",
                    message: "Exception while saving: " + e.Message
                );

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
        private class schema_item(object default_value, bool required, string about, string description, Type type) {

            public object default_value = default_value;
            public bool required = required;
            public string about = about;
            public string description = description;
            public Type type = type;

        }

        private bool value_matches_type(object value, Type type) {

            if (value == null) {

                return false;

            }

            if (type == typeof(string)) {

                return value is string;

            }

            if (type == typeof(bool)) {

                return value is bool;

            }

            if (type == typeof(int)) {

                if (value is int) {

                    return true;

                }

                if (value is long l) {

                    return l >= int.MinValue && l <= int.MaxValue;

                }

                if (value is double dd) {

                    return dd % 1 == 0 && dd >= int.MinValue && dd <= int.MaxValue;

                }

                if (value is float ff) {

                    return ff % 1 == 0 && ff >= int.MinValue && ff <= int.MaxValue;

                }

                if (value is decimal dc) {

                    return dc % 1 == 0 && dc >= int.MinValue && dc <= int.MaxValue;

                }

                return false;

            }

            if (type == typeof(long)) {

                return value is long || value is int;

            }

            if (type == typeof(float)) {

                return value is float || value is double || value is int || value is long || value is decimal;

            }

            if (type == typeof(double)) {

                return value is double || value is float || value is int || value is long || value is decimal;

            }

            return string.Equals(value.GetType().FullName, type.FullName, StringComparison.OrdinalIgnoreCase);

        }

        private bool try_convert_value(object input, Type target_type, out object output) {

            try {

                if (target_type == typeof(string)) {

                    output = input == null ? "" : input.ToString();

                    return true;

                }

                if (input is string s) {

                    if (target_type == typeof(int)) {

                        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var iv)) {

                            output = iv; return true;

                        }
                    }

                    if (target_type == typeof(long)) {

                        if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var lv)) {

                            output = lv; return true;

                        }
                    }

                    if (target_type == typeof(float)) {

                        if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var fv)) {

                            output = fv; return true;

                        }
                    }

                    if (target_type == typeof(double)) {

                        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv)) {

                            output = dv; return true;

                        }
                    }

                    if (target_type == typeof(bool)) {

                        if (bool.TryParse(s, out var bv)) {

                            output = bv; return true;

                        }

                        if (s == "0") {

                            output = false; return true;

                        }

                        if (s == "1") {

                            output = true; return true;

                        }

                    }

                }

                if (input is int i) {

                    if (target_type == typeof(long)) {

                        output = (long)i; return true;

                    }

                    if (target_type == typeof(float)) {

                        output = (float)i; return true;

                    }

                    if (target_type == typeof(double)) {

                        output = (double)i; return true;

                    }

                }

                if (input is long l) {

                    if (target_type == typeof(int)) {

                        output = (int)l; return true;

                    }

                    if (target_type == typeof(float)) {

                        output = (float)l; return true;

                    }

                    if (target_type == typeof(double)) {

                        output = (double)l; return true;

                    }

                }

                if (input is float f) {

                    if (target_type == typeof(double)) {

                        output = (double)f; return true;

                    }

                    if (target_type == typeof(int)) {

                        output = (int)f; return true;

                    }

                    if (target_type == typeof(long)) {

                        output = (long)f; return true;

                    }

                }

                if (input is double d) {

                    if (target_type == typeof(float)) {

                        output = (float)d; return true;

                    }

                    if (target_type == typeof(int)) {

                        output = (int)d; return true;

                    }

                    if (target_type == typeof(long)) {

                        output = (long)d; return true;

                    }

                }

                var converted = Convert.ChangeType(input, target_type, System.Globalization.CultureInfo.InvariantCulture);

                if (converted != null) {

                    output = converted; return true;

                }

                output = input ?? string.Empty;

                return false;

            } catch {

                output = input ?? string.Empty;

                return false;

            }

        }

        private bool is_null_or_empty(object value) {

            if (value == null) {

                return true;

            }

            if (value is string s) {

                return string.IsNullOrWhiteSpace(s);

            }

            return false;

        }



    }

}