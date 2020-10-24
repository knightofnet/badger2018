using System;
using Badger2018.constants;

namespace Badger2018.dto.bdd
{
    public class AbsencesEntryDto
    {

        public DateTime DateJour { get; set; }

        public EnumTypesJournees PartJour { get; set; }
        public EnumTypesAbsences TyAbs { get; set; }

    }
}
