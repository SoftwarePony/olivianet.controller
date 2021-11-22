using Olivianet.Controller.WebApp.Interfaces;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Olivianet.Controller.WebApp.Stores
{
    public class FileCertificateStore : ICertificateStore
    {
        private readonly string _storePath;

        public FileCertificateStore(string storePath)
        {
            _storePath = storePath;
        }

        public X509Certificate2 Generate(string certificateName)
        {
            using (RSA key = RSA.Create(4096))
            {
                CertificateRequest parentReq = new CertificateRequest(
                    $"CN={certificateName}",
                    key,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                parentReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                parentReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

                X509Certificate2 parentCert = parentReq.CreateSelfSigned(
                        DateTimeOffset.UtcNow.AddDays(-1),
                        DateTimeOffset.UtcNow.AddDays(3650));

                return parentCert;
            }  
        }

        public X509Certificate2 Get(string certificateIdentifier)
        {
            return new X509Certificate2(Path.Combine(_storePath, certificateIdentifier));
        }

        public bool IsValid(string certificateIdentifier)
        {
            string certificatePath = Path.Combine(_storePath, certificateIdentifier);
            bool fileExists = File.Exists(certificatePath);
            X509Certificate2 certificate = null;
            if (!fileExists)
            {
                return false;
            }

            try
            {
                certificate = new X509Certificate2(certificatePath);
            }
            catch
            {
                return false;
            }

            if (DateTime.UtcNow < certificate.NotBefore || DateTime.UtcNow > certificate.NotAfter)
            {
                return false;
            }

            return true;
        }

        public bool Store(X509Certificate2 certificate, string certificateIdentifier)
        {
            try
            {
                File.WriteAllBytes(Path.Combine(_storePath, certificateIdentifier), certificate.Export(X509ContentType.Pkcs12));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
