using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer.business
{
    public interface IJobInterface
    {

        void DoJob(AppArgsDto prgOptions);

    }
}
