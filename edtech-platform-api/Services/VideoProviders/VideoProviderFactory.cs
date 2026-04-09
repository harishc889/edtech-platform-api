using System;
using Microsoft.Extensions.Configuration;

namespace edtech_platform_api.Services.VideoProviders
{
    public class VideoProviderFactory
    {
        private readonly IConfiguration _config;

        public VideoProviderFactory(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IVideoProvider CreateProvider(string? providerName = null)
        {
            // Use configured provider or default to Zoom
            var provider = providerName ?? _config["Video:DefaultProvider"] ?? "Zoom";

            return provider.ToLower() switch
            {
                "zoom" => new ZoomProvider(_config),
                "googlemeet" => new GoogleMeetProvider(_config),
                "custom" => new CustomVideoProvider(_config),
                // Add more providers here
                // "teams" => new MicrosoftTeamsProvider(_config),
                // "jitsi" => new JitsiProvider(_config),
                _ => throw new NotSupportedException($"Video provider '{provider}' is not supported")
            };
        }
    }
}
