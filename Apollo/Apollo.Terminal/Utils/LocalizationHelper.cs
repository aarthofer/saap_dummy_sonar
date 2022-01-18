using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Apollo.Terminal.Utils
{
    public enum ApolloLanguages { DE, EN }
    public abstract class LocalizationHelper
    {
        private static ApolloLanguages currentLocale = ApolloLanguages.DE;

        public static ApolloLanguages CurrentLocale { get => currentLocale; set => currentLocale = value; }
        public static CultureInfo CurrentCulture
        {
            get
            {
                return GetCultureInfo(currentLocale);
            }
        }

        public static void ChangeCurrentLanguage(ApolloLanguages locale)
        {
            if (CurrentLocale == locale)
            {
                return;
            }

            CurrentLocale = locale;
            CultureInfo culture = GetCultureInfo(locale);



            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var locRes = new Uri($"i18n/Strings.{locale}.xaml", UriKind.RelativeOrAbsolute);

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = locRes });
        }

        private static CultureInfo GetCultureInfo(ApolloLanguages locale)
        {
            switch (locale)
            {
                case ApolloLanguages.DE:
                    return new CultureInfo("de");
                case ApolloLanguages.EN:
                    return new CultureInfo("en");
                default:
                    return new CultureInfo("de");
            }
        }
    }
}
