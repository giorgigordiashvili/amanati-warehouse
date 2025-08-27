using Amanati.ge.Data;
using Amanati.ge.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

#if WINDOWS
            using Microsoft.UI;
            using Microsoft.UI.Windowing;
            using Windows.Graphics;
#endif

namespace Amanati.ge
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });



            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<SettingsService>();

            //builder.Services.AddSingleton<WeatherForecastService>();

            builder.Services.AddTransient<ConfigPageTimer>();
            builder.Services.AddTransient<NotificationTimer>();
            builder.Services.AddTransient<IdleTimer>();
            builder.Services.AddTransient<OperatorIsActiveCheckTimer>();
            builder.Services.AddTransient<IDialogService, DialogService>();

//            Microsoft.Maui.Handlers.ScrollViewHandler.Mapper.AppendToMapping("Disable_Bounce", (handler, view) =>
//            {
//#if IOS
//                handler.PlatformView.Bounces = false;
//#endif
//            });

#if !DEBUG


#if WINDOWS            

            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windowsLifecycleBuilder =>
                {
                    windowsLifecycleBuilder.OnWindowCreated(window =>
                    {
                        window.ExtendsContentIntoTitleBar = false;
                        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        var id = Win32Interop.GetWindowIdFromWindow(handle);
                        var appWindow = AppWindow.GetFromWindowId(id);
                        switch (appWindow.Presenter)
                        {
                            case OverlappedPresenter overlappedPresenter:
                                overlappedPresenter.SetBorderAndTitleBar(false, false);
                                overlappedPresenter.Maximize();
                                break;
                        }
                    });
                });
            });
#endif
#endif
            builder.UseMauiApp<App>().ConfigureMauiHandlers((handlers) => {
#if IOS
               handlers.AddHandler(typeof(Shell), typeof(CustomShellRenderer));  
#endif
            });

            return builder.Build();
        }
    }
}
