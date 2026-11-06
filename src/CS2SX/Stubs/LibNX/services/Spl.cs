// Auto-generated from services\spl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum SplConfigItem
{
    SplConfigItem_DisableProgramVerification = 1,
    SplConfigItem_DramId = 2,
    SplConfigItem_SecurityEngineIrqNumber = 3,
    SplConfigItem_Version = 4,
    SplConfigItem_HardwareType = 5,
    SplConfigItem_IsRetail = 6,
    SplConfigItem_IsRecoveryBoot = 7,
    SplConfigItem_DeviceId = 8,
    SplConfigItem_BootReason = 9,
    SplConfigItem_MemoryArrange = 10,
    SplConfigItem_IsDebugMode = 11,
    SplConfigItem_KernelMemoryConfiguration = 12,
    SplConfigItem_IsChargerHiZModeEnabled = 13,
    SplConfigItem_IsKiosk = 14,
    SplConfigItem_NewHardwareType = 15,
    SplConfigItem_NewKeyGeneration = 16,
    SplConfigItem_Package2Hash = 17,
}

public enum RsaKeyVersion
{
    RsaKeyVersion_Deprecated = 0,
    RsaKeyVersion_Extended = 1,
}

public static class Spl
{
    public static extern Result splInitialize();
    public static extern void splExit();
    public static extern IntPtr splGetServiceSession();
    public static extern Result splCryptoInitialize();
    public static extern void splCryptoExit();
    public static extern IntPtr splCryptoGetServiceSession();
    public static extern Result splSslInitialize();
    public static extern void splSslExit();
    public static extern IntPtr splSslGetServiceSession();
    public static extern Result splEsInitialize();
    public static extern void splEsExit();
    public static extern IntPtr splEsGetServiceSession();
    public static extern Result splFsInitialize();
    public static extern void splFsExit();
    public static extern IntPtr splFsGetServiceSession();
    public static extern Result splManuInitialize();
    public static extern void splManuExit();
    public static extern IntPtr splManuGetServiceSession();
    public static extern Result splGetConfig(SplConfigItem config_item, ref u64 out_config);
    public static extern Result splUserExpMod(IntPtr input, IntPtr modulus, IntPtr exp, ulong exp_size, IntPtr dst);
    public static extern Result splSetConfig(SplConfigItem config_item, u64 value);
    public static extern Result splGetRandomBytes(IntPtr @out, ulong out_size);
    public static extern Result splIsDevelopment(ref bool out_is_development);
    public static extern Result splSetBootReason(u32 value);
    public static extern Result splGetBootReason(ref u32 out_value);
    public static extern Result splCryptoGenerateAesKek(IntPtr wrapped_kek, u32 key_generation, u32 option, IntPtr out_sealed_kek);
    public static extern Result splCryptoLoadAesKey(IntPtr sealed_kek, IntPtr wrapped_key, u32 keyslot);
    public static extern Result splCryptoGenerateAesKey(IntPtr sealed_kek, IntPtr wrapped_key, IntPtr out_sealed_key);
    public static extern Result splCryptoDecryptAesKey(IntPtr wrapped_key, u32 key_generation, u32 option, IntPtr out_sealed_key);
    public static extern Result splCryptoCryptAesCtr(IntPtr input, IntPtr output, ulong size, u32 keyslot, IntPtr ctr);
    public static extern Result splCryptoComputeCmac(IntPtr input, ulong size, u32 keyslot, IntPtr out_cmac);
    public static extern Result splCryptoLockAesEngine(ref u32 out_keyslot);
    public static extern Result splCryptoUnlockAesEngine(u32 keyslot);
    public static extern Result splCryptoGetSecurityEngineEvent(ref Event out_event);
    public static extern Result splRsaDecryptPrivateKey(IntPtr sealed_kek, IntPtr wrapped_key, IntPtr wrapped_rsa_key, ulong wrapped_rsa_key_size, RsaKeyVersion version, IntPtr dst, ulong dst_size);
    public static extern Result splSslLoadSecureExpModKey(IntPtr sealed_kek, IntPtr wrapped_key, IntPtr wrapped_rsa_key, ulong wrapped_rsa_key_size);
    public static extern Result splSslSecureExpMod(IntPtr input, IntPtr modulus, IntPtr dst);
    public static extern Result splEsLoadRsaOaepKey(IntPtr sealed_kek, IntPtr wrapped_key, IntPtr wrapped_rsa_key, ulong wrapped_rsa_key_size, RsaKeyVersion version);
    public static extern Result splEsUnwrapRsaOaepWrappedTitlekey(IntPtr rsa_wrapped_titlekey, IntPtr modulus, IntPtr label_hash, ulong label_hash_size, u32 key_generation, IntPtr out_sealed_titlekey);
    public static extern Result splEsUnwrapAesWrappedTitlekey(IntPtr aes_wrapped_titlekey, u32 key_generation, IntPtr out_sealed_titlekey);
    public static extern Result splEsLoadSecureExpModKey(IntPtr sealed_kek, IntPtr wrapped_key, IntPtr wrapped_rsa_key, ulong wrapped_rsa_key_size);
    public static extern Result splEsSecureExpMod(IntPtr input, IntPtr modulus, IntPtr dst);
    public static extern Result splEsUnwrapElicenseKey(IntPtr rsa_wrapped_elicense_key, IntPtr modulus, IntPtr label_hash, ulong label_hash_size, u32 key_generation, IntPtr out_sealed_elicense_key);
    public static extern Result splEsLoadElicenseKey(IntPtr sealed_elicense_key, u32 keyslot);
    public static extern Result splFsLoadSecureExpModKey(IntPtr sealed_kek, IntPtr wrapped_key, IntPtr wrapped_rsa_key, ulong wrapped_rsa_key_size, RsaKeyVersion version);
    public static extern Result splFsSecureExpMod(IntPtr input, IntPtr modulus, IntPtr dst);
    public static extern Result splFsGenerateSpecificAesKey(IntPtr wrapped_key, u32 key_generation, u32 option, IntPtr out_sealed_key);
    public static extern Result splFsLoadTitlekey(IntPtr sealed_titlekey, u32 keyslot);
    public static extern Result splFsGetPackage2Hash(IntPtr out_hash);
    public static extern Result splManuEncryptRsaKeyForImport(IntPtr sealed_kek_pre, IntPtr wrapped_key_pre, IntPtr sealed_kek_post, IntPtr wrapped_kek_post, u32 option, IntPtr wrapped_rsa_key, IntPtr out_wrapped_rsa_key, ulong rsa_key_size);
}
