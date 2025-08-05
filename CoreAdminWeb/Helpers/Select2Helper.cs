namespace CoreAdminWeb.Helpers
{
    public static class Select2Helper
    {
        /// <summary>
        /// Retrieves the current selection of items based on the specified selected item and index,  and updates the
        /// provided cache with the selection.
        /// </summary>
        /// <remarks>The method updates the <paramref name="cache"/> by associating the specified
        /// <paramref name="index"/>  with the returned selection. If <paramref name="selectedItem"/> is <see
        /// langword="null"/>, the cache  will store an empty list for the given index.</remarks>
        /// <typeparam name="TModel">The type of the items in the selection. Must be a reference type.</typeparam>
        /// <param name="selectedItem">The currently selected item. If <see langword="null"/>, the selection will be empty.</param>
        /// <param name="index">The index at which the selection is stored in the cache.</param>
        /// <param name="cache">A dictionary used to store the selection, keyed by the specified index.</param>
        /// <returns>A list containing the selected item if <paramref name="selectedItem"/> is not <see langword="null"/>; 
        /// otherwise, an empty list.</returns>
        public static List<TModel> GetCurrentSelectionGeneric<TModel>(TModel? selectedItem, int index, Dictionary<int, List<TModel>> cache)
            where TModel : class
        {
            var selection = selectedItem != null
                ? new List<TModel> { selectedItem }
                : new List<TModel>();

            cache[index] = selection;
            return selection;
        }

        /// <summary>
        /// Handles the change event for a Select2 component, updating the selected value and optionally executing an
        /// action.
        /// </summary>
        /// <remarks>This method processes the selected value from a Select2 component, updates the
        /// provided cache, and invokes the specified action if provided. If the selected value is null, the cache entry
        /// for the specified index will be cleared.</remarks>
        /// <typeparam name="TModel">The type of the model used in the Select2 component. Must be a reference type.</typeparam>
        /// <param name="selected">The selected object, which is expected to be a Select2 component containing the selected value.</param>
        /// <param name="setValue">An action to set the selected value. The value will be null if no selection is made.</param>
        /// <param name="index">The index used to identify the cache entry to update.</param>
        /// <param name="cache">A dictionary that caches the selected values by index. The cache will be updated with the new selection.</param>
        /// <param name="action">An optional action to execute after the selection is processed. Can be null.</param>
        public static void OnSelect2ChangedGeneric<TModel>(object? selected, Action<TModel?> setValue, int index, Dictionary<int, List<TModel>> cache, Action? action = null)
            where TModel : class
        {
            try
            {
                var value = selected switch
                {
                    KeudellCoding.Blazor.AdvancedBlazorSelect2.Select2<TModel, List<TModel>> select2
                        => select2.Value?.FirstOrDefault(),
                    KeudellCoding.Blazor.AdvancedBlazorSelect2.Select2<TModel, IEnumerable<TModel>> select2Enum
                        => select2Enum.Value?.FirstOrDefault(),
                    _ => null
                };

                if (action != null)
                {
                    action();
                }

                setValue(value);

                if (cache.ContainsKey(index))
                {
                    cache[index] = value != null ? new List<TModel> { value } : new List<TModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnSelect2ChangedGeneric: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves an element from a collection by matching its identifier.
        /// </summary>
        /// <remarks>The method supports identifiers of type <see cref="int"/>, <see cref="Guid"/>, and
        /// <see cref="string"/>.  If the identifier cannot be parsed into the specified type <typeparamref
        /// name="TId"/>, the method returns <see langword="null"/>.</remarks>
        /// <typeparam name="TModel">The type of the elements in the collection.</typeparam>
        /// <typeparam name="TId">The type of the identifier used to match elements. Must be a value type that implements <see
        /// cref="IEquatable{T}"/>.</typeparam>
        /// <param name="items">The collection of elements to search. Can be <see langword="null"/>.</param>
        /// <param name="id">The identifier to match. Can be <see langword="null"/> or empty.</param>
        /// <param name="idSelector">A function that extracts the identifier from an element.</param>
        /// <returns>The first element in the collection whose identifier matches the specified <paramref name="id"/>,  or <see
        /// langword="null"/> if no match is found, the collection is <see langword="null"/>, or the identifier is
        /// invalid.</returns>
        public static TModel? GetElementByIdGeneric<TModel, TId>(IEnumerable<TModel>? items, string? id, Func<TModel, TId?> idSelector) where TId : struct, IEquatable<TId>
        {
            if (items == null || string.IsNullOrEmpty(id))
            {
                return default;
            }

            TId parsedId;
            try
            {
                if (typeof(TId) == typeof(int) && int.TryParse(id, out var intId))
                {
                    parsedId = (TId)(object)intId;
                }
                else if (typeof(TId) == typeof(Guid) && Guid.TryParse(id, out var guidId))
                {
                    parsedId = (TId)(object)guidId;
                }
                else if (typeof(TId) == typeof(string))
                {
                    parsedId = (TId)(object)id;
                }
                else
                {
                    return default;
                }
            }
            catch
            {
                return default;
            }

            return items.FirstOrDefault(c =>
                !EqualityComparer<TModel>.Default.Equals(c, default) &&
                idSelector(c)?.Equals(parsedId) == true);
        }
    }
}
