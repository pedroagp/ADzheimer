using System;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using CommandLine;

namespace ADzheimer
{
    class Program
    {
        class Options
        {
            [Option('d', "domain", Required = true, HelpText = "Domain")]
            public string domain { get; set; }
            [Option('u', "username", Required = true, HelpText = "Username")]
            public string username { get; set; }
            [Option('p', "password", Required = true, HelpText = "Current password")]
            public string password { get; set; }
            [Option('s', "sleep", Required = false, HelpText = "Sleep time (ms) between password changes")]
            public int? sleepTime { get; set; }
            [Option('c', "changescount", Required = false, Default = 24, HelpText = "How many password change operations")]
            public int changescount { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => 
            {
                string dummyPw = "DummyP4$$word" + DateTime.Now.Ticks;
                ChangePassword(o.domain, o.username, o.password, $"{dummyPw}0");

                for (int i = 0; i < o.changescount - 1; i++)
                {
                    ChangePassword(o.domain, o.username, $"{dummyPw}{i}", $"{dummyPw}{i + 1}");
                    if (o.sleepTime.HasValue)
                    {
                        Thread.Sleep(o.sleepTime.GetValueOrDefault());
                    }
                }

                ChangePassword(o.domain, o.username, $"{dummyPw}{o.changescount}", o.password);
            });
            Console.WriteLine("Done, press any key to exit");
            Console.ReadKey();
        }

        public static void ChangePassword(string domain, string userName, string oldPassword, string newPassword)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, domain))
                {
                    using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
                        user.ChangePassword(oldPassword, newPassword);
                        Console.WriteLine($"Password set from {oldPassword} to {newPassword}");
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

