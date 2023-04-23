using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Localization
{
    /// <summary>
    /// Lang helper to get strings in current selected lang.
    /// </summary>
    public class LangHelper
    {
        /// <summary>
        /// Current Selected culture
        /// </summary>
        /// <remarks>
        /// If culture to set isnt available the culture doenst change
        /// </remarks>
        public static CultureInfo Culture
        {
            get => _Culture;
            set
            {
                // Test if culture is available
                try
                {
                    _ResManager.GetString("ChatWindow.Title", value);
                }
                catch
                {
                    return;
                }

                _Culture = value;
            }
        }

        /// <summary>
        /// Resource manager that contains lang strings
        /// </summary>
        private static readonly ResourceManager _ResManager;

        private static CultureInfo _Culture;

        /// <summary>
        /// Setup lang helper
        /// </summary>
        static LangHelper()
        {
            // Set default lang
            _Culture = new CultureInfo("en-US");
            _ResManager = new ResourceManager("Localization.Strings", Assembly.GetExecutingAssembly());

            // Try to set system lang
            Culture = CultureInfo.InstalledUICulture;
        }

        /// <summary>
        /// Get a string in the selected lang by the given lang key.
        /// </summary>
        /// <param name="langKey">Lang key for string</param>
        /// <returns>The string in the selected lang. If lang key not found the return value is <see langword="null"/>.</returns>
        public static string GetString(string langKey)
        {
            try
            {
                return _ResManager.GetString(langKey, _Culture);
            }
            catch (MissingManifestResourceException)     // Thrown when resource key not found
            {
                return langKey;
            }
        }

        /// <summary>
        /// Get a string in the selected lang by the given lang key and insert given strings.
        /// <para>
        /// Replace symbol in lang string is '{i}'.
        /// </para>
        /// <para>
        /// If too many insert strings the remain strings will ignore and if not enough insert strings given the rest insert places will be ignored.
        /// </para>
        /// </summary>
        /// <param name="langKey">Lang key for string</param>
        /// <param name="insert">Strings to insert</param>
        /// <returns>The string in the selected lang. If lang key not found the return value is <see langword="null"/>.</returns>
        public static string GetString(string langKey, params string[] insert)
        {
            // Get base lang string and check it
            string baseString = GetString(langKey);

            if (string.IsNullOrEmpty(baseString))
                return null;

            // Replace symbols with given strings to insert
            string readyString = baseString;
            for (int i = 0; i < insert.Length; i++)
                readyString = readyString.Replace($"{{{i}}}", insert[i]);

            return readyString;
        }
    }
}
