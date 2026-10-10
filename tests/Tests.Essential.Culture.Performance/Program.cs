using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using ArkheideSystem.Essential.Culture;

namespace ArkheideSystem.Essential.Culture.Performance;

internal static class Program
{
    private static readonly string[] Cultures = ["en-US", "fr", "zh-CN", "pt-BR", "de-DE", "es-ES", "it-IT", "ja-JP", "ko-KR", "ru-RU",
        "ar-SA", "nl-NL", "pl-PL", "tr-TR", "sv-SE", "da-DK", "fi-FI", "nb-NO", "cs-CZ", "uk-UA",
        "el-GR", "he-IL", "hi-IN", "th-TH", "vi-VN", "id-ID", "ro-RO", "hu-HU", "sk-SK", "bg-BG"];
    private static int sink;

    private static void Main(string[] args)
    {
        var output = Path.GetFullPath(args.Length == 0 ? "artifacts/performance/current.json" : args[0]);
        var scenarios = new[] { (100, 2, 1), (1000, 10, 1), (10000, 30, 1), (10000, 10, 10), (10000, 10, 100) };
        if (args.Length > 1 && args[1] == "large") scenarios = [(10000, 30, 1)];
        var results = new List<object>();
        var assembly = typeof(LocalizationCatalog).Assembly;
        var optionsType = assembly.GetType("ArkheideSystem.Essential.Culture.CatalogLoadOptions");
        foreach (var (keys, languages, modules) in scenarios)
        {
            var documents = Enumerable.Range(0, modules).Select(module => CreateJson(keys, languages, module, modules)).ToArray();
            var directory = Path.Combine(Path.GetTempPath(), "culture-performance-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var paths = documents.Select((document, index) =>
                {
                    var path = Path.Combine(directory, $"Culture.{index}.json");
                    File.WriteAllText(path, document);
                    return path;
                }).ToArray();
                if (modules > 1 && optionsType is null) continue;
                results.Add(Run(keys, languages, modules, "all", Factory(documents, paths, optionsType, false)));
                if (optionsType is not null && languages > 2)
                    results.Add(Run(keys, languages, modules, "enabled-two", Factory(documents, paths, optionsType, true)));
            }
            finally
            {
                foreach (var path in Directory.GetFiles(directory)) File.Delete(path);
                Directory.Delete(directory);
            }
        }
        var report = new
        {
            Runtime = RuntimeInformation.FrameworkDescription, OS = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), ProcessorCount = Environment.ProcessorCount,
            AssemblyVersion = assembly.GetName().Version?.ToString(), TimestampUtc = DateTimeOffset.UtcNow,
            ColdSamples = 7, HotSamples = 7, HotIterations = 200_000, WarmupIterations = 20_000,
            ProcessPeakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64, Results = results
        };
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Saved {results.Count} scenarios to {output}; sink={sink}.");
    }

    private static Func<LocalizationCatalog> Factory(string[] documents, string[] paths, Type? optionsType, bool filter)
    {
        object? options = optionsType is null ? null : Activator.CreateInstance(optionsType,
            new object?[] { filter ? Cultures.Take(2).ToArray() : null, null });
        if (documents.Length == 1)
        {
            if (!filter) return () => LocalizationCatalog.FromJson(documents[0]);
            var method = typeof(LocalizationCatalog).GetMethod("FromJson", [typeof(string), typeof(string), optionsType!])!;
            return () => (LocalizationCatalog)method.Invoke(null, [documents[0], "en-US", options])!;
        }
        var fromFiles = typeof(LocalizationCatalog).GetMethod("FromFiles")!;
        return () => (LocalizationCatalog)fromFiles.Invoke(null, [paths, "en-US", filter ? options : null])!;
    }

    private static object Run(int keys, int languages, int modules, string mode, Func<LocalizationCatalog> factory)
    {
        _ = new LocalizationContext(factory()).Parse("K0");
        var coldTimes = new double[7];
        var coldAllocated = new long[7];
        var retained = new long[7];
        for (var sample = 0; sample < 7; sample++)
        {
            (coldTimes[sample], coldAllocated[sample], retained[sample]) = Cold(factory);
        }
        var shared = factory();
        var firstStart = Stopwatch.GetTimestamp();
        var context = new LocalizationContext(shared);
        var firstSelectionMs = Stopwatch.GetElapsedTime(firstStart).TotalMilliseconds;
        var args = new object?[] { 1, 2, 3, 4 };
        var operations = new Dictionary<string, object>
        {
            ["raw-hit"] = Hot(() => sink ^= context.Parse("K0").Length),
            ["token-hit"] = Hot(() => sink ^= context.Parse("Key.K0").Length),
            ["raw-miss"] = Hot(() => sink ^= context.Parse("Missing").Length),
            ["token-miss"] = Hot(() => sink ^= context.Parse("Key.Missing").Length),
            ["format-one"] = Hot(() => sink ^= context.Parse("F1", 1).Length),
            ["format-two"] = Hot(() => sink ^= context.Parse("F2", 1, 2).Length),
            ["format-three"] = Hot(() => sink ^= context.Parse("F3", 1, 2, 3).Length),
            ["format-many"] = Hot(() => sink ^= context.Parse("FN", args).Length),
            ["warm-switch"] = Hot(() => context.SetCulture(context.Culture == "en-US" ? "fr" : "en-US"), 2_000)
        };
        if (mode == "all")
        {
            operations["parent-switch"] = Hot(() => context.SetCulture(context.Culture == "fr-CA" ? "fr-FR" : "fr-CA"), 100);
            operations["custom-switch"] = Hot(() => context.SetCulture(string.Equals(context.Culture, "x-demo", StringComparison.OrdinalIgnoreCase) ? "x-test" : "x-demo"), 100);
        }
        var concurrentStart = Stopwatch.GetTimestamp();
        Parallel.For(0, 100, i =>
        {
            var scoped = new LocalizationContext(shared, i % 2 == 0 ? "en-US" : "fr");
            for (var j = 0; j < 1000; j++) scoped.TryParse("K0", out _);
        });
        var concurrentMs = Stopwatch.GetElapsedTime(concurrentStart).TotalMilliseconds;
        Console.WriteLine($"{keys} keys / {languages} languages / {modules} modules / {mode}: load {Median(coldTimes):F2} ms; retained {Median(retained):N0} B");
        return new { Keys = keys, Languages = languages, Modules = modules, Mode = mode,
            ColdLoadMedianMs = Median(coldTimes), ColdLoadP95Ms = Percentile(coldTimes, .95),
            ColdAllocatedMedianBytes = Median(coldAllocated), RetainedMedianBytes = Median(retained), FirstSelectionMs = firstSelectionMs,
            Concurrent100ScopesMs = concurrentMs, Operations = operations };
    }

    private static object Hot(Action operation, int iterations = 200_000)
    {
        for (var i = 0; i < Math.Min(20_000, iterations); i++) operation();
        var times = new double[7];
        var allocation = new long[7];
        for (var sample = 0; sample < 7; sample++)
        {
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var start = Stopwatch.GetTimestamp();
            for (var i = 0; i < iterations; i++) operation();
            times[sample] = Stopwatch.GetElapsedTime(start).TotalNanoseconds / iterations;
            allocation[sample] = GC.GetAllocatedBytesForCurrentThread() - allocated;
        }
        return new { Iterations = iterations, Warmup = Math.Min(20_000, iterations), MedianNs = Median(times), P95Ns = Percentile(times, .95), MedianAllocatedBytesPerCall = Median(allocation) / iterations };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (double Milliseconds, long Allocated, long Retained) Cold(Func<LocalizationCatalog> factory)
    {
        Collect();
        var memory = GC.GetTotalMemory(true);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        var start = Stopwatch.GetTimestamp();
        var catalog = factory();
        var milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        var allocation = GC.GetAllocatedBytesForCurrentThread() - allocated;
        var retained = GC.GetTotalMemory(true) - memory;
        GC.KeepAlive(catalog);
        return (milliseconds, allocation, retained);
    }

    private static string CreateJson(int keys, int languages, int module, int modules)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            for (var key = module; key < keys; key += modules) WriteKey(writer, "K" + key, $"Translation {key} " + new string('x', 80), languages);
            if (module == 0)
            {
                WriteKey(writer, "F1", "Value {0:N2}", languages);
                WriteKey(writer, "F2", "Values {0} {1}", languages);
                WriteKey(writer, "F3", "Values {0} {1} {2}", languages);
                WriteKey(writer, "FN", "Values {0} {1} {2} {3}", languages);
            }
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteKey(Utf8JsonWriter writer, string key, string text, int languages)
    {
        writer.WritePropertyName(key);
        writer.WriteStartObject();
        foreach (var culture in Cultures.Take(languages)) writer.WriteString(culture, culture + " " + text);
        writer.WriteEndObject();
    }

    private static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    private static double Median(double[] values) => Percentile(values, .5);
    private static double Median(long[] values) => Median(values.Select(value => (double)value).ToArray());
    private static double Percentile(double[] values, double percentile) => values.Order().ElementAt((int)Math.Ceiling((values.Length - 1) * percentile));
}
