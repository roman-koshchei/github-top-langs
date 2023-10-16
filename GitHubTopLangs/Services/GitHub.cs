using Octokit;

namespace GitHubTopLangs.Services;

public class Github
{
    protected GitHubClient client = new(new ProductHeaderValue("roman-koshchei"));

    public Github(string token)
    {
        var tokenAuth = new Credentials(token);
        client.Credentials = tokenAuth;
    }

    private async Task<IEnumerable<Repository>> UserRepositories(string name)
    {
        return await client.Repository.GetAllForUser(name);
    }

    private async Task<IEnumerable<Repository>> UserWithOrgsRepositories(string name)
    {
        var repos = (await client.Repository.GetAllForUser(name)).ToList();

        var orgs = await client.Organization.GetAllForUser(name);
        foreach (var org in orgs)
        {
            var orgRepos = await client.Repository.GetAllForOrg(org.Login);
            repos.AddRange(orgRepos);
        }

        return repos;
    }

    public class LangCount
    {
        public string Name { get; set; }
        public ulong Count { get; set; }

        public LangCount(string name, ulong count)
        {
            Name = name;
            Count = count;
        }
    };

    private ulong ToUnsignedLong(long number)
    {
        if (number <= 0) return 0;
        return (ulong)number;
    }

    private async Task<IEnumerable<LangCount>> CountLangs(
        IEnumerable<Repository> repositories,
        IEnumerable<Func<Repository, bool>> skipRepoIfs,
        IEnumerable<string> skipLanguages)
    {
        var langs = new List<LangCount>();
        foreach (var repo in repositories)
        {
            if (skipRepoIfs.Any(skip => skip(repo))) continue;

            var repositoryLangs = await client.Repository.GetAllLanguages(repo.Id);
            foreach (var repositoryLang in repositoryLangs)
            {
                if (skipLanguages.Any(x => x.ToLower() == repositoryLang.Name.ToLower())) continue;

                var existingLang = langs.FirstOrDefault(x => x.Name == repositoryLang.Name);
                if (existingLang == null)
                {
                    langs.Add(new LangCount(repositoryLang.Name, ToUnsignedLong(repositoryLang.NumberOfBytes)));
                }
                else
                {
                    existingLang.Count += ToUnsignedLong(repositoryLang.NumberOfBytes);
                }
            }
        }
        return langs;
    }

    public async Task<IEnumerable<Lang>> CountUserLangs(
        string name,
        IEnumerable<string>? exclude,
        IEnumerable<string> hide,
        bool includePrivate = true,
        bool includeOrgs = true,
        bool includeForks = true,
        int count = 5
    )
    {
        // Build list of checks, if true then skip repository
        List<Func<Repository, bool>> skipRepoIfs = new();
        if (!includePrivate)
        {
            skipRepoIfs.Add((repo) => repo.Private);
        }
        if (exclude != null && exclude.Any())
        {
            skipRepoIfs.Add((repo) => exclude.Contains(repo.Name));
        }
        if (!includeForks)
        {
            skipRepoIfs.Add((repo) => repo.Fork);
        }

        IEnumerable<Repository> repos = includeOrgs
            ? await UserWithOrgsRepositories(name)
            : await UserRepositories(name);

        var langCounts = await CountLangs(repos, skipRepoIfs, hide);

        return PreparePercentLangs(langCounts, count);
    }

    //public async Task<IEnumerable<Lang>> CountOrgLangs(string name, int count, bool isPrivate, bool includeForks, string[]? hide = null, string[]? excludeRepos = null)
    //{
    //    Dictionary<string, long> langs = new();

    //    Action<string, long> addLang = hide == null
    //        ? ((langName, langCount) => ToDictionary(langName, langCount, ref langs))
    //        : ((langName, langCount) => HideToDictionary(langName, langCount, ref langs, hide));

    //    // build counter chain
    //    ILangCounter langCounter = new LangCounter(client, addLang);
    //    if (!isPrivate) langCounter = new PublicDecorator(langCounter);
    //    if (excludeRepos != null) langCounter = new RepoExcludeDecorator(langCounter, excludeRepos);
    //    if (!includeForks) langCounter = new NoForkDecorator(langCounter);

    //    ProfileCounter profileCounter = new OrgCounter(client, langCounter);
    //    // run counter chain
    //    await profileCounter.Count(name);

    //    return PrepareLangs(langs, count);
    //}

