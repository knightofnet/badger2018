using AryxDevLibrary.utils.logger;
using Badger2018.business.dbb;
using Badger2018.dto.bdd;
using Badger2018.services.bddLastLayer;

namespace Badger2018.services
{
    public class AbsencesServices
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public void InsertAbsence(AbsencesEntryDto abs)
        {
            _logger.Debug("InsertAbsence(abs: {0})", abs);

            DbbAccessManager dbb = DbbAccessManager.Instance;
            AbsencesBddLayer.InsertAbsence(dbb, abs);

            _logger.Debug("FIN - InsertAbsence(...)");
        }

    }
}
