using System;
using System.Collections;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler.WindowsServiceRunner
{
    public static class QuartzServiceRunner
    {
        public static int Run(string[] args, Installer installer, QuartzService service)
        {
            bool install = false, uninstall = false, console = false, rethrow = false;
            try
            {
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "-i":
                        case "-install":
                            install = true;
                            break;
                        case "-u":
                        case "-uninstall":
                            uninstall = true;
                            break;
                        case "-c":
                        case "-console":
                            console = true;
                            break;
                        default:
                            Console.Error.WriteLine("Argument not expected: " + arg);
                            break;
                    }
                }
                if (uninstall)
                {
                    Install(installer, true, args);
                }
                if (install)
                {
                    Install(installer, false, args);
                }
                if (console)
                {
                    Console.WriteLine("Starting...");
                    service.Start(args);
                    Console.WriteLine("System running; press Enter key to stop");
                    Console.ReadLine();
                }
                else if (!(install || uninstall))
                {
                    rethrow = true; // so that windows sees error... 
                    ServiceBase[] services = {service};
                    ServiceBase.Run(services);
                    rethrow = false;
                }
                return 0;
            }
            catch (Exception ex)
            {
                if (rethrow) throw;
                Console.Error.WriteLine(ex);
                return -1;
            }
            finally
            {
                if (console)
                {
                    service.Stop();
                    Console.WriteLine("System stopped");
                }
            }
        }

        static void Install(Installer installer, bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing");
                using (var inst = new AssemblyInstaller(installer.GetType().Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
