using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reseau.Tools.ConfigWatcher.Deamon.Rules;
using Reseau.Tools.ConfigWatcher.WinApplication.Annotations;
using Strilanc.Value;


namespace Reseau.Tools.ConfigWatcher.WinApplication.FileSystem
{

    using Rules = Lazy<IReadOnlyList<MoveRule>>;
    using RulePart = Tuple<string, string>;

    public class ConfigRearranger
    {
        private IReadOnlyList<MoveRule> _moveRules;

        private readonly string _path;
        private static readonly RegexOptions _invariantOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

        public ConfigRearranger([NotNull] string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            _path = path;
        }

        [NotNull]
        public async Task<IReadOnlyList<MoveRule>> GetMoveRules()
        {
            return _moveRules ?? (_moveRules = await ReadConfig());
        }

        [NotNull]
        private static async Task<IReadOnlyList<MoveRule>> ReadConfig()
        {
            const string path = @".\Resources\rules.json";
                
            if (!File.Exists(path)) 
                throw new InvalidOperationException("rules file is not found");

            return await Task.Run(() =>
            {
                var text = File.ReadAllText(path);

                return JsonConvert
                    .DeserializeObject<List<MoveRule>>(text)
                    .AsReadOnly();
            });
        }

        public async Task<StringBuilder> Rearrange()
        {
            var @out = new StringBuilder();
            foreach (var file in Directory.GetFiles(_path, "*.config", SearchOption.AllDirectories))
            {
                if (!File.Exists(file)) {continue;}

                var moveTo = await GetMoveDestination(file);
                if (!moveTo.HasValue) continue;
                
                var tuple = moveTo.ForceGetValue();
                var path = tuple.Item2.Contains("$") 
                    ? await TranslateAndMove(file, tuple.Item2) 
                    : await MoveFile(file, tuple.Item2);

                if (path == String.Empty)
                {
                    @out.AppendFormat("[KO] File {0} wasn't processed", file)
                        .AppendLine();
                    continue;
                }

                var comment = tuple.Item1.Replace("$PREV_FILE_NAME", Path.GetFileName(file));
                AppendComment(path, comment);
                Cleanup(path);

                @out.AppendFormat("[DONE] File {0} was processed", file)
                    .AppendLine();
            }
            return @out;
        }

        private async void AppendComment([NotNull] string path,
                                         [NotNull] string comment)
        {
            var @new = new StringBuilder()
                .Append("\r\n")
                .Append(comment)
                .ToString();

            await Task.Run(() => File.AppendAllText(path, @new));
        }

        private void Cleanup([NotNull] string path)
        {
            var folder = Path.GetDirectoryName(path);

            if (folder == null) return;

            Directory.GetFiles(folder, "*.config")
                .Where(x => x != path)
                .Where(x =>
                {
                    var file = (Path.GetFileName(x) ?? "").ToUpperInvariant();
                    return file.StartsWith("APP") || file.StartsWith("WEB");
                })
                .AsParallel()
                .ToList()
                .ForEach(File.Delete);
        }

        [NotNull]
        private async Task<string> TranslateAndMove([NotNull] string fullPath,
                                                    [NotNull] string newName)
        {
            if (!newName.Contains("$APP_NAME")) return String.Empty;

            var folder = Path.GetDirectoryName(fullPath);

            if (folder == null) return String.Empty;

            var configFile = Directory.GetFiles(folder, "*.exe.config").FirstOrDefault();

            return configFile == null
                ? String.Empty
                : await MoveFile(fullPath, Path.GetFileName(configFile));
        }

        [NotNull]
        private async Task<string> MoveFile([NotNull] string fullPath,
                                            [NotNull] string newName)
        {
            var destination = new StringBuilder()
                .Append(Path.GetDirectoryName(fullPath))
                .Append(Path.DirectorySeparatorChar)
                .Append(newName)
                .ToString();

            if (File.Exists(destination)) await Task.Run(() => File.Delete(destination));
            await Task.Run(() => File.Move(fullPath, destination));

            return destination;
        }

        [NotNull]
        private async Task<May<RulePart>> GetMoveDestination([NotNull] string filePath)
        {
            var empty = May<RulePart>.NoValue;
            var rule = (await GetMoveRules())
                .FirstOrDefault(x => filePath.Contains(x.PathContains));

            if (rule == null) return empty;

            var options = _invariantOptions;

            var moveResult = rule.Movements
                .Where(x => Regex.IsMatch(Path.GetFileName(filePath), x.Key, options))
                .Select(x => Tuple.Create(rule.CommentToPrepend, x.Value))
                .FirstOrDefault();

            return moveResult == null ? empty : moveResult.Maybe();
        }
    }
}