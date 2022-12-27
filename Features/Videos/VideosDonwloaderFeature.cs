namespace API.Features.Videos;
public sealed class VideosDonwloaderFeature
{
    public sealed class Endpoint : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("api/videos/donwload");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken c)
        {
            var youtubeClient = new YoutubeClient();

            var video = await youtubeClient.Videos
                .GetAsync(r.Url, c);

            var streamManifest = await youtubeClient.Videos.Streams
                .GetManifestAsync(video.Url, c);

            var streamInfo = streamManifest
                .GetMuxedStreams()
                .Where(s => s.Container == Container.Mp4)
                // .GetWithHighestVideoQuality();
                .TryGetWithHighestVideoQuality();

            var videoName = $"{video.Title}.{streamInfo.Container}";

            videoName = videoName.Replace(@"|", @"-");
            videoName = videoName.Replace(@"\", @"-");
            videoName = videoName.Replace(@"/", @"-");
            videoName = videoName.Replace("\"", "'");
            videoName = videoName.Replace("?", " ");

            var fullPath =
                @$"{r.LocalFilePath}\{videoName}";

            await youtubeClient.Videos.Streams
               .DownloadAsync(
                   streamInfo,
                   fullPath,
                   cancellationToken: c);

            await SendOkAsync(new Response
            {
                Message = "Successfully donwloaded all playlist Videos."
            }, c);
        }
    }

    public sealed class Request
    {
        public string Url { get; set; }
        public string LocalFilePath { get; set; }
    }

    public sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(_ => _.Url)
                //.Matches("^(https|http):\\/\\/(?:www\\.)?youtube\\.com\\/watch\\?((v=.*&list=.*)|(list=.*&v=.*))(&.*)*$")
                //.WithMessage("That's not a valid YouTube link!")
                .NotEmpty();

            RuleFor(_ => _.LocalFilePath)
                .NotEmpty();
        }
    }

    public sealed class Response
    {
        public string Message { get; set; }
    }
}
