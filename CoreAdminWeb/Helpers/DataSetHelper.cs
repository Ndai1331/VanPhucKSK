using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;
using System.Data;

namespace CoreAdminWeb.Helpers
{
    public static class DataSetHelper
    {

        public static Status ReadStatus(object val)
        {
            if (val == null || val == DBNull.Value)
            {
                return Status.published;
            }

            if (val is int i)
            {
                return (Status)i;
            }

            var s = val.ToString()?.Trim();

            if (int.TryParse(s, out var n))
            {
                return (Status)n;
            }

            if (Enum.TryParse<Status>(s, true, out var byName))
            {
                return byName;
            }

            // map tay nếu tên enum khác string trong DB
            switch (s?.ToLowerInvariant())
            {
                case "published": return Status.published;
                case "draft": return Status.draft;
                case "removed": return Status.removed;
                default: return Status.published;
            }
        }

        public static bool TryGetOrdinal(IDataRecord r, string name, out int ordinal)
        {
            try { ordinal = r.GetOrdinal(name); return true; }
            catch (IndexOutOfRangeException) { ordinal = -1; return false; }
        }

        public static string ReadString(IDataRecord r, string name)
        {
            return TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i) ? r.GetString(i) : string.Empty;
        }

        public static Guid? ReadGuidNullable(IDataRecord r, string name)
        {
            if (!TryGetOrdinal(r, name, out var i) || i < 0 || r.IsDBNull(i))
            {
                return null;
            }

            var v = r.GetValue(i);
            if (v is Guid g)
            {
                return g;
            }

            if (v is string s && Guid.TryParse(s, out var g2))
            {
                return g2;
            }

            return null;
        }

        public static UserModel? MapUserOrNull(IDataRecord r, string prefix)
        {
            var id = ReadGuidNullable(r, $"{prefix}_id");
            if (id == null || id == Guid.Empty)
            {
                return null;
            }

            return new UserModel
            {
                id = id.Value,
                ma_tai_khoan = ReadString(r, $"{prefix}_ma_tai_khoan"),   // nếu cột này không có trong SELECT thì ReadString trả ""
                first_name = ReadString(r, $"{prefix}_first_name"),
                last_name = ReadString(r, $"{prefix}_last_name")
            };
        }

        public static DateTime? ReadDateTime(IDataRecord r, string name)
        {
            if (TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i))
            {
                return r.GetDateTime(i);
            }

            return null;
        }
        public static int ReadInt(IDataRecord r, string name, int defaultValue)
        {
            if (TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i))
            {
                var v = r.GetValue(i);
                if (v is int n)
                {
                    return n;
                }

                if (v is long l)
                {
                    return (int)l;
                }

                if (v is short s)
                {
                    return s;
                }

                if (v is byte b)
                {
                    return b;
                }

                if (v is string str && int.TryParse(str, out var parsed))
                {
                    return parsed;
                }
            }
            return defaultValue;
        }
        public static int? ReadInt(IDataRecord r, string name)
        {
            if (TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i))
            {
                var v = r.GetValue(i);
                if (v is int n)
                {
                    return n;
                }

                if (v is long l)
                {
                    return (int)l;
                }

                if (v is short s)
                {
                    return s;
                }

                if (v is byte b)
                {
                    return b;
                }

                if (v is string str && int.TryParse(str, out var parsed))
                {
                    return parsed;
                }
            }
            return default;
        }
        public static bool? ReadBool(IDataRecord r, string name)
        {
            if (TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i))
            {
                var v = r.GetValue(i);
                if (v is bool b)
                {
                    return b;
                }
                if (v is int n)
                {
                    return n != 0;
                }
                if (v is long l)
                {
                    return l != 0;
                }
                if (v is short s)
                {
                    return s != 0;
                }
                if (v is byte by)
                {
                    return by != 0;
                }
                if (v is string str)
                {
                    if (bool.TryParse(str, out var parsedBool))
                    {
                        return parsedBool;
                    }
                    if (int.TryParse(str, out var parsedInt))
                    {
                        return parsedInt != 0;
                    }
                }
            }
            return null;
        }
        public static bool ReadBool(IDataRecord r, string name, bool defaultValue = false)
        {
            if (TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i))
            {
                var v = r.GetValue(i);
                if (v is bool b)
                {
                    return b;
                }
                if (v is int n)
                {
                    return n != 0;
                }
                if (v is long l)
                {
                    return l != 0;
                }
                if (v is short s)
                {
                    return s != 0;
                }
                if (v is byte by)
                {
                    return by != 0;
                }
                if (v is string str)
                {
                    if (bool.TryParse(str, out var parsedBool))
                    {
                        return parsedBool;
                    }
                    if (int.TryParse(str, out var parsedInt))
                    {
                        return parsedInt != 0;
                    }
                }
            }
            return defaultValue;
        }
    }
}
