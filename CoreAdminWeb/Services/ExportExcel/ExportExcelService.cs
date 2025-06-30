using CoreAdminWeb.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Reflection;

namespace CoreAdminWeb.Services
{
    public interface IExportExcelService<T>
    {
        Task<byte[]> ExportToExcelAsync(List<T> data, List<string> fields, List<string>? labels = null, string sheetName = "Sheet1");
    }
    public class ExportExcelService<T> : IExportExcelService<T>
    {
        /// <summary>
        /// Exports a list of data to an Excel file with dynamic fields.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fields"></param>
        /// <param name="labels"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportToExcelAsync(List<T> data, List<string> fields, List<string>? labels = null, string sheetName = "Sheet1")
        {
            ExcelPackage.License.SetNonCommercialPersonal("Personal");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Write header
                for (int i = 0; i < fields.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = labels != null && labels.Count > i ? labels[i] : fields[i];
                }

                // Write data
                for (int row = 0; row < data.Count; row++)
                {
                    var item = data[row];
                    if (item != null)
                    {
                        for (int col = 0; col < fields.Count; col++)
                        {
                            string field = fields[col];
                            bool isEnum = false;
                            string enumTypeName = string.Empty;
                            if (field.EndsWith("_enum"))
                            {
                                isEnum = true;
                                field = field.Replace("_enum", "", StringComparison.InvariantCultureIgnoreCase);
                            }
                            else if (field.Contains(":"))
                            {
                                isEnum = true;
                                var strField = field.Split(':');
                                field = strField[0];
                                enumTypeName = strField.Length > 1 ? strField[1] : string.Empty;
                            }
                            var value = GetNestedPropertyValue(item, field);
                            var cell = worksheet.Cells[row + 2, col + 1];

                            if (value is null)
                            {
                                cell.Value = null;
                            }
                            else if (value is bool b)
                            {
                                cell.Value = b ? "X" : "";
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                            else if (value is DateTime dt)
                            {
                                cell.Value = dt;
                                cell.Style.Numberformat.Format = "dd/mm/yyyy";
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            else if (value is DateTimeOffset dto)
                            {
                                cell.Value = dto.DateTime;
                                cell.Style.Numberformat.Format = "dd/mm/yyyy";
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            else if (value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
                            {
                                cell.Value = value;
                                cell.Style.Numberformat.Format = "#,##0";
                                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            else if (isEnum)
                            {
                                if (!string.IsNullOrEmpty(enumTypeName))
                                {
                                    var enumType = GetEnumTypeByName(enumTypeName);
                                    if (enumType != null)
                                        cell.Value = value.GetEnumDescription(enumType);
                                    else
                                        cell.Value = value.ToString();
                                }
                                else
                                    cell.Value = value.GetEnumDescription();
                            }
                            else
                            {
                                cell.Value = value.ToString();
                            }
                        }
                    }
                }

                // Style header
                using (var range = worksheet.Cells[1, 1, 1, fields.Count])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Style table cells
                if (data.Count > 0)
                    using (var range = worksheet.Cells[2, 1, data.Count + 1, fields.Count])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        private object? GetNestedPropertyValue(object? obj, string propertyPath)
        {
            foreach (var part in propertyPath.Split('.'))
            {
                if (obj == null) return null;
                var type = obj.GetType();
                var prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) return null;
                obj = prop.GetValue(obj, null);
            }
            return obj;
        }
        private Type? GetEnumTypeByName(string enumTypeName)
        {
            // Search all loaded assemblies for the enum type by name
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes()
                    .FirstOrDefault(t => t.IsEnum && t.Name.Equals(enumTypeName, StringComparison.InvariantCultureIgnoreCase));
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}