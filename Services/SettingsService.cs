using IdeaMaker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace IdeaMaker.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _settings = new();

        public event EventHandler? PointsChanged;

        public SettingsService()
        {
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            _settingsPath = Path.Combine(appFolder, "settings.json");
            LoadSettings();
        }

        public string ApiKey
        {
            get => _settings.ApiKey;
            set
            {
                _settings.ApiKey = value;
                SaveSettings();
            }
        }

        public string SystemPrompt
        {
            get => _settings.SystemPrompt;
            set
            {
                _settings.SystemPrompt = value;
                SaveSettings();
            }
        }

        public int Points
        {
            get => _settings.Points;
            set
            {
                _settings.Points = value;
                SaveSettings();
                PointsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int CompletedCount
        {
            get => _settings.CompletedCount;
            set
            {
                _settings.CompletedCount = value;
                SaveSettings();
            }
        }

        public List<IdeaTask> History => _settings.History;
        public List<TrashTask> TrashTasks => _settings.TrashTasks;

        public void AddHistory(IdeaTask task)
        {
            _settings.History.Insert(0, task);
            if (_settings.History.Count > 100)
                _settings.History.RemoveRange(100, _settings.History.Count - 100);
            SaveSettings();
        }

        public void AddTrash(TrashTask task)
        {
            _settings.TrashTasks.Insert(0, task);
            SaveSettings();
        }

        public void RemoveTrash(string taskId)
        {
            var task = _settings.TrashTasks.Find(t => t.Id == taskId);
            if (task != null)
            {
                _settings.TrashTasks.Remove(task);
                SaveSettings();
            }
        }

        public void ClearHistory()
        {
            _settings.History.Clear();
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsPath);
                    var loaded = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (loaded != null)
                        _settings = loaded;
                }
                catch
                {
                    _settings = new AppSettings();
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
            }
        }
    }
}
