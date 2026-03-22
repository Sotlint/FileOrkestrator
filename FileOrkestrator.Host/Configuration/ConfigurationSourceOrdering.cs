using Microsoft.Extensions.Configuration.Json;

namespace FileOrkestrator.Host.Configuration;

/// <summary>
/// Настройка порядка источников: по умолчанию user secrets идут после JSON и перезаписывают файлы.
/// Здесь секреты разработчика подключаются раньше <c>appsettings*.json</c>, чтобы значения из JSON имели приоритет.
/// Переменные окружения и аргументы командной строки по-прежнему идут последними и сильнее файлов.
/// </summary>
internal static class ConfigurationSourceOrdering
{
    private const string UserSecretsSourceTypeName = "UserSecretsConfigurationSource";

    /// <summary>
    /// Перемещает источник user secrets перед первым JSON-файлом с <c>appsettings</c> в имени.
    /// </summary>
    public static void PrioritizeJsonFilesOverUserSecrets(ConfigurationManager configuration)
    {
        var sources = configuration.Sources;
        for (var i = 0; i < sources.Count; i++)
        {
            if (sources[i].GetType().Name != UserSecretsSourceTypeName)
                continue;

            var userSecrets = sources[i];
            sources.RemoveAt(i);

            var insertBefore = -1;
            for (var j = 0; j < sources.Count; j++)
            {
                if (sources[j] is JsonConfigurationSource json &&
                    json.Path != null &&
                    json.Path.Contains("appsettings", StringComparison.OrdinalIgnoreCase))
                {
                    insertBefore = j;
                    break;
                }
            }

            if (insertBefore < 0)
                sources.Insert(0, userSecrets);
            else
                sources.Insert(insertBefore, userSecrets);

            return;
        }
    }
}
