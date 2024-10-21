using ManagedCommon;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.run_putty
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "78C15FE08BA94E11B564889A0ABE242E";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "run_putty";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "run_putty Description";

        private PluginInitContext Context { get; set; }


        private bool Disposed { get; set; }

        //during Unit Tests StringMatcher.Instance is null. Create private instance for this case
        public StringMatcher StringMatcherInstance => StringMatcher.Instance ?? new StringMatcher();

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var search = query.Search;
            var results = new List<Result>();

            using (var regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions"))
            {
                foreach (string subkeyname in regkey.GetSubKeyNames())
                {
                    string puttySessionName = Uri.UnescapeDataString(subkeyname);
                    if (puttySessionName.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                        || puttySessionName.Replace("-", "").Replace("_", "").Contains(search, StringComparison.CurrentCultureIgnoreCase)
                        || StringMatcherInstance.FuzzyMatch(search, puttySessionName).IsSearchPrecisionScoreMet())
                    {
                        results.Add(new Result
                        {
                            QueryTextDisplay = search,
                            Title = "PuTTY " + puttySessionName,
                            SubTitle = "PuTTY ssh connection to " + puttySessionName,
                            ToolTipData = new ToolTipData("Start PuTTY with saved session", "ssh to " + puttySessionName),
                            Action = _ =>
                            {
                                StartPuttyWithSavedSession(puttySessionName);
                                return true;
                            },
                            ContextData = search,
                            Icon = () => GetPuttyIconBitmap,
                        });
                    }
                }
            }


            return results;
        }

        private void StartPuttyWithSavedSession(string subkeyname)
        {
            System.Diagnostics.Process.Start("putty.exe", " -load \"" + subkeyname + "\"");

        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            //Context.API.ThemeChanged += OnThemeChanged;
            //UpdateIcon(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string search)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Start PuTTY with saved session",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xF20C", // Keyboard
                        //AcceleratorModifiers = ModifierKeys.Control,
                        //AcceleratorKey = Key.C,
                        Action = _ =>
                        {
                            StartPuttyWithSavedSession(search);
                            return true;
                        },
                    }
                ];
            }

            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }


        BitmapImage puttyIcon => new BitmapImage(new Uri("pack://application:,,,/Community.PowerToys.Run.Plugin.run-putty;component/Images/putty-icon.png"));

        //private void UpdateIcon(Theme theme) => puttyIcon =
        BitmapSource GetPuttyIconBitmap => new BitmapImage(new Uri("pack://application:,,,/Community.PowerToys.Run.Plugin.run-putty;component/" +
                                                            (Context.API.GetCurrentTheme() == Theme.Light || Context.API.GetCurrentTheme() == Theme.HighContrastWhite
                                                                    ? "Images/putty-icon.png"
                                                                    : "Images/putty-icon.png")));

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) 
        {
        }
    }
}
