using System.Security.Cryptography;
using Libplanet.Common;
using Planetarium.Cryptography.BLS12_381;
using PublicKeyBLS = Planetarium.Cryptography.BLS12_381.PublicKey;

namespace VrfTestbed.VrfLib
{
    public class BlsCryptoBackend<T>: IBlsCryptoBackend<T>
        where T : HashAlgorithm
    {
        public BlsSignature Sign(HashDigest<T> messageHash, BlsPrivateKey privateKey)
        {
            if (!(privateKey is BlsPrivateKey blsPk))
            {
                throw new ArgumentException(
                    $"Given public key is not {nameof(BlsPrivateKey)}",
                    nameof(privateKey));
            }

            SecretKey sk = ValidateGetNativePrivateKey(blsPk);
            Msg msg = ConvertHashDigestToNativeMessage(messageHash);

            Signature sign = sk.Sign(msg);
            return new BlsSignature(sign.Serialize());
        }

        public byte[] GeneratePrivateKey()
        {
            SecretKey sk = default;
            sk.SetByCSPRNG();
            return sk.Serialize();
        }

        public BlsPublicKey GetPublicKey(BlsPrivateKey privateKey)
        {
            SecretKey sk = ValidateGetNativePrivateKey(privateKey);

            return new BlsPublicKey(sk.GetPublicKey().Serialize());
        }

        public BlsSignature GetProofOfPossession(BlsPrivateKey privateKey)
        {
            SecretKey sk = ValidateGetNativePrivateKey(privateKey);

            return new BlsSignature(sk.GetPop().Serialize());
        }

        public PublicKeyBLS ValidateGetNativePublicKey(BlsPublicKey publicKey)
        {
            try
            {
                PublicKeyBLS pk = default;
                pk.Deserialize(publicKey.ToByteArray());

                return pk;
            }
            catch (Exception e) when (e is ArgumentException || e is ArithmeticException)
            {
                throw new CryptographicException("Invalid public key.", e);
            }
        }

        public bool VerifyPoP(BlsPublicKey publicKey, BlsSignature proofOfPossession)
        {
            PublicKeyBLS pk = default;
            pk.Deserialize(publicKey.ToByteArray());

            Signature pop = default;
            try
            {
                pop.Deserialize(proofOfPossession.ToByteArray());
            }
            catch (Exception e) when (e is ArithmeticException || e is ArgumentException)
            {
                throw new CryptographicException(
                    "Invalid proof of possession.", e);
            }

            return pk.VerifyPop(pop);
        }

        public SecretKey ValidateGetNativePrivateKey(BlsPrivateKey privateKey)
        {
            SecretKey sk = default;
            try
            {
                sk.Deserialize(privateKey.ToByteArray());
            }
            catch (Exception e) when (e is ArithmeticException || e is ArgumentException)
            {
                throw new CryptographicException("Invalid private key.", e);
            }

            return sk;
        }

        public Signature ValidateGetNativeSignature(byte[] signature)
        {
            Signature sig = default;
            try
            {
                sig.Deserialize(signature);
            }
            catch (Exception e) when (e is ArithmeticException || e is ArgumentException)
            {
                throw new CryptographicException("Invalid signature.", e);
            }

            return sig;
        }

        public BlsSignature AggregateSignature(BlsSignature lhs, BlsSignature rhs)
        {
            Signature lhsSig = ValidateGetNativeSignature(lhs.ToByteArray());
            Signature rhsSig = ValidateGetNativeSignature(rhs.ToByteArray());
            lhsSig.Add(rhsSig);
            return new BlsSignature(lhsSig.Serialize());
        }

        public bool Verify(
            BlsSignature signature, BlsPublicKey[] publicKeys, HashDigest<T> message)
        {
            PublicKeyBLS[] pks = new PublicKeyBLS[publicKeys.Length];
            Msg msg = ConvertHashDigestToNativeMessage(message);

            for (var i = 0; i < pks.Length; ++i)
            {
                pks[i] = ValidateGetNativePublicKey(publicKeys[i]);
            }

            Signature sig = ValidateGetNativeSignature(signature.ToByteArray());
            return sig.FastAggregateVerify(in pks, msg);
        }

        private Msg ConvertHashDigestToNativeMessage(HashDigest<T> hashDigest)
        {
            Msg msg = default;
            try
            {
                msg.Set(hashDigest.ToByteArray());
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(
                    "The message hash is not a valid hash digest.", e);
            }

            return msg;
        }
    }
}