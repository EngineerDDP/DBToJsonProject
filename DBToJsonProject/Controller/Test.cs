using DBToJsonProject.Views.WorkSpace;

namespace DBToJsonProject.Controller
{
    class Test : IApplicationControl
    {
        public void Startup()
        {
            new DbSettingToolBox().Show();
        }
    }
}
