// namespace CoreAdminWeb.Helpers
// {
//     public static class Select2Helper
//     {
//         /// <summary>
//         /// Retrieves the current selection of items based on the specified selected item and index,  and updates the
//         /// provided cache with the selection.
//         /// </summary>
//         /// <remarks>The method updates the <paramref name="cache"/> by associating the specified
//         /// <paramref name="index"/>  with the returned selection. If <paramref name="selectedItem"/> is <see
//         /// langword="null"/>, the cache  will store an empty list for the given index.</remarks>
//         /// <typeparam name="TModel">The type of the items in the selection. Must be a reference type.</typeparam>
//         /// <param name="selectedItem">The currently selected item. If <see langword="null"/>, the selection will be empty.</param>
//         /// <param name="index">The index at which the selection is stored in the cache.</param>
//         /// <param name="cache">A dictionary used to store the selection, keyed by the specified index.</param>
//         /// <returns>A list containing the selected item if <paramref name="selectedItem"/> is not <see langword="null"/>; 
//         /// otherwise, an empty list.</returns>
//         public static List<TModel> GetCurrentSelectionGeneric<TModel>(TModel? selectedItem, int index, Dictionary<int, List<TModel>> cache)
//             where TModel : class
//         {
//             var selection = selectedItem != null
//                 ? new List<TModel> { selectedItem }
//                 : new List<TModel>();

//             cache[index] = selection;
//             return selection;
//         }

//         /// <summary>
//         /// Handles the change event for a Select2 component, updating the selected value and optionally executing an
//         /// action.
//         /// </summary>
//         /// <remarks>This method processes the selected value from a Select2 component, updates the
//         /// provided cache, and invokes the specified action if provided. If the selected value is null, the cache entry
//         /// for the specified index will be cleared.</remarks>
//         /// <typeparam name="TModel">The type of the model used in the Select2 component. Must be a reference type.</typeparam>
//         /// <param name="selected">The selected object, which is expected to be a Select2 component containing the selected value.</param>
//         /// <param name="setValue">An action to set the selected value. The value will be null if no selection is made.</param>
//         /// <param name="index">The index used to identify the cache entry to update.</param>
//         /// <param name="cache">A dictionary that caches the selected values by index. The cache will be updated with the new selection.</param>
//         /// <param name="action">An optional action to execute after the selection is processed. Can be null.</param>
//         public static void OnSelect2ChangedGeneric<TModel>(object? selected, Action<TModel?> setValue, int index, Dictionary<int, List<TModel>> cache, Action? action = null)
//             where TModel : class
//         {
//             try
//             {
//                 var value = selected switch
//                 {
//                     KeudellCoding.Blazor.AdvancedBlazorSelect2.Select2<TModel, List<TModel>> select2
//                         => select2.Value?.FirstOrDefault(),
//                     KeudellCoding.Blazor.AdvancedBlazorSelect2.Select2<TModel, IEnumerable<TModel>> select2Enum
//                         => select2Enum.Value?.FirstOrDefault(),
//                     _ => null
//                 };

//                 if (action != null)
//                 {
//                     action();
//                 }

//                 setValue(value);

//                 if (cache.ContainsKey(index))
//                 {
//                     cache[index] = value != null ? new List<TModel> { value } : new List<TModel>();
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error in OnSelect2ChangedGeneric: {ex.Message}");
//             }
//         }

//         /// <summary>
//         /// Retrieves an element from a collection by matching its identifier.
//         /// </summary>
//         /// <remarks>This method attempts to parse the <paramref name="id"/> as an integer and uses the
//         /// <paramref name="idSelector"/> function to compare the parsed identifier with the elements in the collection.
//         /// If <paramref name="items"/> is <see langword="null"/>, <paramref name="id"/> is not a valid integer, or no
//         /// matching element is found, the method returns <see langword="default"/>.</remarks>
//         /// <typeparam name="TModel">The type of elements in the collection.</typeparam>
//         /// <param name="items">The collection of elements to search. Can be <see langword="null"/>.</param>
//         /// <param name="id">The identifier to match. Must be a valid integer string. Can be <see langword="null"/> or empty.</param>
//         /// <param name="idSelector">A function to extract the identifier from an element. Cannot be <see langword="null"/>.</param>
//         /// <returns>The first element in the collection that matches the specified identifier, or <see langword="default"/> if
//         /// no match is found or if the input parameters are invalid.</returns>
//         public static TModel? GetElementByIdGeneric<TModel>(IEnumerable<TModel>? items, string? id, Func<TModel, int?> idSelector)
//         {
//             if (items == null || string.IsNullOrEmpty(id) || !int.TryParse(id, out int parsedId))
//             {
//                 return default;
//             }

//             return items.FirstOrDefault(c => !EqualityComparer<TModel>.Default.Equals(c, default) && idSelector(c) == parsedId);
//         }
//     }
// }
