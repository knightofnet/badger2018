namespace Badger2018.services
{
    public class ServicesMgr
    {
        private static ServicesMgr _instance;

        public static ServicesMgr Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServicesMgr();
                }
                return _instance;
            }

        }

        public BadgeagesServices BadgeagesServices { get; private set; }

        public JoursServices JoursServices { get; private set; }
        public AbsencesServices AbsencesServices { get; private set; }


        public ServicesMgr()
        {
            BadgeagesServices = new BadgeagesServices();
            JoursServices = new JoursServices();
            AbsencesServices = new AbsencesServices();
        }

    }
}
