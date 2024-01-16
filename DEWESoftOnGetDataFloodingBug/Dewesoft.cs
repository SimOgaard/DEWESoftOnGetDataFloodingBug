using DEWESoft;

namespace DEWESoftOnGetDataFloodingBug
{
    public sealed class Dewesoft
    {
        public static bool StartDewesoftApplication(out App? app)
        {
            const string ProgID = "DEWEsoft.App";
            try
            {
                app = new App();

                if (app is null)
                {
                    Type? appType = Type.GetTypeFromProgID(ProgID);
                    if (appType is null)
                    {
                        Console.WriteLine("Unable to find DEWESoftX type.");
                        return false;
                    }
                    app = StartInterfaceApp(appType) as App;

                    if (app is null)
                    {
                        Console.WriteLine("Unable to start DEWESoftX.exe.");
                        return false;
                    }
                }
                app.Init();
                app.StayOnTop = false;
                app.Visible = true;
                app.SuppressMessages = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to start DEWESoftX.exe.\n{0}", ex.Message);
                app = null;
                return false;
            }

            return true;
        }

        private static object? StartInterfaceApp(Type AppType)
        {
            try
            {
                return Activator.CreateInstance(AppType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CaptureDevice, StartInterfaceApp: Unable to create instance of DEWESoftX.exe.\n\n{0}", ex.Message);
            }
            return null;
        }
    }
}
