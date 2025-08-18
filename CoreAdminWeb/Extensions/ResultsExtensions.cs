namespace CoreAdminWeb.Extensions
{
    public static class ResultsExtensions
    {
        public static IResult WithFileDeletion(this IResult result, string filePath)
        {
            return new FileDeleteResult(result, filePath);
        }

        private sealed class FileDeleteResult : IResult
        {
            private readonly IResult _innerResult;
            private readonly string _filePath;

            public FileDeleteResult(IResult innerResult, string filePath)
            {
                _innerResult = innerResult;
                _filePath = filePath;
            }

            public async Task ExecuteAsync(HttpContext httpContext)
            {
                await _innerResult.ExecuteAsync(httpContext);
                try
                {
                    if (System.IO.File.Exists(_filePath))
                    {
                        System.IO.File.Delete(_filePath);
                    }
                }
                catch
                {
                    // Optionally log error
                }
            }
        }
    }
}
