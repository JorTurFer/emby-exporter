using emby_exporter.Entities;
using Prometheus;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace emby_exporter.Services
{
    public class Scrapper
    {
        private readonly Gauge EmbyLibrary = Metrics.CreateGauge("emby_library_size", "Emby Library Size", new GaugeConfiguration
        {
            LabelNames = new[] { "type" }
        });

        private readonly Gauge EmbyUsers = Metrics.CreateGauge("emby_users_total", "Emby Users");
        private readonly Gauge EmbySessions = Metrics.CreateGauge("emby_video_sessions", "Emby Video Sessions", new GaugeConfiguration
        {
            LabelNames = new[] { "type" }
        });
        private readonly Gauge EmbyDevices = Metrics.CreateGauge("emby_devices_total", "Emby Devices");
        private readonly Gauge EmbyInfo = Metrics.CreateGauge("emby_info", "Emby Info", new GaugeConfiguration
        {
            LabelNames = new[] { "id","operating_system","server_name","version","wan_address" }
        });


        private readonly HttpClient _httpClient;

        public Scrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void Configure()
        {
            Metrics.DefaultRegistry.AddBeforeCollectCallback(async (cancel) =>
            {
                var response = await _httpClient.GetAsync("/Items/Counts");
                var content = await response.Content.ReadAsStringAsync();
                var summary = JsonSerializer.Deserialize<ItemsResponse>(content);
                EmbyLibrary.WithLabels("movies").Set(summary.MovieCount);
                EmbyLibrary.WithLabels("series").Set(summary.SeriesCount);
                EmbyLibrary.WithLabels("episodes").Set(summary.EpisodeCount);

                response = await _httpClient.GetAsync("/Users");
                content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<IList<UsersResponse>>(content);
                EmbyUsers.Set(users.Count);

                response = await _httpClient.GetAsync("/Devices");
                content = await response.Content.ReadAsStringAsync();
                var devices = JsonSerializer.Deserialize<DevicesResponse>(content);
                EmbyDevices.Set(devices.TotalRecordCount);

                response = await _httpClient.GetAsync("/Sessions");
                content = await response.Content.ReadAsStringAsync();
                var sessions = JsonSerializer.Deserialize<IList<SessionsResponse>>(content);
                var videoSessions = sessions.Where(x=>x.NowPlayingItem is not null).ToList();
                EmbySessions.WithLabels("direct-play").Set(videoSessions.Count(x => x.TranscodeType == TranscodeType.Direct));
                EmbySessions.WithLabels("audio-transcode").Set(videoSessions.Count(x => x.TranscodeType == TranscodeType.Audio));
                EmbySessions.WithLabels("video-transcode").Set(videoSessions.Count(x => x.TranscodeType == TranscodeType.Video));
                EmbySessions.WithLabels("full-transcode").Set(videoSessions.Count(x => x.TranscodeType == TranscodeType.Full));


                response = await _httpClient.GetAsync("/System/Info");
                content = await response.Content.ReadAsStringAsync();
                var serverInfo = JsonSerializer.Deserialize<ServerInfoResponse>(content);
                EmbyInfo.WithLabels(serverInfo.Id, serverInfo.OperatingSystem,serverInfo.ServerName,serverInfo.Version,serverInfo.WanAddress).IncTo(1);
            });
        }
    }
}
