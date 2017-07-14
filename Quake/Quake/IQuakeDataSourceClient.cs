using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quake
{
    public interface IQuakeDataSourceClient
    {
        Coordinates MyLocation();
        void QuakeDataSourceNewQuake(Quake quake);
        void QuakeDataSourceChanged();
        void QuakeDataSourceDisplayMessage(string msg);
        QuakeConfig getConfiguration();
    }
}
