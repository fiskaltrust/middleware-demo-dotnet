namespace fiskaltrust.Middleware.Demo.Shared
{
    public static class CaseExtensions
    {
        public static bool HasFlag(this long cse, long flag) => (cse & flag) == flag;
    }
}
