namespace rUIAppModelTester.AppModel;

public static class AppThemeModes
{
    public const string System = "system";
    public const string Light = "light";
    public const string Dark = "dark";

    public static bool IsSupported(string? value)
    {
        return value is System or Light or Dark;
    }
}
