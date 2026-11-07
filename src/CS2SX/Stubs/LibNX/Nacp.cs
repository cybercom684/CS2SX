// Auto-generated from nacp.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX;

public unsafe struct NacpLanguageEntry
{
    public byte name;
    public byte author;
}

public unsafe struct NacpApplicationNeighborDetectionGroupConfiguration
{
    public u64 group_id;
    public u8 key;
}

public unsafe struct NacpNeighborDetectionClientConfiguration
{
    public NacpApplicationNeighborDetectionGroupConfiguration send_group_configuration;
    public NacpApplicationNeighborDetectionGroupConfiguration receivable_group_configurations;
}

public unsafe struct NacpApplicationJitConfiguration
{
    public u64 flags;
    public u64 memory_size;
}

public unsafe struct NacpStruct
{
    // skipped array: NacpLanguageEntry lang[16]
    public u8 isbn;
    public u8 startup_user_account;
    public u8 user_account_switch_lock;
    public u8 add_on_content_registration_type;
    public u32 attribute_flag;
    public u32 supported_language_flag;
    public u32 parental_control_flag;
    public u8 screenshot;
    public u8 video_capture;
    public u8 data_loss_confirmation;
    public u8 play_log_policy;
    public u64 presence_group_id;
    public s8 rating_age;
    public byte display_version;
    public u64 add_on_content_base_id;
    public u64 save_data_owner_id;
    public u64 user_account_save_data_size;
    public u64 user_account_save_data_journal_size;
    public u64 device_save_data_size;
    public u64 device_save_data_journal_size;
    public u64 bcat_delivery_cache_storage_size;
    public u64 application_error_code_category;
    public u64 local_communication_id;
    public u8 logo_type;
    public u8 logo_handling;
    public u8 runtime_add_on_content_install;
    public u8 runtime_parameter_delivery;
    public u8 reserved_x30f4;
    public u8 crash_report;
    public u8 hdcp;
    public u64 pseudo_device_id_seed;
    public byte bcat_passphrase;
    public u8 startup_user_account_option;
    public u8 reserved_for_user_account_save_data_operation;
    public u64 user_account_save_data_size_max;
    public u64 user_account_save_data_journal_size_max;
    public u64 device_save_data_size_max;
    public u64 device_save_data_journal_size_max;
    public u64 temporary_storage_size;
    public u64 cache_storage_size;
    public u64 cache_storage_journal_size;
    public u64 cache_storage_data_and_journal_size_max;
    public u16 cache_storage_index_max;
    public u8 reserved_x318a;
    public u64 play_log_queryable_application_id;
    public u8 play_log_query_capability;
    public u8 repair_flag;
    public u8 program_index;
    public u8 required_network_service_license_on_launch;
    public u32 reserved_x3214;
    public NacpNeighborDetectionClientConfiguration neighbor_detection_client_configuration;
    public NacpApplicationJitConfiguration jit_configuration;
    public u8 reserved_x33c0;
}

public static class Nacp
{
    public static extern Result nacpGetLanguageEntry(ref NacpStruct nacp, ref NacpLanguageEntry langentry);
}
