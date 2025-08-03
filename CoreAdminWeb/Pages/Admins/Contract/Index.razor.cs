using CoreAdminWeb.Commons;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Contract;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using KeudellCoding.Blazor.AdvancedBlazorSelect2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.Contract
{
    public partial class Index(
        IBaseService<ContractModel> MainService,
        IContractDinhMucService ContractDinhMucService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<ContractTypeModel> ContractTypeService,
        IBaseService<DinhMucModel> DinhMucService,
        IFileService _fileService,
        IUserService UserService
    ) : BlazorCoreBase
    {
        private List<ContractModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private bool openDetailDeleteModal = false;
        private ContractModel SelectedItem { get; set; } = new ContractModel();
        private List<ContractDinhMucModel> SelectedItemsDetail { get; set; } = new List<ContractDinhMucModel>();
        private ContractDinhMucModel? SelectedItemDetail { get; set; } = default;
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";
        private string activeDefTab { get; set; } = "tab1";
        private IBrowserFile SelectedFile { get; set; } = default!;
        private string fileContent { get; set; } = string.Empty;
        private FileCRUDModel UploadFileCRUD { get; set; } = new FileCRUDModel();

        public List<DinhMucModel> DinhMucs { get; set; } = new List<DinhMucModel>();


        public Dictionary<int, List<DinhMucModel>> SelectedDinhMucItems { get; set; } = new();
        
        // Cache for filtering to avoid repeated API calls
        private readonly Dictionary<string, List<DinhMucModel>> _dinhMucCache = new();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
        
        // Debouncing for filter function
        private CancellationTokenSource? _filterCancellationTokenSource;
        private readonly object _cacheLock = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData();
                await LoadDinhMucData("");
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "/assets/js/pages/flatpickr.js");
                StateHasChanged();
            }
        }

        private async Task LoadData()
        {
            IsLoading = true;

            BuildPaginationQuery(Page, PageSize);
            BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            if (!string.IsNullOrEmpty(_searchString))
            {
                BuilderQuery += $"&filter[_and][1][_or][0][code][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][1][name][_contains]={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<ContractModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<ContractModel>();
            }
            IsLoading = false;
        }

        private async Task OnPageSizeChanged(int newSize)
        {
            Page = 1;
            PageSize = newSize;
            await LoadData();
        }

        private async Task SelectedPage(int page)
        {
            Page = page;
            await LoadData();
        }

        private async Task LoadDetailData()
        {
            var buildQuery = $"sort=sort";
            buildQuery += $"&filter[_and][][contract][_eq]={SelectedItem.id}";
            buildQuery += $"&filter[_and][][deleted][_eq]=false";
            var result = await ContractDinhMucService.GetAllAsync(buildQuery);
            SelectedItemsDetail = result.Data ?? new List<ContractDinhMucModel>();
        }

        private async Task<IEnumerable<CongTyModel>> LoadCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService);
        }

        private async Task<IEnumerable<ContractTypeModel>> LoadContractTypeData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, ContractTypeService);
        }

        private async Task<IEnumerable<UserModel>> LoadNhanVienData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                // query += $"&filter[_and][][role][_eq]={GlobalConstant.DOCTOR_ROLE_ID}";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(searchText)}";
                }

                var result = await UserService.GetAllAsync(query);
                return result?.IsSuccess == true ? result.Data ?? Enumerable.Empty<UserModel>() : Enumerable.Empty<UserModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
                return Enumerable.Empty<UserModel>();
            }
        }

        /// <summary>
        /// Load DinhMuc data with caching - Optimized version
        /// </summary>
        private async Task<IEnumerable<DinhMucModel>> LoadDinhMucData(string searchText)
        {
            try
            {
                Console.WriteLine($"Loading DinhMuc data with searchText: '{searchText}'");
                
                // Check cache first
                var cacheKey = searchText ?? string.Empty;
                lock (_cacheLock)
                {
                    if (_dinhMucCache.ContainsKey(cacheKey) && 
                        DateTime.Now - _lastCacheUpdate < _cacheExpiry)
                    {
                        Console.WriteLine($"Using cached DinhMuc data for '{searchText}'");
                        DinhMucs = _dinhMucCache[cacheKey];
                        return DinhMucs;
                    }
                }
                
                // Load from API if not in cache or cache expired
                var result = await LoadBlazorTypeaheadData(searchText ?? string.Empty, DinhMucService);
                var resultList = result?.ToList() ?? new List<DinhMucModel>();
                
                // Update cache
                lock (_cacheLock)
                {
                    _dinhMucCache[cacheKey] = resultList;
                    _lastCacheUpdate = DateTime.Now;
                }
                
                DinhMucs = resultList;
                Console.WriteLine($"Loaded {DinhMucs.Count} DinhMuc items from API");
                
                // Trigger UI update after loading data
                StateHasChanged();
                
                return resultList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading DinhMuc data: {ex.Message}");
                DinhMucs = new List<DinhMucModel>();
                return Enumerable.Empty<DinhMucModel>();
            }
        }

        private void OpenDeleteModal(ContractModel item)
        {
            SelectedItem = item;
            openDeleteModal = true;
        }

        private async Task OnDelete()
        {
            var result = await MainService.DeleteAsync(SelectedItem);
            if (result.IsSuccess && result.Data)
            {
                await LoadData();
                AlertService.ShowAlert("Xoá thành công!", "success");
                openDeleteModal = false;
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi xóa dữ liệu", "danger");
            }
        }

        private void CloseDeleteModal()
        {
            SelectedItem = new ContractModel();
            openDeleteModal = false;
        }

        private void OpenDetailDeleteModal(ContractDinhMucModel item)
        {
            SelectedItemDetail = item;

            openDetailDeleteModal = true;
        }

        private void OnDetailDelete()
        {
            if (SelectedItemDetail == null)
            {
                AlertService.ShowAlert("Không có dữ liệu để xóa", "warning");
                return;
            }

            foreach (var item in SelectedItemsDetail)
            {
                if (item.id > 0 && item.id == SelectedItemDetail.id || item.sort > 0 && item.sort == SelectedItemDetail.sort)
                {
                    item.deleted = true;
                }
            }

            SelectedItemDetail = default;

            openDetailDeleteModal = false;

            if (!SelectedItemsDetail.Any(c => c.deleted == null || c.deleted == false))
            {
                SelectedItemsDetail.Add(new ContractDinhMucModel()
                {
                    contract = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty
                });
            }

            StateHasChanged();
        }

        private void CloseDetailDeleteModal()
        {
            SelectedItemDetail = default;

            openDetailDeleteModal = false;
        }

        private void OnAddChiTiet()
        {
            if (SelectedItemsDetail == null)
            {
                SelectedItemsDetail = new List<ContractDinhMucModel>();
            }

            SelectedItemsDetail.Add(new ContractDinhMucModel
            {
                contract = SelectedItem,
                sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                code = string.Empty,
                name = string.Empty
            });
        }

        private async Task OpenAddOrUpdateModal(ContractModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new ContractModel();

            SelectedItemsDetail = new List<ContractDinhMucModel>();
            activeDefTab = "tab1";
            ClearImageUpload();

            if (SelectedItem.id > 0)
            {
                await LoadDetailData();
            }

            if (!SelectedItemsDetail.Any())
            {
                SelectedItemsDetail.Add(new ContractDinhMucModel()
                {
                    contract = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty
                });
            }

            openAddOrUpdateModal = true;

            // Wait for modal to render
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await JsRuntime.InvokeVoidAsync("initializeDatePicker");
            });
        }

        private async Task OnValidSubmit()
        {
            if (!FormValidation())
            {
                return;
            }
            await UpdateImageAsync();
            if (SelectedItem.id == 0)
            {
                var result = await MainService.CreateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    var chiTietList = SelectedItemsDetail
                        .Where(c => c.deleted == false || c.deleted == null)
                        .Select(c =>
                        {
                            c.contract = result.Data;
                            return c;
                        })
                        .ToList();

                    var detailResult = await ContractDinhMucService.CreateAsync(chiTietList);
                    if (!detailResult.IsSuccess)
                    {
                        AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                        return;
                    }
                    await LoadData();
                    ClearDinhMucCache(); // Clear cache after successful operation
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Thêm mới thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi thêm mới dữ liệu", "danger");
                }
            }
            else
            {
                var result = await MainService.UpdateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    var addNewChiTietList = SelectedItemsDetail
                        .Where(c => (c.deleted == false || c.deleted == null) && c.id == 0)
                        .Select(c =>
                        {
                            c.contract = SelectedItem;
                            return c;
                        }).ToList();
                    var removeChiTietList = SelectedItemsDetail
                        .Where(c => c.deleted == true && c.id > 0)
                        .Select(c =>
                        {
                            c.contract = SelectedItem;
                            c.deleted = true;
                            return c;
                        }).ToList();
                    var updateChiTietList = SelectedItemsDetail
                        .Where(c => (c.deleted == false || c.deleted == null) && c.id > 0)
                        .Select(c =>
                        {
                            c.contract = SelectedItem;
                            return c;
                        }).ToList();

                    if (addNewChiTietList.Any())
                    {
                        var detailResult = await ContractDinhMucService.CreateAsync(addNewChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (removeChiTietList.Any())
                    {
                        var detailResult = await ContractDinhMucService.DeleteAsync(removeChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi xóa chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (updateChiTietList.Any())
                    {
                        var detailResult = await ContractDinhMucService.UpdateAsync(updateChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi cập nhật chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    await LoadData();
                    ClearDinhMucCache(); // Clear cache after successful update
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Cập nhật thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật dữ liệu", "danger");
                }
            }
        }

        private bool FormValidation()
        {
            if (SelectedItem.code == null)
            {
                AlertService.ShowAlert("Mã là bắt buộc", "danger");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedItem.name))
            {
                AlertService.ShowAlert("Tên là bắt buộc", "danger");
                return false;
            }

            return true;
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new ContractModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        private void OnCongTyChanged(CongTyModel? selected)
        {
            SelectedItem.cong_ty = selected;
        }

        private void OnContractTypeChanged(ContractTypeModel? selected)
        {
            SelectedItem.contract_type = selected;
        }

        private void OnNhanVienChanged(UserModel? selected)
        {
            SelectedItem.nhan_vien_id = selected;
        }

        private void OnDateChanged(ChangeEventArgs e, string fieldName)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    switch (fieldName)
                    {
                        case "ngay_hop_dong":
                            SelectedItem.ngay_hop_dong = null;
                            break;

                        case "ngay_hieu_luc":
                            SelectedItem.ngay_hieu_luc = null;
                            break;
                        case "ngay_het_han":
                            SelectedItem.ngay_het_han = null;
                            break;
                    }
                    return;
                }

                var parts = dateStr.Split('/');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out int day) &&
                    int.TryParse(parts[1], out int month) &&
                    int.TryParse(parts[2], out int year))
                {
                    var date = new DateTime(year, month, day);

                    switch (fieldName)
                    {
                        case "ngay_hop_dong":
                            SelectedItem.ngay_hop_dong = date;
                            break;

                        case "ngay_hieu_luc":
                            SelectedItem.ngay_hieu_luc = date;
                            break;
                        case "ngay_het_han":
                            SelectedItem.ngay_het_han = date;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private void OnTabChanged(string tab)
        {
            activeDefTab = tab;

            if (activeDefTab == "tab1")
            {
                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }
        /// <summary>
        /// Get current selection for a specific ContractDinhMucModel item - Optimized version
        /// </summary>
        private List<DinhMucModel> GetCurrentSelection(ContractDinhMucModel item, int index)
        {
            // Create selection list only if item.MaDinhMuc exists
            var selection = item?.MaDinhMuc != null 
                ? new List<DinhMucModel> { item.MaDinhMuc } 
                : new List<DinhMucModel>();
            
            // Cache the selection for this index
            SelectedDinhMucItems[index] = selection;
            return selection;
        }

        /// <summary>
        /// Handle Select2 value change for DinhMuc selection - Optimized version
        /// </summary>
        private void OnDinhMucChanged(ContractDinhMucModel item, object? selected, int index)
        {
            try
            {
                // Optimized casting with pattern matching
                item.MaDinhMuc = selected switch
                {
                    KeudellCoding.Blazor.AdvancedBlazorSelect2.Select2<DinhMucModel, List<DinhMucModel>> select2 
                        => select2.Value?.FirstOrDefault(),
                    KeudellCoding.Blazor.AdvancedBlazorSelect2.Select2<DinhMucModel, IEnumerable<DinhMucModel>> select2Enum 
                        => select2Enum.Value?.FirstOrDefault(),
                    _ => null
                };

                // Update cached selection
                if (SelectedDinhMucItems.ContainsKey(index))
                {
                    SelectedDinhMucItems[index] = item.MaDinhMuc != null 
                        ? new List<DinhMucModel> { item.MaDinhMuc } 
                        : new List<DinhMucModel>();
                }

                // Update related calculations if needed
                UpdateThanhTienAfterDinhMucChange(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDinhMucChanged: {ex.Message}");
                AlertService?.ShowAlert("Lỗi khi thay đổi định mức", "danger");
            }
        }

        /// <summary>
        /// Update calculations after DinhMuc change
        /// </summary>
        private void UpdateThanhTienAfterDinhMucChange(ContractDinhMucModel item)
        {
            if (item.MaDinhMuc != null)
            {
                // Copy data from selected DinhMuc if needed
                item.code = item.MaDinhMuc.code;
                item.name = item.MaDinhMuc.name;
                
                // Recalculate amounts if quantities and prices are set
                if (item.so_luong.HasValue && item.don_gia_tt.HasValue)
                {
                    item.thanh_tien_tt = item.so_luong * item.don_gia_tt;
                }
                
                if (item.so_luong.HasValue && item.don_gia_dm.HasValue)
                {
                    item.thanh_tien_dm = item.so_luong * item.don_gia_dm;
                }
            }
        }

        private void UpdateThanhTien(ChangeEventArgs e, ContractDinhMucModel item, string field)
        {
            if (!decimal.TryParse(e.Value?.ToString(), out decimal value))
            {
                AlertService.ShowAlert("Dữ liệu không hợp lệ", "warning");
                return;
            }
            if (value < 0)
            {
                AlertService.ShowAlert("Số lượng không thể nhỏ hơn 0", "warning");
                return;
            }

            switch (field)
            {
                case nameof(item.so_luong):
                    item.so_luong = (int)value;
                    break;
                case nameof(item.don_gia_tt):
                    item.don_gia_tt = value;
                    break;
                case nameof(item.don_gia_dm):
                    item.don_gia_dm = value;
                    break;
            }

            item.thanh_tien_tt = null;
            item.thanh_tien_dm = null;

            if (item.so_luong.HasValue && item.don_gia_tt.HasValue)
            {
                item.thanh_tien_tt = item.so_luong * item.don_gia_tt;
            }

            if (item.so_luong.HasValue && item.don_gia_dm.HasValue)
            {
                item.thanh_tien_dm = item.so_luong * item.don_gia_dm;
            }
        }

        private async Task HandleFileSelect(InputFileChangeEventArgs e)
        {
            var files = e.GetMultipleFiles();
            if (files != null && files.Any())
            {
                await ProcessFile(files[0]);
            }
        }

        private async Task HandleDrop(DragEventArgs e)
        {
            var files = await JsRuntime.InvokeAsync<IReadOnlyList<IBrowserFile>>("getDroppedFiles", e);
            if (files?.Count > 0)
            {
                await ProcessFile(files[0]);
            }
        }

        private async Task ProcessFile(IBrowserFile file)
        {
            SelectedFile = file;

            if (SelectedFile != null)
            {
                var maxAllowSize = 5 * 1024 * 1024;
                if (SelectedFile.Size <= maxAllowSize) // 5MB max size
                {
                    try
                    {
                        var buffer = new byte[SelectedFile.Size];
                        await SelectedFile.OpenReadStream(maxAllowSize).ReadExactlyAsync(buffer);
                        var base64 = Convert.ToBase64String(buffer);
                        fileContent = $"data:{SelectedFile.ContentType};base64,{base64}";
                    }
                    catch (Exception ex)
                    {
                        await JsRuntime.InvokeVoidAsync("alert", $"Error processing image: {ex.Message}");
                    }
                    finally
                    {
                        StateHasChanged();
                    }
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync("alert", "File size exceeds 5MB limit");
                }
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("alert", "Invalid image format");
            }
        }

        async Task DownloadFile()
        {
            await JsRuntime.InvokeVoidAsync("downloadFile", fileContent, SelectedFile.Name);
        }

        private Task HandleDragOver(DragEventArgs e)
        {
            e.DataTransfer.DropEffect = "copy";
            return Task.CompletedTask;
        }

        private Task HandleDragEnter(DragEventArgs e)
        {
            // Optional: Add visual feedback for drag enter
            return Task.CompletedTask;
        }

        private Task HandleDragLeave(DragEventArgs e)
        {
            // Optional: Remove visual feedback for drag leave
            return Task.CompletedTask;
        }
        private async Task UpdateImageAsync()
        {
            if (SelectedFile != null)
            {
                try
                {
                    UploadFileCRUD.folder = Guid.Parse(GlobalConstant.CONTRACT_FOLDER_ID);
                    var fileUploaded = await _fileService.UploadFileAsync(SelectedFile, UploadFileCRUD);

                    if (
                        fileUploaded != null
                        && fileUploaded.IsSuccess
                        && fileUploaded.Data != null
                        && !string.IsNullOrEmpty(fileUploaded.Data.filename_download)
                    )
                    {
                        SelectedItem.file_hd = fileUploaded.Data;
                        // await MainService.UpdateAsync(SelectedItem);;

                        // fileContent = string.Empty;
                        // SelectedFile = default!;
                        // await LoadData();
                    }
                    else
                    {
                        await JsRuntime.InvokeVoidAsync("alert", "Failed to upload image.");
                    }
                }
                catch (Exception ex)
                {
                    await JsRuntime.InvokeVoidAsync("alert", $"Error saving image: {ex.Message}");
                }
                finally
                {
                    StateHasChanged();
                }
            }
        }

        private void ClearImageUpload()
        {
            fileContent = string.Empty;
            SelectedFile = default!;
        }



        /// <summary>
        /// Optimized filter function with debouncing and caching
        /// </summary>
        private async Task<List<DinhMucModel>> filterFunction(IEnumerable<DinhMucModel> allItems, string filter, CancellationToken token) 
        {
            try
            {
                // Cancel previous filter operation
                _filterCancellationTokenSource?.Cancel();
                _filterCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

                // Debouncing - wait 300ms before making API call
                await Task.Delay(300, _filterCancellationTokenSource.Token);

                var filterKey = filter ?? string.Empty;
                
                // Check cache first
                lock (_cacheLock)
                {
                    if (_dinhMucCache.ContainsKey(filterKey) && 
                        DateTime.Now - _lastCacheUpdate < _cacheExpiry)
                    {
                        Console.WriteLine($"Using cached filter result for '{filter}'");
                        return _dinhMucCache[filterKey];
                    }
                }

                // If not in cache, load from API
                Console.WriteLine($"Loading filter data from API for '{filter}'");
                var result = await LoadBlazorTypeaheadData(filter ?? string.Empty, DinhMucService, $"limit=20&offset=0&meta=filter_count");
                var resultList = result?.ToList() ?? new List<DinhMucModel>();

                // Update cache
                lock (_cacheLock)
                {
                    _dinhMucCache[filterKey] = resultList;
                    _lastCacheUpdate = DateTime.Now;
                }

                return resultList;
            }
            catch (OperationCanceledException)
            {
                // Filter operation was cancelled (user typed more characters)
                Console.WriteLine($"Filter operation cancelled for '{filter}'");
                return new List<DinhMucModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in filterFunction: {ex.Message}");
                return new List<DinhMucModel>();
            }
        }

        /// <summary>
        /// Clear cache when needed (e.g., after data changes)
        /// </summary>
        private void ClearDinhMucCache()
        {
            lock (_cacheLock)
            {
                _dinhMucCache.Clear();
                _lastCacheUpdate = DateTime.MinValue;
            }
        }
    }
}