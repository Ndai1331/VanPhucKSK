namespace CoreAdminWeb.Model.Settings
{
    public class SettingModel
    {
        public int? id { get; set; }
        public string? project_name { get; set; }
        public string? project_url { get; set; }
        public string? project_color { get; set; }
        public string? project_text_color { get; set; }
        public string? project_logo { get; set; }
        public string? public_foreground { get; set; }
        public string? public_background { get; set; }
        public string? public_note { get; set; }
        public int? auth_login_attempts { get; set; }
        public string? auth_password_policy { get; set; }
        public string? storage_asset_transform { get; set; }
        public string? storage_asset_presets { get; set; }
        public string? custom_css { get; set; }
        public string? storage_default_folder { get; set; }
        public string? basemaps { get; set; }
        public string? mapbox_key { get; set; }
        public List<ModuleBar> module_bar { get; set; } = new List<ModuleBar>();
        public string? project_descriptor { get; set; }
        public string? default_language { get; set; }
        public string? custom_aspect_ratios { get; set; }
        public string? public_favicon { get; set; }
        public string? default_appearance { get; set; }
        public string? default_theme_light { get; set; }
        public string? theme_light_overrides { get; set; }
        public string? default_theme_dark { get; set; }
        public string? theme_dark_overrides { get; set; }
        public string? report_error_url { get; set; }
        public string? report_bug_url { get; set; }
        public string? report_feature_url { get; set; }
        public bool? public_registration { get; set; }
        public bool? public_registration_verify_email { get; set; }
        public string? public_registration_role { get; set; }
        public string? public_registration_email_filter { get; set; }
        public List<VisualEditorUrl> visual_editor_urls { get; set; } = new List<VisualEditorUrl>();
    }

    public class VisualEditorUrl
    {
        public string? url { get; set; }
        public string? name { get; set; }
    }

    public class ModuleBar
    {
        public string? type { get; set; }
        public string? id { get; set; }
        public bool? enabled { get; set; }
        public bool? locked { get; set; }
    }
}