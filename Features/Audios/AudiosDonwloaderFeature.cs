namespace API.Features.Audios;
public sealed class AudiosDonwloaderFeature
{
    public sealed class Endpoint : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("api/audios/donwload");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken c)
        {
            var youtubeClient = new YoutubeClient();

            var audio = await youtubeClient.Videos
                .GetAsync(r.Url, c);
            
            var streamManifest = await youtubeClient.Videos.Streams
                .GetManifestAsync(audio.Url, c);

            var data = streamManifest
                .GetAudioOnlyStreams()
                .Where(_ => _.Container == Container.Mp3)
                .FirstOrDefault();

            var streamInfo = streamManifest
                .GetVideoOnlyStreams()
                .Where(s => s.Container == Container.Mp3)
                .GetWithHighestBitrate();

            var audioName = $"{audio.Title}.{streamInfo.Container}";

            audioName = audioName.Replace(@"|", @"-");
            audioName = audioName.Replace(@"\", @"-");
            audioName = audioName.Replace(@"/", @"-");
            audioName = audioName.Replace("\"", "'");
            audioName = audioName.Replace("?", " ");

            var fullPath = @$"{r.LocalFilePath}\{audioName}";

            await youtubeClient.Videos.Streams
               .DownloadAsync(
                   streamInfo,
                   fullPath,
                   cancellationToken: c);

            await SendOkAsync(new Response
            {
                Message = "Successfully donwloaded The Audio."
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
