namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public sealed class TweakResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public static TweakResult Ok(string message)
        {
            return new TweakResult
            {
                Success = true,
                Message = message ?? string.Empty
            };
        }

        public static TweakResult Fail(string message)
        {
            return new TweakResult
            {
                Success = false,
                Message = message ?? string.Empty
            };
        }
    }
}