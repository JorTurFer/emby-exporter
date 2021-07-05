using emby_exporter.Entities;
using Prometheus;
using System.Collections.Generic;
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
                EmbyLibrary.WithLabels("movies").IncTo(summary.MovieCount);
                EmbyLibrary.WithLabels("series").IncTo(summary.SeriesCount);
                EmbyLibrary.WithLabels("episodes").IncTo(summary.EpisodeCount);

                response = await _httpClient.GetAsync("/Users");
                content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<IList<UsersResponse>>(content);
                EmbyUsers.IncTo(users.Count);

                response = await _httpClient.GetAsync("/System/Info");
                content = await response.Content.ReadAsStringAsync();
                var serverInfo = JsonSerializer.Deserialize<ServerInfoResponse>(content);
                EmbyInfo.WithLabels(serverInfo.Id, serverInfo.OperatingSystem,serverInfo.ServerName,serverInfo.Version,serverInfo.WanAddress).IncTo(1);
            });
        }
    }
}
