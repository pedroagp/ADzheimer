using System;
using System.Linq;
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
                //
                // Hold current password all through the resetting

                var presentPassword = o.password;

                foreach (int i in Enumerable.Range(1, o.changescount))
                {
                    //
                    // Set next password

                    string nextPassword = $"{o.password}[{i}]";

                    //
                    // Change password

                    ChangePassword(o.domain, o.username, presentPassword, nextPassword);

                    //
                    // Hopefully keep under the radar

                    if (o.sleepTime.HasValue)
                    {
                        Thread.Sleep(o.sleepTime.GetValueOrDefault());
                    }

                    //
                    // nextPassword is the new thing :)

                    presentPassword = nextPassword;
                }

                //
                // finally reset to original password,
                // keeping IT happy

                ChangePassword(o.domain, o.username, presentPassword, o.password);
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

