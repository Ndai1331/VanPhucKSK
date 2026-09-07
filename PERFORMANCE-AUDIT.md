# Van Phuc KSK performance and UI audit

## Scope

This review covered the Blazor Server application, API controllers, HTTP services, import/export workflows, Razor layouts, and the Vite/Tailwind front-end source. The repository does not contain an automated test project or production database query plans, so database improvements below are code-level remediations plus index recommendations to validate with `STATISTICS IO/TIME` and actual execution plans.

## Remediations applied

### Database/API round trips

- Dashboard company-medical metrics were changed from four sequential commands to one SQL batch with multiple result sets.
- Dashboard company-summary metrics were changed from five sequential commands to one SQL batch with multiple result sets.
- Report data plus count, personal records data plus count, group data plus count, and electronic invoices data plus count now use one database round trip per request.
- `KhamSucKhoeKQCLSController` now passes visit IDs as one `STRING_SPLIT` parameter instead of generating one SQL parameter per ID.
- Raw SQL filters in the personal and group endpoints are parameterized. Date filters use half-open ranges (`>= from`, `< day-after-to`) so indexes can be used and time-of-day values are not lost.
- A correlated company lookup in the personal endpoint was replaced with a normal join.
- Database calls use `RequestAborted`, allowing abandoned browser requests to stop consuming database work.

### Service and import performance

- Added a scoped `ApplicationStateService` that shares in-flight settings, logo/favicon, current-user, and menu requests within a Blazor circuit. This removes duplicate layout/page calls and groups menu children in O(P+N) time.
- Added in-flight current-user caching to `UserService`.
- Replaced the static mutable `StringBuilder` used by the base page query builder with request/component-local builders, removing cross-circuit races.
- Fixed the HTTP client token/header race by creating per-request `HttpRequestMessage` instances. Concurrent `Task.WhenAll` calls no longer mutate shared `DefaultRequestHeaders`.
- Fixed the permanent-cancellation-token bug in the local/public clients; cancelling one operation no longer poisons all future requests.
- HTTP responses are disposed after parsing.
- Import services no longer create a child DI scope and then retain dependencies after disposing that scope. Repeated list scans now use `HashSet`/dictionary lookups, import work accepts cancellation, and import progress calculations handle one-row imports correctly.
- Fixed the missing medical-record creation path in the examination-book import and replaced direct `new HttpClient()` calls with the configured factory client.

### UI loading and UX

- Loading overlay now has a visible backdrop, spinner, Vietnamese status text, `role=status`, `aria-live`, `aria-busy`, focus-visible styles, and reduced-motion support.
- Main and admin layouts now have a skip link and semantic `<main>` landmark. The document language is `vi`.
- Main list/report/detail load paths now hide the global overlay in `finally`, including when an API or JS call fails.
- Removed orphaned `Task.Run` calls used only to delay date-picker/currency UI updates. Blazor state changes and JS initialization are now awaited on the renderer context.
- Replaced invalid duplicate Alpine `x-data` attributes on the body.
- Converted sidebar JavaScript links used as buttons to native buttons with `aria-expanded`/`aria-controls`.
- Removed duplicate jQuery, flatpickr, magnific-popup, and form-editor script inclusions from the shared layout.

### Server/static asset pressure

- Enabled response compression before static files.
- Reduced the SignalR receive-message limit from 100 MB to 32 MB, still above the application’s approximately 24 MB upload limit.
- Excluded Vite source files and source maps from ASP.NET static-web-asset enumeration; the Vite source remains available to the front-end toolchain.

## Priority follow-up recommendations

1. Validate indexes against real plans before applying them. Candidate leading keys are:

   - `SoKhamSucKhoe(MaDotKham, deleted, ngay_kham, ma_luot_kham)`
   - `kham_suc_khoe_cong_ty(ma_hop_dong_ksk, ngay_du_kien_kham, deleted)`
   - `kham_suc_khoe_ket_luan(luot_kham, ma_luot_kham, deleted)`
   - `kham_suc_khoe_dinh_muc_thuc_te(contract, deleted, MaDinhMuc)`
   - `kham_suc_khoe_ket_qua_can_lam_sang(luot_kham, ma_luot_kham, kq_cls)`
   - `ket_qua_can_lam_sang_chi_tiet(ma_luot_kham, ma_cls, loai_cls)`

2. The export/import batch loops are intentionally bounded to protect the API and memory. If telemetry shows the API can handle more concurrency, tune the existing bounded parallelism rather than launching unbounded tasks.

3. Add request-duration, SQL duration, row-count, and outbound HTTP telemetry. This is required to distinguish database scans, API latency, serialization, and Word/PDF generation time.

4. Revisit the remaining report query with correlated `STRING_AGG` after capturing an actual plan. A filtered/page-first CTE may help on large datasets, but aggregating the entire result table on every small page can be worse without measured evidence.

5. Update `SixLabors.ImageSharp` from 3.1.1 after compatibility testing. The current restore cache is also invalid (`obj/project.nuget.cache` contains no JSON), which should be repaired by the normal CI restore rather than checked in or manually copied.

6. Restrict the current allow-any-origin CORS policies to the deployed UI/API origins. This is primarily a security fix, but it also prevents unintended cross-origin traffic.

7. Add integration tests for the multi-result-set endpoints, empty/one-item import files, cancellation, token refresh retry, and loading-overlay cleanup.

## Verification

The application builds successfully with:

```text
dotnet build CoreAdminWeb/CoreAdminWeb.csproj --no-restore --nologo -v:minimal -p:StaticWebAssetsEnabled=false
```

Result: **0 errors, 127 warnings**. The warnings are existing model nullability, obsolete FTP/PDF APIs, and unused members; they should be reduced in a separate cleanup pass. The normal static-web-assets build remains dominated by the repository’s very large generated/static asset tree, which is why the verification build disables that packaging phase.

The Vite package dependencies are not installed in this checkout (`CoreAdminWeb/node_modules` is absent), so a front-end bundle build was not run. Run `npm ci && npm run build` in `CoreAdminWeb` in CI or on the UI build machine.
