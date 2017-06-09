namespace vHackApi
{
    public static class DbManager
    {
        static vhackdbEntities model;
        static DbManager()
        {
            model = new vhackdbEntities();
        }
    }
}
