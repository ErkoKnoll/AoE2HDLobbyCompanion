using Backend.Constants;
using Backend.Global;
using Backend.Utils;
using Commons.Models;
using Database;
using Database.Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class ConfigurationController : IDisposable {
        private IRepository _repository { get; set; }

        public ConfigurationController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet]
        public Configuration Get() {
            var configuration = _repository.KeyValuePairs.SingleOrDefault(k => k.Key == Keys.CONFIGURATION);
            if (configuration == null) {
                var cmdPath = "C:\\Windows\\System32\\cmd.exe";
                try {
                    var sysRoot = Environment.GetEnvironmentVariable("SystemRoot");
                    if (sysRoot != null && sysRoot.Length > 0) {
                        cmdPath = sysRoot + "\\System32\\cmd.exe";
                    }
                    LogUtils.Info("Setting CMD path to: " + cmdPath);
                } catch (Exception e) {
                    LogUtils.Error("Failed to find CMD path. Defaulting to: " + cmdPath, e);
                }
                configuration = new KeyValuePair {
                    Key = Keys.CONFIGURATION,
                    Value = JsonConvert.SerializeObject(new Configuration {
                        ClientId = Guid.NewGuid().ToString(),
                        ShowOverlay = true,
                        ShowOverlayWhenFocused = false,
                        CmdPath = cmdPath
                    })
                };
                _repository.Add(configuration);
                _repository.SaveChanges();
            }
            var config = JsonConvert.DeserializeObject<Configuration>(configuration.Value);
            ApplyConfiguration(config);
            return config;
        }

        [HttpPut]
        public void Put([FromBody] Configuration configuration) {
            var existingConfiguration = _repository.KeyValuePairs.SingleOrDefault(k => k.Key == Keys.CONFIGURATION);
            if (!configuration.ShowOverlay) {
                configuration.ShowOverlayWhenFocused = false;
                configuration.ShowOverlayWhenInLobby = false;
            }
            if (existingConfiguration == null) {
                _repository.Add(new KeyValuePair {
                    Key = Keys.CONFIGURATION,
                    Value = JsonConvert.SerializeObject(configuration)
                });
            } else {
                existingConfiguration.Value = JsonConvert.SerializeObject(configuration);
            }
            _repository.SaveChanges();
            ApplyConfiguration(configuration);
        }

        private void ApplyConfiguration(Configuration configuration) {
            Variables.Config = configuration;
            if (Variables.OverlayWindow != null) {
                Variables.OverlayWindow.UpdateConfiguration(configuration, Variables.LobbySession);
            }
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}
