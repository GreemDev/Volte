﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using Volte.Core.Files.Objects;
using Newtonsoft.Json;
using Volte.Core.Discord;

namespace Volte.Core.Files.Readers {
    public static class ServerConfig {
        private static readonly List<Server> Config = new List<Server>();
        private const string FilePath = "data/serverconfigs.json";

        static ServerConfig() {
            try {
                var jText = File.ReadAllText(FilePath);
                Config = JsonConvert.DeserializeObject<List<Server>>(jText);
            }
            catch (Exception) {
                Save();
            }
        }

        /// <summary>
        ///     Checks if a config with the given guild id exists.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns>System.Boolean</returns>
        public static bool Exists(IGuild guild) {
            return Config.Any(c => c.ServerId == guild.Id);
        }

        /// <summary>
        ///     Gets a server config, if it exists. If it doesn't, then it creates one.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns>SIVA.Core.Files.Objects.Server</returns>
        public static Server Get(IGuild guild) {
            return Config.FirstOrDefault(x => x.ServerId == guild.Id) ?? Create(guild);
        }

        /// <summary>
        ///     Write all the config changes to the disk.
        /// </summary>
        public static void Save() {
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        /// <summary>
        ///     Creates a config with the given IGuild.
        /// </summary>
        /// <param name="guild">Guild to get the ID of.</param>
        /// <returns>SIVA.Core.Files.Objects.Server</returns>
        public static Server Create(IGuild guild) {
            var newConf = new Server {
                ServerId = guild.Id,
                GuildOwnerId = VolteBot.Client.GetGuild(guild.Id).OwnerId,
                Autorole = string.Empty,
                SupportChannelName = string.Empty,
                SupportRole = "Support",
                SupportChannelId = ulong.MinValue,
                CommandPrefix = "$",
                WelcomeChannel = ulong.MinValue,
                WelcomeMessage = string.Empty,
                LeavingMessage = string.Empty,
                WelcomeColourR = 112,
                WelcomeColourG = 0,
                WelcomeColourB = 251,
                EmbedColourR = 112,
                EmbedColourG = 0,
                EmbedColourB = 251,
                Antilink = false,
                VerifiedGuild = false
            };
            Config.Add(newConf);
            Save();
            return newConf;
        }
    }
}