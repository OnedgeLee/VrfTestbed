using System.Security.Cryptography;
using Libplanet.Common;
using Libplanet.Crypto;
using Planetarium.Cryptography.BLS12_381;
using PublicKeyBLS = Planetarium.Cryptography.BLS12_381.PublicKey;

namespace VrfTestbed.VrfLib
{
    public interface IBlsCryptoBackend<T> 
        where T : HashAlgorithm
    {
        public byte[] GeneratePrivateKey();

        BlsPublicKey GetPublicKey(BlsPrivateKey privateKey);

        BlsSignature GetProofOfPossession(BlsPrivateKey privateKey);

        PublicKeyBLS ValidateGetNativePublicKey(BlsPublicKey publicKey);

        SecretKey ValidateGetNativePrivateKey(BlsPrivateKey privateKey);

        Signature ValidateGetNativeSignature(byte[] signature);

        bool VerifyPoP(BlsPublicKey publicKey, BlsSignature proofOfPossession);

        BlsSignature Sign(HashDigest<T> messageHash, BlsPrivateKey privateKey);

        BlsSignature AggregateSignature(BlsSignature lhs, BlsSignature rhs);

        bool Verify(BlsSignature signature, BlsPublicKey[] publicKeys, HashDigest<T> message);
    }
}
