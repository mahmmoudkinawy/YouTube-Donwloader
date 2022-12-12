namespace API.Features.Playlists;

public class PlaylistDonwloaderFeature
{
    public class Endpoint : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("api/playlists/donwload");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken c)
        {
            var _youtubeClient = new YoutubeClient();

            var videos = await _youtubeClient.Playlists
                .GetVideosAsync(r.Url, c);

            int counter = 0;

            foreach (var video in videos)
            {
                var streamManifest = await _youtubeClient.Videos.Streams
                    .GetManifestAsync(video.Url, c);

                var streamInfo = streamManifest
                    .GetMuxedStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestVideoQuality();

                var videoName = $"{++counter}-{video.Title}.{streamInfo.Container}";

                videoName = videoName.Replace(@"|", @"-");
                videoName = videoName.Replace(@"\", @"-");
                videoName = videoName.Replace(@"/", @"-");
                videoName = videoName.Replace("\"", "'");
                videoName = videoName.Replace("?", " ");

                var fullPath =
                    @$"{r.LocalFilePath}\{videoName}";

                await _youtubeClient.Videos.Streams
                   .DownloadAsync(
                       streamInfo,
                       fullPath,
                       cancellationToken: c);
            }

            await SendOkAsync(new Response
            {
                Message = "Successfully donwloaded all playlist Videos."
            }, c);
        }
    }

    public class Request
    {
        public string Url { get; set; }
        public string LocalFilePath { get; set; }
    }

    public class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(_ => _.Url)
                .Matches("^(https|http):\\/\\/(?:www\\.)?youtube\\.com\\/watch\\?((v=.*&list=.*)|(list=.*&v=.*))(&.*)*$")
                .WithMessage("That's not a valid YouTube link!")
                .NotEmpty();

            RuleFor(_ => _.LocalFilePath)
                .NotEmpty();
        }
    }

    public class Response
    {
        public string Message { get; set; }
    }
}
