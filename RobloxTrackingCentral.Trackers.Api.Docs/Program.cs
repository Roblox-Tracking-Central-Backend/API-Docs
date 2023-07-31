using LibGit2Sharp;
using System.Reflection;
using System.Text.Json;
using System.Threading.Channels;

namespace RobloxTrackingCentral.Trackers.Api.Docs
{
    internal class Program
    {
        static readonly string? PersonalToken = Environment.GetEnvironmentVariable("RTC_APIDOCS_TOKEN");
        static readonly string? AuthUsername = Environment.GetEnvironmentVariable("RTC_APIDOCS_USER");

        static async Task Main(string[] args)
        {
            Console.WriteLine("Roblox Tracking Central");
            Console.WriteLine("API Docs Tracker");
            Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version}");

            string? personalToken = Environment.GetEnvironmentVariable("RTC_APIDOCS_TOKEN");
            string? authUsername = Environment.GetEnvironmentVariable("RTC_APIDOCS_USER");

            if (string.IsNullOrEmpty(personalToken) || string.IsNullOrEmpty(authUsername))
            {
                Console.WriteLine("Environment variables are missing or empty");
                return;
            }

            // TODO: sync w/ latest instead of deleting
            if (Directory.Exists(Constants.ClonePath))
            {
                Console.WriteLine("Deleting current clone directory");
                DirectoryHelper.ForceDelete(Constants.ClonePath);
            }

            Console.WriteLine("Cloning " + Constants.Tracker);
            string gitPath = Repository.Clone("https://github.com/" + Constants.Tracker + ".git", Constants.ClonePath);
            Repository repository = new Repository(gitPath);

            Console.WriteLine("Fetching APIs list from " + Constants.Backend);
            string apisJsonStr = await Http.Client.GetStringRetry("https://raw.githubusercontent.com/" + Constants.Backend + "/main/APIs.json");
            List<string> apis = JsonSerializer.Deserialize<List<string>>(apisJsonStr)!;

            Queue<string> apisQueue = new Queue<string>();
            foreach (string api in apis)
                apisQueue.Enqueue(api);

            Console.WriteLine($"Got {apis.Count} APIs");
            Console.WriteLine($"Using {Config.Default.Workers} workers");

            Console.WriteLine("Starting" + nameof(WorkerFactory));
            WorkerFactory factory = new WorkerFactory(repository, apisQueue);

            List<Task> workers = new List<Task>();

            for (int i = 1; i <= Config.Default.Workers; i++)
                workers.Add(factory.Create());

            Task.WaitAll(workers.ToArray());

            Console.WriteLine("Workers have finished");

            Console.WriteLine("Committing changes");

            try
            {
                var time = DateTimeOffset.Now;
                var signature = new Signature("Roblox Tracking Central", "rtc@rtc.local", time);
                var commit = repository.Commit($"{time.ToString("dd/MM/yyyy HH:mm:ss")} [{string.Join(", ", factory.Changes)}]", signature, signature);
                Console.WriteLine("Committing!");

                var remote = repository.Network.Remotes["origin"];
                var options = new PushOptions
                {
                    CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = AuthUsername, Password = PersonalToken }
                };
                var pushRefSpec = "refs/heads/main";
                repository.Network.Push(remote, pushRefSpec, options);
            }
            catch (EmptyCommitException) // any better way?
            {
                Console.WriteLine("No changes found");
            }
        }
    }
}