    private static IEnumerable<Lang> PreparePercentLangs(IEnumerable<LangCount> langCounts, int count)
    {
        var takenLangCounts = langCounts.OrderByDescending(lang => lang.Count).Take(count);

        ulong sum = 0;
        foreach (var lang in langCounts) sum += lang.Count;

        List<Lang> langsList = new(takenLangCounts.Count());
        foreach (var lang in takenLangCounts)
        {
            langsList.Add(new Lang(lang.Name, lang.Count * 1.0 / sum));
        }
        return langsList;
    }
}

public class Lang
{
    public string Name { get; set; }
    public double Percent { get; set; }

    public Lang(string name, double percent)
    {
        Name = name;
        Percent = percent;
    }

    public static Dictionary<string, string> Colors { get; } = new()
    {
      { "Mercury", "#ff2b2b" },
      { "TypeScript", "#3178c6" },
      { "PureBasic", "#5a6986" },
      { "Objective-C++", "#6866fb" },
      { "Self", "#0579aa" },
      { "edn", "#db5855" },
      { "NewLisp", "#87AED7" },
      { "Jupyter Notebook", "#DA5B0B" },
      { "Rebol", "#358a5b" },
      { "Frege", "#00cafe" },
      { "Dart","#2bb7f6" },
      { "AspectJ", "#a957b0" },
      { "Shell", "#89e051" },
      { "Web Ontology Language", "#9cc9dd" },
      { "xBase", "#403a40" },
      { "Eiffel", "#946d57" },
      { "RAML", "#77d9fb" },
      { "MTML", "#b7e1f4" },
      { "Racket", "#22228f" },
      { "Elixir", "#6e4a7e" },
      { "Nix", "#7e7eff" },
      { "SAS", "#B34936" },
      { "Agda", "#315665" },
      { "wisp", "#7582D1" },
      { "D", "#ba595e" },
      { "Kotlin", "#F18E33" },
      { "Opal", "#f7ede0" },
      { "Crystal", "#776791" },
      { "Objective-C", "#438eff" },
      { "ColdFusion CFC", "#ed2cd6" },
      { "Oz", "#fab738" },
      { "Mirah", "#c7a938" },
      { "Objective-J", "#ff0c5a" },
      { "Gosu", "#82937f" },
      { "FreeMarker", "#0050b2" },
      { "Ruby", "#701516" },
      { "Component Pascal", "#b0ce4e" },
      { "Arc", "#aa2afe" },
      { "Brainfuck", "#2F2530" },
      { "Nit", "#009917" },
      { "APL", "#5A8164" },
      { "Go", "#76e1fe" },
      { "Visual Basic", "#945db7" },
      { "PHP", "#4F5D95" },
      { "Cirru", "#ccccff" },
      { "SQF", "#3F3F3F" },
      { "Glyph", "#e4cc98" },
      { "Java", "#b07219" },
      { "MAXScript", "#00a6a6" },
      { "Scala", "#DC322F" },
      { "Makefile", "#427819" },
      { "ColdFusion", "#ed2cd6" },
      { "Perl", "#0298c3" },
      { "Lua", "#000080" },
      { "Vue", "#2c3e50" },
      { "Verilog", "#b2b7f8" },
      { "Factor", "#636746" },
      { "Haxe", "#df7900" },
      { "Pure Data", "#91de79" },
      { "Forth", "#341708" },
      { "Red", "#ee0000" },
      { "Hy", "#7790B2" },
      { "Volt", "#1F1F1F" },
      { "LSL" ,"#3d9970" },
      { "eC", "#913960" },
      { "CoffeeScript", "#244776" },
      { "HTML", "#e44b23"  },
      { "Lex", "#DBCA00" },
      { "API Blueprint", "#2ACCA8"  },
      { "Swift", "#ffac45" },
      { "C", "#555555" },
      { "AutoHotkey", "#6594b9" },
      { "Isabelle", "#FEFE00" },
      { "Metal", "#8f14e9" },
      { "Clarion", "#db901e" },
      { "JSONiq", "#40d47e" },
      { "Boo", "#d4bec1" },
      { "AutoIt", "#1C3552" },
      { "Clojure", "#db5855" },
      { "Rust", "#dea584" },
      { "Prolog", "#74283c" },
      { "SourcePawn", "#5c7611" },
      { "AMPL", "#E6EFBB" },
      { "FORTRAN", "#4d41b1" },
      { "ANTLR", "#9DC3FF" },
      { "Harbour", "#0e60e3" },
      { "Tcl", "#e4cc98" },
      { "BlitzMax", "#cd6400" },
      { "X10", "#4B6BEF" },
      { "PigLatin", "#fcd7de" },
      { "Lasso", "#999999" },
      { "ECL", "#8a1267" },
      { "VHDL", "#adb2cb" },
      { "Elm", "#60B5CC" },
      { "Propeller Spin", "#7fa2a7" },
      { "IDL", "#a3522f" },
      { "ATS", "#1ac620" },
      { "Ada", "#02f88c" },
      { "Unity3D Asset", "#ab69a1" },
      { "Nu", "#c9df40" },
      { "LFE", "#004200" },
      { "SuperCollider", "#46390b" },
      { "Oxygene", "#cdd0e3" },
      { "ASP", "#6a40fd" },
      { "Assembly", "#6E4C13" },
      { "Gnuplot", "#f0a9f0" },
      { "JFlex", "#DBCA00" },
      { "NetLinx", "#0aa0ff" },
      { "Turing", "#45f715" },
      { "Vala", "#fbe5cd" },
      { "Processing", "#0096D8" },
      { "Arduino", "#bd79d1" },
      { "FLUX", "#88ccff" },
      { "NetLogo", "#ff6375" },
      { "C#", "#5ecc64" },
      { "CSS", "#563d7c" },
      { "Emacs Lisp", "#c065db" },
      { "Stan", "#b2011d" },
      { "SaltStack", "#646464" },
      { "QML", "#44a51c" },
      { "Pike", "#005390" },
      { "LOLCODE", "#cc9900" },
      { "ooc", "#b0b77e" },
      { "Handlebars", "#01a9d6" },
      { "J", "#9EEDFF" },
      { "Mask", "#f97732" },
      { "EmberScript", "#FFF4F3" },
      { "TeX", "#3D6117" },
      { "Nemerle", "#3d3c6e" },
      { "KRL", "#28431f" },
      { "Ren'Py", "#ff7f7f" },
      { "Unified Parallel C", "#4e3617" },
      { "Golo", "#88562A" },
      { "Fancy", "#7b9db4" },
      { "OCaml", "#3be133" },
      { "Shen", "#120F14" },
      { "Pascal", "#b0ce4e" },
      { "F#", "#b845fc" },
      { "Puppet", "#302B6D" },
      { "ActionScript", "#882B0F" },
      { "Diff", "#88dddd" },
      { "Ragel in Ruby Host", "#9d5200" },
      { "Fantom", "#dbded5" },
      { "Zephir", "#118f9e" },
      { "Click", "#E4E6F3" },
      { "Smalltalk", "#596706" },
      { "DM", "#447265" },
      { "Ioke", "#078193" },
      { "PogoScript", "#d80074" },
      { "LiveScript", "#499886" },
      { "JavaScript", "#f1e05a" },
      { "VimL", "#199f4b" },
      { "PureScript", "#1D222D" },
      { "ABAP", "#E8274B" },
      { "Matlab", "#bb92ac" },
      { "Slash", "#007eff" },
      { "R", "#198ce7" },
      { "Erlang", "#B83998" },
      { "Pan", "#cc0000" },
      { "LookML", "#652B81" },
      { "Eagle", "#814C05" },
      { "Scheme", "#1e4aec" },
      { "PLSQL", "#dad8d8" },
      { "Python", "#3572A5" },
      { "Max", "#c4a79c" },
      { "Common Lisp", "#3fb68b" },
      { "Latte", "#A8FF97" },
      { "XQuery", "#5232e7" },
      { "Omgrofl", "#cabbff" },
      { "XC", "#99DA07" },
      { "Nimrod", "#37775b" },
      { "SystemVerilog", "#DAE1C2" },
      { "Chapel", "#8dc63f" },
      { "Groovy", "#e69f56" },
      { "Dylan", "#6c616e" },
      { "E", "#ccce35" },
      { "Parrot", "#f3ca0a" },
      { "Grammatical Framework", "#79aa7a" },
      { "Game Maker Language", "#8fb200" },
      { "Papyrus", "#6600cc" },
      { "NetLinx+ERB", "#747faa" },
      { "Clean", "#3F85AF" },
      { "Alloy", "#64C800" },
      { "Squirrel", "#800000" },
      { "PAWN", "#dbb284" },
      { "UnrealScript", "#a54c4d" },
      { "Standard ML", "#dc566d" },
      { "Slim", "#ff8f77" },
      { "Perl6", "#0000fb" },
      { "Julia", "#a270ba" },
      { "Haskell", "#29b544" },
      { "NCL", "#28431f" },
      { "Io", "#a9188d" },
      { "Rouge", "#cc0088" },
      { "C++", "#f34b7d" },
      { "AGS Script", "#B9D9FF" },
      { "Dogescript", "#cca760" },
      { "nesC", "#94B0C7" }
    };
}