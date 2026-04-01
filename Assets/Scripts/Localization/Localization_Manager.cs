using UnityEngine;
using UnityEngine.Localization.Settings;

public class Localization_Manager : MonoBehaviour
{
    private async void Awake()
    {
        await LocalizationSettings.InitializationOperation.Task;

        var locale = LocalizationSettings.AvailableLocales.GetLocale("en");

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
    }
}