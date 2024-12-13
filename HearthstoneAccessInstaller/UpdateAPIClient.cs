using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
namespace HearthstoneAccessInstaller;
public class Release
{
    public string? Hearthstone_Version { get; set; }
    public int Accessibility_Version { get; }
    public string? Changelog { get; set; }
    public DateTimeOffset? Upload_Time { get; set; }
    public string Url { get; }
    [JsonConstructor]
    public Release(int accessibility_version, string url)
    {
        (Accessibility_Version, Url) = (accessibility_version, url);
    }
}


public class ReleaseChannel
{
    public string Name { get; }
    public string Description { get; }
    public Release Latest_Release { get; }
    [JsonConstructor]
    public ReleaseChannel(string name, string description, Release latest_release)
    {
        (Name, Description, Latest_Release) = (name, description, latest_release);
    }

}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ReleaseChannel[]))]
internal partial class ChannelsJsonContext : JsonSerializerContext
{
}


public class UpdateClient
{
    private HttpClient httpClient;
    private const string BASE_ADDRESS = "https://hearthstoneaccess.com/api/v1/";
    public UpdateClient()
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(BASE_ADDRESS),
            Timeout = TimeSpan.FromSeconds(30),
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ReleaseChannel[]> GetReleaseChannels()
    {
        using (var response = await httpClient.GetAsync("release-channels"))
        {
            response.EnsureSuccessStatusCode();
            ReleaseChannel[]? channels = await response.Content.ReadFromJsonAsync<ReleaseChannel[]>(ChannelsJsonContext.Default.ReleaseChannelArray);
            if (channels == null) throw new ArgumentNullException();
            return channels;
        }
    }
}
