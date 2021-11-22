using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Olivianet.Controller.WebApp.Interfaces
{
    public interface ICertificateStore
    {
        X509Certificate2 Generate(string certificateName);
        bool Store(X509Certificate2 certificate, string certificateIdentifier);
        X509Certificate2 Get(string certificateIdentifier);
        bool IsValid(string certificateIdentifier);
    }
}
